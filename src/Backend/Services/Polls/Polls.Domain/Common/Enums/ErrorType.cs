namespace Polls.Domain.Common.Enums;

public enum ErrorType
{
    Unknown = 0,
    NotFound = 1,
    Conflict = 2,
    Validation = 3,
    Forbidden = 4,
    Failure = 5
}
