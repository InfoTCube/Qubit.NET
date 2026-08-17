# Changelog

All notable changes to this project are documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] — unreleased

First packaged release. Qubit.NET is now installable with `dotnet add package Qubit.NET`
and usable from Unity.

### Added

- **NuGet package** with Source Link, symbol package, XML documentation and icon.
- **Multi-targeting**: `netstandard2.1` (Unity), `net8.0` and `net10.0`.
- `QuantumCircuit.ToDiagram()` — returns the ASCII circuit diagram as a string, so the
  drawer works without a console (Unity, ASP.NET, tests). `Draw()` still prints in color.
- `QuantumGates.Format(matrix)` — the string-returning counterpart of `Print`.
- `QuantumCircuit.MaxQubitCount` constant.
- Continuous integration and a tag-driven NuGet release workflow.
- An xUnit test suite covering gate algebra, the textbook Bell and GHZ states, measurement
  statistics and collapse, circuit rendering, and a regression test for every bug below.
- XML documentation for every public gate matrix and for the `State` enum.

### Performance

- Single-qubit and controlled gates are applied **in place**. A circuit now allocates one
  state vector rather than one per gate: a 20-qubit GHZ chain went from 328 MB of
  allocations to 16 MB, and from 83 ms to 34 ms.
- Gate application is parallelized above 2¹⁶ amplitudes.
- The general multi-qubit path no longer calls `Array.IndexOf` inside its per-amplitude
  loop, and skips zero amplitudes and zero coefficients.
- Added a BenchmarkDotNet project under `benchmarks/`.

### Changed

- **Breaking**: `Examples.Example` is now `Circuits.BellStates`.
- **Breaking**: `StateVector` may be mutated in place by gate application, so a reference
  held across a gate call is no longer a snapshot. Clone it if you need one.
- The 24 near-identical gate methods now delegate to two shared helpers, cutting
  `QuantumCircuit.cs` by roughly a third with no change to their signatures or docs.
- **Breaking**: the maximum qubit count is 26, down from a documented 30 that could never
  actually allocate (2³⁰ `Complex` values is 17 GB, and the CLR caps a single array at 2 GB).
- **Breaking**: `Initialize` now rejects unnormalized states instead of silently producing
  a non-physical one.
- **Breaking**: constructing a circuit with an invalid qubit count throws
  `ArgumentOutOfRangeException` rather than `ArgumentException`/`AggregateException`.

### Fixed

- `IsUnitary` compared floating-point values for exact equality, so every matrix built
  from `1/√2` was rejected — `qc.Custom(hadamard, 0)` threw "The provided matrix is not
  unitary." Comparisons now use a tolerance.
- `Simulator.Run` mutated the gate list of the circuit it was given, so calling it twice
  on the same circuit returned wrong results the second time.
- The copy constructor documented a deep copy but shared the gate list and modification
  flags with the original, so gates applied to a copy also affected the source circuit.
- Toffoli gates rendered their second control as a target marker in circuit diagrams.
- `GetStringResult` threw an exception when every measurement count was zero.
- Renamed the internal `ApplayGate` to `ApplyGate`.
