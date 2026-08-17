<!--
  This is the readme shown on nuget.org, not the one on GitHub.
  Keep it short and keep every URL absolute — nuget.org cannot resolve relative
  links or images. The full documentation lives in README.md.
-->

# Qubit.NET

**A quantum computing simulator for .NET — and the only one that runs in Unity.**

Build quantum circuits, apply gates, measure results, draw circuit diagrams, and export to
OpenQASM so your circuit runs on real hardware. No dependencies.

```bash
dotnet add package Qubit.NET
```

Targets **.NET Standard 2.1**, **.NET 8** and **.NET 10**.

---

## Quick start

```csharp
using Qubit.NET;

var qc = new QuantumCircuit(2);

qc.H(0);          // superposition
qc.CNOT(0, 1);    // entangle

Console.WriteLine(qc);           // 0.7071|00> + 0.7071|11>
Console.WriteLine(qc.Measure()); // "00" or "11", never "01" or "10"
```

Run it many times and count outcomes:

```csharp
using Qubit.NET.Simulation;

qc.Measure();
var result = Simulator.Run(qc, 1000)[0];

Console.WriteLine(result);                   // {'00': 512, '11': 488}
Console.WriteLine(result.Probability("11")); // 0.488
```

## Draw circuits

```csharp
qc.Draw();               // colored, to the console
qc.ToDiagram();          // the same thing as a string
```

```
q2 (0): ───────────[+]──[X]──[M]─
                    |    |    |
q1 (0): ──────[+]───@────|───[M]─
               |    |    |    |
q0 (0): ─[H]───@────@───[X]──[M]─
```

## Unity

Qubit.NET ships a `netstandard2.1` build, so it works in Unity 2021.2+. Install through
[NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity), or drop
`lib/netstandard2.1/Qubit.NET.dll` into `Assets/Plugins/`.

Unity has no `Console`, so use the string-returning APIs:

```csharp
Debug.Log(qc.ToDiagram());

var (x, y, z) = qc.BlochVector(0);   // drive a Bloch sphere gizmo
arrow.transform.localPosition = new Vector3((float)x, (float)y, (float)z);
```

A qubit entangled with others sits *inside* the Bloch sphere — maximally entangled means
the origin, which makes entanglement something you can actually see.

## Built-in algorithms

```csharp
using Qubit.NET.Circuits;

var grover = Algorithms.Grover(2, c => c.CZ(0, 1));   // marks |11>
Console.WriteLine(grover.ToHistogram());              // 11 | #########  1.000

var bv = Algorithms.BernsteinVazirani([true, false, true]);
Console.WriteLine(bv.Measure(2, 1, 0));               // 101
```

Also: Deutsch–Jozsa, teleportation, superdense coding, QFT, and the Bell and GHZ states.

## Run on real hardware

```csharp
using Qubit.NET.Export;

File.WriteAllText("bell.qasm", qc.ToQasm());
```

```python
# Qiskit
qc = QuantumCircuit.from_qasm_file("bell.qasm")
```

## What else is in the box

- **27 gates** — Pauli, Hadamard, phase, √X/√Y/√Z, rotations, U3, controlled forms, Toffoli, Fredkin, and your own unitary matrices
- **Mid-circuit measurement and classical feedforward** — `MeasureInto` and `When`, enough for teleportation and error correction
- **Partial measurement** of any subset of qubits
- **Pluggable randomness** via `IRandomSource` — seed it for reproducible tests, or plug in a quantum RNG
- **In-place simulation** — a 20-qubit GHZ chain runs in 34 ms and allocates one state vector, not one per gate

Up to 26 qubits; memory is the limit, since the state vector holds 2ⁿ complex amplitudes
(20 qubits ≈ 16 MB, 24 ≈ 256 MB).

---

**[Full documentation, examples and source on GitHub →](https://github.com/InfoTCube/Qubit.NET)**

MIT licensed. Issues and pull requests welcome.
