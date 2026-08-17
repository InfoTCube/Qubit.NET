using System.Text;

namespace Qubit.NET.Simulation;

/// <summary>
/// The outcome counts of one measurement across every shot of a simulation.
/// </summary>
public sealed class MeasurementResult
{
    private readonly int[] _counts;

    internal MeasurementResult(int[] counts, int qubitCount)
    {
        _counts = counts;
        QubitCount = qubitCount;
    }

    /// <summary>
    /// Number of qubits involved in this measurement, and so the width of each outcome
    /// bitstring.
    /// </summary>
    public int QubitCount { get; }

    /// <summary>
    /// Total number of shots recorded.
    /// </summary>
    public int Shots => _counts.Sum();

    /// <summary>
    /// How many times each observed outcome occurred, keyed by bitstring. Outcomes that
    /// never occurred are omitted.
    /// </summary>
    public IReadOnlyDictionary<string, int> Counts
    {
        get
        {
            Dictionary<string, int> counts = new();

            for (int i = 0; i < _counts.Length; i++)
            {
                if (_counts[i] != 0)
                    counts[Convert.ToString(i, 2).PadLeft(QubitCount, '0')] = _counts[i];
            }

            return counts;
        }
    }

    /// <summary>
    /// The observed frequency of an outcome, in [0, 1].
    /// </summary>
    /// <param name="outcome">The outcome bitstring, for example "01".</param>
    /// <returns>The fraction of shots that produced <paramref name="outcome"/>.</returns>
    public double Probability(string outcome)
    {
        int shots = Shots;

        if (shots == 0) return 0;

        return Counts.TryGetValue(outcome, out int count) ? (double)count / shots : 0;
    }

    /// <summary>
    /// The outcome that occurred most often, or null if no shots were recorded.
    /// </summary>
    public string? MostFrequent
    {
        get
        {
            string? best = null;
            int bestCount = 0;

            foreach (var pair in Counts)
            {
                if (pair.Value <= bestCount) continue;

                best = pair.Key;
                bestCount = pair.Value;
            }

            return best;
        }
    }

    /// <summary>
    /// Formats the counts as a dictionary literal, for example <c>{'00': 512, '11': 488}</c>.
    /// </summary>
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append('{');

        bool any = false;

        foreach (var pair in Counts)
        {
            if (any) sb.Append(", ");

            sb.Append('\'').Append(pair.Key).Append("': ").Append(pair.Value);
            any = true;
        }

        return sb.Append('}').ToString();
    }

    internal void Record(int outcome) => _counts[outcome]++;
}
