using Qubit.NET.Circuits;
using Qubit.NET.Gates;
using Qubit.NET.Utilities;
using Qubit.NET;

namespace QubitNet.Tests;

/// <summary>
/// Golden-string tests for the ASCII circuit renderer. These exist mostly to catch
/// accidental changes to alignment, which is easy to break and hard to notice.
/// </summary>
public class DrawerTests
{
    private static string[] Lines(QuantumCircuit qc) =>
        qc.ToDiagram().TrimEnd().Split('\n').Select(l => l.TrimEnd()).ToArray();

    [Fact]
    public void Bell_circuit_renders_as_expected()
    {
        string[] lines = Lines(BellStates.PhiPlus());

        Assert.Equal("q1 (0): ──────[+]─", lines[0]);
        Assert.Equal("               |", lines[1]);
        Assert.Equal("q0 (0): ─[H]───@──", lines[2]);
    }

    [Fact]
    public void Empty_circuit_still_draws_its_wires()
    {
        string[] lines = Lines(new QuantumCircuit(2));

        Assert.Equal(2, lines.Count(l => l.StartsWith('q')));
        Assert.All(lines.Where(l => l.StartsWith('q')), l => Assert.Contains('─', l));
    }

    [Fact]
    public void Initialized_qubits_show_their_starting_state()
    {
        QuantumCircuit qc = new(3);
        qc.Initialize(0, State.Plus);
        qc.Initialize(1, State.One);

        string diagram = qc.ToDiagram();

        Assert.Contains("q0 (+)", diagram);
        Assert.Contains("q1 (1)", diagram);
        Assert.Contains("q2 (0)", diagram);
    }

    [Fact]
    public void Every_gate_type_has_a_symbol()
    {
        QuantumCircuit qc = new(3);
        qc.I(0); qc.H(0); qc.X(0); qc.Y(0); qc.Z(0);
        qc.S(0); qc.Sdag(0); qc.T(0); qc.Tdag(0);
        qc.Rx(0, 0.1); qc.Ry(0, 0.1); qc.Rz(0, 0.1);
        qc.SX(0); qc.SY(0); qc.SZ(0); qc.U3(0, 0.1, 0.2, 0.3);
        qc.CNOT(0, 1); qc.CY(0, 1); qc.CZ(0, 1); qc.CH(0, 1);
        qc.SWAP(0, 1); qc.Toffoli(0, 1, 2); qc.Fredkin(0, 1, 2);
        qc.Custom(QuantumGates.H, 0);
        qc.Measure();

        string diagram = qc.ToDiagram();

        // A blank symbol would mean a gate type fell through the symbol table.
        Assert.DoesNotContain("[ ]", diagram);
        Assert.Contains("[H]", diagram);
        Assert.Contains("[S†]", diagram);
        Assert.Contains("[U3]", diagram);
        Assert.Contains("[M]", diagram);
        Assert.Contains("[C]", diagram);
    }

    [Fact]
    public void Fredkin_draws_one_control_and_two_swap_targets()
    {
        QuantumCircuit qc = new(3);
        qc.Fredkin(0, 1, 2);

        string diagram = qc.ToDiagram();

        Assert.Equal(1, diagram.Count(c => c == '@'));
        Assert.Equal(2, CountOccurrences(diagram, "[X]"));
    }

    [Fact]
    public void ToDiagram_needs_no_console()
    {
        // Draw() writes to the console; ToDiagram() must not, so that Unity and test hosts
        // without a console can still render circuits.
        TextWriter original = Console.Out;
        Console.SetOut(TextWriter.Null);

        try
        {
            Assert.NotEmpty(BellStates.GHZ().ToDiagram());
        }
        finally
        {
            Console.SetOut(original);
        }
    }

    [Fact]
    public void Draw_writes_the_same_text_that_ToDiagram_returns()
    {
        QuantumCircuit qc = BellStates.GHZ();

        StringWriter captured = new();
        TextWriter original = Console.Out;
        Console.SetOut(captured);

        try
        {
            qc.Draw();
        }
        finally
        {
            Console.SetOut(original);
        }

        Assert.Equal(Normalize(qc.ToDiagram()), Normalize(captured.ToString()));
    }

    private static string Normalize(string s) =>
        string.Join("\n", s.Replace("\r", string.Empty).Split('\n').Select(l => l.TrimEnd())).TrimEnd();

    private static int CountOccurrences(string haystack, string needle)
    {
        int count = 0, index = 0;

        while ((index = haystack.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += needle.Length;
        }

        return count;
    }
}
