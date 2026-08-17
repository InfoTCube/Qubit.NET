namespace Qubit.NET.Circuits;

/// <summary>
/// Textbook quantum algorithms, ready to run or to read as worked examples.
/// </summary>
public static class Algorithms
{
    /// <summary>
    /// Applies the Quantum Fourier Transform to the given qubits, in place.
    /// </summary>
    /// <param name="circuit">The circuit to append to.</param>
    /// <param name="qubits">
    /// The qubits to transform, most significant first. Defaults to the whole register.
    /// </param>
    /// <remarks>
    /// The QFT is the engine behind Shor's algorithm and quantum phase estimation. This is
    /// the standard Hadamard-and-controlled-phase construction, with the final swaps that
    /// restore qubit order.
    /// </remarks>
    public static void QFT(this QuantumCircuit circuit, params int[] qubits)
    {
        int[] targets = qubits.Length == 0
            ? Enumerable.Range(0, circuit.QubitCount).ToArray()
            : qubits;

        int n = targets.Length;

        for (int i = 0; i < n; i++)
        {
            circuit.H(targets[i]);

            for (int j = i + 1; j < n; j++)
            {
                // Controlled phase of pi / 2^(j - i), the defining rotation of the QFT.
                double angle = System.Math.PI / (1 << (j - i));

                circuit.CRz(targets[j], targets[i], angle);
            }
        }

        // The construction above emits the output in reverse qubit order.
        for (int i = 0; i < n / 2; i++)
            circuit.SWAP(targets[i], targets[n - 1 - i]);
    }

    /// <summary>
    /// Builds a Deutsch-Jozsa circuit, which decides in a single query whether a function is
    /// constant or balanced.
    /// </summary>
    /// <param name="inputQubits">Number of input qubits the oracle acts on.</param>
    /// <param name="oracle">
    /// Applies the phase oracle. It receives the circuit; input qubits are 0 to
    /// <paramref name="inputQubits"/> - 1 and the ancilla is the last qubit.
    /// </param>
    /// <returns>
    /// The prepared circuit. Measuring the input qubits gives all zeros if the function is
    /// constant, and anything else if it is balanced.
    /// </returns>
    public static QuantumCircuit DeutschJozsa(int inputQubits, Action<QuantumCircuit> oracle)
    {
        if (oracle is null) throw new ArgumentNullException(nameof(oracle));

        QuantumCircuit qc = new(inputQubits + 1);

        // Ancilla into |-> so the oracle's answer lands in the phase.
        qc.X(inputQubits);
        qc.H(inputQubits);

        for (int q = 0; q < inputQubits; q++)
            qc.H(q);

        oracle(qc);

        for (int q = 0; q < inputQubits; q++)
            qc.H(q);

        return qc;
    }

    /// <summary>
    /// Builds a Bernstein-Vazirani circuit, which recovers a hidden bit string in one query.
    /// </summary>
    /// <param name="secret">The hidden bit string the oracle encodes, least significant bit first.</param>
    /// <returns>
    /// The prepared circuit. Measuring the input qubits returns <paramref name="secret"/>.
    /// </returns>
    public static QuantumCircuit BernsteinVazirani(bool[] secret)
    {
        if (secret is null) throw new ArgumentNullException(nameof(secret));

        int n = secret.Length;

        return DeutschJozsa(n, qc =>
        {
            // The oracle computes the dot product of the input with the secret.
            for (int q = 0; q < n; q++)
            {
                if (secret[q]) qc.CNOT(q, n);
            }
        });
    }

    /// <summary>
    /// Builds a Grover search circuit over <paramref name="qubits"/> qubits.
    /// </summary>
    /// <param name="qubits">Number of qubits, giving a search space of 2^qubits.</param>
    /// <param name="oracle">
    /// Marks the solution states by flipping their phase. It receives the circuit.
    /// </param>
    /// <param name="iterations">
    /// Number of Grover iterations. Defaults to the optimal count for a single solution,
    /// which is floor(pi/4 * sqrt(2^qubits)).
    /// </param>
    /// <returns>The prepared circuit; measuring it yields a solution with high probability.</returns>
    /// <remarks>
    /// Grover's algorithm finds a marked item among N in O(sqrt(N)) queries rather than the
    /// O(N) a classical search needs.
    /// </remarks>
    public static QuantumCircuit Grover(int qubits, Action<QuantumCircuit> oracle, int iterations = -1)
    {
        if (oracle is null) throw new ArgumentNullException(nameof(oracle));

        QuantumCircuit qc = new(qubits);

        if (iterations < 0)
            iterations = (int)System.Math.Floor(System.Math.PI / 4 * System.Math.Sqrt(1 << qubits));

        // Uniform superposition over the whole search space.
        for (int q = 0; q < qubits; q++)
            qc.H(q);

        for (int i = 0; i < iterations; i++)
        {
            oracle(qc);
            Diffuse(qc, qubits);
        }

        return qc;
    }

