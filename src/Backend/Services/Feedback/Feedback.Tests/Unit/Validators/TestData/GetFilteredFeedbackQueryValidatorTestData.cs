using Xunit;

namespace Feedback.Tests.Unit.Validators.TestData;

public static class GetFilteredFeedbackQueryValidatorTestData
{
    private const int MaxSearchTermLength = 100;
    private const int MaxPageSize = 100;

    private static readonly DateTimeOffset Earlier = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Later = new(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

    public static TheoryData<int> InvalidPages => new()
    {
        0,
        -1,
        int.MinValue
    };

    public static TheoryData<int> ValidPages => new()
    {
        1,
        2,
        int.MaxValue
    };

    public static TheoryData<int> InvalidPageSizes => new()
    {
        0,
        -1,
        MaxPageSize + 1,
        int.MaxValue
    };

    public static TheoryData<int> ValidPageSizes => new()
    {
        1,
        10,
        MaxPageSize
    };

    public static TheoryData<string> TooLongSearchTerms => new()
    {
        new string('a', MaxSearchTermLength + 1),
        new string('a', MaxSearchTermLength * 2)
    };

    public static TheoryData<string?> ValidSearchTerms => new()
    {
        null,
        "",
        "great",
        new string('a', MaxSearchTermLength)
    };

    public static TheoryData<DateTimeOffset?, DateTimeOffset?> InvalidDateRanges => new()
    {
        { Later, Earlier },
        { Later, Earlier.AddTicks(-1) }
    };

    public static TheoryData<DateTimeOffset?, DateTimeOffset?> ValidDateRanges => new()
    {
        { Earlier, Later },
        { Earlier, Earlier },
        { Earlier, null },
        { null, Later },
        { null, null }
    };
}
