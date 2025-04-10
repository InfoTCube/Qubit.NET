using System;
using System.Numerics;

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

    public static Complex[,] T => new Complex[,]
    {
        { 1, 0 },
        { 0, TElement }
    };

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
}