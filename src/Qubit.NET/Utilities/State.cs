namespace Qubit.NET.Utilities;

/// <summary>
/// A predefined single-qubit basis state that a qubit can be initialized to.
/// </summary>
public enum State
{
    /// <summary>The |0⟩ state.</summary>
    Zero,

    /// <summary>The |1⟩ state.</summary>
    One,

    /// <summary>The |+⟩ state, (|0⟩ + |1⟩)/√2.</summary>
    Plus,

    /// <summary>The |−⟩ state, (|0⟩ − |1⟩)/√2.</summary>
    Minus,

    /// <summary>An arbitrary state given by explicit α and β amplitudes.</summary>
    Custom
}
