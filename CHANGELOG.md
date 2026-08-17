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
- XML documentation for every public gate matrix and for the `State` enum.

### Changed

- **Breaking**: `Examples.Example` is now `Circuits.BellStates`.
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
