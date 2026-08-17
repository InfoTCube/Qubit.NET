<img src="./img/qubitnet.png" alt="Qubit.NET logo" style="height: 250px"/>

# Qubit.NET

# 🧠 C# Quantum Computing Simulation Library

[![NuGet](https://img.shields.io/nuget/v/Qubit.NET.svg)](https://www.nuget.org/packages/Qubit.NET/)
[![Downloads](https://img.shields.io/nuget/dt/Qubit.NET.svg)](https://www.nuget.org/packages/Qubit.NET/)
[![CI](https://github.com/InfoTCube/Qubit.NET/actions/workflows/ci.yml/badge.svg)](https://github.com/InfoTCube/Qubit.NET/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

**Qubit.NET** is a lightweight quantum circuit simulation library written in C#. It lets you build quantum circuits, initialize qubits, apply common quantum gates, and measure results — all on a classical computer. Perfect for learning, prototyping, or integrating quantum logic into .NET applications.

The state vector holds 2ⁿ complex amplitudes, so memory is the limit: **20 qubits ≈ 16 MB, 24 ≈ 256 MB, 26 ≈ 1 GB** (the hard ceiling, set by the CLR's 2 GB single-array limit).

---

### 📥 Install

```bash
dotnet add package Qubit.NET
```

Zero dependencies. Targets **.NET Standard 2.1**, **.NET 8** and **.NET 10**.

### 🎮 Unity

Qubit.NET ships a `netstandard2.1` build, so it works in Unity 2021.2+. Either install it
through [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity), or drop
`lib/netstandard2.1/Qubit.NET.dll` from the package into `Assets/Plugins/`.

Unity has no `Console`, so use the string-returning APIs:

```csharp
Debug.Log(qc.ToDiagram());                     // instead of qc.Draw()
Debug.Log(QuantumGates.Format(QuantumGates.H)); // instead of QuantumGates.Print(...)
```

---

## 🚀 Quick Start

```csharp
using Qubit.Net;

//qubits are created in 0 state
var qc = new QuantumCircuit(2);

// Apply Hadamard to qubit 0
qc.H(0);

// Apply CNOT (qubit 0 → control, qubit 1 → target)
qc.CNOT(0, 1);

// Draw a circuit
qc.Draw();

// Measure full state
Console.WriteLine($"Measured: {qc.Measure()}"); // Possible: 00 or 11
```

`Draw()` prints a colored ASCII diagram to the console — `ToDiagram()` returns the same
thing as a string:

```
q2 (0): ───────────[+]──[X]──[M]─
                    |    |    |
q1 (0): ──────[+]───@────|───[M]─
               |    |    |    |
q0 (0): ─[H]───@────@───[X]──[M]─
```

---

## 🧰 Features

### 🧩 Qubit Initialization

You can initialize any qubit to one of the predefined basis states:

- `|0⟩` → `State.Zero`
- `|1⟩` → `State.One`
- `|+⟩` → `State.Plus`
- `|−⟩` → `State.Minus`

```csharp
qc.Initialize(0, State.Minus);
```

or in any custom state, given as amplitudes α and β:

```csharp
// |ψ⟩ = (|0⟩ + i|1⟩) / √2
qc.Initialize(0, new Complex(1 / Math.Sqrt(2), 0), new Complex(0, 1 / Math.Sqrt(2)));
```

> ⚠️ The state must be normalized — `|α|² + |β|² = 1` — or an `ArgumentException` is thrown.

> ⚠️ Initialization can only be done **before any gate is applied** to that qubit.  
> This is internally tracked using a private `_isQubitModified` array.

---

### 🌀 Gate Application

Qubit.NET includes several built-in quantum gates:

#### ✅ Single-Qubit Gates

| Method           | Description                             |
|------------------|-----------------------------------------|
| `I(q)`           | Identity                                |
| `H(q)`           | Hadamard                                |
| `X(q)`           | Pauli-X (NOT)                           |
| `Y(q)`           | Pauli-Y                                 |
| `Z(q)`           | Pauli-Z                                 |
| `S(q)`           | Phase gate (√Z)                         |
| `Sdag(q)`        | Conjugate transpose of S (S†)           |
| `T(q)`           | T gate (fourth root of Z)               |
| `Tdag(q)`        | Conjugate transpose of T (T†)           |
| `Rx(q, θ)`       | Rotation around X-axis by angle θ       |
| `Ry(q, θ)`       | Rotation around Y-axis by angle θ       |
| `Rz(q, θ)`       | Rotation around Z-axis by angle θ       |
| `SX(q)`          | Square-root of Pauli-X (√X)             |
| `SY(q)`          | Square-root of Pauli-Y (√Y)             |
| `SZ(q)`          | Square-root of Pauli-Z (√Z), aka S gate |
| `U3(q, θ, φ, λ)` | General single-qubit rotation gate      |

```csharp
qc.H(0);
qc.X(1);
```

#### ✅ Two-Qubit Gates

| Method               | Description                 |
|----------------------|-----------------------------|
| `CNOT(c, t)`         | Controlled-NOT gate         |
| `CY(c, t)`           | Controlled-Y gate           |
| `CZ(c, t)`           | Controlled-Z gate           |
| `CH(c, t)`           | Controlled-Hadamard gate    |
| `CRx(c, t, θ)`       | Controlled-Rx gate          |
| `CRy(c, t, θ)`       | Controlled-Ry gate          |
| `CRz(c, t, θ)`       | Controlled-Rz gate          |
| `CU3(c, t, θ, φ, λ)` | Controlled-U3 gate          |
| `SWAP(q1, q2)`       | SWAP gate (exchanges qubits)|

```csharp
qc.CNOT(0, 1);
```

#### ✅ Three-Qubit Gates

| Method           | Description               |
|------------------|---------------------------|
| `Toffoli(c1, c2, t)` | Toffoli (CC-NOT) gate |
| `Fredkin(c, t1, t2)` | Fredkin (C-SWAP) gate |

```csharp
qc.Toffoli(0, 1, 2);
qc.Fredkin(0, 1, 2);
```

#### ✅ Custom Gate Support

You can custom gates for 1-4 qubits.
Remember that matrix must be a square matrix of size 2^n x 2^n, where n is number of qubits involved.
The matrix must be unitary — 𝑈†𝑈 = 𝐼

```csharp
// Equivalent to CNOT(0, 1)

var cx = new Complex[,]
{
    { 1, 0, 0, 0 },
    { 0, 1, 0, 0 },
    { 0, 0, 0, 1 },
    { 0, 0, 1, 0 }
};

qc.Custom(cx, 0, 1);
```

---

### 📏 Measurement

Measure the entire quantum system and get a classical bitstring (e.g. `"00"`, `"11"`).
You can get one result using basic vector state real-time simulator. You can also perform partial measurements to observe only selected qubits, yielding a shorter bitstring corresponding to the measured subset - the bits in the result are ordered exactly as the qubit indices are listed in the argument.

```csharp
string result = qc.Measure();

string result = qc.Measure(0, 2);
```

The measurement collapses the quantum state probabilistically based on the amplitudes.

---

### ⚙️ Simulation

The `Simulator` class provides functionality to simulate quantum circuits and measure the results. It allows you to run a quantum circuit multiple times and analyze the measurement outcomes. It returns an array of measurments for each `qc.Measure()`

#### Example:

```csharp
QuantumCircuit qc = new QuantumCircuit(2);
qc.H(0);
qc.CNOT(0, 1);
qc.Measure();

MeasurementResult result = Simulator.Run(qc, 1000)[0];

Console.WriteLine(result);                    // {'00': 512, '11': 488}
Console.WriteLine(result.Counts["00"]);       // 512
Console.WriteLine(result.Probability("11"));  // 0.488
Console.WriteLine(result.MostFrequent);       // 00
Console.WriteLine(result.Shots);              // 1000
```

`Simulator.Run` never modifies the circuit you hand it, so you can run the same circuit as
many times as you like.

---

### 🔀 Classical bits and feedforward

Measuring writes into a classical bit — one per qubit, so measuring qubit `q` fills bit `q`
unless you say otherwise with `MeasureInto`. `When` then conditions later gates on that bit,
which is what mid-circuit measurement and error correction need.

```csharp
qc.MeasureInto(qubit: 0, classicalBit: 0);
qc.When(classicalBit: 0, value: 1, c => c.X(2));   // X(2) runs only if bit 0 came out 1
```

Conditional gates are always *recorded*, so `Simulator.Run` re-evaluates the condition on
every shot against that shot's own outcomes.

Quantum teleportation in full:

```csharp
var qc = new QuantumCircuit(3);

qc.Ry(0, theta);        // the message on qubit 0

qc.H(1);                // entangle qubits 1 and 2
qc.CNOT(1, 2);

qc.CNOT(0, 1);          // Bell-basis measurement of qubits 0 and 1
qc.H(0);
qc.MeasureInto(0, 0);
qc.MeasureInto(1, 1);

qc.When(1, 1, c => c.X(2));   // corrections
qc.When(0, 1, c => c.Z(2));

// qubit 2 now holds the state qubit 0 started in
```

> Qubit.NET deliberately has no separate `ClassicalRegister` type. The classical register in
> Qiskit exists mainly to express feedforward and result layout; `When` and `MeasureInto`
> cover both without making every circuit declare two registers up front.

---

 ### 🎲 Randomness source

Qubit.NET uses a pluggable randomness system through the IRandomSource interface. By default, it uses a pseudo-random generator (PseudoRandomSource). You can swap this out for your custom implementation.

```csharp
using Qubit.NET.Utilities;

public class FixedRandomSource : IRandomSource
{
    public double NextDouble() => 0.42; // Always returns the same value
}
```

Then you can use it in QuantumCircuit:

```csharp
QuantumCircuit qc = new QuantumCircuit(2);
qc.RandomSource = new FixedRandomSource();
```

### 🧪 Built-in algorithms

```csharp
using Qubit.NET.Circuits;

// Grover search: finds the marked item in O(sqrt(N))
var grover = Algorithms.Grover(2, c => c.CZ(0, 1));   // marks |11>
Console.WriteLine(grover.ToHistogram());              // 11 | ####...####  1.000

// Bernstein-Vazirani: recovers a hidden bit string in a single query
var bv = Algorithms.BernsteinVazirani([true, false, true]);
Console.WriteLine(bv.Measure(2, 1, 0));               // 101

// Deutsch-Jozsa, teleportation, superdense coding, QFT
var dj = Algorithms.DeutschJozsa(3, c => c.CNOT(0, 3));
var tp = Algorithms.Teleportation(c => c.Ry(0, 0.9));
var sd = Algorithms.SuperdenseCoding(true, false);

qc.QFT();   // Quantum Fourier Transform, in place
```

Plus `BellStates.PhiPlus/PhiMinus/PsiPlus/PsiMinus/GHZ()`.

---

### 📤 OpenQASM export

Export to OpenQASM 2.0 and run your circuit on real hardware through Qiskit or IBM Quantum:

```csharp
using Qubit.NET.Export;

File.WriteAllText("teleport.qasm", Algorithms.Teleportation(c => c.Ry(0, 0.9)).ToQasm());
```

```qasm
OPENQASM 2.0;
include "qelib1.inc";

qreg q[3];
creg c[3];

ry(0.9) q[0];
h q[1];
cx q[1],q[2];
cx q[0],q[1];
h q[0];
measure q[0] -> c[0];
measure q[1] -> c[1];
if (c[1]==1) x q[2];
if (c[0]==1) z q[2];
```

```python
# Qiskit
qc = QuantumCircuit.from_qasm_file("teleport.qasm")
```

---

### 📊 Inspecting the state

```csharp
using Qubit.NET.Visualization;

var (x, y, z) = qc.BlochVector(0);   // Bloch sphere coordinates — drive a Unity gizmo
qc.QubitProbability(0);              // P(qubit 0 = |1>), tracing out the rest
Console.WriteLine(qc.ToHistogram()); // ASCII bar chart of outcome probabilities
```

A qubit entangled with others sits inside the sphere — maximally entangled means the
origin, which makes entanglement something you can actually *see*.

---

## ⚡ Performance

Gates are applied in place, so a circuit allocates one state vector regardless of how many
gates you apply, and gate application is parallelized above ~16 qubits.

| Circuit                    | 16 qubits | 20 qubits | 22 qubits |
|----------------------------|----------:|----------:|----------:|
| Hadamard on every qubit    |    2.4 ms |     36 ms |    147 ms |
| GHZ (H + CNOT chain)       |    2.3 ms |     34 ms |    142 ms |
| Measure all qubits         |    2.7 ms |     44 ms |    188 ms |

<sub>BenchmarkDotNet, .NET 10, Ryzen desktop. Reproduce with
`dotnet run -c Release --project benchmarks/Qubit.NET.Benchmarks`.</sub>

Memory is the real limit — the state vector holds 2ⁿ complex amplitudes at 16 bytes each:

| Qubits | State vector |
|-------:|-------------:|
|     16 |         1 MB |
|     20 |        16 MB |
|     24 |       256 MB |
|     26 |    1 GB (max) |

---

## 📌 Future Roadmap

- [x] Circuit export in QASM
- [x] Mid-circuit measurement and classical feedforward
- [x] Bloch sphere coordinates and probability histograms
- [x] Built-in algorithms (Grover, Deutsch–Jozsa, Bernstein–Vazirani, QFT, teleportation)
- [ ] Noise simulation (decoherence, damping)
- [ ] Entanglement entropy measurements
- [ ] QASM import
- [ ] Multi-controlled gates and circuit inverses

---

## 🚢 Releasing

Publishing uses [NuGet Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing),
so no long-lived API key is stored anywhere. The workflow exchanges a GitHub OIDC token for
a key that expires after an hour.

One-time setup:

1. On nuget.org: **your username → Trusted Publishing → add a policy**
   - Repository Owner: `InfoTCube`
   - Repository: `Qubit.NET`
   - Workflow File: `release.yml` *(file name only, no path)*
   - Environment: leave empty
2. In GitHub repo settings, add a secret `NUGET_USER` holding your nuget.org **username**
   (the profile name, not your email).

To release:

```bash
git tag v1.0.0
git push origin v1.0.0
```

The workflow builds all three target frameworks, runs the tests, packs, and pushes.

> A new policy on a repository that nuget.org has not seen publish before is *temporarily
> active for 7 days*. The first successful publish locks it to the repository permanently.

---

## 💡 Contributions

Pull requests, suggestions, and feature requests are welcome!  
Feel free to fork and extend the library.

---

## 👤 Author

Created by **Tymoteusz Marzec**  
Find me on GitHub: [@InfoTCube](https://github.com/InfoTCube)