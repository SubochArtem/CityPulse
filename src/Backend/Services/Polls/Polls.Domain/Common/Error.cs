using Polls.Domain.Common.Enums;

namespace Polls.Domain.Common;

public record Error(string Description, ErrorType Type)
{
    public static Error NotFound(string description) =>
        new(description, ErrorType.NotFound);

    public static Error Validation(string description) =>
        new(description, ErrorType.Validation);

    public static Error Conflict(string description) =>
        new(description, ErrorType.Conflict);

    public static Error Failure(string description) =>
        new(description, ErrorType.Failure);
}
