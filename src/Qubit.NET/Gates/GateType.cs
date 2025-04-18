namespace Qubit.NET.Gates;

/// <summary>
/// Defines the types of quantum gates supported by the simulator.
/// Includes standard single-qubit, multi-qubit, and measurement operations.
/// </summary>
internal enum GateType
{
    H,
    X,
    Y,
    Z,
    S,
    Sdag,
    T,
    Tdag,
    CNOT,
    CY,
    CZ,
    CH,
    SWAP,
    Toffoli,
    Fredkin,
    Measure
}