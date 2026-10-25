using Polls.Domain.Common.Enums;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Tests.Unit.Errors.TestData;
using Xunit;

namespace Polls.Tests.Unit.Errors.Tests;

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

    [Theory]
    [MemberData(nameof(IdeaErrorsTestData.AllAccessStatuses), MemberType = typeof(IdeaErrorsTestData))]
    public void InvalidStatus_AnyAccessStatus_ReturnsConflictErrorMentioningStatus(IdeaAccessStatus status)
    {
        var error = IdeaErrors.InvalidStatus(status);

        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Contains(status.ToString(), error.Description);
    }

    [Theory]
    [MemberData(nameof(IdeaErrorsTestData.AllApprovalStatuses), MemberType = typeof(IdeaErrorsTestData))]
    public void InvalidStatus_AnyApprovalStatus_ReturnsConflictErrorMentioningStatus(IdeaApprovalStatus status)
    {
        var error = IdeaErrors.InvalidStatus(status);

        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Contains(status.ToString(), error.Description);
    }
}
