using Bogus;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;

namespace Polls.Tests.TestData;

internal static class IdeaFakers
{
    private const int Seed = 20261024;
    private const string DefaultLocale = "en";

    public static Faker<Idea> Ideas() =>
        new Faker<Idea>(DefaultLocale)
            .UseSeed(Seed)
            .StrictMode(true)
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.CreatedAt, f => f.Date.PastOffset().ToUniversalTime())
            .RuleFor(x => x.UpdatedAt, _ => null)
            .RuleFor(x => x.Title, f => f.Lorem.Sentence(3))
            .RuleFor(x => x.Description, f => f.Lorem.Paragraph())
            .RuleFor(x => x.UserId, f => f.Random.Guid())
            .RuleFor(x => x.PollId, f => f.Random.Guid())
            .RuleFor(x => x.AccessStatus, _ => IdeaAccessStatus.Active)
            .RuleFor(x => x.ApprovalStatus, _ => IdeaApprovalStatus.Pending)
            .Ignore(x => x.Poll)
            .Ignore(x => x.Images);
}
