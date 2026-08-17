using System;
using System.Numerics;
using System.Text;
using Qubit.NET.Utilities;

namespace Qubit.NET.Gates;

/// <summary>
/// Contains standard quantum gate matrices.
/// </summary>
public static class QuantumGates
{
    private static readonly double InvSqrt2 = 1.0 / System.Math.Sqrt(2);
    private static readonly Complex TElement = new Complex(System.Math.Cos(System.Math.PI / 4), 
        System.Math.Sin(System.Math.PI / 4));
    
    /// <summary>
    /// The identity gate I. Leaves the qubit unchanged.
    /// </summary>
    public static Complex[,] I => new Complex[,]
    {
        { 1, 0 },
        { 0, 1 }
    };
    
    /// <summary>
    /// The Hadamard gate H. Maps |0⟩ to |+⟩ and |1⟩ to |−⟩, creating an equal superposition.
    /// </summary>
    public static Complex[,] H => new Complex[,]
    {
        { InvSqrt2, InvSqrt2 },
        { InvSqrt2, -InvSqrt2 }
    };

    /// <summary>
    /// The Pauli-X (NOT) gate. Flips |0⟩ and |1⟩.
    /// </summary>
    public static Complex[,] X => new Complex[,]
    {
        { 0, 1 },
        { 1, 0 }
    };
    
    /// <summary>
    /// The Pauli-Y gate. A bit flip combined with a phase flip.
    /// </summary>
    public static Complex[,] Y => new Complex[,]
    {
        { 0, -Complex.ImaginaryOne },
        { Complex.ImaginaryOne, 0 }
    };

    /// <summary>
    /// The Pauli-Z gate. Leaves |0⟩ alone and maps |1⟩ to −|1⟩.
    /// </summary>
    public static Complex[,] Z => new Complex[,]
    {
        { 1, 0 },
        { 0, -1 }
    };

    /// <summary>
    /// The S (phase) gate. Applies a π/2 phase to |1⟩.
    /// </summary>
    public static Complex[,] S => new Complex[,]
    {
        { 1, 0 },
        { 0, Complex.ImaginaryOne }
    };

    /// <summary>
    /// The S† gate, the inverse of <see cref="S"/>. Applies a −π/2 phase to |1⟩.
    /// </summary>
    public static Complex[,] Sdag => new Complex[,]
    {
        { 1, 0 },
        { 0, -Complex.ImaginaryOne }
    };

    /// <summary>
    /// The T gate. Applies a π/4 phase to |1⟩; the fourth root of Z.
    /// </summary>
    public static Complex[,] T => new Complex[,]
    {
        { 1, 0 },
        { 0, TElement }
    };

    /// <summary>
    /// The T† gate, the inverse of <see cref="T"/>. Applies a −π/4 phase to |1⟩.
    /// </summary>
    public static Complex[,] Tdag => new Complex[,]
    {
        { 1, 0 },
        { 0, Complex.Conjugate(TElement) }
    };

    /// <summary>
    /// Builds a rotation of <paramref name="theta"/> radians about the X axis of the Bloch sphere.
    /// </summary>
    /// <param name="theta">The rotation angle in radians.</param>
    /// <returns>The gate matrix.</returns>
    public static Complex[,] Rx(double theta)
    {
        Complex cosTheta = Complex.Cos(theta / 2);
        Complex sinTheta = Complex.Sin(theta / 2);
        
        return new Complex[,]
        {
            { cosTheta, -Complex.ImaginaryOne * sinTheta },
            { -Complex.ImaginaryOne * sinTheta, cosTheta }
        };
    }

    /// <summary>
    /// Builds a rotation of <paramref name="theta"/> radians about the Y axis of the Bloch sphere.
    /// </summary>
    /// <param name="theta">The rotation angle in radians.</param>
    /// <returns>The gate matrix.</returns>
    public static Complex[,] Ry(double theta)
    {
        Complex cosTheta = Complex.Cos(theta / 2);
        Complex sinTheta = Complex.Sin(theta / 2);
        
        return new Complex[,]
        {
            { cosTheta, -sinTheta },
            { sinTheta, cosTheta }
        };
    }

    /// <summary>
    /// Builds a rotation of <paramref name="theta"/> radians about the Z axis of the Bloch sphere.
    /// </summary>
    /// <param name="theta">The rotation angle in radians.</param>
    /// <returns>The gate matrix.</returns>
    public static Complex[,] Rz(double theta)
    {
        Complex expNeg = Complex.Exp(-Complex.ImaginaryOne * (theta / 2));
        Complex expPos = Complex.Exp(Complex.ImaginaryOne * (theta / 2));

        return new Complex[,]
        {
            { expNeg, 0 },
            { 0, expPos }
        };
    }
    
