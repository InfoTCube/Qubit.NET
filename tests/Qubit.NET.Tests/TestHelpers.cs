using System.Numerics;
using Qubit.NET;
using Qubit.NET.Utilities;

namespace QubitNet.Tests;

/// <summary>
/// A deterministic <see cref="IRandomSource"/> that always returns the same value,
/// so measurement outcomes become predictable.
/// </summary>
public sealed class FixedRandomSource(double value) : IRandomSource
{
    public double NextDouble() => value;
}

/// <summary>
/// An <see cref="IRandomSource"/> backed by a seeded <see cref="Random"/>, for tests that
/// need a spread of outcomes but must still reproduce across runs.
/// </summary>
public sealed class SeededRandomSource(int seed) : IRandomSource
{
    private readonly Random _random = new(seed);

    public double NextDouble() => _random.NextDouble();
}

public static class Amplitudes
{
    public const double Tolerance = 1e-9;

    public static readonly double InvSqrt2 = 1.0 / Math.Sqrt(2);

    /// <summary>
    /// Asserts that the circuit's state vector equals <paramref name="expected"/> entry by entry.
    /// </summary>
    public static void AssertState(QuantumCircuit qc, params Complex[] expected)
    {
        Assert.Equal(expected.Length, qc.StateVector.Length);

        for (int i = 0; i < expected.Length; i++)
        {
            Assert.True(Complex.Abs(qc.StateVector[i] - expected[i]) < Tolerance,
                $"amplitude[{i}]: expected {expected[i]}, got {qc.StateVector[i]}");
        }
    }

    /// <summary>
    /// Asserts that the state vector is normalized, i.e. the probabilities sum to 1.
    /// </summary>
    public static void AssertNormalized(QuantumCircuit qc)
    {
        double total = qc.StateVector.Sum(a => a.Real * a.Real + a.Imaginary * a.Imaginary);

        Assert.True(Math.Abs(total - 1.0) < Tolerance, $"state is not normalized: |psi|^2 = {total}");
    }

    /// <summary>
    /// Applies a gate matrix to a fresh single-qubit circuit and returns the resulting state.
    /// </summary>
    public static Complex[] ApplyToFreshQubit(Complex[,] gate)
    {
        QuantumCircuit qc = new(1);
        qc.Custom(gate, 0);
        return qc.StateVector;
    }

    /// <summary>
    /// Multiplies two square complex matrices.
    /// </summary>
    public static Complex[,] Multiply(Complex[,] a, Complex[,] b)
    {
        int n = a.GetLength(0);
        Complex[,] result = new Complex[n, n];

        for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
        {
            Complex sum = Complex.Zero;
            for (int k = 0; k < n; k++)
                sum += a[i, k] * b[k, j];
            result[i, j] = sum;
        }

        return result;
    }

    /// <summary>
    /// Asserts that two square complex matrices are equal entry by entry.
    /// </summary>
    public static void AssertMatrixEqual(Complex[,] expected, Complex[,] actual)
    {
        int n = expected.GetLength(0);
        Assert.Equal(n, actual.GetLength(0));

        for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
        {
            Assert.True(Complex.Abs(expected[i, j] - actual[i, j]) < Tolerance,
                $"[{i},{j}]: expected {expected[i, j]}, got {actual[i, j]}");
        }
    }
}
