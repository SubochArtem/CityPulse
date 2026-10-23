using Polls.Domain.Common.Enums;
using Polls.Domain.Ideas;
using Xunit;

namespace Polls.Tests.Unit.Ideas.Tests;

public sealed class IdeaErrorsTests
{
    [Fact]
    public void NotFound_WithId_ReturnsNotFoundErrorMentioningId()
    {
        var id = Guid.NewGuid();

        var error = IdeaErrors.NotFound(id);

        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.Contains(id.ToString(), error.Description);
    }

    [Fact]
    public void AlreadyApproved_WithId_ReturnsConflictErrorMentioningId()
    {
        var id = Guid.NewGuid();

        var error = IdeaErrors.AlreadyApproved(id);

        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Contains(id.ToString(), error.Description);
    }

    [Fact]
    public void NotOwner_WithId_ReturnsForbiddenErrorMentioningId()
    {
        var id = Guid.NewGuid();

        var error = IdeaErrors.NotOwner(id);

        Assert.Equal(ErrorType.Forbidden, error.Type);
        Assert.Contains(id.ToString(), error.Description);
    }
}
