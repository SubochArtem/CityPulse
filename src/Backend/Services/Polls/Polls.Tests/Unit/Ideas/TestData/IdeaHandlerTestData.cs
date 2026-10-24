using Polls.Domain.Ideas.Enums;
using Xunit;

namespace Polls.Tests.Unit.Ideas.TestData;

public static class IdeaHandlerTestData
{
    public static TheoryData<IdeaAccessStatus> NotActiveStatuses => new()
    {
        IdeaAccessStatus.Undefined,
        IdeaAccessStatus.Inactive,
        IdeaAccessStatus.RestrictedByContext,
        IdeaAccessStatus.RestrictedByAuthor
    };

    public static TheoryData<IdeaAccessStatus?> AnyFilterStatuses => new()
    {
        null,
        IdeaAccessStatus.Undefined,
        IdeaAccessStatus.Active,
        IdeaAccessStatus.Inactive,
        IdeaAccessStatus.RestrictedByContext,
        IdeaAccessStatus.RestrictedByAuthor
    };

    public static TheoryData<IdeaAccessStatus?> NonActiveFilterStatuses => new()
    {
        null,
        IdeaAccessStatus.Undefined,
        IdeaAccessStatus.Inactive,
        IdeaAccessStatus.RestrictedByContext,
        IdeaAccessStatus.RestrictedByAuthor
    };
}
