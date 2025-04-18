﻿using System.Numerics;
using Qubit.NET.Gates;
using Qubit.NET.Simulation;
using Qubit.NET.Utilities;

namespace Qubit.NET.Demo;

public static class Program
{
    public static void Main(string[] args)
    {
        QuantumCircuit qc = new QuantumCircuit(2);

        qc.H(0);
        qc.CNOT(0, 1);

        Console.WriteLine(qc.ToString());
        
        qc.Measure();
        
        QuantumGates.Print(QuantumGates.H);
        
        Console.WriteLine(Simulator.Run(qc, 1000)[0].GetStringResult());
    }
}

