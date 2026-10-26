using Users.Business.Constants;
using Xunit;

namespace Users.Tests.Integration.Users.TestData;

public static class GetUsersPaginationValidationTestData
{
    public static TheoryData<string, string> InvalidPagination => new()
    {
        { "Page=0", ValidationConstants.Pagination.PageInvalid },
        { "Page=-1", ValidationConstants.Pagination.PageInvalid },
        { "PageSize=0", ValidationConstants.Pagination.PageSizeTooSmall },
        { "PageSize=-5", ValidationConstants.Pagination.PageSizeTooSmall },
        { "PageSize=101", ValidationConstants.Pagination.PageSizeTooLarge }
    };

    public static TheoryData<string> BoundaryPagination => new()
    {
        "Page=1&PageSize=1",
        "Page=1&PageSize=100"
    };

    public static TheoryData<string> NonNumericPagination => new()
    {
        "Page=abc",
        "PageSize=abc"
    };
}
