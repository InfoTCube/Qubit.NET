using Qubit.NET;
using Qubit.NET.Circuits;
using Qubit.NET.Simulation;
using Qubit.NET.Visualization;
using static QubitNet.Tests.Amplitudes;

namespace QubitNet.Tests;

/// <summary>
/// The textbook algorithms, checked against the answers they are supposed to produce.
/// </summary>
public class AlgorithmTests
{
    [Fact]
    public void Deutsch_Jozsa_reports_constant_for_a_constant_function()
    {
        // A constant oracle does nothing at all, so every input qubit returns to |0>.
        QuantumCircuit qc = Algorithms.DeutschJozsa(3, _ => { });
        qc.RandomSource = new SeededRandomSource(1);

        Assert.Equal("000", qc.Measure(0, 1, 2));
    }

    [Fact]
    public void Deutsch_Jozsa_reports_balanced_for_a_balanced_function()
    {
        // f(x) = x0 is balanced, so the result must never be all zeros.
        for (int seed = 0; seed < 10; seed++)
        {
            QuantumCircuit qc = Algorithms.DeutschJozsa(3, c => c.CNOT(0, 3));
            qc.RandomSource = new SeededRandomSource(seed);

            Assert.NotEqual("000", qc.Measure(0, 1, 2));
        }
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    public void Bernstein_Vazirani_recovers_the_secret_in_one_query(bool b0, bool b1, bool b2)
    {
        bool[] secret = [b0, b1, b2];

        QuantumCircuit qc = Algorithms.BernsteinVazirani(secret);
        qc.RandomSource = new SeededRandomSource(3);

        // Measure(2, 1, 0) puts qubit 2 first, matching the secret written most significant first.
        string measured = qc.Measure(2, 1, 0);
        string expected = string.Concat(secret.Reverse().Select(b => b ? '1' : '0'));

        Assert.Equal(expected, measured);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Grover_finds_the_marked_item(int target)
    {
        // Phase-mark exactly one of the four basis states.
        QuantumCircuit qc = Algorithms.Grover(2, c =>
        {
            if ((target & 1) == 0) c.X(0);
            if ((target & 2) == 0) c.X(1);

            c.CZ(0, 1);

            if ((target & 1) == 0) c.X(0);
            if ((target & 2) == 0) c.X(1);
        });

        var probabilities = qc.GetProbabilities();
        string expected = Convert.ToString(target, 2).PadLeft(2, '0');

        // One iteration on four items is exact: the marked state has probability 1.
        Assert.Equal(1.0, probabilities[expected], 9);
    }

    [Fact]
    public void Teleportation_delivers_the_message()
    {
        QuantumCircuit qc = Algorithms.Teleportation(c => c.Ry(0, 0.9));
        qc.RandomSource = new SeededRandomSource(5);

        double expected = Math.Sin(0.45) * Math.Sin(0.45);

        Assert.Equal(expected, qc.QubitProbability(2), 9);
    }

    [Theory]
    [InlineData(false, false, "00")]
    [InlineData(false, true, "01")]
    [InlineData(true, false, "10")]
    [InlineData(true, true, "11")]
    public void Superdense_coding_sends_two_bits_on_one_qubit(bool first, bool second, string expected)
    {
        QuantumCircuit qc = Algorithms.SuperdenseCoding(first, second);
        qc.RandomSource = new SeededRandomSource(2);

        // Qubit 0 carries the first bit, qubit 1 the second.
        Assert.Equal(expected, qc.Measure(0, 1));
    }

    [Fact]
    public void QFT_of_the_all_zero_state_is_a_uniform_superposition()
    {
        QuantumCircuit qc = new(3);
        qc.QFT();

        var probabilities = qc.GetProbabilities();

        Assert.Equal(8, probabilities.Count);
        Assert.All(probabilities.Values, p => Assert.Equal(0.125, p, 9));
    }

    [Fact]
    public void QFT_preserves_normalization()
    {
        QuantumCircuit qc = new(4);
        qc.X(0);
        qc.H(2);
        qc.QFT();

        AssertNormalized(qc);
    }

    [Fact]
    public void Algorithms_reject_a_null_oracle()
    {
        Assert.Throws<ArgumentNullException>(() => Algorithms.DeutschJozsa(2, null!));
        Assert.Throws<ArgumentNullException>(() => Algorithms.Grover(2, null!));
        Assert.Throws<ArgumentNullException>(() => Algorithms.BernsteinVazirani(null!));
    }

    [Fact]
    public void Grover_over_more_than_three_qubits_reports_the_limit() =>
        Assert.Throws<NotSupportedException>(() => Algorithms.Grover(4, c => c.Z(0)));
}
