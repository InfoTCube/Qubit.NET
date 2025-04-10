namespace Qubit.NET.Utilities;

/// <summary>
/// Exception thrown when a qubit index is out of the valid range.
/// </summary>
[Serializable]
public class QubitIndexOutOfRangeException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QubitIndexOutOfRangeException"/> class.
    /// </summary>
    public QubitIndexOutOfRangeException() 
        : base("The specified qubit index is out of range.") {}
    
    /// <summary>
    /// Initializes a new instance of the <see cref="QubitIndexOutOfRangeException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public QubitIndexOutOfRangeException(string message)
        : base(message) {}
    
    /// <summary>
    /// Initializes a new instance of the <see cref="QubitIndexOutOfRangeException"/> class
    /// with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public QubitIndexOutOfRangeException(string message, Exception inner)
        : base(message, inner) {}
}