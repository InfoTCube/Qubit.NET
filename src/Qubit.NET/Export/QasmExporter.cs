using System.Globalization;
using System.Text;
using Qubit.NET.Gates;

namespace Qubit.NET.Export;

/// <summary>
/// Exports circuits to OpenQASM 2.0, the interchange format used by Qiskit and IBM Quantum.
/// </summary>
public static class QasmExporter
{
    /// <summary>
    /// Converts the circuit to an OpenQASM 2.0 program.
    /// </summary>
    /// <param name="circuit">The circuit to export.</param>
    /// <returns>The QASM source text.</returns>
    /// <example>
    /// <code>
    /// var qc = new QuantumCircuit(2);
    /// qc.H(0);
    /// qc.CNOT(0, 1);
    /// qc.Measure();
    ///
    /// File.WriteAllText("bell.qasm", qc.ToQasm());
    /// </code>
    /// The result can be loaded straight into Qiskit with
    /// <c>QuantumCircuit.from_qasm_file("bell.qasm")</c>.
    /// </example>
    /// <exception cref="NotSupportedException">
    /// Thrown if the circuit contains a custom gate, which has no QASM equivalent.
    /// </exception>
    public static string ToQasm(this QuantumCircuit circuit)
    {
        StringBuilder sb = new();

        sb.AppendLine("OPENQASM 2.0;");
        sb.AppendLine("include \"qelib1.inc\";");
        sb.AppendLine();
        sb.AppendLine($"qreg q[{circuit.QubitCount}];");
        sb.AppendLine($"creg c[{circuit.QubitCount}];");
        sb.AppendLine();

        // Initializations are state preparation, which QASM cannot express directly. The
        // basis states have exact gate equivalents; anything else is reported as a comment
        // rather than silently dropped.
        foreach (var init in circuit.Initializations)
        {
            switch (init.BasicState)
            {
                case Utilities.State.Zero:
                    break;
                case Utilities.State.One:
                    sb.AppendLine($"x q[{init.QubitIndex}];");
                    break;
                case Utilities.State.Plus:
                    sb.AppendLine($"h q[{init.QubitIndex}];");
                    break;
                case Utilities.State.Minus:
                    sb.AppendLine($"x q[{init.QubitIndex}];");
                    sb.AppendLine($"h q[{init.QubitIndex}];");
                    break;
                default:
                    sb.AppendLine($"// qubit {init.QubitIndex} initialized to a custom state "
                                  + "that OPENQASM 2.0 cannot express");
                    break;
            }
        }

        foreach (var gate in circuit.Gates)
        {
            if (gate.ConditionBit is { } bit)
                sb.Append($"if (c[{bit}]=={gate.ConditionValue}) ");

            sb.AppendLine(Emit(gate));
        }

        return sb.ToString();
    }

    private static string Emit(Gate gate)
    {
        int[] t = gate.TargetQubits;
        int[] c = gate.ControlQubits;

        return gate.GateType switch
        {
            GateType.I => $"id q[{t[0]}];",
            GateType.H => $"h q[{t[0]}];",
            GateType.X => $"x q[{t[0]}];",
            GateType.Y => $"y q[{t[0]}];",
            GateType.Z => $"z q[{t[0]}];",
            GateType.S => $"s q[{t[0]}];",
            GateType.Sdag => $"sdg q[{t[0]}];",
            GateType.T => $"t q[{t[0]}];",
            GateType.Tdag => $"tdg q[{t[0]}];",
            GateType.SX => $"sx q[{t[0]}];",
            // No qelib1 primitive for sqrt(Y) or sqrt(Z); both have exact u3 forms.
            GateType.SY => $"u3(pi/2,0,0) q[{t[0]}];",
            GateType.SZ => $"s q[{t[0]}];",
            GateType.Rx => $"rx({Angles(gate)}) q[{t[0]}];",
            GateType.Ry => $"ry({Angles(gate)}) q[{t[0]}];",
            GateType.Rz => $"rz({Angles(gate)}) q[{t[0]}];",
            GateType.U3 => $"u3({Angles(gate)}) q[{t[0]}];",
            GateType.CNOT => $"cx q[{c[0]}],q[{t[0]}];",
            GateType.CY => $"cy q[{c[0]}],q[{t[0]}];",
            GateType.CZ => $"cz q[{c[0]}],q[{t[0]}];",
            GateType.CH => $"ch q[{c[0]}],q[{t[0]}];",
            GateType.CRx => $"crx({Angles(gate)}) q[{c[0]}],q[{t[0]}];",
            GateType.CRy => $"cry({Angles(gate)}) q[{c[0]}],q[{t[0]}];",
            GateType.CRz => $"crz({Angles(gate)}) q[{c[0]}],q[{t[0]}];",
            GateType.CU3 => $"cu3({Angles(gate)}) q[{c[0]}],q[{t[0]}];",
            GateType.SWAP => $"swap q[{t[0]}],q[{t[1]}];",
            GateType.Toffoli => $"ccx q[{c[1]}],q[{c[0]}],q[{t[0]}];",
            GateType.Fredkin => $"cswap q[{c[0]}],q[{t[1]}],q[{t[0]}];",
            GateType.Measure => Measure(gate),
            GateType.Custom => throw new NotSupportedException(
                "Custom gates have no OPENQASM 2.0 equivalent. Rebuild the operation from "
                + "standard gates before exporting."),
            _ => throw new NotSupportedException($"Gate {gate.GateType} cannot be exported to QASM.")
        };
    }

    private static string Measure(Gate gate)
    {
        StringBuilder sb = new();

        for (int i = 0; i < gate.TargetQubits.Length; i++)
        {
            int classicalBit = i < gate.ClassicalBits.Length ? gate.ClassicalBits[i] : gate.TargetQubits[i];

            if (i > 0) sb.AppendLine();

            sb.Append($"measure q[{gate.TargetQubits[i]}] -> c[{classicalBit}];");
        }

        return sb.ToString();
    }

    /// <summary>
    /// The angles the gate was built from, comma separated. Recorded at construction, so no
    /// reconstruction from the matrix is needed.
    /// </summary>
    private static string Angles(Gate gate) =>
        string.Join(",", gate.Parameters.Select(Format));

    private static string Format(double value) =>
        value.ToString("0.############################", CultureInfo.InvariantCulture);
}
