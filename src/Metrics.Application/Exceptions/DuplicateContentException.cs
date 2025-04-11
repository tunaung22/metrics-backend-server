namespace Metrics.Application.Exceptions;

public class DuplicateContentException : Exception
{
    public DuplicateContentException() : base("Duplicate content.")
    {
    }

    public DuplicateContentException(string? message) : base(message)
    {
    }

    public DuplicateContentException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

}
