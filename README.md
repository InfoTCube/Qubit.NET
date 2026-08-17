<img src="./img/qubitnet.png" alt="Qubit.NET logo" style="height: 250px"/>

# Qubit.NET

# 🧠 C# Quantum Computing Simulation Library

**Qubit.NET** is a lightweight quantum circuit simulation library written in C#. It lets you build quantum circuits, initialize qubits, apply common quantum gates, and measure results — all on a classical computer. Perfect for learning, prototyping, or integrating quantum logic into .NET applications.

The state vector holds 2ⁿ complex amplitudes, so memory is the limit: **20 qubits ≈ 16 MB, 24 ≈ 256 MB, 26 ≈ 1 GB** (the hard ceiling, set by the CLR's 2 GB single-array limit).

---

### ✅ Requirements
- .NET 6.0 or newer
- `System.Numerics` (for complex numbers — included in .NET)

### 📥 Setup
Clone or download the repository:

```bash
git clone https://github.com/InfoTCube/Qubit.Net.git
cd Qubit.NET
```

Add the project to your solution or include the `.cs` files (`QuantumCircuit.cs`, `QuantumGates.cs`, etc.) in your C# project.

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

string results = Simulator.Run(qc, 1000)[0].GetStringResult();
Console.WriteLine(results);
```

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

## 📌 Future Roadmap

- [ ] Entanglement entropy measurements
- [ ] Noise simulation (decoherence, damping)
- [ ] Circuit export in QASM

---

## 💡 Contributions

Pull requests, suggestions, and feature requests are welcome!  
Feel free to fork and extend the library.

---

## 👤 Author

Created by **Tymoteusz Marzec**  
Find me on GitHub: [@InfoTCube](https://github.com/InfoTCube)