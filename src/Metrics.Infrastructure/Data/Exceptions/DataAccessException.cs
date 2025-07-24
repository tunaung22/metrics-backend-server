namespace Metrics.Infrastructure.Data.Exceptions;

public class DataAccessException : Exception
{
    // Basic constructors
    public DataAccessException() : base() { }
    public DataAccessException(string message) : base(message) { }
    public DataAccessException(string message, Exception innerException)
        : base(message, innerException) { }

    // Optional: Add error code support
    public string? ErrorCode { get; init; }
}
