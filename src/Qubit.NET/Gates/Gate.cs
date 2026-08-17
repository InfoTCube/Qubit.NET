using System.Numerics;

namespace Qubit.NET.Gates;

/// <summary>
/// Represents a quantum gate used in a quantum circuit simulation.
/// Encapsulates the gate type, matrix representation, target qubits,
/// and optional control qubits for controlled operations.
/// </summary>
internal class Gate
{
    public GateType GateType { get; set; }

    /// <summary>
    /// The gate's unitary matrix. Null only for <see cref="GateType.Measure"/>, which is
    /// not a unitary operation.
    /// </summary>
    public Complex[,]? Matrix { get; set; }

    /// <summary>Qubits the gate acts on. Empty is never valid for a recorded gate.</summary>
    public int[] TargetQubits { get; set; } = [];

    /// <summary>Control qubits, empty for uncontrolled gates.</summary>
    public int[] ControlQubits { get; set; } = [];

    /// <summary>
    /// Classical bit this gate is conditioned on, or null if it always runs.
    /// Set by <see cref="QuantumCircuit.When"/> to express classical feedforward.
    /// </summary>
    public int? ConditionBit { get; set; }

    /// <summary>
    /// Value <see cref="ConditionBit"/> must hold for the gate to run.
    /// </summary>
    public int ConditionValue { get; set; }

    /// <summary>
    /// For measurement gates, the classical bit each measured qubit writes into,
    /// positionally matching <see cref="TargetQubits"/>.
    /// </summary>
    public int[] ClassicalBits { get; set; } = [];

    /// <summary>
    /// The angles a parameterized gate was built from, in declaration order. Kept because
    /// recovering them from the matrix afterwards is lossy around wrapping and sign.
    /// </summary>
    public double[] Parameters { get; set; } = [];
}
