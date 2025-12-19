using JetBrains.Annotations;

namespace Showdown.NET.Exceptions;

/// <summary>
/// Exception thrown when Showdown.NET initialization fails.
/// </summary>
[PublicAPI]
public class ShowdownInitializationException : ShowdownException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShowdownInitializationException"/> class.
    /// </summary>
    public ShowdownInitializationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShowdownInitializationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ShowdownInitializationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShowdownInitializationException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ShowdownInitializationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
