namespace Metrics.Application.Results;

public class Result
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = null!;
    public ErrorType ErrorType { get; set; }

    public static Result Success()
    {
        return new Result
        {
            IsSuccess = true
        };
    }

    public static Result Fail(string errorMessage, ErrorType errorType)
    {
        return new Result
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorType = errorType
        };
    }
}
