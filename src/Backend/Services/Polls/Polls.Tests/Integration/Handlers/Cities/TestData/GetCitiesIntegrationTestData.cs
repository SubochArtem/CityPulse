using Polls.Application.Common.Constants;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Cities.TestData;

public sealed record FilterQueryCase(string Query)
{
    public override string ToString() => Query;
}

public static class GetCitiesIntegrationTestData
{
    private const int UnsupportedStatus = 99;
    private const int InvalidPage = 0;
    private const int InvalidPageSize = 0;

    public static TheoryData<FilterQueryCase> InvalidFilters => new()
    {
        new FilterQueryCase($"Status={UnsupportedStatus}"),
        new FilterQueryCase($"Page={InvalidPage}"),
        new FilterQueryCase($"PageSize={InvalidPageSize}"),
        new FilterQueryCase($"PageSize={ValidationConstants.Pagination.MaxPageSize + 1}")
    };
}
