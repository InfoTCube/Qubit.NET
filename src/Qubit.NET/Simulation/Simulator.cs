using System.Numerics;
using System.Text;
using Qubit.NET.Gates;
using Qubit.NET.Math;

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
    /// A list of integer arrays representing measurement results for each shot.
    /// Each array contains counts of observed outcomes.
    /// </returns>
    public static IList<int[]> Run(QuantumCircuit qc, int shots = 1)
    {
        if(qc.Gates.All(g => g.GateType != GateType.Measure)) return new List<int[]>();
        
        Complex[] stateVector = new Complex[1 << qc.QubitCount];
        stateVector[0] = new Complex(1, 0);

        foreach (var init in qc.Initializations)
        {
            stateVector = QuantumMath.InitializeState(stateVector, init.QubitIndex, init.Alpha, init.Beta);
        }

        GateType currentGateType = qc.Gates.First().GateType;

        while (currentGateType != GateType.Measure && qc.Gates.Count != 0)
        {
            Gate currentGate = qc.Gates.First();
            
            stateVector = ApplayGate(stateVector, currentGate, currentGateType);

            qc.Gates.RemoveAt(0);
            
            currentGateType = qc.Gates.Count > 0 ? qc.Gates.First().GateType : currentGateType;
        }
        
        IList<int[]> results = new List<int[]>();
        
        for (int i = 0; i < shots; i++)
        {
            Complex[] modStateVector = (Complex[])stateVector.Clone();

            int measurmentNumber = 0;
            
            foreach (var gate in qc.Gates)
            {
                if (gate.GateType == GateType.Measure)
                {
                    int num = MeasureState(modStateVector);

                    if (measurmentNumber + 1 > results.Count)
                    {
                        results.Add(new int[1L << qc.QubitCount]);
                    }
                    
                    results[measurmentNumber][num]++;

                    measurmentNumber++;
                }
                else
                {
                    modStateVector = ApplayGate(modStateVector, gate, gate.GateType);
                }
            }
        }
        
        return results;
    }

    /// <summary>
    /// Converts the result of a quantum circuit simulation into a human-readable string.
    /// </summary>
    /// <param name="result">An array of measurement result counts.</param>
    /// <returns>
    /// A string formatted as a dictionary, where keys are binary representations of measurement outcomes,
    /// and values are the counts of how often each outcome occurred.
    /// </returns>
    public static string GetStringResult(this int[] result, int numQubits)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        for (int i = 0; i < result.Length; i++)
        {
            if(result[i] == 0)
                continue;
            
            sb.Append("'");
            sb.Append(Convert.ToString(i, 2).PadLeft(numQubits, '0'));
            sb.Append("': ");
            sb.Append(result[i].ToString());
            sb.Append(", ");
        }
        
        sb.Remove(sb.Length - 2, 2);
        sb.Append("}");
        
        return sb.ToString();
    }

    /// <summary>
    /// Applies a quantum gate to the given state vector, modifying it according to the specified gate type.
    /// Handles single-qubit and multi-qubit gates including controlled gates.
    /// </summary>
    /// <param name="stateVector">The current quantum state vector.</param>
    /// <param name="currentGate">The gate to apply.</param>
    /// <param name="gateType">The type of gate being applied.</param>
    private static Complex[] ApplayGate(Complex[] stateVector, Gate currentGate, GateType gateType)
    {
        switch (gateType)
        {
            case GateType.I or GateType.H or GateType.X or GateType.Y or GateType.Z or GateType.S or GateType.Sdag 
                or GateType.T or GateType.Tdag or GateType.Rx or GateType.Ry or GateType.Rz:
                stateVector = QuantumMath.ApplySingleQubitGate(stateVector, currentGate.Matrix, currentGate.TargetQubits.First());
                break;
            case GateType.CNOT or GateType.CY or GateType.CZ or GateType.CH or GateType.CRx or GateType.CRy or GateType.CRz:
                stateVector = QuantumMath.ApplyMultiQubitGate(stateVector, currentGate.Matrix, 
                    [currentGate.TargetQubits.First(), currentGate.ControlQubits.First()]);
                break;
            case GateType.SWAP:
                stateVector = QuantumMath.ApplyMultiQubitGate(stateVector, currentGate.Matrix,
                    currentGate.TargetQubits.ToArray());
                break;
            case GateType.Toffoli:
                stateVector = QuantumMath.ApplyMultiQubitGate(stateVector, currentGate.Matrix,
                    [currentGate.TargetQubits.First(), currentGate.ControlQubits[0], currentGate.ControlQubits[1]]);
                break;
            case GateType.Fredkin:
                stateVector = QuantumMath.ApplyMultiQubitGate(stateVector, currentGate.Matrix,
                    [currentGate.TargetQubits[1], currentGate.TargetQubits[0], currentGate.ControlQubits.First()]);
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
    /// <returns>The index of the measured basis state, representing the outcome of the measurement.</returns>
    private static int MeasureState(Complex[] stateVector)
    {
        // Perform a measurement by sampling from the current state vector probabilities
        var result = QuantumMath.SampleMeasurement(stateVector);
    
        // Collapse the quantum state to the measured state (collapse the superposition)
        stateVector = QuantumMath.CollapseToState(stateVector, result);

        return result;
    }
}