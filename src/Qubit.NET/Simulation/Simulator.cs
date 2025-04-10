using System.Text;

namespace Qubit.NET.Simulation;

/// <summary>
/// Provides functionality to simulate quantum circuits.
/// </summary>
public static class Simulator
{
    /// <summary>
    /// Simulates a quantum circuit multiple times and records the measurement results.
    /// </summary>
    /// <param name="qc">The quantum circuit to simulate.</param>
    /// <param name="shots">The number of times the circuit should be run (shots).</param>
    /// <returns>
    /// An integer array where each index represents a possible measurement outcome, and
    /// the value at that index is the number of times that outcome was observed.
    /// </returns>
    public static int[] Run(QuantumCircuit qc, int shots)
    {
        int[] result = new int[qc.QubitCount*qc.QubitCount];
        
        for (int i = 0; i < shots; i++)
        {
            QuantumCircuit quantumCircuit = new QuantumCircuit(qc);
            int num = Convert.ToInt32(quantumCircuit.Measure(), 2);

            result[num]++;
        }
        
        return result;
    }

    /// <summary>
    /// Converts the result of a quantum circuit simulation into a human-readable string.
    /// </summary>
    /// <param name="result">An array of measurement result counts.</param>
    /// <returns>
    /// A string formatted as a dictionary, where keys are binary representations of measurement outcomes,
    /// and values are the counts of how often each outcome occurred.
    /// </returns>
    public static string GetStringResult(this int[] result)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        for (int i = 0; i < result.Length; i++)
        {
            if(result[i] == 0)
                continue;
            
            sb.Append("'");
            sb.Append(Convert.ToString(i, 2).PadLeft((int)System.Math.Sqrt(result.Length), '0'));
            sb.Append("': ");
            sb.Append(result[i].ToString());
            sb.Append(", ");
        }
        
        sb.Remove(sb.Length - 2, 2);
        sb.Append("}");
        
        return sb.ToString();
    }
}