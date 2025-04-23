namespace Qubit.NET.Examples;

public static class Example
{
    public static QuantumCircuit PhiPlus()
    {
        QuantumCircuit phiPlus = new QuantumCircuit(2);
        
        phiPlus.H(0);
        phiPlus.CNOT(0, 1);
        
        return phiPlus;
    }

    public static QuantumCircuit PhiMinus()
    {
        QuantumCircuit phiMinus = PhiPlus();
        
        phiMinus.Z(1);
        
        return phiMinus;
    }
    
    public static QuantumCircuit PsiPlus()
    {
        QuantumCircuit psiPlus = PhiPlus();
        
        psiPlus.X(1);
        
        return psiPlus;
    }
    
    public static QuantumCircuit PsiMinus()
    {
        QuantumCircuit psiMinus = PsiPlus();
        
        psiMinus.Z(1);
        
        return psiMinus;
    }

    public static QuantumCircuit GHZ()
    {
        QuantumCircuit ghz = new QuantumCircuit(3);
        
        ghz.H(0);
        ghz.CNOT(0, 1);
        ghz.CNOT(0, 2);

        return ghz;
    }
}