using Polls.Application.Common.Constants;
using Polls.Domain.Cities.Enums;
using Xunit;

namespace Polls.Tests.Unit.Validators.TestData;

public static class CityFilterValidatorTestData
{
    public static TheoryData<int> InvalidPages => new()
    {
        0,
        -1,
        int.MinValue
    };

    public static TheoryData<int> ValidPages => new()
    {
        ValidationConstants.Pagination.MinPage,
        2,
        int.MaxValue
    };

    public static TheoryData<int> TooSmallPageSizes => new()
    {
        ValidationConstants.Pagination.MinPageSize - 1,
        -1,
        int.MinValue
    };

    public static TheoryData<int> TooLargePageSizes => new()
    {
        ValidationConstants.Pagination.MaxPageSize + 1,
        int.MaxValue
    };

    public static TheoryData<int> ValidPageSizes => new()
    {
        ValidationConstants.Pagination.MinPageSize,
        10,
        ValidationConstants.Pagination.MaxPageSize
    };

    public static TheoryData<string> TooLongSearchTerms => new()
    {
        new string('a', ValidationConstants.Search.MaxSearchTermLength + 1),
        new string('a', ValidationConstants.Search.MaxSearchTermLength * 2)
    };

    public static TheoryData<string?> ValidSearchTerms => new()
    {
        null,
        "",
        "   ",
        "warsaw",
        new string('a', ValidationConstants.Search.MaxSearchTermLength),
        new string(' ', ValidationConstants.Search.MaxSearchTermLength * 2)
    };

    public static TheoryData<CityStatus> UnknownStatuses => new()
    {
        (CityStatus)99,
        (CityStatus)(-1)
    };

    public static TheoryData<CityStatus?> ValidStatuses => new()
    {
        null,
        CityStatus.Undefined,
        CityStatus.Active,
        CityStatus.Inactive
    };
}
