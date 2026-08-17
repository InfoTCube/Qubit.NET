using System.Numerics;
using Qubit.NET;
using Qubit.NET.Simulation;
using Qubit.NET.Utilities;
using static QubitNet.Tests.Amplitudes;

namespace QubitNet.Tests;

/// <summary>
/// Classical feedforward: measuring into a classical bit and conditioning later gates on it.
/// Quantum teleportation is the canonical use, and its correctness is the real test here.
/// </summary>
public class FeedforwardTests
{
    [Fact]
    public void Classical_bits_start_at_zero() =>
        Assert.Equal(0, new QuantumCircuit(2).ClassicalBit(0));

    [Fact]
    public void MeasureInto_records_the_outcome()
    {
        QuantumCircuit qc = new(2);
        qc.X(0);

        int result = qc.MeasureInto(0, 1);

        Assert.Equal(1, result);
        Assert.Equal(1, qc.ClassicalBit(1));
        Assert.Equal(0, qc.ClassicalBit(0));
    }

    [Fact]
    public void Measure_populates_the_matching_classical_bits()
    {
        QuantumCircuit qc = new(3);
        qc.X(0);
        qc.X(2);
        qc.Measure();

        Assert.Equal(1, qc.ClassicalBit(0));
        Assert.Equal(0, qc.ClassicalBit(1));
        Assert.Equal(1, qc.ClassicalBit(2));
    }

    [Fact]
    public void Partial_measure_populates_only_the_qubits_it_measured()
    {
        QuantumCircuit qc = new(3);
        qc.X(2);
        qc.Measure(2, 0);

        Assert.Equal(1, qc.ClassicalBit(2));
        Assert.Equal(0, qc.ClassicalBit(0));
    }

    [Fact]
    public void When_applies_the_body_only_if_the_bit_matches()
    {
        QuantumCircuit applied = new(2);
        applied.X(0);
        applied.MeasureInto(0, 0);
        applied.When(0, 1, c => c.X(1));

        // Bit 0 came out 1, so the X ran: |11>, index 3.
        AssertState(applied, 0, 0, 0, 1);

        QuantumCircuit skipped = new(2);
        skipped.MeasureInto(0, 0);
        skipped.When(0, 1, c => c.X(1));

        // Bit 0 came out 0, so the X was skipped: |00>, index 0.
        AssertState(skipped, 1, 0, 0, 0);
    }

    [Fact]
    public void Conditional_gates_are_still_recorded_when_skipped()
    {
        QuantumCircuit qc = new(2);
        qc.MeasureInto(0, 0);
        qc.When(0, 1, c => c.X(1));

        // The gate must appear in the diagram even though it did not run, because the
        // simulator re-evaluates the condition per shot.
        Assert.Contains("[X]", qc.ToDiagram());
    }

    [Fact]
    public void A_conditional_gate_is_drawn_after_the_measurement_it_depends_on()
    {
        QuantumCircuit qc = new(2);
        qc.H(0);
        qc.MeasureInto(0, 0);
        qc.When(0, 1, c => c.X(1));

        string[] lines = qc.ToDiagram().Split('\n');

        string qubit1 = lines.First(l => l.StartsWith("q1"));
        string qubit0 = lines.First(l => l.StartsWith("q0"));

        // The conditional X must sit to the right of the M that wrote its classical bit.
        Assert.True(qubit1.IndexOf("[X]", StringComparison.Ordinal) > qubit0.IndexOf("[M]", StringComparison.Ordinal),
            $"conditional gate drawn before its measurement:\n{qc.ToDiagram()}");
    }

    [Fact]
    public void When_nests()
    {
        QuantumCircuit qc = new(3);
        qc.X(0);
        qc.X(1);
        qc.MeasureInto(0, 0);
        qc.MeasureInto(1, 1);

        qc.When(0, 1, c => c.When(1, 1, inner => inner.X(2)));

        Assert.Equal(1, qc.ClassicalBit(0));
        Assert.Equal(1, qc.ClassicalBit(1));
        AssertState(qc, 0, 0, 0, 0, 0, 0, 0, 1);   // |111>
    }

