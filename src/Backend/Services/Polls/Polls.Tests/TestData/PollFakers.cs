using Bogus;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;

namespace Polls.Tests.TestData;

internal static class PollFakers
{
    private const int Seed = 20261024;
    private const string DefaultLocale = "en";
    private const int MinBudget = 1;
    private const int MaxBudget = 1_000_000;

    public static Faker<Poll> Polls() =>
        new Faker<Poll>(DefaultLocale)
            .UseSeed(Seed)
            .StrictMode(true)
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.CreatedAt, _ => DateTimeOffset.UtcNow.AddHours(-1))
            .RuleFor(x => x.UpdatedAt, _ => null)
            .RuleFor(x => x.Title, f => f.Lorem.Sentence(3))
            .RuleFor(x => x.Description, f => f.Lorem.Paragraph())
            .RuleFor(x => x.CityId, f => f.Random.Guid())
            .RuleFor(x => x.EndsAt, _ => DateTimeOffset.UtcNow.AddDays(10))
            .RuleFor(x => x.Type, _ => PollType.UserVote)
            .RuleFor(x => x.BudgetAmount, f => f.Random.Decimal(MinBudget, MaxBudget))
            .RuleFor(x => x.Status, _ => PollStatus.Active)
            .Ignore(x => x.Ideas)
            .Ignore(x => x.Images);
}
