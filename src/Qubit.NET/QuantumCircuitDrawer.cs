using System.Text;
using Qubit.NET.Gates;
using Qubit.NET.Utilities;

namespace Qubit.NET;

/// <summary>
/// Renders quantum circuits as ASCII diagrams.
/// </summary>
public static class QuantumCircuitDrawer
{
    /// <summary>
    /// Draws an ASCII representation of the quantum circuit in the console,
    /// with gates highlighted using console colors.
    /// This includes gate positions across qubits and visual connections between them.
    /// </summary>
    /// <param name="circuit">The quantum circuit to draw.</param>
    /// <remarks>
    /// Requires a console. In environments without one (Unity, ASP.NET, tests),
    /// use <see cref="ToDiagram"/> and write the string wherever you need it.
    /// </remarks>
    public static void Draw(this QuantumCircuit circuit)
    {
        ConsoleColor defaultColor = Console.ForegroundColor;

        foreach (string line in circuit.ToDiagram().Split('\n'))
        {
            // Qubit labels ("q0 (0): ") are yellow, gate glyphs are blue, wires stay plain.
            // Connector-only lines carry no label, so bodyStart is 0 there.
            int labelEnd = line.IndexOf(": ", StringComparison.Ordinal);
            int bodyStart = labelEnd >= 0 ? labelEnd + 2 : 0;

            if (labelEnd >= 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(line.Substring(0, bodyStart));
                Console.ForegroundColor = defaultColor;
            }

            foreach (char c in line.Substring(bodyStart))
            {
                bool glyph = c is '[' or ']' or '@' or '|' || (c != '─' && c != ' ' && c != '\r');

                Console.ForegroundColor = glyph ? ConsoleColor.DarkBlue : defaultColor;
                Console.Write(c);
            }

            Console.ForegroundColor = defaultColor;
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Builds an ASCII representation of the quantum circuit and returns it as a string.
    /// This includes gate positions across qubits and visual connections between them.
    /// </summary>
    /// <param name="circuit">The quantum circuit to render.</param>
    /// <returns>A multi-line string containing the circuit diagram.</returns>
    /// <example>
    /// <code>
    /// Debug.Log(circuit.ToDiagram());   // Unity
    /// </code>
    /// </example>
    public static string ToDiagram(this QuantumCircuit circuit)
    {
        IList<IList<(string, int)>> gatePositions = new List<IList<(string, int)>>();
        IList<IList<int>> barPositions = new List<IList<int>>();
        IList<int> gateWidths = new List<int>();

        InitializeStructures(gatePositions, barPositions, circuit.QubitCount);

        int lastGate = AssignGatePositions(gatePositions, barPositions, gateWidths, circuit);

        StringBuilder sb = new StringBuilder();

        PrintGates(sb, gatePositions, barPositions, gateWidths, circuit, lastGate);

        return sb.ToString();
    }

    /// <summary>
    /// Initializes the data structures used for tracking gate symbols and bar positions
    /// for each qubit line in the quantum circuit.
    /// </summary>
    /// <param name="gatePositions">A list where each sublist will hold the gate symbols and their horizontal positions for each qubit.</param>
    /// <param name="barPositions">A list where each sublist will hold the horizontal positions of vertical bars connecting multi-qubit gates.</param>
    /// <param name="qubitCount">The total number of qubits in the circuit.</param>
    private static void InitializeStructures(IList<IList<(string, int)>> gatePositions, IList<IList<int>> barPositions, int qubitCount)
    {
        for (int i = 0; i < qubitCount; i++)
        {
            gatePositions.Add(new List<(string, int)>());
            barPositions.Add(new List<int>());
        }
    }

    /// <summary>
    /// Calculates and assigns horizontal positions for gates in the quantum circuit diagram,
    /// ensuring proper alignment and avoiding visual overlap between gates.
    /// </summary>
    /// <param name="gatePositions">A list of gate symbols and their positions for each qubit line.</param>
    /// <param name="barPositions">A list of vertical bar positions used to connect multi-qubit gates.</param>
    /// <param name="widths">A list where each integer holds an additional width of a gate.</param>
    /// <param name="circuit">The quantum circuit containing gates and qubit configuration.</param>
    /// <returns>The horizontal position (index) of the rightmost gate, used for drawing alignment.</returns>
    /// <remarks>
    /// This method handles both single and multi-qubit gates, places control and target markers,
    /// and avoids overlapping gates by checking for conflicts. For multi-qubit gates, vertical bars
    /// are also placed between the involved qubits.
    /// </remarks>
    private static int AssignGatePositions(IList<IList<(string, int)>> gatePositions, IList<IList<int>> barPositions,
        IList<int> widths, QuantumCircuit circuit)
    {
        int lastGate = 0;
        
        foreach (var gate in circuit.Gates)
        {
            var controlQubits = gate.ControlQubits;
            var targetQubits = gate.TargetQubits;

            string[] reps;

            // Measure and Custom span an arbitrary number of qubits, so their symbols are
            // repeated to match rather than looked up in the fixed symbol table.
            if(gate.GateType == GateType.Measure)
                reps = Enumerable.Repeat("M", targetQubits.Length).ToArray();
            else if(gate.GateType == GateType.Custom)
                reps = Enumerable.Repeat("C", targetQubits.Length).ToArray();
            else
                reps = Helpers.GateTypeToCharRepresentation(gate.GateType);
            
            var involvedQubits = controlQubits.Concat(targetQubits).ToList();

            int farthestIndex = 0;
            
            foreach (int qubit in involvedQubits)
            {
                int current = 0;

                if (gatePositions[circuit.QubitCount - 1 - qubit].Any())
                {
                    current = gatePositions[circuit.QubitCount - 1 - qubit].Last().Item2 + 1;
                }
                
                farthestIndex = current > farthestIndex ? current : farthestIndex;
            }

            bool conflict = true;

            while (conflict)
            {
                if(involvedQubits.Count == 0) break;
                
                int minQubit = involvedQubits.Min();
                int maxQubit = involvedQubits.Max();

                conflict = false;
                
                for (int q = circuit.QubitCount - 2 - minQubit; q >=  circuit.QubitCount - maxQubit; q--)
                {
                    var gatesAtThisQubit = gatePositions[q];

                    if (gatesAtThisQubit.Where(g => g.Item2 == farthestIndex).Any())
                    {
                        conflict = true;
                        farthestIndex++;
                        break;
                    }
                }
            }
            
            // Assign the gate to involved qubits
            int iter = 0;
            foreach (var qubit in involvedQubits)
            {
                // Add a gate at a correct position
                gatePositions[circuit.QubitCount-1-qubit].Add((reps[iter] ?? " ", farthestIndex));
                iter++;
            }
            
            // Set additional length of a gate if needed
            int maxWidth = reps.OrderByDescending(r => r.Length).First().Length - 1;
            while(widths.Count <= farthestIndex) widths.Add(0);
            widths[farthestIndex] = maxWidth > widths[farthestIndex] ? maxWidth : widths[farthestIndex];

            if (involvedQubits.Count > 1)
            {
                for (int i = circuit.QubitCount - 1 - involvedQubits.Max(); i <= circuit.QubitCount - 2 - involvedQubits.Min(); i++)
                {
                    if (!involvedQubits.Contains(circuit.QubitCount-1-i))
                    {
                        gatePositions[i].Add(("|", farthestIndex));
                    }
                    barPositions[i].Add(farthestIndex);
                }
            }
            
            lastGate = farthestIndex > lastGate ? farthestIndex : lastGate;
        }

        return lastGate;
    }

    /// <summary>
    /// Renders a visual representation of the quantum circuit into a string builder.
    /// </summary>
    /// <param name="sb">The builder receiving the diagram.</param>
    /// <param name="gatePositions">A list of gate symbols and their positions for each qubit line.</param>
    /// <param name="barPositions">A list of vertical bar positions for multi-qubit gates.</param>
    /// <param name="widths">A list where each integer holds an additional width of a gate.</param>
    /// <param name="circuit">The quantum circuit containing qubit information and initial states.</param>
    /// <param name="lastGate">The index of the rightmost gate, used for alignment and padding.</param>
    /// <remarks>
    /// This method draws each qubit line with its gates and initial state, using ASCII characters.
    /// Multi-qubit gates are connected with vertical bars. Coloring is applied separately by
    /// <see cref="Draw"/> so that this renderer stays free of any console dependency.
    /// </remarks>
    private static void PrintGates(StringBuilder sb, IList<IList<(string, int)>> gatePositions,
        IList<IList<int>> barPositions, IList<int> widths, QuantumCircuit circuit, int lastGate)
    {
        int counter = -1;
        for (int i = circuit.QubitCount-1; i >= 0; i--)
        {
            InitialState? initialState = circuit.Initializations.FirstOrDefault(init => init.QubitIndex == i);
            char initState = initialState == null ? '0' : Helpers.InitialStateToCharRepresentation(initialState.BasicState);

            sb.Append($"q{i} ({initState}): ");

            foreach (var gatePos in gatePositions[circuit.QubitCount-1-i])
            {
                int additionalWidth = widths.Skip(counter+1).Take(gatePos.Item2 - counter - 1).Sum();
                sb.Append('\u2500', ((gatePos.Item2-counter-1)*5)+additionalWidth);
                counter = gatePos.Item2;

                if (gatePos.Item1 == "|")
                {
                    sb.Append('\u2500', 2);
                    sb.Append('|');
                    sb.Append('\u2500', 2+widths[counter]);
                    continue;
                }
                if (gatePos.Item1 == "@")
                {
                    sb.Append('\u2500', 2);
                    sb.Append('@');
                    sb.Append('\u2500', 2+widths[counter]);
                    continue;
                }

                sb.Append('\u2500');
                sb.Append('[').Append(gatePos.Item1).Append(']');
                sb.Append('\u2500', 2+widths[counter]-gatePos.Item1.Length);
            }

            if (!gatePositions[circuit.QubitCount - 1 - i].Any())
            {
                int additionalWidth = widths.Sum();
                sb.Append('\u2500', ((lastGate+1)*5)+additionalWidth);
            }
            else if (counter < lastGate)
            {
                int additionalWidth = widths.Skip(counter+1).Take(lastGate - counter).Sum();
                sb.Append('\u2500', ((lastGate-counter)*5)+additionalWidth);
            }

            sb.AppendLine();
            counter = -1;

            sb.Append(' ', 8);
            foreach (var barPos in barPositions[circuit.QubitCount-1-i])
            {
                if(i == 0) continue;

                int additionalWidth = widths.Skip(counter+1).Take(barPos - counter - 1).Sum();
                sb.Append(' ', ((barPos-counter-1)*5)+additionalWidth);
                counter = barPos;

                sb.Append(' ', 2);
                sb.Append('|');
                sb.Append(' ', 2+widths[barPos]);
            }

            sb.AppendLine();
            counter = -1;
        }
    }
}