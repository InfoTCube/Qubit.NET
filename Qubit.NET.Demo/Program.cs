using Qubit.NET.Gates;
using Qubit.NET.Simulation;

namespace Qubit.NET.Demo;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        BellState();
        Teleportation();
    }

    private static void BellState()
    {
        Console.WriteLine("=== Bell state ===\n");

        QuantumCircuit qc = new QuantumCircuit(2);

        qc.H(0);
        qc.CNOT(0, 1);

        Console.WriteLine($"State:  {qc}");

        QuantumGates.Print(QuantumGates.H);

        qc.Measure();
        qc.Draw();

        MeasurementResult result = Simulator.Run(qc, 1000)[0];

        Console.WriteLine($"\nCounts:      {result}");
        Console.WriteLine($"P(11):       {result.Probability("11"):F3}");
        Console.WriteLine($"Most common: {result.MostFrequent}\n");
    }

    private static void Teleportation()
    {
        Console.WriteLine("=== Teleportation ===\n");

        // Qubit 0 carries the message, qubits 1 and 2 share a Bell pair.
        QuantumCircuit qc = new QuantumCircuit(3);

        qc.Ry(0, 0.8);

        qc.H(1);
        qc.CNOT(1, 2);

        qc.CNOT(0, 1);
        qc.H(0);
        qc.MeasureInto(0, 0);
        qc.MeasureInto(1, 1);

        // Corrections conditioned on the two measurement outcomes.
        qc.When(1, 1, c => c.X(2));
        qc.When(0, 1, c => c.Z(2));

        qc.Draw();

        double expected = System.Math.Cos(0.4) * System.Math.Cos(0.4);
        double actual = qc.GetProbabilities()
            .Where(p => p.Key[0] == '0')   // qubit 2 is the leading bit
            .Sum(p => p.Value);

        Console.WriteLine($"\nMeasured bits: c0={qc.ClassicalBit(0)} c1={qc.ClassicalBit(1)}");
        Console.WriteLine($"P(qubit 2 = |0>): {actual:F6}  (expected {expected:F6})");
    }
}
