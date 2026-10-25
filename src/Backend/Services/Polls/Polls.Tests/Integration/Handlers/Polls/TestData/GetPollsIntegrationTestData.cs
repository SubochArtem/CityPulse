using Polls.Application.Common.Constants;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Polls.TestData;

public sealed record PollFilterQueryCase(string Query)
{
    public override string ToString() => Query;
}

public static class GetPollsIntegrationTestData
{
    private const int UnsupportedValue = 99;
    private const int InvalidPage = 0;
    private const int InvalidPageSize = 0;

    public static TheoryData<PollFilterQueryCase> InvalidFilters => new()
    {
        new PollFilterQueryCase($"Status={UnsupportedValue}"),
        new PollFilterQueryCase($"Type={UnsupportedValue}"),
        new PollFilterQueryCase($"CityId={Guid.Empty}"),
        new PollFilterQueryCase($"Page={InvalidPage}"),
        new PollFilterQueryCase($"PageSize={InvalidPageSize}"),
        new PollFilterQueryCase($"PageSize={ValidationConstants.Pagination.MaxPageSize + 1}")
    };
}