    /// <summary>
    /// The √X gate. Applied twice it equals <see cref="X"/>.
    /// </summary>
    public static Complex[,] SX => new Complex[,]
    {
        { (1 + Complex.ImaginaryOne) / 2, (1 - Complex.ImaginaryOne) / 2 },
        { (1 - Complex.ImaginaryOne) / 2, (1 + Complex.ImaginaryOne) / 2 }
    };
    
    /// <summary>
    /// The √Y gate. Applied twice it equals <see cref="Y"/>.
    /// </summary>
    public static Complex[,] SY => new Complex[,]
    {
        { (1 + Complex.ImaginaryOne) / 2, (-1 - Complex.ImaginaryOne) / 2 },
        { (1 + Complex.ImaginaryOne) / 2, (1 + Complex.ImaginaryOne) / 2 }
    };

    /// <summary>
    /// The √Z gate, identical to <see cref="S"/>. Applied twice it equals <see cref="Z"/>.
    /// </summary>
    public static Complex[,] SZ => S;

    /// <summary>
    /// Builds the general single-qubit rotation U3, which can express any single-qubit unitary.
    /// </summary>
    /// <param name="theta">The rotation angle in radians.</param>
    /// <param name="phi">The phase angle in radians applied before the rotation.</param>
    /// <param name="lambda">The phase angle in radians applied after the rotation.</param>
    /// <returns>The gate matrix.</returns>
    public static Complex[,] U3(double theta, double phi, double lambda)
    {
        Complex cosTheta = Complex.Cos(theta / 2);
        Complex sinTheta = Complex.Sin(theta / 2);
        Complex expPhi = Complex.Exp(Complex.ImaginaryOne * phi);
        Complex expLambda = Complex.Exp(Complex.ImaginaryOne * lambda);
        Complex expPhiPlusLambda = Complex.Exp(Complex.ImaginaryOne * (phi + lambda));

        return new Complex[,]
        {
            { cosTheta, -expLambda * sinTheta },
            { expPhi * sinTheta, expPhiPlusLambda * cosTheta }
        };
    }
    
    /// <summary>
    /// The controlled-NOT (CX) gate. Applies <see cref="X"/> to the target when the control is |1⟩.
    /// </summary>
    public static Complex[,] CNOT => new Complex[,]
    {
        { 1, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 0, 1 },
        { 0, 0, 1, 0 }
    };
    
    /// <summary>
    /// The controlled-Z gate. Applies <see cref="Z"/> to the target when the control is |1⟩.
    /// </summary>
    public static Complex[,] CZ => new Complex[,]
    {
        { 1, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 1, 0 },
        { 0, 0, 0, -1 }
    };
    
    /// <summary>
    /// The controlled-Y gate. Applies <see cref="Y"/> to the target when the control is |1⟩.
    /// </summary>
    public static Complex[,] CY => new Complex[,]
    {
        { 1, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 0, -Complex.ImaginaryOne },
        { 0, 0, Complex.ImaginaryOne, 0 }
    };
    
    /// <summary>
    /// The controlled-Hadamard gate. Applies <see cref="H"/> to the target when the control is |1⟩.
    /// </summary>
    public static Complex[,] CH => new Complex[,]
    {
        { 1, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, InvSqrt2, InvSqrt2 },
        { 0, 0, InvSqrt2, -InvSqrt2 }
    };
    
    /// <summary>
    /// Builds a controlled <see cref="Rx"/> gate.
    /// </summary>
    /// <param name="theta">The rotation angle in radians.</param>
    /// <returns>The gate matrix.</returns>
    public static Complex[,] CRx(double theta)
    {
        Complex cosTheta = Complex.Cos(theta / 2);
        Complex sinTheta = Complex.Sin(theta / 2);
        
        return new Complex[,]
        {
            { 1, 0, 0, 0 },
            { 0, 1, 0, 0 },
            { 0, 0, cosTheta, -Complex.ImaginaryOne * sinTheta },
            { 0, 0, -Complex.ImaginaryOne * sinTheta, cosTheta }
        };
    }
    
    /// <summary>
    /// Builds a controlled <see cref="Ry"/> gate.
    /// </summary>
    /// <param name="theta">The rotation angle in radians.</param>
    /// <returns>The gate matrix.</returns>
    public static Complex[,] CRy(double theta)
    {
        Complex cosTheta = Complex.Cos(theta / 2);
        Complex sinTheta = Complex.Sin(theta / 2);
        
        return new Complex[,]
        {
            { 1, 0, 0, 0 },
            { 0, 1, 0, 0 },
            { 0, 0, cosTheta, -sinTheta },
            { 0, 0, sinTheta, cosTheta }
        };
    }
    
