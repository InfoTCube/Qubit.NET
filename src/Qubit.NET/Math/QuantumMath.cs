using System.Numerics;
using Qubit.NET.Utilities;

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
        if (BitOperations.PopCount((uint)state.Length) != 1)
            throw new ArgumentException("State vector length must be a power of 2.");
        
        if (gate.GetLength(0) != 2 || gate.GetLength(1) != 2)
            throw new ArgumentException("Single qubit gate must be a 2x2 matrix.");
    
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
    /// Samples a measurement from the quantum state vector for a multi-qubit system. More efficient then partial measurment
    /// </summary>
    /// <param name="state">The current quantum state vector.</param>
    /// <param name="rand">Source of random numbers, can be psuedo, true random or custom.</param>
    /// <returns>The measured state as a bitstring, represented as an integer.</returns>
    internal static int SampleMeasurement(Complex[] state, IRandomSource rand)
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
        double randomValue = rand.NextDouble();
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
    
    /// <summary>
    /// Samples a measurement result from a subset of qubits in a quantum state vector (partial measurement).
    /// The method computes the probabilities of all possible outcomes on the measured qubits and returns
    /// a randomly chosen outcome based on those probabilities.
    /// </summary>
    /// <param name="state">The current quantum state vector of the entire system.</param>
    /// <param name="measuredQubits">
    /// Array of qubit indices to be measured. The result's bit order corresponds to the order of indices in this array.
    /// </param>
    /// <param name="rand">Source of random numbers, can be psuedo, true random or custom.</param>
    /// <returns>
    /// An integer representing the measurement outcome, encoded as a bitstring ordered according to <paramref name="measuredQubits"/>.
    /// </returns>
    internal static int SamplePartialMeasurement(Complex[] state, int[] measuredQubits, IRandomSource rand)
    {
        int outcomeCount = 1 << measuredQubits.Length;
        double[] outcomeProbabilities = new double[outcomeCount];
        double totalProbability = 0.0;

        // Precompute bit shifts for each measured qubit (bit order: highest index -> lowest)
        int[] bitShifts = new int[measuredQubits.Length];
        for (int j = 0; j < measuredQubits.Length; j++)
        {
            bitShifts[j] = measuredQubits.Length - 1 - j;
        }

        for (int i = 0; i < state.Length; i++)
        {
            int partialMeasurement = 0;
            for (int bit = 0; bit < measuredQubits.Length; bit++)
            {
                // Extract bit value of qubit measuredQubits[bit] in index i
                if (((i >> measuredQubits[bit]) & 1) != 0)
                {
                    partialMeasurement |= (1 << bitShifts[bit]);
                }
            }

            double prob = state[i].Real * state[i].Real + state[i].Imaginary * state[i].Imaginary;
            outcomeProbabilities[partialMeasurement] += prob;
            totalProbability += prob;
        }

        // Normalize
        for (int i = 0; i < outcomeCount; i++)
        {
            outcomeProbabilities[i] /= totalProbability;
        }

        // Sample
        double randomValue = rand.NextDouble();
        double cumulative = 0.0;

        for (int i = 0; i < outcomeCount; i++)
        {
            cumulative += outcomeProbabilities[i];
            if (randomValue <= cumulative)
            {
                return i;
            }
        }

        return outcomeCount - 1;
    }
    
    /// <summary>
    /// Collapses the quantum state vector based on the result of a partial measurement.
    /// Only the amplitudes consistent with the measured result on the specified qubits are preserved;
    /// all other amplitudes are set to zero. The remaining state is renormalized to maintain a valid quantum state.
    /// </summary>
    /// <param name="state">The quantum state vector before measurement.</param>
    /// <param name="measuredQubits">Indices of the qubits that were measured.</param>
    /// <param name="result">
    /// The integer-encoded result of the measurement, where the bit order corresponds to the order of qubit indices in <paramref name="measuredQubits"/>.
    /// </param>
    /// <returns>The collapsed and normalized quantum state vector after partial measurement.</returns>
    internal static Complex[] CollapseToPartialMeasurement(Complex[] state, int[] measuredQubits, int result)
    {
        Complex[] collapsed = new Complex[state.Length];
        double normSquared = 0.0;

        for (int i = 0; i < state.Length; i++)
        {
            int extracted = 0;
            for (int bit = 0; bit < measuredQubits.Length; bit++)
            {
                int qubitIndex = measuredQubits[bit];
                int bitValue = (i >> qubitIndex) & 1;
                extracted |= (bitValue << (measuredQubits.Length - 1 - bit));
            }

            if (extracted == result)
            {
                collapsed[i] = state[i];
                normSquared += state[i].Magnitude * state[i].Magnitude;
            }
            else
            {
                collapsed[i] = Complex.Zero;
            }
        }
        
        double norm = System.Math.Sqrt(normSquared);
        for (int i = 0; i < collapsed.Length; i++)
        {
            collapsed[i] /= norm;
        }

        return collapsed;
    }
    
    /// <summary>
    /// Determines whether a given complex matrix is unitary.
    /// </summary>
    /// <param name="matrix">
    /// A matrix of complex numbers to be tested for unitarity.
    /// </param>
    /// <returns>
    /// <c>true</c> if the matrix is unitary (i.e., its conjugate transpose is its inverse); otherwise, <c>false</c>.
    /// </returns>
    internal static bool IsUnitary(Complex[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        // Matrix must be square
        if (rows != cols)
            return false;

        // Step 1: Calculate the conjugate transpose (A†) of the matrix
        Complex[,] conjugateTranspose = new Complex[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                conjugateTranspose[i, j] = Complex.Conjugate(matrix[j, i]);
            }
        }

        // Step 2: Multiply the matrix by its conjugate transpose (A * A†)
        Complex[,] result = new Complex[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Complex sum = Complex.Zero;
                for (int k = 0; k < rows; k++)
                {
                    sum += matrix[i, k] * conjugateTranspose[k, j];
                }
                result[i, j] = sum;
            }
        }

        // Step 3: Check if the result is the identity matrix
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (i == j)
                {
                    // Diagonal elements must be 1
                    if (!result[i, j].Equals(Complex.One))
                    {
                        return false;
                    }
                }
                else
                {
                    // Off-diagonal elements must be 0
                    if (!result[i, j].Equals(Complex.Zero))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }
}