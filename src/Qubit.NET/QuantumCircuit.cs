﻿using System.Numerics;
using System.Text;
using Qubit.NET.Gates;
using Qubit.NET.Math;
using Qubit.NET.Utilities;

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
    /// A list of quantum gates applied in the circuit.
    /// </summary>
    internal IList<Gate> Gates = new List<Gate>();
    
    /// <summary>
    /// A list of qubit initializations, each represented as a tuple:
    /// (qubit index, amplitude of |0⟩, amplitude of |1⟩).
    /// </summary>
    internal IList<Tuple<int, Complex, Complex>> Initializations = new List<Tuple<int, Complex, Complex>>();
    
    /// <summary>
    /// Tracks whether each qubit has been modified (i.e., had a gate applied).
    /// This is used to prevent initialization of qubits after they have been altered,
    /// maintaining consistency with quantum circuit behavior.
    /// </summary>
    private readonly bool[] _isQubitModified;

    /// <summary>
    /// Initializes quantum circuit with specified number of qubits in 0 state.
    /// </summary>
    /// <param name="qubitCount">Number of qubits. (0-100] qubits are supported.</param>
    public QuantumCircuit(int qubitCount)
    {
        if (qubitCount <= 0)
            throw new ArgumentException("Qubit count must be positive.");
        if (qubitCount > 100)
            throw new AggregateException("Qubit count can be at most 100.");
        
        QubitCount = qubitCount;
        StateVector = new Complex[1 << qubitCount];
        StateVector[0] = new Complex(1, 0);
        _isQubitModified = new bool[qubitCount];
    }

    /// <summary>
    /// Creates a new instance of the <see cref="QuantumCircuit"/> class by copying the properties of the provided <paramref name="qc"/> object.
    /// Initializes the new quantum circuit with the same number of qubits and state vector as the original.
    /// This constructor creates a deep copy, ensuring the new instance is independent of the original.
    /// </summary>
    /// <param name="qc">The <see cref="QuantumCircuit"/> object to copy.</param>
    public QuantumCircuit(QuantumCircuit qc)
    {
        QubitCount = qc.QubitCount;
        StateVector = qc.StateVector;
        _isQubitModified = qc._isQubitModified;
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

        Gate measure = new Gate()
        {
            GateType = GateType.Measure
        };
        
        Gates.Add(measure);
        
        return Convert.ToString(result, 2).PadLeft(QubitCount, '0');
    }

    /// <summary>
    /// Initializes the specified qubit to a predefined basis state: |0⟩, |1⟩, |+⟩, or |−⟩.
    /// This operation modifies the quantum state to reflect the chosen single-qubit state.
    /// </summary>
    /// <param name="qubit">The index of the qubit to initialize.</param>
    /// <param name="state">The desired single-qubit state to initialize to.</param>
    /// <exception cref="ArgumentException">Thrown when an unknown state is provided.</exception>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if the qubit index is out of range.
    /// </exception>
    public void Initialize(int qubit, State state)
    {
        Complex alpha, beta;

        switch (state)
        {
            case State.Zero:
                alpha = Complex.One;
                beta = Complex.Zero;
                break;
            case State.One:
                alpha = Complex.Zero;
                beta = Complex.One;
                break;
            case State.Plus:
                alpha = beta = new Complex(1 / System.Math.Sqrt(2), 0);
                break;
            case State.Minus:
                alpha = new Complex(1 / System.Math.Sqrt(2), 0);
                beta = new Complex(-1 / System.Math.Sqrt(2), 0);
                break;
            default:
                throw new ArgumentException("Unknown basis state.");
        }

        Initialize(qubit, alpha, beta);
    }
    
    /// <summary>
    /// Initializes the specified qubit to an arbitrary single-qubit state defined by amplitudes α and β.
    /// </summary>
    /// <param name="qubit">The index of the qubit to initialize.</param>
    /// <param name="alpha">Amplitude for the |0⟩ component of the qubit.</param>
    /// <param name="beta">Amplitude for the |1⟩ component of the qubit.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if the qubit index is out of range.
    /// </exception>
    public void Initialize(int qubit, Complex alpha, Complex beta)
    {
        CheckQubit(qubit);
        
        if (_isQubitModified[qubit])
            throw new InvalidOperationException($"Qubit {qubit} has already been modified and cannot be re-initialized.");

        StateVector = QuantumMath.InitializeState(StateVector, qubit, alpha, beta);
        
        _isQubitModified[qubit] = true;
        Initializations.Add(new Tuple<int, Complex, Complex>(qubit, alpha, beta));
    }

    /// <summary>
    /// Applies the Hadamard gate (H) to the specified qubit.
    /// The Hadamard gate creates a superposition of the basis states.
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the Hadamard gate to.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if the qubit index is out of range.
    /// </exception>
    public void H(int qubit)
    {
        CheckQubit(qubit);
        
        Gate hGate = new Gate
        {
            GateType = GateType.H,
            Matrix = QuantumGates.H,
            TargetQubits = [qubit]
        };
        
        Gates.Add(hGate);
        
        ApplyGate(QuantumGates.H, qubit);
    }

    /// <summary>
    /// Applies the Pauli-X gate (X) to the specified qubit.
    /// The Pauli-X gate flips the state of the qubit (|0> ↔ |1>).
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the Pauli-X gate to.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if the qubit index is out of range.
    /// </exception>
    public void X(int qubit)
    {
        CheckQubit(qubit);
        
        Gate xGate = new Gate
        {
            GateType = GateType.X,
            Matrix = QuantumGates.X,
            TargetQubits = [qubit]
        };
        
        Gates.Add(xGate);
        
        ApplyGate(QuantumGates.X, qubit);
    }

    /// <summary>
    /// Applies the Pauli-Y gate (Y) to the specified qubit.
    /// The Pauli-Y gate performs a bit-flip followed by a phase flip on the qubit.
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the Pauli-Y gate to.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if the qubit index is out of range.
    /// </exception>
    public void Y(int qubit)
    {
        CheckQubit(qubit);
        
        Gate yGate = new Gate
        {
            GateType = GateType.Y,
            Matrix = QuantumGates.Y,
            TargetQubits = [qubit]
        };
        
        Gates.Add(yGate);
        
        ApplyGate(QuantumGates.Y, qubit);
    }

    /// <summary>
    /// Applies the Pauli-Z gate (Z) to the specified qubit.
    /// The Pauli-Z gate applies a phase flip to the qubit (|0> ↔ |1> with a phase of -1 for |1>).
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the Pauli-Z gate to.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if the qubit index is out of range.
    /// </exception>
    public void Z(int qubit)
    {
        CheckQubit(qubit);
        
        Gate zGate = new Gate
        {
            GateType = GateType.Z,
            Matrix = QuantumGates.Z,
            TargetQubits = [qubit]
        };
        
        Gates.Add(zGate);
        
        ApplyGate(QuantumGates.Z, qubit);
    }

    /// <summary>
    /// Applies the S gate (phase gate) to the specified qubit.
    /// The S gate applies a phase shift of π/2 to the qubit, leaving |0⟩ unchanged and mapping |1⟩ to i·|1⟩.
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the S gate to.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if the qubit index is out of range.
    /// </exception>
    public void S(int qubit)
    {
        CheckQubit(qubit);
        
        Gate sGate = new Gate
        {
            GateType = GateType.S,
            Matrix = QuantumGates.S,
            TargetQubits = [qubit]
        };
        
        Gates.Add(sGate);
        
        ApplyGate(QuantumGates.S, qubit);
    }

    /// <summary>
    /// Applies the S† gate (inverse phase gate) to the specified qubit.
    /// The S† gate undoes the effect of the S gate by applying a phase shift of -π/2,
    /// leaving |0⟩ unchanged and mapping |1⟩ to -i·|1⟩.
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the S† gate to.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if the qubit index is out of range.
    /// </exception>
    public void Sdag(int qubit)
    {
        CheckQubit(qubit);
        
        Gate sDagGate = new Gate
        {
            GateType = GateType.Sdag,
            Matrix = QuantumGates.Sdag,
            TargetQubits = [qubit]
        };
        
        Gates.Add(sDagGate);
        
        ApplyGate(QuantumGates.Sdag, qubit);
    }

    /// <summary>
    /// Applies the T gate (π/4 phase gate) to the specified qubit.
    /// The T gate applies a phase shift of π/4 to the qubit, leaving |0⟩ unchanged and mapping |1⟩ to e^(iπ/4)·|1⟩.
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the T gate to.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if the qubit index is out of range.
    /// </exception>
    public void T(int qubit)
    {
        CheckQubit(qubit);
        
        Gate tGate = new Gate
        {
            GateType = GateType.T,
            Matrix = QuantumGates.T,
            TargetQubits = [qubit]
        };
        
        Gates.Add(tGate);
        
        ApplyGate(QuantumGates.T, qubit);
    }

    /// <summary>
    /// Applies the T† gate (inverse of the T gate) to the specified qubit.
    /// The T† gate reverses the π/4 phase shift applied by the T gate,
    /// leaving |0⟩ unchanged and mapping |1⟩ to e^(-iπ/4)·|1⟩.
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the T† gate to.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if the qubit index is out of range.
    /// </exception>
    public void Tdag(int qubit)
    {
        CheckQubit(qubit);
        
        Gate tDagGate = new Gate
        {
            GateType = GateType.Tdag,
            Matrix = QuantumGates.Tdag,
            TargetQubits = [qubit]
        };
        
        Gates.Add(tDagGate);
        
        ApplyGate(QuantumGates.Tdag, qubit);
    }

    /// <summary>
    /// Applies the CNOT (also called CX) gate (Controlled-NOT) to the specified qubits.
    /// The CNOT gate flips the target qubit if the control qubit is in state |1>.
    /// </summary>
    /// <param name="controlQubit">The index of the control qubit.</param>
    /// <param name="targetQubit">The index of the target qubit.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if any of the qubit indices are out of range.
    /// </exception>
    public void CNOT(int controlQubit, int targetQubit)
    {
        CheckQubit(controlQubit);
        CheckQubit(targetQubit);
        
        Gate cnotGate = new Gate
        {
            GateType = GateType.CNOT,
            Matrix = QuantumGates.CNOT,
            TargetQubits = [targetQubit],
            ControlQubits = [controlQubit]
        };
        
        Gates.Add(cnotGate);
        
        ApplyGate(QuantumGates.CNOT, [targetQubit, controlQubit]);
    }

    /// <summary>
    /// Applies the CY gate (Controlled-Y) to the specified qubits.
    /// The CY gate applies the Pauli-Y operation to the target qubit if the control qubit is in state |1⟩.
    /// </summary>
    /// <param name="controlQubit">The index of the control qubit.</param>
    /// <param name="targetQubit">The index of the target qubit.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if any of the qubit indices are out of range.
    /// </exception>
    public void CY(int controlQubit, int targetQubit)
    {
        CheckQubit(controlQubit);
        CheckQubit(targetQubit);
        
        Gate cyGate = new Gate
        {
            GateType = GateType.CY,
            Matrix = QuantumGates.CY,
            TargetQubits = [targetQubit],
            ControlQubits = [controlQubit]
        };
        
        Gates.Add(cyGate);
        
        ApplyGate(QuantumGates.CY, [targetQubit, controlQubit]);
    }

    /// <summary>
    /// Applies the CZ gate (Controlled-Z) to the specified qubits.
    /// The CZ gate applies the Pauli-Z operation to the target qubit if the control qubit is in state |1⟩.
    /// </summary>
    /// <param name="controlQubit">The index of the control qubit.</param>
    /// <param name="targetQubit">The index of the target qubit.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if any of the qubit indices are out of range.
    /// </exception>
    public void CZ(int controlQubit, int targetQubit)
    {
        CheckQubit(controlQubit);
        CheckQubit(targetQubit);
        
        Gate czGate = new Gate
        {
            GateType = GateType.CZ,
            Matrix = QuantumGates.CZ,
            TargetQubits = [targetQubit],
            ControlQubits = [controlQubit]
        };
        
        Gates.Add(czGate);
        
        ApplyGate(QuantumGates.CZ, [targetQubit, controlQubit]);
    }

    /// <summary>
    /// Applies the CH gate (Controlled-Hadamard) to the specified qubits.
    /// The CH gate applies the Hadamard transformation to the target qubit if the control qubit is in state |1⟩.
    /// </summary>
    /// <param name="controlQubit">The index of the control qubit.</param>
    /// <param name="targetQubit">The index of the target qubit.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if any of the qubit indices are out of range.
    /// </exception>
    public void CH(int controlQubit, int targetQubit)
    {
        CheckQubit(controlQubit);
        CheckQubit(targetQubit);
        
        Gate chGate = new Gate
        {
            GateType = GateType.CH,
            Matrix = QuantumGates.CH,
            TargetQubits = [targetQubit],
            ControlQubits = [controlQubit]
        };
        
        Gates.Add(chGate);
        
        ApplyGate(QuantumGates.CH, [targetQubit, controlQubit]);
    }

    /// <summary>
    /// Applies the SWAP gate to the specified qubits, exchanging their states.
    /// </summary>
    /// <param name="firstQubit">The index of the first qubit.</param>
    /// <param name="secondQubit">The index of the second qubit.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if any of the qubit indices are out of range.
    /// </exception>
    public void SWAP(int firstQubit, int secondQubit)
    {
        CheckQubit(firstQubit);
        CheckQubit(secondQubit);
        
        Gate swapGate = new Gate
        {
            GateType = GateType.SWAP,
            Matrix = QuantumGates.SWAP,
            TargetQubits = [secondQubit, firstQubit],
        };
        
        Gates.Add(swapGate);
        
        ApplyGate(QuantumGates.SWAP, [secondQubit, firstQubit]);
    }

    /// <summary>
    /// Applies the Toffoli(CCX) gate (Controlled-Controlled-NOT) to the specified qubits.
    /// The Toffoli gate flips the target qubit if both control qubits are in state |1⟩.
    /// </summary>
    /// <param name="firstControlQubit">The index of the first control qubit.</param>
    /// <param name="secondControlQubit">The index of the second control qubit.</param>
    /// <param name="targetQubit">The index of the target qubit to be flipped.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if any of the qubit indices are out of range.
    /// </exception>
    public void Toffoli(int firstControlQubit, int secondControlQubit, int targetQubit)
    {
        CheckQubit(firstControlQubit);
        CheckQubit(secondControlQubit);
        CheckQubit(targetQubit);
        
        Gate toffoliGate = new Gate
        {
            GateType = GateType.Toffoli,
            Matrix = QuantumGates.Toffoli,
            TargetQubits = [targetQubit],
            ControlQubits = [secondControlQubit, firstControlQubit]
        };
        
        Gates.Add(toffoliGate);
        
        ApplyGate(QuantumGates.Toffoli, [targetQubit, firstControlQubit, secondControlQubit]);
    }

    /// <summary>
    /// Applies the Fredkin gate (Controlled-SWAP) to the specified qubits.
    /// The Fredkin gate swaps the two target qubits if the control qubit is in state |1⟩.
    /// </summary>
    /// <param name="controlQubit">The index of the control qubit.</param>
    /// <param name="firstTargetQubit">The index of the first target qubit to be swapped.</param>
    /// <param name="secondTargetQubit">The index of the second target qubit to be swapped.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if any of the qubit indices are out of range.
    /// </exception>
    public void Fredkin(int controlQubit, int firstTargetQubit, int secondTargetQubit)
    {
        CheckQubit(controlQubit);
        CheckQubit(firstTargetQubit);
        CheckQubit(secondTargetQubit);
        
        Gate fredkinGate = new Gate
        {
            GateType = GateType.Fredkin,
            Matrix = QuantumGates.Fredkin,
            TargetQubits = [secondTargetQubit, firstTargetQubit],
            ControlQubits = [controlQubit]
        };
        
        Gates.Add(fredkinGate);
        
        ApplyGate(QuantumGates.Fredkin, [secondTargetQubit, firstTargetQubit, controlQubit]);
    }

    /// <summary>
    /// Converts a quantum state vector represented as an array of complex numbers into a string representation.
    /// The format of the string is: a |00⟩ + b |01⟩ + c |10⟩ + d |11⟩, where the coefficients are complex numbers.
    /// </summary>
    /// <returns>A string representation of the quantum state vector in the form: a |00⟩ + b |01⟩ + c |10⟩ + d |11⟩.</returns>
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < StateVector.Length; i++)
        {
            double realPart = StateVector[i].Real;
            double imaginaryPart = StateVector[i].Imaginary;
            
            string amplitude = Helpers.FormatComplex(realPart, imaginaryPart);
            
            if (string.IsNullOrEmpty(amplitude)) continue;

            string binaryState = Convert.ToString(i, 2).PadLeft(QubitCount, '0');
            
            if (i > 0)
            {
                sb.Append(" + ");
            }
            sb.Append(amplitude + "|" + binaryState + ">");
        }

        return sb.ToString();
    }
    
    /// <summary>
    /// Applies a single-qubit gate given by a unitary matrix to the specified qubit.
    /// </summary>
    /// <param name="matrix">The 2x2 unitary matrix representing the gate.</param>
    /// <param name="targetQubit">The index of the qubit to which the gate is applied.</param>
    private void ApplyGate(Complex[,] matrix, int targetQubit)
    {
        StateVector = QuantumMath.ApplySingleQubitGate(StateVector, matrix, targetQubit);
        
        _isQubitModified[targetQubit] = true;
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
        
        CheckIfDifferentQubits(targetQubits);
        
        StateVector = QuantumMath.ApplyMultiQubitGate(StateVector, matrix, targetQubits);

        foreach (var qubit in targetQubits)
            _isQubitModified[qubit] = true;
    }

    /// <summary>
    /// Checks if the specified qubit index is within the valid range [0, QubitCount).
    /// Throws a <see cref="QubitIndexOutOfRangeException"/> if the index is out of range.
    /// </summary>
    /// <param name="qubit">The index of the qubit to check.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown when the qubit index is less than 0 or greater than or equal to <see cref="QubitCount"/>.
    /// </exception>
    private void CheckQubit(int qubit)
    {
        if (qubit < 0 || qubit >= QubitCount)
            throw new QubitIndexOutOfRangeException($"Invalid index ({qubit}): qubit index must be between [0 and {QubitCount})");
    }
    
    /// <summary>
    /// Checks if all qubits in the provided array are distinct.
    /// This method ensures that the user applies quantum gates to different qubits, 
    /// and throws an exception if any qubit is repeated in the array.
    /// </summary>
    /// <param name="qubits">An array of qubits to apply quantum gate to.</param>
    /// <exception cref="ArgumentException">Thrown when a duplicate qubit is found in the array, indicating that the gate would be applied to the same qubit at different positions.</exception>
    private void CheckIfDifferentQubits(int[] qubits)
    {
        HashSet<int> seen = new HashSet<int>();
    
        foreach (int qubit in qubits)
        {
            if (!seen.Add(qubit))
                throw new ArgumentException("Every gate argument must be a different Qubit");
        }
    }
}
