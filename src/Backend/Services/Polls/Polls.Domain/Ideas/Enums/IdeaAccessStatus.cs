namespace Polls.Domain.Ideas.Enums;

public enum IdeaAccessStatus
{
    Undefined = 0,
    Active = 1,
    Inactive = 2,
    Restricted = 3,
    RestrictedByContext = 4,
    RestrictedByAuthor = 5
}
