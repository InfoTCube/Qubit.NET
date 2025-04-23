using System;
using System.Numerics;
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
    
    public static Complex[,] H => new Complex[,]
    {
        { InvSqrt2, InvSqrt2 },
        { InvSqrt2, -InvSqrt2 }
    };

    public static Complex[,] X => new Complex[,]
    {
        { 0, 1 },
        { 1, 0 }
    };
    
    public static Complex[,] Y => new Complex[,]
    {
        { 0, -Complex.ImaginaryOne },
        { Complex.ImaginaryOne, 0 }
    };

    public static Complex[,] Z => new Complex[,]
    {
        { 1, 0 },
        { 0, -1 }
    };

    public static Complex[,] S => new Complex[,]
    {
        { 1, 0 },
        { 0, Complex.ImaginaryOne }
    };

    public static Complex[,] Sdag => new Complex[,]
    {
        { 1, 0 },
        { 0, -Complex.ImaginaryOne }
    };

    public static Complex[,] T => new Complex[,]
    {
        { 1, 0 },
        { 0, TElement }
    };

    public static Complex[,] Tdag => new Complex[,]
    {
        { 1, 0 },
        { 0, Complex.Conjugate(TElement) }
    };

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
    
    public static Complex[,] CNOT => new Complex[,]
    {
        { 1, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 0, 1 },
        { 0, 0, 1, 0 }
    };
    
    public static Complex[,] CZ => new Complex[,]
    {
        { 1, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 1, 0 },
        { 0, 0, 0, -1 }
    };
    
    public static Complex[,] CY => new Complex[,]
    {
        { 1, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 0, -Complex.ImaginaryOne },
        { 0, 0, Complex.ImaginaryOne, 0 }
    };
    
    public static Complex[,] CH => new Complex[,]
    {
        { 1, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, InvSqrt2, InvSqrt2 },
        { 0, 0, InvSqrt2, -InvSqrt2 }
    };

    public static Complex[,] SWAP => new Complex[,]
    {
        { 1, 0, 0, 0 },
        { 0, 0, 1, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 0, 1 }
    };

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
    
    public static void Print(Complex[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        
        int maxLength = 0;
        foreach (var elem in matrix)
        {
            string formatted = Helpers.FormatComplex(elem.Real, elem.Imaginary);
            maxLength = System.Math.Max(maxLength, formatted.Length);
        }
        
        Console.Write("\u250c ");
        Console.Write(new string(' ', (maxLength + 1) * cols));
        Console.WriteLine("\u2510");
        
        for (int i = 0; i < rows; i++)
        {
            Console.Write("\u2502 ");
            for (int j = 0; j < cols; j++)
            {
                string representation = Helpers.FormatComplex(matrix[i, j].Real, matrix[i, j].Imaginary);

                if (representation == string.Empty) representation = "0";
                
                Console.Write(representation.PadRight(maxLength + 1));
            }
            Console.WriteLine("\u2502");
        }
        
        Console.Write("\u2514 ");
        Console.Write(new string(' ', (maxLength + 1) * cols));
        Console.WriteLine("\u2518");
    }
}