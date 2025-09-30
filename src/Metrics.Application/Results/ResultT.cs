namespace Metrics.Application.Results;

public class ResultT<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string ErrorMessage { get; set; } = null!;
    public ErrorType ErrorType { get; set; }

    public static ResultT<T> Success(T data)
    {
        return new ResultT<T>
        {
            IsSuccess = true,
            Data = data
        };
    }

    public static ResultT<T> Fail(string errorMessage, ErrorType errorType)
    {
        return new ResultT<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorType = errorType
        };
    }

}



// public class Result<T>
// {
//     public bool Success { get; set; }
//     public T? Data { get; set; }
//     public string Error { get; set; } = null!;

//     public static Result<T> Ok(T data) => new Result<T> { Success = true, Data = data };
//     public static Result<T> Fail(string error) => new Result<T> { Success = false, Error = error };
// }
