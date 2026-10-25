using Polls.Application.Common.Constants;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Ideas.TestData;

public sealed record InvalidIdeaCase(string Name, string Title, string? Description)
{
    public override string ToString() => Name;
}

public static class CreateIdeaIntegrationTestData
{
    private const string ValidTitle = "More benches";

    public static TheoryData<InvalidIdeaCase> InvalidInputs => new()
    {
        new InvalidIdeaCase("EmptyTitle", string.Empty, null),
        new InvalidIdeaCase("TooLongTitle", new string('a', ValidationConstants.Idea.MaxTitleLength + 1), null),
        new InvalidIdeaCase(
            "TooLongDescription",
            ValidTitle,
            new string('a', ValidationConstants.Idea.MaxDescriptionLength + 1))
    };
}
