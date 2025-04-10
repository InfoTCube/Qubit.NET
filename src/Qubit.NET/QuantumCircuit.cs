using System.Numerics;
using Qubit.NET.Gates;
using Qubit.NET.Math;

namespace Qubit.NET;

/// <summary>
/// Represents a quantum register containing a given number of qubits.
/// Holds the state vector and provides methods to apply quantum gates and measure probabilities.
/// </summary>
public class QuantumCircuit
{
    /// <summary>
    /// Number of qubits in the register.
    /// </summary>
    public int QubitCount { get; }
    
    /// <summary>
    /// State vector representing the current quantum state.
    /// </summary>
    public Complex[] StateVector { get; private set; }

    /// <summary>
    /// Initializes quantum circuit with specified number of qubits in 0 state.
    /// </summary>
    /// <param name="qubitCount">Number of qubits.</param>
    public QuantumCircuit(int qubitCount)
    {
        if (qubitCount <= 0)
            throw new ArgumentException("Qubit count must be positive.");
        
        QubitCount = qubitCount;
        StateVector = new Complex[1 << qubitCount];
        StateVector[0] = new Complex(1, 0);
    }

    public QuantumCircuit(QuantumCircuit qc)
    {
        QubitCount = qc.QubitCount;
        StateVector = qc.StateVector;
    }

    /// <summary>
    /// Measures the quantum register, collapsing the state to one of the basis states.
    /// The measurement is a probabilistic process where the state collapses to a classical bit (0 or 1) with respective probabilities.
    /// The method updates the quantum state vector after the measurement, reducing the state to the measured result.
    /// </summary>
    /// <returns>The index of the measured basis state, representing the outcome of the measurement.</returns>
    public string Measure()
    {
        // Perform a measurement by sampling from the current state vector probabilities
        var result = QuantumMath.SampleMeasurement(StateVector);
    
        // Collapse the quantum state to the measured state (collapse the superposition)
        StateVector = QuantumMath.CollapseToState(StateVector, result);

        return Convert.ToString(result, 2).PadLeft(QubitCount, '0');
    }
    
    /// <summary>
    /// Applies the Hadamard gate (H) to the specified qubit.
    /// The Hadamard gate creates a superposition of the basis states.
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the Hadamard gate to.</param>
    public void H(int qubit)
    {
        ApplyGate(QuantumGates.H, qubit);
    }

    /// <summary>
    /// Applies the Pauli-X gate (X) to the specified qubit.
    /// The Pauli-X gate flips the state of the qubit (|0> ↔ |1>).
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the Pauli-X gate to.</param>
    public void X(int qubit)
    {
        ApplyGate(QuantumGates.X, qubit);
    }

    /// <summary>
    /// Applies the Pauli-Y gate (Y) to the specified qubit.
    /// The Pauli-Y gate performs a bit-flip followed by a phase flip on the qubit.
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the Pauli-Y gate to.</param>
    public void Y(int qubit)
    {
        ApplyGate(QuantumGates.Y, qubit);
    }

    
    /// <summary>
    /// Applies the Pauli-Z gate (Z) to the specified qubit.
    /// The Pauli-Z gate applies a phase flip to the qubit (|0> ↔ |1> with a phase of -1 for |1>).
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the Pauli-Z gate to.</param>
    public void Z(int qubit)
    {
        ApplyGate(QuantumGates.Z, qubit);
    }

    /// <summary>
    /// Applies the S gate (phase gate) to the specified qubit.
    /// The S gate applies a phase shift of π/2 to the qubit, leaving |0⟩ unchanged and mapping |1⟩ to i·|1⟩.
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the S gate to.</param>
    public void S(int qubit)
    {
        ApplyGate(QuantumGates.S, qubit);
    }

    /// <summary>
    /// Applies the T gate (π/4 phase gate) to the specified qubit.
    /// The T gate applies a phase shift of π/4 to the qubit, leaving |0⟩ unchanged and mapping |1⟩ to e^(iπ/4)·|1⟩.
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the T gate to.</param>
    public void T(int qubit)
    {
        ApplyGate(QuantumGates.T, qubit);
    }
    
    /// <summary>
    /// Applies the CNOT (also called CX) gate (Controlled-NOT) to the specified qubits.
    /// The CNOT gate flips the target qubit if the control qubit is in state |1>.
    /// </summary>
    /// <param name="controlQubit">The index of the control qubit.</param>
    /// <param name="targetQubit">The index of the target qubit.</param>
    public void CNOT(int controlQubit, int targetQubit)
    {
        ApplyGate(QuantumGates.CNOT, [targetQubit, controlQubit]);
    }

