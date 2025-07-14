namespace CustomerServiceApp.Application.Common.Models;

/// <summary>
/// Represents the result of an operation with success/failure state and optional data
/// </summary>
/// <typeparam name="T">Type of the result data</typeparam>
public class Result<T>
{
    private Result(bool isSuccess, T? data, string? error, IEnumerable<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        Errors = errors?.ToList() ?? new List<string>();
    }
    
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Data { get; }
    public string? Error { get; }
    public IReadOnlyList<string> Errors { get; }
    
    public static Result<T> Success(T data) => new(true, data, null);
    public static Result<T> Failure(string error) => new(false, default, error);
    public static Result<T> Failure(IEnumerable<string> errors) => new(false, default, null, errors);
}

/// <summary>
/// Represents the result of an operation without return data
/// </summary>
public class Result
{
    private Result(bool isSuccess, string? error, IEnumerable<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors?.ToList() ?? new List<string>();
    }
    
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public IReadOnlyList<string> Errors { get; }
    
    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
    public static Result Failure(IEnumerable<string> errors) => new(false, null, errors);
}