    [Fact]
    public void When_restores_the_previous_condition_after_the_body()
    {
        QuantumCircuit qc = new(2);
        qc.MeasureInto(0, 0);

        qc.When(0, 1, c => c.X(1));   // skipped, bit is 0
        qc.X(1);                       // must still run

        AssertState(qc, 0, 0, 1, 0);   // |10>
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.25)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public void Teleportation_moves_an_arbitrary_state_to_the_third_qubit(double theta)
    {
        // Qubit 0 holds the message, qubits 1 and 2 share a Bell pair. After the protocol
        // qubit 2 must hold exactly the state qubit 0 started in.
        QuantumCircuit qc = new(3);
        qc.RandomSource = new SeededRandomSource(7);

        // Prepare the message: Ry(theta)|0> = cos(theta/2)|0> + sin(theta/2)|1>.
        qc.Ry(0, theta);

        // Entangle qubits 1 and 2.
        qc.H(1);
        qc.CNOT(1, 2);

        // Bell-basis measurement of qubits 0 and 1.
        qc.CNOT(0, 1);
        qc.H(0);
        qc.MeasureInto(0, 0);
        qc.MeasureInto(1, 1);

        // Corrections driven by the two classical bits.
        qc.When(1, 1, c => c.X(2));
        qc.When(0, 1, c => c.Z(2));

        // Qubit 2 now carries the message. Marginalize the other two out.
        double expectedZero = Math.Cos(theta / 2) * Math.Cos(theta / 2);
        double expectedOne = Math.Sin(theta / 2) * Math.Sin(theta / 2);

        (double zero, double one) = QubitMarginal(qc, 2);

        Assert.Equal(expectedZero, zero, 9);
        Assert.Equal(expectedOne, one, 9);
    }

    [Fact]
    public void Teleportation_works_for_every_measurement_branch()
    {
        // The four Bell outcomes each need a different correction, so run enough seeds to
        // hit all of them and confirm the result never depends on which branch was taken.
        for (int seed = 0; seed < 20; seed++)
        {
            QuantumCircuit qc = new(3);
            qc.RandomSource = new SeededRandomSource(seed);

            qc.Ry(0, 0.8);
            qc.H(1);
            qc.CNOT(1, 2);
            qc.CNOT(0, 1);
            qc.H(0);
            qc.MeasureInto(0, 0);
            qc.MeasureInto(1, 1);
            qc.When(1, 1, c => c.X(2));
            qc.When(0, 1, c => c.Z(2));

            (double zero, _) = QubitMarginal(qc, 2);

            Assert.Equal(Math.Cos(0.4) * Math.Cos(0.4), zero, 9);
        }
    }

    [Fact]
    public void Simulator_reevaluates_conditions_per_shot()
    {
        // Measure a qubit in superposition, then flip a second qubit whenever the first
        // came out 1. The two qubits must therefore always agree.
        QuantumCircuit qc = new(2);
        qc.H(0);
        qc.MeasureInto(0, 0);
        qc.When(0, 1, c => c.X(1));
        qc.Measure();

        var result = Simulator.Run(qc, 400)[1];

        Assert.Equal(400, result.Shots);
        Assert.False(result.Counts.ContainsKey("01"), "qubit 1 flipped without its control bit");
        Assert.False(result.Counts.ContainsKey("10"), "qubit 1 failed to flip when its control bit was set");
        Assert.True(result.Counts["00"] > 100);
        Assert.True(result.Counts["11"] > 100);
    }

    [Fact]
    public void Reset_clears_state_gates_and_classical_bits()
    {
        QuantumCircuit qc = new(2);
        qc.X(0);
        qc.MeasureInto(0, 0);

        qc.Reset();

        AssertState(qc, 1, 0, 0, 0);
        Assert.Equal(0, qc.ClassicalBit(0));
        Assert.DoesNotContain("[X]", qc.ToDiagram());

        // A reset circuit must be reusable, including re-initialization.
        qc.Initialize(0, Qubit.NET.Utilities.State.One);
        AssertState(qc, 0, 1, 0, 0);
    }

    [Fact]
    public void Out_of_range_classical_bits_are_rejected()
    {
        QuantumCircuit qc = new(2);

        Assert.Throws<QubitIndexOutOfRangeException>(() => qc.ClassicalBit(2));
        Assert.Throws<QubitIndexOutOfRangeException>(() => qc.MeasureInto(0, 5));
        Assert.Throws<QubitIndexOutOfRangeException>(() => qc.When(9, 1, c => c.X(0)));
    }

    /// <summary>
    /// Total probability of finding one qubit in |0> and in |1>, tracing out the rest.
    /// </summary>
    private static (double Zero, double One) QubitMarginal(QuantumCircuit qc, int qubit)
    {
        double zero = 0, one = 0;
        int mask = 1 << qubit;

        for (int i = 0; i < qc.StateVector.Length; i++)
        {
            Complex amplitude = qc.StateVector[i];
            double probability = amplitude.Real * amplitude.Real + amplitude.Imaginary * amplitude.Imaginary;

            if ((i & mask) == 0) zero += probability;
            else one += probability;
        }

        return (zero, one);
    }
}
