using System.Numerics;
using Qubit.NET.Circuits;
using Qubit.NET.Utilities;
using static QubitNet.Tests.Amplitudes;
using Qubit.NET;

namespace QubitNet.Tests;

/// <summary>
/// End-to-end checks that circuits produce the textbook states, and that measurement
/// collapses them correctly.
/// </summary>
public class CircuitTests
{
    // State vector index i encodes qubit q in bit q, so for two qubits the order is
    // |q1 q0>: index 1 is |01> (qubit 0 set), index 2 is |10> (qubit 1 set).

    [Fact]
    public void PhiPlus_is_00_plus_11()
    {
        AssertState(BellStates.PhiPlus(), InvSqrt2, 0, 0, InvSqrt2);
        AssertNormalized(BellStates.PhiPlus());
    }

    [Fact]
    public void PhiMinus_is_00_minus_11() =>
        AssertState(BellStates.PhiMinus(), InvSqrt2, 0, 0, -InvSqrt2);

    [Fact]
    public void PsiPlus_is_01_plus_10() =>
        AssertState(BellStates.PsiPlus(), 0, InvSqrt2, InvSqrt2, 0);

    [Fact]
    public void PsiMinus_is_01_minus_10() =>
        AssertState(BellStates.PsiMinus(), 0, InvSqrt2, -InvSqrt2, 0);

    [Fact]
    public void GHZ_is_000_plus_111() =>
        AssertState(BellStates.GHZ(), InvSqrt2, 0, 0, 0, 0, 0, 0, InvSqrt2);

    [Fact]
    public void Hadamard_creates_an_equal_superposition()
    {
        QuantumCircuit qc = new(1);
        qc.H(0);

        AssertState(qc, InvSqrt2, InvSqrt2);
    }

    [Fact]
    public void X_flips_the_qubit()
    {
        QuantumCircuit qc = new(1);
        qc.X(0);

        AssertState(qc, 0, 1);
    }

    [Fact]
    public void Applying_a_gate_twice_returns_to_the_start()
    {
        QuantumCircuit qc = new(1);
        qc.H(0);
        qc.H(0);

        AssertState(qc, 1, 0);
    }

    [Fact]
    public void Toffoli_flips_the_target_only_when_both_controls_are_set()
    {
        QuantumCircuit both = new(3);
        both.X(0);
        both.X(1);
        both.Toffoli(0, 1, 2);

        // |111> is index 7.
        AssertState(both, 0, 0, 0, 0, 0, 0, 0, 1);

        QuantumCircuit one = new(3);
        one.X(0);
        one.Toffoli(0, 1, 2);

        // Target must stay |0>, so the state is still |001> = index 1.
        AssertState(one, 0, 1, 0, 0, 0, 0, 0, 0);
    }

    [Fact]
    public void SWAP_exchanges_two_qubits()
    {
        QuantumCircuit qc = new(2);
        qc.X(0);
        qc.SWAP(0, 1);

        // |01> becomes |10>, i.e. index 1 becomes index 2.
        AssertState(qc, 0, 0, 1, 0);
    }

    [Fact]
    public void GetProbabilities_reports_a_fair_coin_for_a_Bell_state()
    {
        var probabilities = BellStates.PhiPlus().GetProbabilities();

        Assert.Equal(2, probabilities.Count);
        Assert.Equal(0.5, probabilities["00"], Tolerance);
        Assert.Equal(0.5, probabilities["11"], Tolerance);
    }

    [Theory]
    [InlineData(0.1, "00")]
    [InlineData(0.9, "11")]
    public void Measuring_a_Bell_state_never_yields_a_mixed_outcome(double roll, string expected)
    {
        QuantumCircuit qc = BellStates.PhiPlus();
        qc.RandomSource = new FixedRandomSource(roll);

        Assert.Equal(expected, qc.Measure());
    }

    [Fact]
    public void Bell_state_measurements_are_perfectly_correlated()
    {
        // The whole point of entanglement: "01" and "10" must never appear.
        for (int seed = 0; seed < 50; seed++)
        {
            QuantumCircuit qc = BellStates.PhiPlus();
            qc.RandomSource = new SeededRandomSource(seed);

            string result = qc.Measure();

            Assert.True(result is "00" or "11", $"seed {seed} produced {result}");
        }
    }

    [Fact]
    public void Measurement_collapses_the_state()
    {
        QuantumCircuit qc = BellStates.PhiPlus();
        qc.RandomSource = new FixedRandomSource(0.1);

        qc.Measure();

        AssertState(qc, 1, 0, 0, 0);
    }

    [Fact]
    public void Measuring_one_qubit_of_a_Bell_pair_collapses_the_other()
    {
        QuantumCircuit qc = BellStates.PhiPlus();
        qc.RandomSource = new FixedRandomSource(0.9);

        string result = qc.Measure(0);

        Assert.Equal("1", result);
        AssertNormalized(qc);

        // Qubit 0 came out |1>, so qubit 1 must now be |1> as well: only |11> survives.
        var probabilities = qc.GetProbabilities();
        Assert.Single(probabilities);
        Assert.Equal(1.0, probabilities["11"], Tolerance);
    }

    [Fact]
    public void Partial_measurement_returns_bits_in_argument_order()
    {
        QuantumCircuit qc = new(3);
        qc.X(2);   // |100>

        Assert.Equal("01", qc.Measure(0, 2));
        Assert.Equal("10", new QuantumCircuit(3).Also(c => c.X(2)).Measure(2, 0));
    }

    [Theory]
    [InlineData(State.Zero, 1, 0)]
    [InlineData(State.One, 0, 1)]
    public void Initialize_sets_a_basis_state(State state, double alpha, double beta)
    {
        QuantumCircuit qc = new(1);
        qc.Initialize(0, state);

        AssertState(qc, alpha, beta);
    }

    [Fact]
    public void Initialize_sets_the_plus_state()
    {
        QuantumCircuit qc = new(1);
        qc.Initialize(0, State.Plus);

        AssertState(qc, InvSqrt2, InvSqrt2);
    }

    [Fact]
    public void Initialize_rejects_an_already_modified_qubit()
    {
        QuantumCircuit qc = new(1);
        qc.H(0);

        Assert.Throws<InvalidOperationException>(() => qc.Initialize(0, State.One));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(27)]
    public void Invalid_qubit_counts_are_rejected(int count) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new QuantumCircuit(count));

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void Out_of_range_qubit_indices_are_rejected(int qubit) =>
        Assert.Throws<QubitIndexOutOfRangeException>(() => new QuantumCircuit(2).H(qubit));

    [Fact]
    public void A_gate_cannot_target_the_same_qubit_twice() =>
        Assert.Throws<ArgumentException>(() => new QuantumCircuit(2).CNOT(0, 0));
}

internal static class CircuitExtensions
{
    /// <summary>Applies an action to a circuit and returns it, for terser test setup.</summary>
    public static QuantumCircuit Also(this QuantumCircuit qc, Action<QuantumCircuit> action)
    {
        action(qc);
        return qc;
    }
}
