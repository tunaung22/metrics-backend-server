namespace Metrics.Application.Results;

public enum ErrorType
{
    None = 0,
    NotFound,
    ConcurrencyConflict,
    InvalidArgument,
    ValidationError,
    DatabaseError,
    DuplicateKey,
    UnexpectedError
}

public class Result<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string ErrorMessage { get; set; } = null!;
    public ErrorType ErrorType { get; set; }

    public static Result<T> Ok(T data)
    {
        return new Result<T>
        {
            Success = true,
            Data = data
        };
    }

    public static Result<T> Fail(string errorMessage, ErrorType errorType)
    {
        return new Result<T>
        {
            Success = false,
            ErrorMessage = errorMessage,
            ErrorType = errorType
        };
    }

}
