﻿using Qubit.NET.Gates;
using Qubit.NET.Simulation;

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
        
        qc.Draw();

        var results = Simulator.Run(qc, 1000);

        foreach (var result in results)
        {
            Console.WriteLine(result.GetStringResult());
        }
    }
}

