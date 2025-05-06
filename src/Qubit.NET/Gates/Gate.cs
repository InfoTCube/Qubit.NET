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
    public Complex[,]? Matrix { get; set; }
    public int[]? TargetQubits { get; set; }
    public int[]? ControlQubits { get; set; }
}