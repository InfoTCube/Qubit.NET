using System.Numerics;
using Qubit.NET.Circuits;
using Qubit.NET.Gates;
using Qubit.NET.Simulation;
using static QubitNet.Tests.Amplitudes;
using Qubit.NET;

namespace QubitNet.Tests;

/// <summary>
/// One test per bug fixed in the 1.0.0 release. Each of these failed before the fix.
/// </summary>
public class RegressionTests
{
    [Fact]
    public void Custom_accepts_a_Hadamard_matrix()
    {
        // IsUnitary used exact floating-point equality, so (1/sqrt2)^2 + (1/sqrt2)^2
        // came out as 1.0000000000000002 and every 1/sqrt2 gate was rejected.
        QuantumCircuit qc = new(1);

        qc.Custom(QuantumGates.H, 0);

        AssertState(qc, InvSqrt2, InvSqrt2);
    }

    [Fact]
    public void Run_does_not_consume_the_circuit()
    {
        // Run used to strip gates off the circuit with Gates.RemoveAt(0), so the second
        // call replayed a circuit with no Hadamard and reported 100% |00>.
        QuantumCircuit qc = BellStates.PhiPlus();
        qc.Measure();

        var first = Simulator.Run(qc, 500)[0];
        var second = Simulator.Run(qc, 500)[0];

        foreach (var result in new[] { first, second })
        {
            Assert.Equal(500, result.Shots);
            Assert.True(result.Counts["00"] > 150, "expected roughly half the shots in |00>");
            Assert.True(result.Counts["11"] > 150, "expected roughly half the shots in |11>");
            Assert.False(result.Counts.ContainsKey("01"));
            Assert.False(result.Counts.ContainsKey("10"));
        }
    }

    [Fact]
    public void Run_leaves_the_gate_list_intact()
    {
        QuantumCircuit qc = BellStates.PhiPlus();
        qc.Measure();

        string before = qc.ToDiagram();
        Simulator.Run(qc, 10);

        Assert.Equal(before, qc.ToDiagram());
    }

    [Fact]
    public void Copying_a_circuit_leaves_the_original_alone()
    {
        // The copy constructor documented a deep copy but shared the gate list and the
        // modified-qubit flags, so gates applied to the copy leaked into the original.
        QuantumCircuit original = new(2);
        original.H(0);

        QuantumCircuit copy = new(original);
        copy.X(1);

        AssertState(original, InvSqrt2, InvSqrt2, 0, 0);
        AssertState(copy, 0, 0, InvSqrt2, InvSqrt2);
    }

    [Fact]
    public void Copying_a_circuit_does_not_share_its_diagram()
    {
        QuantumCircuit original = new(2);
        original.H(0);

        string before = original.ToDiagram();

        QuantumCircuit copy = new(original);
        copy.X(1);
        copy.CNOT(0, 1);

        Assert.Equal(before, original.ToDiagram());
    }

    [Fact]
    public void Initialize_rejects_an_unnormalized_state()
    {
        // This is the example the README used to ship: |1+i|^2 + |2+2i|^2 = 10.
        QuantumCircuit qc = new(1);

        Assert.Throws<ArgumentException>(() => qc.Initialize(0, new Complex(1, 1), new Complex(2, 2)));
    }

    [Fact]
    public void Initialize_accepts_a_normalized_complex_state()
    {
        QuantumCircuit qc = new(1);
        qc.Initialize(0, new Complex(InvSqrt2, 0), new Complex(0, InvSqrt2));

        AssertNormalized(qc);
        Assert.Equal(1.0, qc.GetProbabilities().Values.Sum(), Tolerance);
    }

    [Fact]
    public void Toffoli_draws_both_controls_as_control_markers()
    {
        // The symbol table returned ["@", "+", "+"], so the second control rendered as a
        // target marker.
        QuantumCircuit qc = new(3);
        qc.Toffoli(0, 1, 2);

        string diagram = qc.ToDiagram();

        Assert.Equal(2, diagram.Count(c => c == '@'));
        Assert.Contains("[+]", diagram);
    }

    [Fact]
    public void Formatting_handles_a_result_with_no_shots()
    {
        // sb.Remove(sb.Length - 2, 2) threw when no outcome had been recorded.
        QuantumCircuit qc = BellStates.PhiPlus();
        qc.Measure();

        var result = Simulator.Run(qc, 0);

        Assert.True(result.Count == 0 || result[0].GetStringResult() == "{}");
    }

    [Fact]
    public void GetStringResult_formats_counts()
    {
        QuantumCircuit qc = BellStates.PhiPlus();
        qc.Measure();

        var result = Simulator.Run(qc, 100)[0];
        string formatted = result.GetStringResult();

        Assert.StartsWith("{", formatted);
        Assert.EndsWith("}", formatted);
        Assert.Contains("'00': ", formatted);
        Assert.Contains("'11': ", formatted);
        Assert.Equal(100, result.Shots);
    }

    [Fact]
    public void Qubit_count_above_the_maximum_throws_the_documented_exception()
    {
        // Used to throw AggregateException, and the documented limit of 30 could never
        // actually allocate.
        Assert.Equal(26, QuantumCircuit.MaxQubitCount);
        Assert.Throws<ArgumentOutOfRangeException>(() => new QuantumCircuit(QuantumCircuit.MaxQubitCount + 1));
    }

    [Fact]
    public void Run_returns_nothing_when_the_circuit_has_no_measurement() =>
        Assert.Empty(Simulator.Run(BellStates.PhiPlus(), 10));
}
