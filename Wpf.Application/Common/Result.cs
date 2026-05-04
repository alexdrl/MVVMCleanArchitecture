namespace Wpf.Application.Common;

/// <summary>Represents the outcome of an operation, carrying a value or an error message.</summary>
public class Result<T>
{
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public bool IsSuccess { get; }

    private Result(T value)
    {
        Value = value;
        IsSuccess = true;
    }

    private Result(string errorMessage)
    {
        ErrorMessage = errorMessage;
        IsSuccess = false;
    }

    /// <summary>Creates a successful result with the given value.</summary>
    public static Result<T> Success(T value) => new(value);

    /// <summary>Creates a failed result with the given error message.</summary>
    public static Result<T> Failure(string errorMessage) => new(errorMessage);
}
