using Qubit.NET.Utilities;

namespace Qubit.NET;

public static class QuantumCircuitDrawer
{
    /// <summary>
    /// Draws an ASCII representation of the quantum circuit in the console.
    /// This includes gate positions across qubits and visual connections between them.
    /// </summary>
    /// <param name="circuit">The quantum circuit to draw.</param>
    public static void Draw(this QuantumCircuit circuit)
    {
        IList<IList<(string, int)>> gatePositions = new List<IList<(string, int)>>();
        IList<IList<int>> barPositions = new List<IList<int>>();
        IList<int> gateWidths = new List<int>();

        InitializeStructures(gatePositions, barPositions, circuit.QubitCount);

        int lastGate = AssignGatePositions(gatePositions, barPositions, gateWidths, circuit);
        
        PrintGates(gatePositions, barPositions, gateWidths, circuit, lastGate);
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
            var controlQubits = gate.ControlQubits ?? Array.Empty<int>();
            var targetQubits = gate.TargetQubits ?? Array.Empty<int>();
            
            string[] reps = Helpers.GateTypeToCharRepresentation(gate.GateType);
            
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
    /// Renders a visual representation of the quantum circuit to the console.
    /// </summary>
    /// <param name="gatePositions">A list of gate symbols and their positions for each qubit line.</param>
    /// <param name="barPositions">A list of vertical bar positions for multi-qubit gates.</param>
    /// <param name="widths">A list where each integer holds an additional width of a gate.</param>
    /// <param name="circuit">The quantum circuit containing qubit information and initial states.</param>
    /// <param name="lastGate">The index of the rightmost gate, used for alignment and padding.</param>
    /// <remarks>
    /// This method draws each qubit line with its gates and initial state, using ASCII characters.
    /// Gates are color-highlighted and aligned based on position. Multi-qubit gates are connected with vertical bars.
    /// </remarks>
    private static void PrintGates(IList<IList<(string, int)>> gatePositions, IList<IList<int>> barPositions,
        IList<int> widths, QuantumCircuit circuit, int lastGate)
    {
        ConsoleColor defaultColor = Console.ForegroundColor;

        int counter = -1;
        for (int i = circuit.QubitCount-1; i >= 0; i--)
        {
            InitialState? initialState = circuit.Initializations.FirstOrDefault(init => init.QubitIndex == i);
            char initState = initialState == null ? '0' : Helpers.InitialStateToCharRepresentation(initialState.BasicState);
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"q{i} ({initState}): ");
            Console.ForegroundColor = defaultColor;
            
            foreach (var gatePos in gatePositions[circuit.QubitCount-1-i])
            {
                int additionalWidth = widths.Skip(counter+1).Take(gatePos.Item2 - counter - 1).Sum();
                Console.Write(new string('\u2500', ((gatePos.Item2-counter-1)*5)+additionalWidth));
                counter = gatePos.Item2;
                
                if (gatePos.Item1 == "|")
                {
                    Console.Write(new string('\u2500', 2));
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    Console.Write("|");
                    Console.ForegroundColor = defaultColor;
                    Console.Write(new string('\u2500', 2+widths[counter]));
                    continue;
                } 
                if (gatePos.Item1 == "@")
                {
                    Console.Write(new string('\u2500', 2));
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    Console.Write("@");
                    Console.ForegroundColor = defaultColor;
                    Console.Write(new string('\u2500', (2+widths[counter])));
                    continue;
                }
                
                Console.Write("\u2500");
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.Write($"[{gatePos.Item1}]");
                Console.ForegroundColor = defaultColor;
                Console.Write(new string('\u2500', (2+widths[counter]-gatePos.Item1.Length)));
            }

            if (!gatePositions[circuit.QubitCount - 1 - i].Any())
            {
                int additionalWidth = widths.Sum();
                Console.Write(new string('\u2500', ((lastGate+1)*5)+additionalWidth));
            } 
            else if (counter < lastGate)
            {
                int additionalWidth = widths.Skip(counter+1).Take(lastGate - counter).Sum();
                Console.Write(new string('\u2500', ((lastGate-counter)*5)+additionalWidth));
            }

            Console.WriteLine();
            counter = -1;
            
            Console.Write(new String(' ', 8));
            foreach (var barPos in barPositions[circuit.QubitCount-1-i])
            {
                if(i == 0) continue;

                int additionalWidth = widths.Skip(counter+1).Take(barPos - counter - 1).Sum();
                Console.Write(new string(' ', ((barPos-counter-1)*5)+additionalWidth));
                counter = barPos;
                
                Console.Write(new string(' ', 2));
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.Write("|");
                Console.ForegroundColor = defaultColor;
                Console.Write(new string(' ', (2+widths[barPos])));
            }
            
            Console.WriteLine();
            counter = -1;
        }
    }
}