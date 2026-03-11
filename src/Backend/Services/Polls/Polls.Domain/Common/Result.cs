namespace Polls.Domain.Common;

public abstract class Result
{
    protected Result()
    {
        IsSuccess = true;
    }

    protected Result(IReadOnlyList<Error> errors)
    {
        Errors = errors;
        IsSuccess = false;
    }

    public IReadOnlyList<Error> Errors { get; } = [];
    public bool IsSuccess { get; }
}

public class Result<T> : Result
{
    private Result(T value)
    {
        Value = value;
    }

    private Result(IReadOnlyList<Error> errors) : base(errors){}

    public T? Value { get; }

    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }

    public static Result<T> Failure(Error error)
    {
        return new Result<T>([error]);
    }

    public static Result<T> Failure(IReadOnlyList<Error> errors)
    {
        return new Result<T>(errors);
    }

    public static implicit operator Result<T>(T value)
    {
        return Success(value);
    }

    public static implicit operator Result<T>(Error error)
    {
        return Failure(error);
    }

    public static implicit operator Result<T>(List<Error> errors)
    {
        return Failure(errors);
    }

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<IReadOnlyList<Error>, TResult> onError)
    {
        return IsSuccess ? onSuccess(Value!) : onError(Errors);
    }
}