    /// <summary>
    /// Builds a controlled <see cref="Rz"/> gate.
    /// </summary>
    /// <param name="theta">The rotation angle in radians.</param>
    /// <returns>The gate matrix.</returns>
    public static Complex[,] CRz(double theta)
    {
        Complex expNeg = Complex.Exp(-Complex.ImaginaryOne * (theta / 2));
        Complex expPos = Complex.Exp(Complex.ImaginaryOne * (theta / 2));
        
        return new Complex[,]
        {
            { 1, 0, 0, 0 },
            { 0, 1, 0, 0 },
            { 0, 0, expNeg, 0 },
            { 0, 0, 0, expPos }
        };
    }
    
    /// <summary>
    /// Builds a controlled <see cref="U3"/> gate.
    /// </summary>
    /// <param name="theta">The rotation angle in radians.</param>
    /// <param name="phi">The phase angle in radians applied before the rotation.</param>
    /// <param name="lambda">The phase angle in radians applied after the rotation.</param>
    /// <returns>The gate matrix.</returns>
    public static Complex[,] CU3(double theta, double phi, double lambda)
    {
        Complex cosTheta = Complex.Cos(theta / 2);
        Complex sinTheta = Complex.Sin(theta / 2);
        Complex expPhi = Complex.Exp(Complex.ImaginaryOne * phi);
        Complex expLambda = Complex.Exp(Complex.ImaginaryOne * lambda);
        Complex expPhiPlusLambda = Complex.Exp(Complex.ImaginaryOne * (phi + lambda));

        return new Complex[,]
        {
            { 1, 0, 0, 0 },
            { 0, 1, 0, 0 },
            { 0, 0, cosTheta, -expLambda * sinTheta },
            { 0, 0, expPhi * sinTheta, expPhiPlusLambda * cosTheta }
        };
    }

    /// <summary>
    /// The SWAP gate. Exchanges the states of two qubits.
    /// </summary>
    public static Complex[,] SWAP => new Complex[,]
    {
        { 1, 0, 0, 0 },
        { 0, 0, 1, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 0, 1 }
    };

    /// <summary>
    /// The Toffoli (CCX) gate. Flips the target when both controls are |1⟩.
    /// </summary>
    public static Complex[,] Toffoli => new Complex[,]
    {
        { 1, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 1, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 1, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 1, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 1, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 1, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 1 },
        { 0, 0, 0, 0, 0, 0, 1, 0 }
    };
    
    /// <summary>
    /// The Fredkin (CSWAP) gate. Swaps the two targets when the control is |1⟩.
    /// </summary>
    public static Complex[,] Fredkin => new Complex[,]
    {
        { 1, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 1, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 1, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 1, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 1, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 1, 0 },
        { 0, 0, 0, 0, 0, 1, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 1 }
    };
    
    /// <summary>
    /// Writes a bracketed, column-aligned rendering of a matrix to the console.
    /// </summary>
    /// <param name="matrix">The matrix to print.</param>
    /// <remarks>
    /// Requires a console. In environments without one (Unity, ASP.NET, tests),
    /// use <see cref="Format"/> and write the string wherever you need it.
    /// </remarks>
    public static void Print(Complex[,] matrix)
    {
        Console.WriteLine(Format(matrix));
    }

    /// <summary>
    /// Builds a bracketed, column-aligned rendering of a matrix and returns it as a string.
    /// </summary>
    /// <param name="matrix">The matrix to render.</param>
    /// <returns>A multi-line string containing the formatted matrix.</returns>
    /// <example>
    /// <code>
    /// Debug.Log(QuantumGates.Format(QuantumGates.H));   // Unity
    /// </code>
    /// </example>
    public static string Format(Complex[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        int maxLength = 0;
        foreach (var elem in matrix)
        {
            string formatted = Helpers.FormatComplex(elem.Real, elem.Imaginary);
            maxLength = System.Math.Max(maxLength, formatted.Length);
        }

        StringBuilder sb = new StringBuilder();

        sb.Append("\u250c ").Append(' ', (maxLength + 1) * cols).AppendLine("\u2510");

        for (int i = 0; i < rows; i++)
        {
            sb.Append("\u2502 ");
            for (int j = 0; j < cols; j++)
            {
                string representation = Helpers.FormatComplex(matrix[i, j].Real, matrix[i, j].Imaginary);

                if (representation == string.Empty) representation = "0";

                sb.Append(representation.PadRight(maxLength + 1));
            }
            sb.AppendLine("\u2502");
        }

        sb.Append("\u2514 ").Append(' ', (maxLength + 1) * cols).Append("\u2518");

        return sb.ToString();
    }
}