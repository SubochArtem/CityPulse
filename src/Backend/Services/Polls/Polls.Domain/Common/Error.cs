using Polls.Domain.Common.Enums;

namespace Polls.Domain.Common;

public record Error(string Description, ErrorType Type)
{
    public static Error NotFound(string description) =>
        new(description, ErrorType.NotFound);
    
    public static Error Conflict(string description) =>
        new(description, ErrorType.Conflict);

    public static Error Forbidden(string description) =>
        new(description, ErrorType.Forbidden);
    
    public static Error Undefined =>
        new(string.Empty,ErrorType.Undefined);
}
