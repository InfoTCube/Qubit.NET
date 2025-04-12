using System.Numerics;

namespace Qubit.NET.Math;

/// <summary>
/// Provides quantum mathematical operations for manipulating qubit states and measurement.
/// </summary>
internal static class QuantumMath
{
    /// <summary>
    /// Applies a single-qubit gate to a specific qubit in a multi-qubit state vector.
    /// </summary>
    /// <param name="gate">The 2x2 gate matrix.</param>
    /// <param name="state">The current full quantum state vector.</param>
    /// <param name="targetQubit">The index of the qubit to apply the gate on (0 = least significant).</param>
    /// <returns>The updated quantum state vector.</returns>
    internal static Complex[] ApplySingleQubitGate(Complex[] state, Complex[,] gate, int targetQubit)
    {
        int n = (int)System.Math.Log2(state.Length);
        if (state.Length != 1 << n)
            throw new ArgumentException("State vector length must be a power of 2.");
    
        Complex[] newState = new Complex[state.Length];

        for (int i = 0; i < state.Length; i++)
        {
            int bit = (i >> targetQubit) & 1;
            int flippedIndex = i ^ (1 << targetQubit);
            
            if (bit == 0)
            {
                Complex a = state[i];
                Complex b = state[flippedIndex];

                newState[i] += gate[0, 0] * a + gate[0, 1] * b;
                newState[flippedIndex] += gate[1, 0] * a + gate[1, 1] * b;
            }
        }
        
        return newState;
    }
    
    /// <summary>
    /// Applies a multi-qubit gate to a subset of qubits in a multi-qubit state vector.
    /// </summary>
    /// <param name="state">The current full quantum state vector.</param>
    /// <param name="gate">The unitary matrix representing the multi-qubit gate.</param>
    /// <param name="targetQubits">An array of qubit indices to which the gate should be applied.</param>
    /// <returns>The updated quantum state vector after applying the gate.</returns>
    public static Complex[] ApplyMultiQubitGate(Complex[] state, Complex[,] gate, int[] targetQubits)
    {
        Complex[] newState = new Complex[state.Length]; // Initialize to zeros

        // Iterate through each basis state
        for (int i = 0; i < state.Length; i++)
        {
            // Extract the values of target qubits (as bits)
            int targetBits = 0;
            foreach (int qubit in targetQubits)
            {
                targetBits |= ((i >> qubit) & 1) << Array.IndexOf(targetQubits, qubit);
            }

            // Apply gate operation to this basis state
            for (int j = 0; j < (1 << targetQubits.Length); j++)
            {
                // Create the new basis state index
                int newIndex = i;
                for (int k = 0; k < targetQubits.Length; k++)
                {
                    int currentBit = (j >> k) & 1;
                    int oldBit = (i >> targetQubits[k]) & 1;
                    if (currentBit != oldBit)
                    {
                        newIndex ^= (1 << targetQubits[k]);
                    }
                }

                // Apply the gate coefficient
                int gateRow = j;
                int gateCol = targetBits;
                newState[newIndex] += gate[gateRow, gateCol] * state[i];
            }
        }

        return newState;
    }

    /// <summary>
    /// Initializes the specified qubit to an arbitrary single-qubit state defined by amplitudes α and β.
    /// This is done by transforming the existing quantum state vector accordingly.
    /// </summary>
    /// <param name="state">The current full quantum state vector.</param>
    /// <param name="targetQubit">The index of the qubit to initialize.</param>
    /// <param name="alpha">Amplitude for the |0⟩ component of the qubit.</param>
    /// <param name="beta">Amplitude for the |1⟩ component of the qubit.</param>
    public static Complex[] InitializeState(Complex[] state, int targetQubit, Complex alpha, Complex beta)
    {
        Complex[] newState = new Complex[state.Length];

        for (int i = 0; i < state.Length; i++)
        {
            int bit = (i >> targetQubit) & 1;
            int baseIndex = i & ~(1 << targetQubit);
            
            if (bit == 0)
            {
                newState[baseIndex] += alpha * state[i];
                newState[baseIndex | (1 << targetQubit)] += beta * state[i];
            }
            else
            {
                newState[baseIndex] += beta * state[i];
                newState[baseIndex | (1 << targetQubit)] += -alpha * state[i];
            }
        }

        return newState;
    }
    
    /// <summary>
    /// Samples a measurement from the quantum state vector for a multi-qubit system.
    /// </summary>
    /// <param name="state">The current quantum state vector.</param>
    /// <returns>The measured state as a bitstring, represented as an integer.</returns>
    internal static int SampleMeasurement(Complex[] state)
    {
        double totalProbability = 0.0;
        double[] probabilities = new double[state.Length];

        // Calculate the probability for each basis state
        for (int i = 0; i < state.Length; i++)
        {
            probabilities[i] = state[i].Real * state[i].Real + state[i].Imaginary * state[i].Imaginary;
            totalProbability += probabilities[i];
        }

        // Normalize the probabilities
        for (int i = 0; i < state.Length; i++)
        {
            probabilities[i] /= totalProbability;
        }
    
        // Generate a random number and pick the state based on probability
        double randomValue = new Random().NextDouble();
        double cumulativeProbability = 0.0;

        for (int i = 0; i < state.Length; i++)
        {
            cumulativeProbability += probabilities[i];
            if (randomValue <= cumulativeProbability)
            {
                return i; // This index corresponds to the measured basis state
            }
        }

        return state.Length - 1; // Return last index if something goes wrong
    }
    
    /// <summary>
    /// Collapses the quantum state to a single basis state based on the measured result.
    /// After measurement, all states except the measured state are set to zero, and the measured state is set to 1.
    /// </summary>
    /// <param name="state">The quantum state vector before measurement.</param>
    /// <param name="result">The index of the measured basis state.</param>
    /// <returns>The collapsed quantum state vector with all other states set to 0, except for the measured state.</returns>
    internal static Complex[] CollapseToState(Complex[] state, int result)
    {
        // Create a new state vector, initially filled with 0
        Complex[] collapsedState = new Complex[state.Length];

        // Set the component of the measured state to 1 (collapse to the measured state)
        collapsedState[result] = Complex.One;
        
        return collapsedState;
    }
}