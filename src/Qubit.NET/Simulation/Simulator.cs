using System.Numerics;
using System.Text;
using Qubit.NET.Gates;
using Qubit.NET.Math;
using Qubit.NET.Utilities;

namespace Qubit.NET.Simulation;

/// <summary>
/// Provides functionality to simulate quantum circuits.
/// </summary>
public static class Simulator
{
    /// <summary>
    /// Simulates the quantum circuit by applying gates up to the first measurement,
    /// and then continues the simulation for a specified number of shots,
    /// recording the results of measurements for each run.
    /// </summary>
    /// <param name="qc">The quantum circuit to simulate.</param>
    /// <param name="shots">The number of times the simulation should be run. Default is 1.</param>
    /// <returns>
    /// One <see cref="MeasurementResult"/> per measurement in the circuit, in the order the
    /// measurements appear, each holding the outcome counts across all shots.
    /// Empty if the circuit contains no measurement.
    /// </returns>
    public static IList<MeasurementResult> Run(QuantumCircuit qc, int shots = 1)
    {
        if (qc.Gates.All(g => g.GateType != GateType.Measure)) return new List<MeasurementResult>();

        Complex[] stateVector = new Complex[1 << qc.QubitCount];
        stateVector[0] = new Complex(1, 0);

        foreach (var init in qc.Initializations)
        {
            stateVector = QuantumMath.InitializeState(stateVector, init.QubitIndex, init.Alpha, init.Beta);
        }

        // Work on a local copy: Run must never mutate the circuit it is handed, or a
        // second Run(qc, shots) would replay a circuit stripped of its own gates.
        List<Gate> remainingGates = qc.Gates.ToList();

        // Apply the deterministic prefix (everything before the first measurement) once,
        // then replay only the rest per shot. A conditional gate depends on a measurement,
        // so it can never appear in the prefix.
        while (remainingGates.Count > 0 && remainingGates[0].GateType != GateType.Measure)
        {
            Gate currentGate = remainingGates[0];

            stateVector = ApplyGate(stateVector, currentGate, currentGate.GateType);

            remainingGates.RemoveAt(0);
        }

        List<MeasurementResult> results = new();

        // Classical bits are re-derived every shot, so feedforward follows that shot's own
        // measurement outcomes.
        int[] classicalBits = new int[qc.QubitCount];

        for (int i = 0; i < shots; i++)
        {
            Complex[] modStateVector = (Complex[])stateVector.Clone();

            Array.Clear(classicalBits, 0, classicalBits.Length);

            int measurmentNumber = 0;

            foreach (var gate in remainingGates)
            {
                if (gate.ConditionBit is { } bit && classicalBits[bit] != gate.ConditionValue)
                    continue;

                if (gate.GateType == GateType.Measure)
                {
                    int num = MeasureState(ref modStateVector, gate.TargetQubits, qc);

                    if (measurmentNumber + 1 > results.Count)
                        results.Add(new MeasurementResult(
                            new int[1 << gate.TargetQubits.Length], gate.TargetQubits.Length));

                    results[measurmentNumber].Record(num);

                    RecordClassicalBits(gate, num, classicalBits, qc.QubitCount);

                    measurmentNumber++;
                }
                else
                {
                    modStateVector = ApplyGate(modStateVector, gate, gate.GateType);
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Writes a measurement outcome into the classical bits the gate targets.
    /// </summary>
    /// <param name="gate">The measurement gate.</param>
    /// <param name="outcome">The packed measurement outcome.</param>
    /// <param name="classicalBits">The classical register for the current shot.</param>
    /// <param name="qubitCount">Number of qubits in the circuit.</param>
    /// <remarks>
    /// The two measurement paths pack their outcome differently: a full-register
    /// measurement returns a basis index, where qubit q sits at bit q, while a partial
    /// measurement packs the listed qubits with the first one most significant.
    /// </remarks>
    private static void RecordClassicalBits(Gate gate, int outcome, int[] classicalBits, int qubitCount)
    {
        int width = gate.TargetQubits.Length;
        bool fullRegister = width == qubitCount;

        for (int b = 0; b < gate.ClassicalBits.Length && b < width; b++)
        {
            int shift = fullRegister ? gate.TargetQubits[b] : width - 1 - b;

            classicalBits[gate.ClassicalBits[b]] = (outcome >> shift) & 1;
        }
    }

    /// <summary>
    /// Converts the result of a quantum circuit simulation into a human-readable string.
    /// </summary>
    /// <param name="result">The measurement result to format.</param>
    /// <returns>
    /// A string formatted as a dictionary, where keys are binary representations of measurement outcomes,
    /// and values are the counts of how often each outcome occurred.
    /// </returns>
    public static string GetStringResult(this MeasurementResult result) => result.ToString();

    /// <summary>
    /// Applies a quantum gate to the given state vector, modifying it according to the specified gate type.
    /// Handles single-qubit and multi-qubit gates including controlled gates.
    /// </summary>
    /// <param name="stateVector">The current quantum state vector.</param>
    /// <param name="currentGate">The gate to apply.</param>
    /// <param name="gateType">The type of gate being applied.</param>
    private static Complex[] ApplyGate(Complex[] stateVector, Gate currentGate, GateType gateType)
    {
        // Measure gates are handled by the caller and never reach here, so every gate at
        // this point must carry a matrix.
        Complex[,] matrix = currentGate.Matrix
                            ?? throw new InvalidOperationException($"Gate {gateType} has no matrix.");

        switch (gateType)
        {
            case GateType.I or GateType.H or GateType.X or GateType.Y or GateType.Z or GateType.S or GateType.Sdag 
                or GateType.T or GateType.Tdag or GateType.Rx or GateType.Ry or GateType.Rz or GateType.SX 
                or GateType.SY or GateType.SZ or GateType.U3:
                stateVector = QuantumMath.ApplySingleQubitGate(stateVector, matrix, currentGate.TargetQubits[0]);
                break;
            case GateType.CNOT or GateType.CY or GateType.CZ or GateType.CH or GateType.CRx or GateType.CRy
                or GateType.CRz or GateType.CU3:
                stateVector = QuantumMath.ApplyControlledSingleQubitGate(stateVector,
                    QuantumMath.ControlledCore(matrix),
                    currentGate.TargetQubits[0], currentGate.ControlQubits[0]);
                break;
            case GateType.SWAP:
                stateVector = QuantumMath.ApplyMultiQubitGate(stateVector, matrix,
                    currentGate.TargetQubits);
                break;
            case GateType.Toffoli:
                stateVector = QuantumMath.ApplyControlledSingleQubitGate(stateVector,
                    QuantumMath.ControlledCore(matrix),
                    currentGate.TargetQubits[0], currentGate.ControlQubits[0], currentGate.ControlQubits[1]);
                break;
            case GateType.Fredkin:
                stateVector = QuantumMath.ApplyMultiQubitGate(stateVector, matrix,
                    [currentGate.TargetQubits[1], currentGate.TargetQubits[0], currentGate.ControlQubits[0]]);
                break;
            case GateType.Custom:
                stateVector = QuantumMath.ApplyMultiQubitGate(stateVector, matrix,
                    Enumerable.Reverse(currentGate.TargetQubits).ToArray());
                break;
        }

        return stateVector;
    }
    
    /// <summary>
    /// Measures the quantum register, collapsing the state to one of the basis states.
    /// The measurement is a probabilistic process where the state collapses to a classical bit (0 or 1) with respective probabilities.
    /// The method updates the quantum state vector after the measurement, reducing the state to the measured result.
    /// </summary>
    /// <param name="stateVector">The current state vector.</param>
    /// <param name="qubits">An array of qubits to measure</param>
    /// <param name="qc">Quantum circuit running.</param>
    /// <returns>The index of the measured basis state, representing the outcome of the measurement.</returns>
    private static int MeasureState(ref Complex[] stateVector, int[] qubits, QuantumCircuit qc)
    {
        int result;
        
        // If measuring all qubits, use much more efficient method
        if (qubits.Length == qc.QubitCount)
        {
            // Perform a measurement by sampling from the current state vector probabilities
            result = QuantumMath.SampleMeasurement(stateVector, qc.RandomSource);
    
            // Collapse the quantum state to the measured state (collapse the superposition)
            stateVector = QuantumMath.CollapseToState(stateVector, result);
            
            return result;
        }
        
        // Perform a measurement by sampling from the current state vector probabilities
        result = QuantumMath.SamplePartialMeasurement(stateVector, qubits, qc.RandomSource);
    
        // Collapse the quantum state to the measured state (collapse the superposition)
        stateVector = QuantumMath.CollapseToPartialMeasurement(stateVector, qubits, result);

        return result;
    }
}