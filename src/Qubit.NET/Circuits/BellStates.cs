namespace Qubit.NET.Circuits;

/// <summary>
/// Ready-made circuits preparing the four maximally entangled two-qubit Bell states
/// and the three-qubit GHZ state.
/// </summary>
public static class BellStates
{
    /// <summary>
    /// Prepares |Φ⁺⟩ = (|00⟩ + |11⟩)/√2.
    /// </summary>
    /// <returns>A two-qubit circuit in the |Φ⁺⟩ state.</returns>
    public static QuantumCircuit PhiPlus()
    {
        QuantumCircuit phiPlus = new QuantumCircuit(2);

        phiPlus.H(0);
        phiPlus.CNOT(0, 1);

        return phiPlus;
    }

    /// <summary>
    /// Prepares |Φ⁻⟩ = (|00⟩ − |11⟩)/√2.
    /// </summary>
    /// <returns>A two-qubit circuit in the |Φ⁻⟩ state.</returns>
    public static QuantumCircuit PhiMinus()
    {
        QuantumCircuit phiMinus = PhiPlus();

        phiMinus.Z(1);

        return phiMinus;
    }

    /// <summary>
    /// Prepares |Ψ⁺⟩ = (|01⟩ + |10⟩)/√2.
    /// </summary>
    /// <returns>A two-qubit circuit in the |Ψ⁺⟩ state.</returns>
    public static QuantumCircuit PsiPlus()
    {
        QuantumCircuit psiPlus = PhiPlus();

        psiPlus.X(1);

        return psiPlus;
    }

    /// <summary>
    /// Prepares |Ψ⁻⟩ = (|01⟩ − |10⟩)/√2.
    /// </summary>
    /// <returns>A two-qubit circuit in the |Ψ⁻⟩ state.</returns>
    public static QuantumCircuit PsiMinus()
    {
        QuantumCircuit psiMinus = PsiPlus();

        psiMinus.Z(1);

        return psiMinus;
    }

    /// <summary>
    /// Prepares the three-qubit GHZ state, (|000⟩ + |111⟩)/√2.
    /// </summary>
    /// <returns>A three-qubit circuit in the GHZ state.</returns>
    public static QuantumCircuit GHZ()
    {
        QuantumCircuit ghz = new QuantumCircuit(3);

        ghz.H(0);
        ghz.CNOT(0, 1);
        ghz.CNOT(0, 2);

        return ghz;
    }
}
