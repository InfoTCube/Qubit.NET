using Qubit.NET;

namespace Qubit.NET.Demo;

public static class Program
{
    public static void Main(string[] args)
    {
        QuantumCircuit qc = new QuantumCircuit(2);

        qc.H(0);
        qc.CNOT(0, 1);

        Console.WriteLine(qc.ToString());
        
        Console.WriteLine(Simulator.Run(qc, 1000).GetStringResult());
    }
}

