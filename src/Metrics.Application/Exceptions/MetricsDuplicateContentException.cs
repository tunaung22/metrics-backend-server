namespace Metrics.Application.Exceptions;

public class MetricsDuplicateContentException : Exception
{
    public MetricsDuplicateContentException() : base("Duplicate content.")
    {
    }

    public MetricsDuplicateContentException(string? message) : base(message)
    {
    }

    public MetricsDuplicateContentException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

}
