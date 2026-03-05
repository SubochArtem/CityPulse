namespace Polls.Domain.Common;

public class Result<T>
{
    public T? Value { get; }
    public Error? Error { get; }
    public bool IsSuccess { get; }

    private Result(T value)
    {
        Value = value;
        IsSuccess = true;
    }

    private Result(Error error)
    {
        Error = error;
        IsSuccess = false;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onError)
    {
        return IsSuccess ? onSuccess(Value!) : onError(Error!);
    }
}
