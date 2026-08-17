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
}
