using Polls.Domain.Ideas.Enums;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Ideas.TestData;

public static class GetIdeaIntegrationTestData
{
    public static TheoryData<IdeaAccessStatus> RestrictedAccessStatuses => new()
    {
        IdeaAccessStatus.Inactive,
        IdeaAccessStatus.RestrictedByContext,
        IdeaAccessStatus.RestrictedByAuthor
    };
}
