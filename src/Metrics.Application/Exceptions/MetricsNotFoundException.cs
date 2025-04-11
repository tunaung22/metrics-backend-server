namespace Metrics.Application.Exceptions;

public class MetricsNotFoundException : Exception
{
    public MetricsNotFoundException() : base("Duplicate content.")
    {
    }

    public MetricsNotFoundException(string? message) : base(message)
    {
    }

    public MetricsNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

}
