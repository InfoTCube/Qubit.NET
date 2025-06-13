using System.Collections;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Security.AccessControl;
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
    /// Source of randomness used by the simulator.
    /// Defaults to a pseudo-random number generator but can be replaced
    /// with truly random numbers from API or even custom implementation.
    /// </summary>
    public IRandomSource RandomSource { get; set; } = new PseudoRandomSource();

    /// <summary>
    /// A list of quantum gates applied in the circuit.
    /// </summary>
    internal IList<Gate> Gates = new List<Gate>();
    
    /// <summary>
    /// A list of qubit initializations, each represented as a InitialState class.
    /// </summary>
    internal IList<InitialState> Initializations = new List<InitialState>();
    
    /// <summary>
    /// Tracks whether each qubit has been modified (i.e., had a gate applied).
    /// This is used to prevent initialization of qubits after they have been altered,
    /// maintaining consistency with quantum circuit behavior.
    /// </summary>
    private readonly bool[] _isQubitModified;

    /// <summary>
    /// Initializes quantum circuit with specified number of qubits in 0 state.
    /// </summary>
    /// <param name="qubitCount">Number of qubits. (0-30] qubits are supported.</param>
    public QuantumCircuit(int qubitCount)
    {
        if (qubitCount <= 0)
            throw new ArgumentException("Qubit count must be positive.");
        if (qubitCount > 30)
            throw new AggregateException("Qubit count can be at most 30.");
        
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
        Gates = qc.Gates;
        Initializations = qc.Initializations;
        _isQubitModified = qc._isQubitModified;
    }

    /// <summary>
    /// Measures the quantum register, collapsing the state to one of the basis states.
    /// The measurement is a probabilistic process where the state collapses to a classical bit (0 or 1) with respective probabilities.
    /// The method updates the quantum state vector after the measurement, reducing the state to the measured result.
    /// </summary>
    /// <returns>The index of the measured basis state, representing the outcome of the measurement.</returns>
    public string Measure(params int[] qubits)
    {
        foreach (var q in qubits)
            CheckQubit(q);
        
        CheckIfDifferentQubits(qubits);
        
        Gate measure = new Gate()
        {
            GateType = GateType.Measure,
            TargetQubits = qubits.Length == 0 ? Enumerable.Range(0, QubitCount).ToArray() : qubits
        };
        
        Gates.Add(measure);
        
        if (qubits.Length == 0)
        {
            // Perform a measurement by sampling from the current state vector probabilities
            var result = QuantumMath.SampleMeasurement(StateVector, RandomSource);
    
            // Collapse the quantum state to the measured state (collapse the superposition)
            StateVector = QuantumMath.CollapseToState(StateVector, result);
            
            return Convert.ToString(result, 2).PadLeft(QubitCount, '0');
        }
        
        var partialResult = QuantumMath.SamplePartialMeasurement(StateVector, qubits, RandomSource);
        StateVector = QuantumMath.CollapseToPartialMeasurement(StateVector, qubits, partialResult);
            
        return Convert.ToString(partialResult, 2).PadLeft(qubits.Length, '0');
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

        Initialize(qubit, alpha, beta, state);
    }
    
    /// <summary>
    /// Initializes the specified qubit to an arbitrary single-qubit state defined by amplitudes α and β.
    /// </summary>
    /// <param name="qubit">The index of the qubit to initialize.</param>
    /// <param name="alpha">Amplitude for the |0⟩ component of the qubit.</param>
    /// <param name="beta">Amplitude for the |1⟩ component of the qubit.</param>
    /// /// <param name="state">Optional description of the state.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if the qubit index is out of range.
    /// </exception>
    public void Initialize(int qubit, Complex alpha, Complex beta, State state = State.Custom)
    {
        CheckQubit(qubit);
        
        if (_isQubitModified[qubit])
            throw new InvalidOperationException($"Qubit {qubit} has already been modified and cannot be re-initialized.");

        StateVector = QuantumMath.InitializeState(StateVector, qubit, alpha, beta);
        
        _isQubitModified[qubit] = true;

        InitialState initial = new InitialState()
        {
            QubitIndex = qubit,
            Alpha = alpha,
            Beta = beta,
            BasicState = state
        };
        
        Initializations.Add(initial);
    }

    /// <summary>
    /// Applies the Identity gate (I) to the specified qubit.
    /// The Identity Gate leaves the qubit's state unchanged, essentially performing no operation on it.
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the Identity gate to.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if the qubit index is out of range.
    /// </exception>
    public void I(int qubit)
    {
        CheckQubit(qubit);
        
        Gate iGate = new Gate
        {
            GateType = GateType.I,
            Matrix = QuantumGates.I,
            TargetQubits = [qubit]
        };
        
        Gates.Add(iGate);
        
        ApplyGate(QuantumGates.I, qubit);
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
    /// Applies the Rx gate to the specified qubit.
    /// The Rx gate rotates the state of the target qubit around the x-axis of the Bloch sphere by the given angle θ.
    /// The matrix representation of the Rx gate is used to modify the qubit's state accordingly.
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the Rx gate to.</param>
    /// <param name="theta">The angle (in radians) by which to rotate the qubit's state around the x-axis.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if the qubit index is out of range, indicating that the specified qubit does not exist in the system.
    /// </exception>
    public void Rx(int qubit, double theta)
    {
        CheckQubit(qubit);
        
        Gate rxGate = new Gate
        {
            GateType = GateType.Rx,
            Matrix = QuantumGates.Rx(theta),
            TargetQubits = [qubit]
        };
        
        Gates.Add(rxGate);
        
        ApplyGate(QuantumGates.Rx(theta), qubit);
    }
    
    /// <summary>
    /// Applies the Ry gate to the specified qubit.
    /// The Ry gate rotates the state of the target qubit around the y-axis of the Bloch sphere by the given angle θ.
    /// The matrix representation of the Ry gate is used to modify the qubit's state accordingly.
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the Ry gate to.</param>
    /// <param name="theta">The angle (in radians) by which to rotate the qubit's state around the y-axis.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if the qubit index is out of range, indicating that the specified qubit does not exist in the system.
    /// </exception>
    public void Ry(int qubit, double theta)
    {
        CheckQubit(qubit);
        
        Gate ryGate = new Gate
        {
            GateType = GateType.Ry,
            Matrix = QuantumGates.Ry(theta),
            TargetQubits = [qubit]
        };
        
        Gates.Add(ryGate);
        
        ApplyGate(QuantumGates.Ry(theta), qubit);
    }

    /// <summary>
    /// Applies the Rz gate to the specified qubit.
    /// The Rz gate rotates the state of the target qubit around the z-axis of the Bloch sphere by the given angle θ.
    /// The matrix representation of the Rz gate is used to modify the qubit's state accordingly.
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the Rz gate to.</param>
    /// <param name="theta">The angle (in radians) by which to rotate the qubit's state around the z-axis.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if the qubit index is out of range, indicating that the specified qubit does not exist in the system.
    /// </exception>
    public void Rz(int qubit, double theta)
    {
        CheckQubit(qubit);
        
        Gate rzGate = new Gate
        {
            GateType = GateType.Rz,
            Matrix = QuantumGates.Rz(theta),
            TargetQubits = [qubit]
        };
        
        Gates.Add(rzGate);
        
        ApplyGate(QuantumGates.Rz(theta), qubit);
    }
    
    /// <summary>
    /// Applies the square-root of Pauli-X gate (SX) to the specified qubit.
    /// The SX gate performs a rotation of π/2 around the X-axis on the Bloch sphere, 
    /// acting as the square root of the X (NOT) gate.
    /// When applied twice, it is equivalent to the Pauli-X gate.
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the SX gate to.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if the qubit index is out of range.
    /// </exception>
    public void SX(int qubit)
    {
        CheckQubit(qubit);
        
        Gate sxGate = new Gate
        {
            GateType = GateType.SX,
            Matrix = QuantumGates.SX,
            TargetQubits = [qubit]
        };
        
        Gates.Add(sxGate);
        
        ApplyGate(QuantumGates.SX, qubit);
    }
    
    /// <summary>
    /// Applies the square-root of Pauli-Y gate (SY) to the specified qubit.
    /// The SY gate performs a rotation of π/2 around the Y-axis on the Bloch sphere, 
    /// acting as the square root of the Y gate.
    /// When applied twice, it is equivalent to the Pauli-Y gate.
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the SY gate to.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if the qubit index is out of range.
    /// </exception>
    public void SY(int qubit)
    {
        CheckQubit(qubit);
        
        Gate syGate = new Gate
        {
            GateType = GateType.SY,
            Matrix = QuantumGates.SY,
            TargetQubits = [qubit]
        };
        
        Gates.Add(syGate);
        
        ApplyGate(QuantumGates.SY, qubit);
    }
    
    /// <summary>
    /// Applies the square-root of Pauli-Z gate (SZ), also known as the S gate, to the specified qubit.
    /// The SZ gate performs a π/2 rotation around the Z-axis on the Bloch sphere,
    /// acting as the square root of the Z gate.
    /// When applied twice, it is equivalent to the Pauli-Z gate.
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the SZ gate to.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if the qubit index is out of range.
    /// </exception>
    public void SZ(int qubit)
    {
        CheckQubit(qubit);
        
        Gate syGate = new Gate
        {
            GateType = GateType.SZ,
            Matrix = QuantumGates.SZ,
            TargetQubits = [qubit]
        };
        
        Gates.Add(syGate);
        
        ApplyGate(QuantumGates.SZ, qubit);
    }
    
    /// <summary>
    /// Applies the U3 gate to the specified qubit.
    /// The U3 gate is the most general single-qubit gate, capable of performing any rotation
    /// on the Bloch sphere using three angles: θ (theta), φ (phi), and λ (lambda).
    /// The matrix representation of the U3 gate is used to modify the qubit's state accordingly.
    /// </summary>
    /// <param name="qubit">The index of the qubit to apply the U3 gate to.</param>
    /// <param name="theta">The rotation angle (in radians) around the x-axis and z-axis combined.</param>
    /// <param name="phi">The phase angle (in radians) applied before the rotation.</param>
    /// <param name="lambda">The phase angle (in radians) applied after the rotation.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if the qubit index is out of range, indicating that the specified qubit does not exist in the system.
    /// </exception>
    public void U3(int qubit, double theta, double phi, double lambda)
    {
        CheckQubit(qubit);
        
        Gate u3Gate = new Gate
        {
            GateType = GateType.U3,
            Matrix = QuantumGates.U3(theta, phi, lambda),
            TargetQubits = [qubit]
        };
        
        Gates.Add(u3Gate);
        
        ApplyGate(QuantumGates.U3(theta, phi, lambda), qubit);
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
    /// Applies the CRx gate (Controlled-Rx) to the specified qubits.
    /// The CRx gate applies the Rx to the target qubit if the control qubit is in state |1⟩.
    /// </summary>
    /// <param name="controlQubit">The index of the control qubit.</param>
    /// <param name="targetQubit">The index of the target qubit.</param>
    /// <param name="theta">The angle (in radians) by which to rotate the qubit's state around the x-axis.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if any of the qubit indices are out of range.
    /// </exception>
    public void CRx(int controlQubit, int targetQubit, double theta)
    {
        CheckQubit(controlQubit);
        CheckQubit(targetQubit);
        
        Gate crxGate = new Gate
        {
            GateType = GateType.CRx,
            Matrix = QuantumGates.CRx(theta),
            TargetQubits = [targetQubit],
            ControlQubits = [controlQubit]
        };
        
        Gates.Add(crxGate);
        
        ApplyGate(QuantumGates.CRx(theta), [targetQubit, controlQubit]);
    }
    
    /// <summary>
    /// Applies the CRy gate (Controlled-Ry) to the specified qubits.
    /// The CRy gate applies the Ry to the target qubit if the control qubit is in state |1⟩.
    /// </summary>
    /// <param name="controlQubit">The index of the control qubit.</param>
    /// <param name="targetQubit">The index of the target qubit.</param>
    /// <param name="theta">The angle (in radians) by which to rotate the qubit's state around the y-axis.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if any of the qubit indices are out of range.
    /// </exception>
    public void CRy(int controlQubit, int targetQubit, double theta)
    {
        CheckQubit(controlQubit);
        CheckQubit(targetQubit);
        
        Gate cryGate = new Gate
        {
            GateType = GateType.CRy,
            Matrix = QuantumGates.CRy(theta),
            TargetQubits = [targetQubit],
            ControlQubits = [controlQubit]
        };
        
        Gates.Add(cryGate);
        
        ApplyGate(QuantumGates.CRy(theta), [targetQubit, controlQubit]);
    }
    
    /// <summary>
    /// Applies the CRz gate (Controlled-Rz) to the specified qubits.
    /// The CRz gate applies the Rz to the target qubit if the control qubit is in state |1⟩.
    /// </summary>
    /// <param name="controlQubit">The index of the control qubit.</param>
    /// <param name="targetQubit">The index of the target qubit.</param>
    /// <param name="theta">The angle (in radians) by which to rotate the qubit's state around the z-axis.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if any of the qubit indices are out of range.
    /// </exception>
    public void CRz(int controlQubit, int targetQubit, double theta)
    {
        CheckQubit(controlQubit);
        CheckQubit(targetQubit);
        
        Gate crzGate = new Gate
        {
            GateType = GateType.CRz,
            Matrix = QuantumGates.CRz(theta),
            TargetQubits = [targetQubit],
            ControlQubits = [controlQubit]
        };
        
        Gates.Add(crzGate);
        
        ApplyGate(QuantumGates.CRz(theta), [targetQubit, controlQubit]);
    }
    
    /// <summary>
    /// Applies the CU3 gate to the specified qubits.
    /// The CU3 gate applies the U3 to the target qubit if the control qubit is in state |1⟩.
    /// </summary>
    /// <param name="controlQubit">The index of the control qubit.</param>
    /// <param name="targetQubit">The index of the target qubit.</param>
    /// <param name="theta">The rotation angle (in radians) around the x-axis and z-axis combined.</param>
    /// <param name="phi">The phase angle (in radians) applied before the rotation.</param>
    /// <param name="lambda">The phase angle (in radians) applied after the rotation.</param>
    /// <exception cref="QubitIndexOutOfRangeException">
    /// Thrown if any of the qubit indices are out of range.
    /// </exception>
    public void CU3(int controlQubit, int targetQubit, double theta, double phi, double lambda)
    {
        CheckQubit(controlQubit);
        CheckQubit(targetQubit);
        
        Gate cu3Gate = new Gate
        {
            GateType = GateType.CU3,
            Matrix = QuantumGates.CU3(theta, phi, lambda),
            TargetQubits = [targetQubit],
            ControlQubits = [controlQubit]
        };
        
        Gates.Add(cu3Gate);
        
        ApplyGate(QuantumGates.CU3(theta, phi, lambda), [targetQubit, controlQubit]);
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
    /// Applies a custom quantum gate defined by a unitary matrix to the specified target qubits.
    /// </summary>
    /// <param name="matrix">
    /// A unitary matrix of size 2ⁿ×2ⁿ representing the quantum operation, where n is the number of target qubits (1 to 4).
    /// </param>
    /// <param name="qubits">
    /// Indices of the target qubits to which the custom gate is applied. Must contain 1 to 4 distinct qubit indices.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if the number of qubits is not between 1 and 4, if qubit indices are not distinct,
    /// if the matrix dimensions do not match 2ⁿ×2ⁿ, or if the matrix is not unitary.
    /// </exception>
    public void Custom(Complex[,] matrix, params int[] qubits)
    {
        int numQubits = qubits.Length;
        if (numQubits < 1 || numQubits > 4)
        {
            throw new ArgumentException("Custom gate can only be applied to 1 to 4 qubits.");
        }

        foreach (int q in qubits)
            CheckQubit(q);
        
        CheckIfDifferentQubits(qubits);
        
        int expectedMatrixSize = 1 << numQubits; // 2^n
        if (matrix.GetLength(0) != expectedMatrixSize || matrix.GetLength(1) != expectedMatrixSize)
        {
            throw new ArgumentException($"Matrix size must be {expectedMatrixSize}x{expectedMatrixSize} for {numQubits} qubits.");
        }
        
        if (!QuantumMath.IsUnitary(matrix))
        {
            throw new ArgumentException("The provided matrix is not unitary.");
        }
        
        Gate customGate = new Gate
        {
            GateType = GateType.Custom,
            Matrix = matrix,
            TargetQubits = qubits
        };
        
        Gates.Add(customGate);
        
        ApplyGate(matrix, qubits.Reverse().ToArray());
    }

    /// <summary>
    /// Converts a quantum state vector represented as an array of complex numbers into a string representation.
    /// The format of the string is: a |00⟩ + b |01⟩ + c |10⟩ + d |11⟩, where the coefficients are complex numbers.
    /// </summary>
    /// <returns>A string representation of the quantum state vector in the form: a |00⟩ + b |01⟩ + c |10⟩ + d |11⟩.</returns>
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        bool any = false;
        
        for (int i = 0; i < StateVector.Length; i++)
        {
            double realPart = StateVector[i].Real;
            double imaginaryPart = StateVector[i].Imaginary;
            
            string amplitude = Helpers.FormatComplex(realPart, imaginaryPart);
            
            if (string.IsNullOrEmpty(amplitude)) continue;

            string binaryState = Convert.ToString(i, 2).PadLeft(QubitCount, '0');
            
            if (any) sb.Append(" + ");
            if(amplitude == "1") amplitude = string.Empty;
            
            sb.Append(amplitude + "|" + binaryState + ">");
            any = true;
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
