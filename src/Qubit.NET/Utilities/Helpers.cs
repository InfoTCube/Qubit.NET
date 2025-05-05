using Qubit.NET.Gates;

namespace Qubit.NET.Utilities;

/// <summary>
/// Provides utility methods for working with complex numbers.
/// </summary>
internal static class Helpers
{
    /// <summary>
    /// Formats a complex number as a string. If the imaginary part is zero, only the real part is included. 
    /// If the real part is zero, only the imaginary part is included. Both parts are included if they are non-zero.
    /// </summary>
    /// <param name="real">The real part of the complex number.</param>
    /// <param name="imaginary">The imaginary part of the complex number.</param>
    /// <returns>A string representation of the complex number, in the form: "real + imaginary*i" or "real" or "imaginary*i".</returns>
    internal static string FormatComplex(double real, double imaginary)
    {
        if (imaginary == 0 && real == 0)
            return String.Empty;
        
        if (imaginary == 0)
            return real.ToString();
        
        if (real == 0)
            return $"{imaginary}i";
        
        return $"{real} + {imaginary}i";
    }

    /// <summary>
    /// Returns a string array representation of a quantum gate, where each element corresponds
    /// to the symbol that should be displayed on a specific qubit line in a circuit diagram.
    /// The representation includes control symbols (●), targets (e.g., X, H), and measurements (M),
    /// and is ordered according to the gate's multi-qubit structure.
    /// </summary>
    /// <param name="gateType">The type of the quantum gate.</param>
    /// <returns>
    /// An array of strings representing the visual components of the gate
    /// for use in a circuit drawing. Each string corresponds to one qubit line.
    /// </returns>
    internal static string[] GateTypeToCharRepresentation(GateType gateType)
    {
        return gateType switch
        {
            GateType.I => ["I"],
            GateType.H => ["H"],
            GateType.X => ["X"],
            GateType.Y => ["Y"],
            GateType.Z => ["Z"],
            GateType.S => ["S"],
            GateType.Sdag => ["S†"],
            GateType.T => ["T"],
            GateType.Tdag => ["T†"],
            GateType.Rx => ["Rx"],
            GateType.Ry => ["Ry"],
            GateType.Rz => ["Rz"],
            GateType.U3 => ["U3"],
            GateType.CNOT => ["@", "+"],
            GateType.CY => ["@", "Y"],
            GateType.CZ => ["@", "Z"],
            GateType.CH => ["@", "H"],
            GateType.CRx => ["@", "Rx"],
            GateType.CRy => ["@", "Ry"],
            GateType.CRz => ["@", "Rz"],
            GateType.CU3 => ["@", "U3"],
            GateType.SWAP => ["X", "X"],
            GateType.Toffoli => ["@", "+", "+"],
            GateType.Fredkin => ["@", "X", "X"],
            GateType.Measure => ["M"],
            _ => [" "]
        };
    }
    
    /// <summary>
    /// Converts a quantum <see cref="State"/> to its corresponding character representation.
    /// </summary>
    /// <param name="state">The initial quantum state to convert.</param>
    /// <returns>A character representing the specified quantum state:
    /// '0' for Zero, '1' for One, '+' for Plus, '-' for Minus, and 'P'(psi) for Custom.</returns>
    internal static char InitialStateToCharRepresentation(State state)
    {
        return state switch
        {
            State.One => '1',
            State.Zero => '0',
            State.Plus => '+',
            State.Minus => '-',
            State.Custom => 'P',
            _ => ' '
        };
    }
}