using Polls.Application.Common.Constants;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Ideas.TestData;

public sealed record IdeaFilterQueryCase(string Query)
{
    public override string ToString() => Query;
}

public static class GetIdeasIntegrationTestData
{
    private const int UnsupportedValue = 99;
    private const int InvalidPage = 0;
    private const int InvalidPageSize = 0;

    public static TheoryData<IdeaFilterQueryCase> InvalidFilters => new()
    {
        new IdeaFilterQueryCase($"AccessStatus={UnsupportedValue}"),
        new IdeaFilterQueryCase($"ApprovalStatus={UnsupportedValue}"),
        new IdeaFilterQueryCase($"Page={InvalidPage}"),
        new IdeaFilterQueryCase($"PageSize={InvalidPageSize}"),
        new IdeaFilterQueryCase($"PageSize={ValidationConstants.Pagination.MaxPageSize + 1}")
    };
}
