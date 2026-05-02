using Domain.Errors;

namespace Domain;

public class Result
{
    protected Result(bool isSuccess, DomainError? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("Success result cannot have an error.");
        if (!isSuccess && error is null)
            throw new InvalidOperationException("Failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public DomainError? Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(DomainError error) => new(false, error);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, null);
    public static Result<TValue> Failure<TValue>(DomainError error) => new(default, false, error);
}