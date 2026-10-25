using Polls.Application.Common.Constants;
using Polls.Domain.Polls.Enums;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Polls.TestData;

public sealed record InvalidPollCase(
    string Name,
    string Title,
    string? Description,
    double EndsInDays,
    decimal Budget,
    PollType Type)
{
    public override string ToString() => Name;
}

public static class CreatePollIntegrationTestData
{
    private const string ValidTitle = "Budget vote";
    private const double ValidEndsInDays = 10;
    private const decimal ValidBudget = 1000;
    private const double PastDays = -1;
    private const int UnsupportedType = 99;

    public static TheoryData<InvalidPollCase> InvalidInputs => new()
    {
        Case("EmptyTitle", title: string.Empty),
        Case("TooLongTitle", title: new string('a', ValidationConstants.Poll.MaxTitleLength + 1)),
        Case("TooLongDescription", description: new string('a', ValidationConstants.Poll.MaxDescriptionLength + 1)),
        Case("EndsInThePast", endsInDays: PastDays),
        Case("TooShortDuration", endsInDays: ValidationConstants.Poll.MinDurationDays - 1),
        Case("TooLongDuration", endsInDays: ValidationConstants.Poll.MaxDurationDays + 1),
        Case("ZeroBudget", budget: 0),
        Case("NegativeBudget", budget: -1),
        Case("TooHighBudget", budget: ValidationConstants.Poll.MaxBudgetAmount + 1),
        Case("UnsupportedType", type: (PollType)UnsupportedType)
    };

    private static InvalidPollCase Case(
        string name,
        string title = ValidTitle,
        string? description = null,
        double endsInDays = ValidEndsInDays,
        decimal budget = ValidBudget,
        PollType type = PollType.UserVote) =>
        new(name, title, description, endsInDays, budget, type);
}
