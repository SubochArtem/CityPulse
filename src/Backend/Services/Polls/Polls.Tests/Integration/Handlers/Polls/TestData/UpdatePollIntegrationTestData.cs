using Polls.Application.Common.Constants;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Polls.TestData;

public sealed record InvalidUpdateCase(
    string Name,
    string Title,
    string? Description,
    double EndsInDays,
    decimal Budget)
{
    public override string ToString() => Name;
}

public static class UpdatePollIntegrationTestData
{
    private const string ValidTitle = "Updated vote";
    private const double ValidEndsInDays = 15;
    private const decimal ValidBudget = 2500;
    private const double PastDays = -1;

    public static TheoryData<InvalidUpdateCase> InvalidInputs => new()
    {
        Case("EmptyTitle", title: string.Empty),
        Case("TooLongTitle", title: new string('a', ValidationConstants.Poll.MaxTitleLength + 1)),
        Case("TooLongDescription", description: new string('a', ValidationConstants.Poll.MaxDescriptionLength + 1)),
        Case("EndsInThePast", endsInDays: PastDays),
        Case("ZeroBudget", budget: 0),
        Case("NegativeBudget", budget: -1),
        Case("TooHighBudget", budget: ValidationConstants.Poll.MaxBudgetAmount + 1)
    };

    private static InvalidUpdateCase Case(
        string name,
        string title = ValidTitle,
        string? description = null,
        double endsInDays = ValidEndsInDays,
        decimal budget = ValidBudget) =>
        new(name, title, description, endsInDays, budget);
}
