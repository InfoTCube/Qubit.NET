using System.Globalization;
using System.Numerics;
using Qubit.NET;
using Qubit.NET.Circuits;
using Qubit.NET.Gates;

namespace QubitNet.Tests;

/// <summary>
/// State and matrix formatting. These strings are the first thing a user sees, and they
/// must not change with the machine's locale.
/// </summary>
public class FormattingTests
{
    [Fact]
    public void State_string_is_readable()
    {
        Assert.Equal("0.7071|00> + 0.7071|11>", BellStates.PhiPlus().ToString());
    }

    [Fact]
    public void Basis_states_drop_the_redundant_coefficient()
    {
        QuantumCircuit qc = new(2);
        qc.X(0);

        Assert.Equal("|01>", qc.ToString());
    }

    [Fact]
    public void Negative_amplitudes_keep_their_sign()
    {
        Assert.Equal("0.7071|00> + -0.7071|11>", BellStates.PhiMinus().ToString());
    }

    [Fact]
    public void Numerical_noise_is_not_printed()
    {
        // H twice is the identity, but leaves ~1e-17 in the other amplitude.
        QuantumCircuit qc = new(1);
        qc.H(0);
        qc.H(0);

        Assert.Equal("|0>", qc.ToString());
    }

    [Fact]
    public void Imaginary_amplitudes_are_formatted_as_complex_numbers()
    {
        QuantumCircuit qc = new(1);
        qc.H(0);
        qc.S(0);

        // (|0> + i|1>)/sqrt(2)
        Assert.Equal("0.7071|0> + 0.7071i|1>", qc.ToString());
    }

    [Fact]
    public void A_negative_imaginary_part_reads_as_a_subtraction()
    {
        // 0.5 - 0.5i, via the SX gate's off-diagonal entry.
        string formatted = QuantumGates.Format(QuantumGates.SX);

        Assert.Contains("0.5 - 0.5i", formatted);
        Assert.DoesNotContain("+ -", formatted);
    }

    [Theory]
    [InlineData("pl-PL")]   // comma decimal separator
    [InlineData("de-DE")]
    [InlineData("en-US")]
    public void Output_does_not_depend_on_the_current_culture(string culture)
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo(culture);

        try
        {
            Assert.Equal("0.7071|00> + 0.7071|11>", BellStates.PhiPlus().ToString());
            Assert.DoesNotContain(",", QuantumGates.Format(QuantumGates.H));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
