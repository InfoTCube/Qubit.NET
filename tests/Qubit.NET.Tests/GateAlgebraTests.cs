using System.Numerics;
using Qubit.NET.Gates;
using static QubitNet.Tests.Amplitudes;
using Qubit.NET;

namespace QubitNet.Tests;

/// <summary>
/// Checks the gate matrices themselves: that they are unitary and that they satisfy the
/// algebraic identities that define them.
/// </summary>
public class GateAlgebraTests
{
    public static TheoryData<string, Complex[,]> AllGates() => new()
    {
        { nameof(QuantumGates.I), QuantumGates.I },
        { nameof(QuantumGates.H), QuantumGates.H },
        { nameof(QuantumGates.X), QuantumGates.X },
        { nameof(QuantumGates.Y), QuantumGates.Y },
        { nameof(QuantumGates.Z), QuantumGates.Z },
        { nameof(QuantumGates.S), QuantumGates.S },
        { nameof(QuantumGates.Sdag), QuantumGates.Sdag },
        { nameof(QuantumGates.T), QuantumGates.T },
        { nameof(QuantumGates.Tdag), QuantumGates.Tdag },
        { nameof(QuantumGates.SX), QuantumGates.SX },
        { nameof(QuantumGates.SY), QuantumGates.SY },
        { nameof(QuantumGates.SZ), QuantumGates.SZ },
        { nameof(QuantumGates.CNOT), QuantumGates.CNOT },
        { nameof(QuantumGates.CY), QuantumGates.CY },
        { nameof(QuantumGates.CZ), QuantumGates.CZ },
        { nameof(QuantumGates.CH), QuantumGates.CH },
        { nameof(QuantumGates.SWAP), QuantumGates.SWAP },
        { nameof(QuantumGates.Toffoli), QuantumGates.Toffoli },
        { nameof(QuantumGates.Fredkin), QuantumGates.Fredkin },
        { "Rx(0.7)", QuantumGates.Rx(0.7) },
        { "Ry(0.7)", QuantumGates.Ry(0.7) },
        { "Rz(0.7)", QuantumGates.Rz(0.7) },
        { "U3(0.3,0.5,0.7)", QuantumGates.U3(0.3, 0.5, 0.7) },
        { "CRx(0.7)", QuantumGates.CRx(0.7) },
        { "CRy(0.7)", QuantumGates.CRy(0.7) },
        { "CRz(0.7)", QuantumGates.CRz(0.7) },
        { "CU3(0.3,0.5,0.7)", QuantumGates.CU3(0.3, 0.5, 0.7) },
    };

    [Theory]
    [MemberData(nameof(AllGates))]
    public void Every_gate_is_unitary(string name, Complex[,] gate)
    {
        // U†U must be the identity. Exercised through Custom(), which validates unitarity
        // and only accepts matrices of up to 4 qubits.
        int qubits = (int)Math.Log2(gate.GetLength(0));

        QuantumCircuit qc = new(qubits);

        Exception? thrown = Record.Exception(() => qc.Custom(gate, Enumerable.Range(0, qubits).ToArray()));

        Assert.True(thrown is null, $"{name} was rejected as non-unitary: {thrown?.Message}");
    }

    [Fact]
    public void Non_unitary_matrix_is_rejected()
    {
        QuantumCircuit qc = new(1);

        Assert.Throws<ArgumentException>(() => qc.Custom(new Complex[,] { { 1, 1 }, { 1, 1 } }, 0));
    }

    [Theory]
    [InlineData(nameof(QuantumGates.SX), nameof(QuantumGates.X))]
    [InlineData(nameof(QuantumGates.SY), nameof(QuantumGates.Y))]
    [InlineData(nameof(QuantumGates.SZ), nameof(QuantumGates.Z))]
    public void Square_root_gate_squared_equals_its_parent(string root, string parent)
    {
        Complex[,] rootMatrix = Matrix(root);
        Complex[,] parentMatrix = Matrix(parent);

        AssertMatrixEqual(parentMatrix, Multiply(rootMatrix, rootMatrix));
    }

    [Fact]
    public void H_squared_is_identity() =>
        AssertMatrixEqual(QuantumGates.I, Multiply(QuantumGates.H, QuantumGates.H));

    [Fact]
    public void S_squared_is_Z() =>
        AssertMatrixEqual(QuantumGates.Z, Multiply(QuantumGates.S, QuantumGates.S));

    [Fact]
    public void T_to_the_fourth_is_Z()
    {
        Complex[,] tSquared = Multiply(QuantumGates.T, QuantumGates.T);

        AssertMatrixEqual(QuantumGates.Z, Multiply(tSquared, tSquared));
    }

    [Theory]
    [InlineData(nameof(QuantumGates.S), nameof(QuantumGates.Sdag))]
    [InlineData(nameof(QuantumGates.T), nameof(QuantumGates.Tdag))]
    public void Gate_times_its_dagger_is_identity(string gate, string dagger) =>
        AssertMatrixEqual(QuantumGates.I, Multiply(Matrix(gate), Matrix(dagger)));

    [Fact]
    public void Rotation_by_two_pi_about_x_is_minus_identity()
    {
        // Rx(2*pi) = -I: a 2*pi rotation returns a spin-1/2 system to itself up to a global phase.
        Complex[,] expected = { { -1, 0 }, { 0, -1 } };

        AssertMatrixEqual(expected, QuantumGates.Rx(2 * Math.PI));
    }

    [Fact]
    public void U3_reproduces_H_at_the_standard_angles() =>
        AssertMatrixEqual(QuantumGates.H, QuantumGates.U3(Math.PI / 2, 0, Math.PI));

    private static Complex[,] Matrix(string name) => name switch
    {
        nameof(QuantumGates.I) => QuantumGates.I,
        nameof(QuantumGates.X) => QuantumGates.X,
        nameof(QuantumGates.Y) => QuantumGates.Y,
        nameof(QuantumGates.Z) => QuantumGates.Z,
        nameof(QuantumGates.S) => QuantumGates.S,
        nameof(QuantumGates.Sdag) => QuantumGates.Sdag,
        nameof(QuantumGates.T) => QuantumGates.T,
        nameof(QuantumGates.Tdag) => QuantumGates.Tdag,
        nameof(QuantumGates.SX) => QuantumGates.SX,
        nameof(QuantumGates.SY) => QuantumGates.SY,
        nameof(QuantumGates.SZ) => QuantumGates.SZ,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "unknown gate")
    };
}
