using System;
using System.Numerics;

namespace Qubit.NET.Gates;

/// <summary>
/// Contains standard quantum gate matrices.
/// </summary>
public static class QuantumGates
{
    private static readonly double InvSqrt2 = 1.0 / System.Math.Sqrt(2);

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

    public static Complex[,] CNOT => new Complex[,]
    {
        { 1, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 0, 1 },
        { 0, 0, 1, 0 }
    };

    public static Complex[,] SWAP => new Complex[,]
    {
        { 1, 0, 0, 0 },
        { 0, 0, 1, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 0, 1 }
    };
}