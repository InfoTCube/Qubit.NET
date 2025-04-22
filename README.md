# Qubit.NET

# 🧠 C# Quantum Computing Simulation Library

**Qubit.NET** is a lightweight quantum circuit simulation library written in C#. It allows users to simulate quantum circuits up to 30 qubits, initialize qubits, apply common quantum gates, and measure results — all using a classical computer. Perfect for learning, prototyping, or integrating quantum logic into .NET applications.

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

or in any custom state

```csharp
qc.Initialize(0, new Complex(1, 1), new Complex(2, 2));
```

> ⚠️ Initialization can only be done **before any gate is applied** to that qubit.  
> This is internally tracked using a private `_isQubitModified` array.

---

### 🌀 Gate Application

QuantumNet includes several built-in quantum gates:

#### ✅ Single-Qubit Gates

| Method    | Description                   |
|-----------|-------------------------------|
| `H(q)`    | Hadamard                      |
| `X(q)`    | Pauli-X (NOT)                 |
| `Y(q)`    | Pauli-Y                       |
| `Z(q)`    | Pauli-Z                       |
| `S(q)`    | Phase gate (√Z)               |
| `Sdag(q)` | Conjugate transpose of S (S†) |
| `T(q)`    | T gate (fourth root of Z)     |
| `Tdag(q)` | Conjugate transpose of T (T†) |

```csharp
qc.H(0);
qc.X(1);
```

#### ✅ Two-Qubit Gates

| Method           | Description                 |
|------------------|-----------------------------|
| `CNOT(c, t)`     | Controlled-NOT gate         |
| `CY(c, t)`       | Controlled-Y gate           |
| `CZ(c, t)`       | Controlled-Z gate           |
| `CH(c, t)`       | Controlled-Hadamard gate    |
| `SWAP(q1, q2)`   | SWAP gate (exchanges qubits)|

```csharp
qc.CNOT(0, 1);
```

#### ✅ Three-Qubit Gates

| Method           | Description               |
|------------------|---------------------------|
| `Toffoli(c, t1, t2)` | Toffoli (CC-NOT) gate |
| `Fredkin(c, t1, t2)` | Fredkin (C-SWAP) gate |

```csharp
qc.Toffoli(0, 1, 2);
qc.Fredkin(0, 1, 2);
```

#### ⏳ Custom Gate Support

Work in progess...

---

### 📏 Measurement

Measure the entire quantum system and get a classical bitstring (e.g. `"00"`, `"11"`).
You can get one result using basic vector state real-time simulator.

```csharp
string result = qc.Measure();
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

## 📌 Future Roadmap

- [ ] Partial qubit measurement
- [ ] Entanglement entropy measurements
- [ ] Noise simulation (decoherence, damping)
- [ ] Visualization of circuits and states
- [ ] Circuit export in QASM or JSON
- [ ] Built-in examples: Bell state, teleportation, etc.

---

## 💡 Contributions

Pull requests, suggestions, and feature requests are welcome!  
Feel free to fork and extend the library.

---

## 👤 Author

Created by **Tymoteusz Marzec**  
Find me on GitHub: [@InfoTCube](https://github.com/InfoTCube)