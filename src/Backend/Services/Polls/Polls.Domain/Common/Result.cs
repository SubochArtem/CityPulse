namespace Polls.Domain.Common;

public abstract class Result
{
    protected Result()
    {
        Errors = [];
    }

    protected Result(params Error[] errors)
    {
        Errors = errors;
    }

    public IReadOnlyList<Error> Errors { get; }

    public bool IsSuccess => Errors.Count == 0;
    
    public Error Error => Errors.Count > 0 ? Errors[0] : Error.Undefined;
}

public class Result<T> : Result
{
    private Result(T value)
    {
        Value = value;
    }

    private Result(params Error[] errors) : base(errors){}

    public T? Value { get; }

    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }

    public static Result<T> Failure(params Error[] errors)
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
}
