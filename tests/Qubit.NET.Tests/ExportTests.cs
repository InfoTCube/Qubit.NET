using Qubit.NET;
using Qubit.NET.Circuits;
using Qubit.NET.Export;
using Qubit.NET.Gates;
using Qubit.NET.Utilities;
using Qubit.NET.Visualization;

namespace QubitNet.Tests;

/// <summary>
/// OpenQASM 2.0 export and the state-inspection helpers.
/// </summary>
public class ExportTests
{
    [Fact]
    public void Bell_circuit_exports_to_valid_qasm()
    {
        QuantumCircuit qc = BellStates.PhiPlus();
        qc.Measure();

        string[] lines = qc.ToQasm()
            .Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToArray();

        Assert.Equal("OPENQASM 2.0;", lines[0]);
        Assert.Equal("include \"qelib1.inc\";", lines[1]);
        Assert.Equal("qreg q[2];", lines[2]);
        Assert.Equal("creg c[2];", lines[3]);
        Assert.Equal("h q[0];", lines[4]);
        Assert.Equal("cx q[0],q[1];", lines[5]);
        Assert.Equal("measure q[0] -> c[0];", lines[6]);
        Assert.Equal("measure q[1] -> c[1];", lines[7]);
    }

    [Fact]
    public void Rotation_angles_are_exported_exactly()
    {
        QuantumCircuit qc = new(2);
        qc.Rx(0, 0.25);
        qc.Ry(0, -1.5);
        qc.Rz(1, 3.14159);
        qc.CRy(0, 1, 0.5);

        string qasm = qc.ToQasm();

        Assert.Contains("rx(0.25) q[0];", qasm);
        Assert.Contains("ry(-1.5) q[0];", qasm);
        Assert.Contains("rz(3.14159) q[1];", qasm);
        Assert.Contains("cry(0.5) q[0],q[1];", qasm);
    }

    [Fact]
    public void U3_exports_all_three_angles()
    {
        QuantumCircuit qc = new(1);
        qc.U3(0, 0.1, 0.2, 0.3);

        Assert.Contains("u3(0.1,0.2,0.3) q[0];", qc.ToQasm());
    }

    [Fact]
    public void Three_qubit_gates_export()
    {
        QuantumCircuit qc = new(3);
        qc.Toffoli(0, 1, 2);
        qc.Fredkin(0, 1, 2);
        qc.SWAP(0, 1);

        string qasm = qc.ToQasm();

        Assert.Contains("ccx q[0],q[1],q[2];", qasm);
        Assert.Contains("cswap q[0],", qasm);
        Assert.Contains("swap q[", qasm);
    }

    [Fact]
    public void Conditional_gates_export_as_qasm_if_statements()
    {
        QuantumCircuit qc = Algorithms.Teleportation(c => c.Ry(0, 0.5));

        string qasm = qc.ToQasm();

        Assert.Contains("if (c[1]==1) x q[2];", qasm);
        Assert.Contains("if (c[0]==1) z q[2];", qasm);
    }

    [Fact]
    public void Initializations_export_as_their_gate_equivalents()
    {
        QuantumCircuit qc = new(3);
        qc.Initialize(0, State.One);
        qc.Initialize(1, State.Plus);
        qc.Initialize(2, State.Minus);

        string qasm = qc.ToQasm();

        Assert.Contains("x q[0];", qasm);
        Assert.Contains("h q[1];", qasm);
        Assert.Contains("x q[2];", qasm);
    }

    [Fact]
    public void Custom_gates_are_reported_rather_than_silently_dropped()
    {
        QuantumCircuit qc = new(1);
        qc.Custom(QuantumGates.H, 0);

        Assert.Throws<NotSupportedException>(() => qc.ToQasm());
    }

    [Fact]
    public void Every_standard_gate_exports_without_throwing()
    {
        QuantumCircuit qc = new(3);
        qc.I(0); qc.H(0); qc.X(0); qc.Y(0); qc.Z(0);
        qc.S(0); qc.Sdag(0); qc.T(0); qc.Tdag(0);
        qc.SX(0); qc.SY(0); qc.SZ(0);
        qc.Rx(0, 0.1); qc.Ry(0, 0.1); qc.Rz(0, 0.1); qc.U3(0, 0.1, 0.2, 0.3);
        qc.CNOT(0, 1); qc.CY(0, 1); qc.CZ(0, 1); qc.CH(0, 1);
        qc.CRx(0, 1, 0.1); qc.CRy(0, 1, 0.1); qc.CRz(0, 1, 0.1); qc.CU3(0, 1, 0.1, 0.2, 0.3);
        qc.SWAP(0, 1); qc.Toffoli(0, 1, 2); qc.Fredkin(0, 1, 2);
        qc.Measure();

        string qasm = qc.ToQasm();

        // Nothing was dropped or commented out: 4 header lines (version, include, qreg,
        // creg) + 27 gates + one measure statement per qubit.
        Assert.DoesNotContain("//", qasm);
        Assert.Equal(4 + 27 + 3, qasm.Split('\n').Count(l => l.TrimEnd().EndsWith(';')));
    }

    [Fact]
    public void Bloch_vector_of_a_fresh_qubit_points_up() =>
        AssertVector((0, 0, 1), new QuantumCircuit(1).BlochVector(0));

    [Fact]
    public void Bloch_vector_after_X_points_down()
    {
        QuantumCircuit qc = new(1);
        qc.X(0);

        AssertVector((0, 0, -1), qc.BlochVector(0));
    }

    [Fact]
    public void Bloch_vector_after_H_points_along_x()
    {
        QuantumCircuit qc = new(1);
        qc.H(0);

        AssertVector((1, 0, 0), qc.BlochVector(0));
    }

    [Fact]
    public void Bloch_vector_of_a_maximally_entangled_qubit_is_at_the_origin()
    {
        // Each half of a Bell pair is maximally mixed on its own.
        AssertVector((0, 0, 0), BellStates.PhiPlus().BlochVector(0));
        AssertVector((0, 0, 0), BellStates.PhiPlus().BlochVector(1));
    }

    [Fact]
    public void Qubit_probability_reports_the_marginal()
    {
        QuantumCircuit qc = new(2);
        qc.H(0);

        Assert.Equal(0.5, qc.QubitProbability(0), 9);
        Assert.Equal(0.0, qc.QubitProbability(1), 9);
    }

    [Fact]
    public void Histogram_charts_the_outcome_probabilities()
    {
        string histogram = BellStates.PhiPlus().ToHistogram(20);
        string[] lines = histogram.Split('\n');

        Assert.Equal(2, lines.Length);
        Assert.StartsWith("00 | ", lines[0]);
        Assert.StartsWith("11 | ", lines[1]);
        Assert.EndsWith("0.500", lines[0].TrimEnd());
        Assert.Equal(20, lines[0].Count(c => c == '#'));
    }

    [Fact]
    public void Inspection_helpers_reject_out_of_range_qubits()
    {
        QuantumCircuit qc = new(2);

        Assert.Throws<ArgumentOutOfRangeException>(() => qc.BlochVector(2));
        Assert.Throws<ArgumentOutOfRangeException>(() => qc.QubitProbability(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => qc.ToHistogram(0));
    }

    private static void AssertVector((double X, double Y, double Z) expected,
        (double X, double Y, double Z) actual)
    {
        Assert.Equal(expected.X, actual.X, 9);
        Assert.Equal(expected.Y, actual.Y, 9);
        Assert.Equal(expected.Z, actual.Z, 9);
    }
}
