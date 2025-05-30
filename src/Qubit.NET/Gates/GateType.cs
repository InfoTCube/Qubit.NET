namespace Qubit.NET.Gates;

/// <summary>
/// Defines the types of quantum gates supported by the simulator.
/// Includes standard single-qubit, multi-qubit, and measurement operations.
/// </summary>
internal enum GateType
{
    I,
    H,
    X,
    Y,
    Z,
    S,
    Sdag,
    T,
    Tdag,
    Rx,
    Ry,
    Rz,
    SX,
    SY,
    SZ,
    U3,
    CNOT,
    CY,
    CZ,
    CH,
    CRx,
    CRy,
    CRz,
    CU3,
    SWAP,
    Toffoli,
    Fredkin,
    Custom,
    Measure
}