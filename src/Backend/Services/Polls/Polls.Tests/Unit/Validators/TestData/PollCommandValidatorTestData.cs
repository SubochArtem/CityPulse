using Polls.Application.Common.Constants;
using Xunit;

namespace Polls.Tests.Unit.Validators.TestData;

public static class PollCommandValidatorTestData
{
    public static TheoryData<string?> EmptyTitles => new()
    {
        null,
        "",
        "   "
    };

    public static TheoryData<string> TooLongTitles => new()
    {
        new string('a', ValidationConstants.Poll.MaxTitleLength + 1),
        new string('a', ValidationConstants.Poll.MaxTitleLength * 2)
    };

    public static TheoryData<string> ValidTitles => new()
    {
        "a",
        "Budget for the new playground",
        new string('a', ValidationConstants.Poll.MaxTitleLength)
    };

    public static TheoryData<string> TooLongDescriptions => new()
    {
        new string('a', ValidationConstants.Poll.MaxDescriptionLength + 1),
        new string('a', ValidationConstants.Poll.MaxDescriptionLength * 2)
    };

    public static TheoryData<string?> ValidDescriptions => new()
    {
        null,
        "",
        "Choose how to spend the city budget",
        new string('a', ValidationConstants.Poll.MaxDescriptionLength)
    };

    public static TheoryData<decimal> NonPositiveBudgets => new()
    {
        ValidationConstants.Poll.MinBudgetAmount,
        -0.01m,
        -1,
        decimal.MinValue
    };

    public static TheoryData<decimal> TooHighBudgets => new()
    {
        ValidationConstants.Poll.MaxBudgetAmount + 0.01m,
        decimal.MaxValue
    };

    public static TheoryData<decimal> ValidBudgets => new()
    {
        0.01m,
        1,
        ValidationConstants.Poll.MaxBudgetAmount
    };

    public static TheoryData<TimeSpan> PastEndDateOffsets => new()
    {
        TimeSpan.FromSeconds(-1),
        TimeSpan.FromMinutes(-1),
        TimeSpan.FromDays(-1),
        TimeSpan.FromDays(-365)
    };
}
