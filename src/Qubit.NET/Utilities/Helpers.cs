namespace Qubit.NET.Utilities;

/// <summary>
/// Provides utility methods for working with complex numbers.
/// </summary>
internal static class Helpers
{
    /// <summary>
    /// Formats a complex number as a string. If the imaginary part is zero, only the real part is included. 
    /// If the real part is zero, only the imaginary part is included. Both parts are included if they are non-zero.
    /// </summary>
    /// <param name="real">The real part of the complex number.</param>
    /// <param name="imaginary">The imaginary part of the complex number.</param>
    /// <returns>A string representation of the complex number, in the form: "real + imaginary*i" or "real" or "imaginary*i".</returns>
    internal static string FormatComplex(double real, double imaginary)
    {
        if (imaginary == 0 && real == 0)
            return String.Empty;
        
        if (imaginary == 0)
            return real.ToString();
        
        if (real == 0)
            return $"{imaginary}i";
        
        return $"{real} + {imaginary}i";
    }
}