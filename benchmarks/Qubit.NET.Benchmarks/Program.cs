using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Qubit.NET;

BenchmarkRunner.Run<GateBenchmarks>();

/// <summary>
/// Measures how gate application scales with register size. The state vector doubles with
/// every qubit, so these numbers are the practical ceiling on circuit size.
/// </summary>
[MemoryDiagnoser]
public class GateBenchmarks
{
    [Params(10, 16, 20, 22)]
    public int Qubits { get; set; }

    /// <summary>
    /// A Hadamard on every qubit: the cheapest way to touch the whole state vector once
    /// per qubit, and the standard opening move of most algorithms.
    /// </summary>
    [Benchmark]
    public QuantumCircuit HadamardLayer()
    {
        QuantumCircuit qc = new(Qubits);

        for (int q = 0; q < Qubits; q++)
            qc.H(q);

        return qc;
    }

    /// <summary>
    /// A GHZ state: one Hadamard followed by a chain of CNOTs, exercising the multi-qubit
    /// path across the full register.
    /// </summary>
    [Benchmark]
    public QuantumCircuit GhzChain()
    {
        QuantumCircuit qc = new(Qubits);
        qc.H(0);

        for (int q = 1; q < Qubits; q++)
            qc.CNOT(0, q);

        return qc;
    }

    /// <summary>
    /// Full-register measurement, which samples and collapses the whole state vector.
    /// </summary>
    [Benchmark]
    public string MeasureAll()
    {
        QuantumCircuit qc = new(Qubits);

        for (int q = 0; q < Qubits; q++)
            qc.H(q);

        return qc.Measure();
    }
}
