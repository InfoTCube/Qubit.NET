namespace Qubit.NET.Utilities;

/// <summary>
/// A pseudo-random number generator based on .NET's built-in <see cref="System.Random"/>.
/// Used as the default randomness source in the simulator.
/// </summary>
public class PseudoRandomSource : IRandomSource
{
    private readonly Random _random = new();
    
    /// <summary>
    /// Returns the next pseudo-random double value in the range [0.0, 1.0).
    /// </summary>
    public double NextDouble() => _random.NextDouble();
}