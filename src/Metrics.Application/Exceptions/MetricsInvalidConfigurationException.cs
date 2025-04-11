namespace Metrics.Application.Exceptions;

public class MetricsInvalidConfigurationException : Exception
{
    public MetricsInvalidConfigurationException() : base("Invalid Configuration.")
    {
    }

    public MetricsInvalidConfigurationException(string? message) : base(message)
    {
    }

    public MetricsInvalidConfigurationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

}