    /// <summary>
    /// The Grover diffusion operator, reflecting amplitudes about their mean.
    /// </summary>
    private static void Diffuse(QuantumCircuit qc, int qubits)
    {
        for (int q = 0; q < qubits; q++)
        {
            qc.H(q);
            qc.X(q);
        }

        // Multi-controlled Z over every qubit, phase-flipping only |11...1>.
        MultiControlledZ(qc, qubits);

        for (int q = 0; q < qubits; q++)
        {
            qc.X(q);
            qc.H(q);
        }
    }

    /// <summary>
    /// Flips the phase of the all-ones state across <paramref name="qubits"/> qubits.
    /// </summary>
    private static void MultiControlledZ(QuantumCircuit qc, int qubits)
    {
        int last = qubits - 1;

        switch (qubits)
        {
            case 1:
                qc.Z(0);
                break;
            case 2:
                qc.CZ(0, 1);
                break;
            case 3:
                // H-CCX-H turns a Toffoli into a controlled-controlled-Z.
                qc.H(last);
                qc.Toffoli(0, 1, last);
                qc.H(last);
                break;
            default:
                throw new NotSupportedException(
                    "Multi-controlled Z is implemented for up to 3 qubits. Supply your own "
                    + "diffusion oracle for wider searches.");
        }
    }

    /// <summary>
    /// Builds a quantum teleportation circuit that moves the state of qubit 0 onto qubit 2.
    /// </summary>
    /// <param name="prepareMessage">
    /// Prepares the state to teleport on qubit 0. Defaults to leaving it in |0⟩.
    /// </param>
    /// <returns>
    /// The completed circuit. Qubit 2 holds the state qubit 0 was prepared in, and classical
    /// bits 0 and 1 hold the Bell measurement outcome.
    /// </returns>
    /// <remarks>
    /// Uses mid-circuit measurement and classical feedforward, so the corrections are real
    /// conditional gates rather than the deferred-measurement equivalent.
    /// </remarks>
    public static QuantumCircuit Teleportation(Action<QuantumCircuit>? prepareMessage = null)
    {
        QuantumCircuit qc = new(3);

        prepareMessage?.Invoke(qc);

        // Entangle the pair shared between sender and receiver.
        qc.H(1);
        qc.CNOT(1, 2);

        // Bell-basis measurement of the message and the sender's half.
        qc.CNOT(0, 1);
        qc.H(0);
        qc.MeasureInto(0, 0);
        qc.MeasureInto(1, 1);

        // Corrections driven by the two classical outcomes.
        qc.When(1, 1, c => c.X(2));
        qc.When(0, 1, c => c.Z(2));

        return qc;
    }

    /// <summary>
    /// Builds a superdense coding circuit, sending two classical bits on one qubit.
    /// </summary>
    /// <param name="firstBit">The bit recovered from qubit 0.</param>
    /// <param name="secondBit">The bit recovered from qubit 1.</param>
    /// <returns>
    /// The completed circuit. <c>Measure(0, 1)</c> returns the two bits that were sent.
    /// </returns>
    public static QuantumCircuit SuperdenseCoding(bool firstBit, bool secondBit)
    {
        QuantumCircuit qc = new(2);

        // Shared entangled pair.
        qc.H(0);
        qc.CNOT(0, 1);

        // The sender encodes two bits by acting on their qubit alone. Z moves the Bell
        // state along the phase axis, which the decoder reads off qubit 0; X moves it along
        // the parity axis, which shows up on qubit 1.
        if (firstBit) qc.Z(0);
        if (secondBit) qc.X(0);

        // The receiver decodes.
        qc.CNOT(0, 1);
        qc.H(0);

        return qc;
    }
}
