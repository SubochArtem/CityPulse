namespace Polls.Domain.Ideas.Enums;

public enum IdeaAccessStatus
{
    Undefined = 0,
    Active = 1,
    Inactive = 2,
    RestrictedByContext = 3,
    RestrictedByAuthor = 4
}
