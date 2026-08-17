using System.Numerics;
using System.Text;

namespace Qubit.NET.Visualization;

/// <summary>
/// Reads out a circuit's state in forms that are easy to draw: Bloch sphere coordinates,
/// per-qubit probabilities, and an ASCII histogram.
/// </summary>
public static class StateInspector
{
    /// <summary>
    /// The Bloch sphere coordinates of a single qubit, obtained by tracing out the rest of
    /// the register.
    /// </summary>
    /// <param name="circuit">The circuit to inspect.</param>
    /// <param name="qubit">The qubit to locate on the sphere.</param>
    /// <returns>
    /// The point (x, y, z) on or inside the unit sphere. A pure state sits on the surface;
    /// a qubit entangled with others sits strictly inside, and at the origin when maximally
    /// entangled.
    /// </returns>
    /// <example>
    /// <code>
    /// var (x, y, z) = qc.BlochVector(0);
    /// arrow.transform.localPosition = new Vector3((float)x, (float)y, (float)z); // Unity
    /// </code>
    /// </example>
    public static (double X, double Y, double Z) BlochVector(this QuantumCircuit circuit, int qubit)
    {
        if (qubit < 0 || qubit >= circuit.QubitCount)
            throw new ArgumentOutOfRangeException(nameof(qubit), qubit,
                $"Qubit index must be between [0 and {circuit.QubitCount})");

        // Reduced density matrix of the qubit: rho01 is the coherence, and the populations
        // give the z component.
        Complex rho01 = Complex.Zero;
        double rho00 = 0, rho11 = 0;

        int mask = 1 << qubit;
        Complex[] state = circuit.StateVector;

        for (int i = 0; i < state.Length; i++)
        {
            if ((i & mask) != 0) continue;

            Complex a = state[i];            // amplitude with the qubit in |0>
            Complex b = state[i | mask];     // same basis state, qubit in |1>

            rho00 += a.Real * a.Real + a.Imaginary * a.Imaginary;
            rho11 += b.Real * b.Real + b.Imaginary * b.Imaginary;
            rho01 += a * Complex.Conjugate(b);
        }

        return (2 * rho01.Real, 2 * rho01.Imaginary, rho00 - rho11);
    }

    /// <summary>
    /// The probability of finding a single qubit in |1⟩, tracing out the rest of the register.
    /// </summary>
    /// <param name="circuit">The circuit to inspect.</param>
    /// <param name="qubit">The qubit to measure the marginal of.</param>
    /// <returns>A probability in [0, 1].</returns>
    public static double QubitProbability(this QuantumCircuit circuit, int qubit)
    {
        if (qubit < 0 || qubit >= circuit.QubitCount)
            throw new ArgumentOutOfRangeException(nameof(qubit), qubit,
                $"Qubit index must be between [0 and {circuit.QubitCount})");

        double one = 0;
        int mask = 1 << qubit;
        Complex[] state = circuit.StateVector;

        for (int i = 0; i < state.Length; i++)
        {
            if ((i & mask) == 0) continue;

            one += state[i].Real * state[i].Real + state[i].Imaginary * state[i].Imaginary;
        }

        return one;
    }

    /// <summary>
    /// Renders the circuit's outcome probabilities as an ASCII bar chart.
    /// </summary>
    /// <param name="circuit">The circuit to chart.</param>
    /// <param name="width">Width of the longest bar, in characters.</param>
    /// <returns>A multi-line string, one row per outcome with non-negligible probability.</returns>
    /// <example>
    /// <code>
    /// 00 | ####################  0.500
    /// 11 | ####################  0.500
    /// </code>
    /// </example>
    public static string ToHistogram(this QuantumCircuit circuit, int width = 40)
    {
        if (width < 1) throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be positive.");

        var probabilities = circuit.GetProbabilities();

        if (probabilities.Count == 0) return string.Empty;

        double max = probabilities.Values.Max();
        StringBuilder sb = new();

        foreach (var pair in probabilities.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            int bars = max > 0 ? (int)System.Math.Round(pair.Value / max * width) : 0;

            sb.Append(pair.Key)
              .Append(" | ")
              .Append('#', bars)
              .Append(new string(' ', width - bars))
              .Append("  ")
              .AppendLine(pair.Value.ToString("F3", System.Globalization.CultureInfo.InvariantCulture));
        }

        return sb.ToString().TrimEnd();
    }
}
