namespace Qubit.NET.Utilities;

/// <summary>
/// Defines an abstraction for a source of randomness,
/// allowing different implementations such as pseudo-random or custom random generators.
/// </summary>
public interface IRandomSource
{
    /// <summary>
    /// Returns the next random double value in the range [0.0, 1.0).
    /// </summary>
    double NextDouble();
}