    /// <summary>
    /// Applies the CY gate (Controlled-Y) to the specified qubits.
    /// The CY gate applies the Pauli-Y operation to the target qubit if the control qubit is in state |1⟩.
    /// </summary>
    /// <param name="controlQubit">The index of the control qubit.</param>
    /// <param name="targetQubit">The index of the target qubit.</param>
    public void CY(int controlQubit, int targetQubit)
    {
        ApplyGate(QuantumGates.CY, [targetQubit, controlQubit]);
    }
    
    /// <summary>
    /// Applies the CZ gate (Controlled-Z) to the specified qubits.
    /// The CZ gate applies the Pauli-Z operation to the target qubit if the control qubit is in state |1⟩.
    /// </summary>
    /// <param name="controlQubit">The index of the control qubit.</param>
    /// <param name="targetQubit">The index of the target qubit.</param>
    public void CZ(int controlQubit, int targetQubit)
    {
        ApplyGate(QuantumGates.CZ, [targetQubit, controlQubit]);
    }

    /// <summary>
    /// Applies the CH gate (Controlled-Hadamard) to the specified qubits.
    /// The CH gate applies the Hadamard transformation to the target qubit if the control qubit is in state |1⟩.
    /// </summary>
    /// <param name="controlQubit">The index of the control qubit.</param>
    /// <param name="targetQubit">The index of the target qubit.</param>
    public void CH(int controlQubit, int targetQubit)
    {
        ApplyGate(QuantumGates.CH, [targetQubit, controlQubit]);
    }

    /// <summary>
    /// Applies the SWAP gate to the specified qubits, exchanging their states.
    /// </summary>
    /// <param name="firstQubit">The index of the first qubit.</param>
    /// <param name="secondQubit">The index of the second qubit.</param>
    public void SWAP(int firstQubit, int secondQubit)
    {
        ApplyGate(QuantumGates.SWAP, [secondQubit, firstQubit]);
    }

    /// <summary>
    /// Applies the Toffoli(CCX) gate (Controlled-Controlled-NOT) to the specified qubits.
    /// The Toffoli gate flips the target qubit if both control qubits are in state |1⟩.
    /// </summary>
    /// <param name="firstControlQubit">The index of the first control qubit.</param>
    /// <param name="secondControlQubit">The index of the second control qubit.</param>
    /// <param name="targetQubit">The index of the target qubit to be flipped.</param>
    public void Toffoli(int firstControlQubit, int secondControlQubit, int targetQubit)
    {
        ApplyGate(QuantumGates.Toffoli, [targetQubit, firstControlQubit, secondControlQubit]);
    }

    /// <summary>
    /// Applies the Fredkin gate (Controlled-SWAP) to the specified qubits.
    /// The Fredkin gate swaps the two target qubits if the control qubit is in state |1⟩.
    /// </summary>
    /// <param name="controlQubit">The index of the control qubit.</param>
    /// <param name="firstTargetQubit">The index of the first target qubit to be swapped.</param>
    /// <param name="secondTargetQubit">The index of the second target qubit to be swapped.</param>
    public void Fredkin(int controlQubit, int firstTargetQubit, int secondTargetQubit)
    {
        ApplyGate(QuantumGates.Fredkin, [secondTargetQubit, firstTargetQubit, controlQubit]);
    }
    
    /// <summary>
    /// Applies a single-qubit gate given by a unitary matrix to the specified qubit.
    /// </summary>
    /// <param name="matrix">The 2x2 unitary matrix representing the gate.</param>
    /// <param name="targetQubit">The index of the qubit to which the gate is applied.</param>
    private void ApplyGate(Complex[,] matrix, int targetQubit)
    {
        StateVector = QuantumMath.ApplySingleQubitGate(StateVector, matrix, targetQubit);
    }

    /// <summary>
    /// Applies a multi-qubit gate given by a unitary matrix to the specified qubits.
    /// </summary>
    /// <param name="matrix">The unitary matrix representing the multi-qubit gate.</param>
    /// <param name="targetQubits">An array of indices representing the target qubits.</param>
    private void ApplyGate(Complex[,] matrix, int[] targetQubits)
    {
        if (QubitCount < targetQubits.Length)
        {
            throw new InvalidOperationException($"You cannot apply {targetQubits.Length} qubit gate to {QubitCount} qubit circuit.");    
        }
        
        StateVector = QuantumMath.ApplyMultiQubitGate(StateVector, matrix, targetQubits);
    }
}
