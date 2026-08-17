using System.Numerics;
using Qubit.NET;
using Qubit.NET.Gates;
using static QubitNet.Tests.Amplitudes;

namespace QubitNet.Tests;

/// <summary>
/// Controlled gates take a specialized in-place route rather than the general multi-qubit
/// path. These tests pin the two against each other so the fast path cannot drift.
/// </summary>
public class ControlledGateTests
{
    public static TheoryData<string, Action<QuantumCircuit>, Complex[,]> ControlledGates() => new()
    {
        { "CNOT", qc => qc.CNOT(0, 1), QuantumGates.CNOT },
        { "CY", qc => qc.CY(0, 1), QuantumGates.CY },
        { "CZ", qc => qc.CZ(0, 1), QuantumGates.CZ },
        { "CH", qc => qc.CH(0, 1), QuantumGates.CH },
        { "CRx", qc => qc.CRx(0, 1, 0.7), QuantumGates.CRx(0.7) },
        { "CRy", qc => qc.CRy(0, 1, 0.7), QuantumGates.CRy(0.7) },
        { "CRz", qc => qc.CRz(0, 1, 0.7), QuantumGates.CRz(0.7) },
        { "CU3", qc => qc.CU3(0, 1, 0.3, 0.5, 0.7), QuantumGates.CU3(0.3, 0.5, 0.7) },
    };

    [Theory]
    [MemberData(nameof(ControlledGates))]
    public void Fast_path_matches_the_general_matrix_path(string name, Action<QuantumCircuit> apply, Complex[,] matrix)
    {
        // Start from a state with amplitude on every basis state, so any disagreement shows.
        QuantumCircuit viaGate = Spread();
        apply(viaGate);

        QuantumCircuit viaCustom = Spread();
        viaCustom.Custom(matrix, 0, 1);

        for (int i = 0; i < viaGate.StateVector.Length; i++)
        {
            Assert.True(Complex.Abs(viaGate.StateVector[i] - viaCustom.StateVector[i]) < Tolerance,
                $"{name} amplitude[{i}]: fast path {viaGate.StateVector[i]}, general path {viaCustom.StateVector[i]}");
        }
    }

    [Fact]
    public void Toffoli_fast_path_matches_the_general_matrix_path()
    {
        QuantumCircuit viaGate = Spread(3);
        viaGate.Toffoli(0, 1, 2);

        QuantumCircuit viaCustom = Spread(3);
        viaCustom.Custom(QuantumGates.Toffoli, 0, 1, 2);

        for (int i = 0; i < viaGate.StateVector.Length; i++)
        {
            Assert.True(Complex.Abs(viaGate.StateVector[i] - viaCustom.StateVector[i]) < Tolerance,
                $"amplitude[{i}]: fast path {viaGate.StateVector[i]}, general path {viaCustom.StateVector[i]}");
        }
    }

    [Fact]
    public void Controlled_gates_stay_normalized()
    {
        QuantumCircuit qc = Spread(3);
        qc.CNOT(0, 1);
        qc.CH(1, 2);
        qc.CRy(0, 2, 1.1);
        qc.Toffoli(0, 1, 2);

        AssertNormalized(qc);
    }

    [Fact]
    public void A_controlled_gate_cannot_use_one_qubit_as_both_control_and_target() =>
        Assert.Throws<ArgumentException>(() => new QuantumCircuit(2).CZ(1, 1));

    [Fact]
    public void Toffoli_rejects_a_repeated_qubit() =>
        Assert.Throws<ArgumentException>(() => new QuantumCircuit(3).Toffoli(0, 1, 1));

    /// <summary>
    /// Builds a circuit whose every basis state carries a distinct, non-trivial amplitude.
    /// </summary>
    private static QuantumCircuit Spread(int qubits = 2)
    {
        QuantumCircuit qc = new(qubits);

        for (int q = 0; q < qubits; q++)
        {
            qc.H(q);
            qc.Rz(q, 0.3 * (q + 1));
            qc.Ry(q, 0.4 * (q + 1));
        }

        return qc;
    }
}
