using Users.Business.Constants;
using Xunit;

namespace Users.Tests.Integration.Users.TestData;

public static class GetUsersPaginationValidationTestData
{
    public const string PageKey = "Page";
    public const string PageSizeKey = "PageSize";

    public static TheoryData<string, string, string> InvalidPagination => new()
    {
        { "Page=0", PageKey, ValidationConstants.Pagination.PageInvalid },
        { "Page=-1", PageKey, ValidationConstants.Pagination.PageInvalid },
        { "PageSize=0", PageSizeKey, ValidationConstants.Pagination.PageSizeTooSmall },
        { "PageSize=-5", PageSizeKey, ValidationConstants.Pagination.PageSizeTooSmall },
        { "PageSize=101", PageSizeKey, ValidationConstants.Pagination.PageSizeTooLarge }
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
