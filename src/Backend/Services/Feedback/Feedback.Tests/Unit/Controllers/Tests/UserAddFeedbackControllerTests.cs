using ErrorOr;
using Feedback.API.Features.Feedback;
using Feedback.API.Features.Feedback.AddFeedback;
using Feedback.Tests.Infrastructure;
using Xunit;

namespace Feedback.Tests.Unit.Controllers.Tests;

public sealed class UserAddFeedbackControllerTests : ControllerTestsBase
{
    private const string CommentText = "Great idea!";

    private readonly UserAddFeedbackController _sut;

    public UserAddFeedbackControllerTests()
    {
        _sut = WithUser(new UserAddFeedbackController(Sender.Object), UserId, UserCityId);
    }

    private static ErrorOr<FeedbackResponse> Failure() => Error.Failure("Feedback.Test", "failure");

    [Fact]
    public async Task AddFeedback_Comment_SendsRestrictedCommandWithUserAndCityFromClaims()
    {
        var ideaId = Guid.NewGuid();
        var expected = Failure();
        using var cts = new CancellationTokenSource();
        SetupSend<AddFeedbackCommand, FeedbackResponse>(expected);

        var result = await _sut.AddFeedback(ideaId, new AddFeedbackRequest(CommentText), cts.Token);

        Assert.Equal(expected, result);
        var command = SentRequest<AddFeedbackCommand>();
        Assert.Equal(UserId, command.UserId);
        Assert.Equal(UserCityId, command.UserCityId);
        Assert.Equal(ideaId, command.IdeaId);
        Assert.Equal(CommentText, command.Text);
        Assert.False(command.BypassRestrictions);
        Assert.Equal(cts.Token, SentToken<AddFeedbackCommand>());
    }

    [Fact]
    public async Task AddFeedback_Like_SendsCommandWithoutText()
    {
        SetupSend<AddFeedbackCommand, FeedbackResponse>(Failure());

        await _sut.AddFeedback(Guid.NewGuid(), new AddFeedbackRequest(null), CancellationToken.None);

        var command = SentRequest<AddFeedbackCommand>();
        Assert.Null(command.Text);
        Assert.False(command.BypassRestrictions);
    }

    [Fact]
    public async Task AddFeedback_ClaimsAreMissing_SendsCommandWithEmptyUserAndCity()
    {
        var sut = WithUser(new UserAddFeedbackController(Sender.Object));
        SetupSend<AddFeedbackCommand, FeedbackResponse>(Failure());

        await sut.AddFeedback(Guid.NewGuid(), new AddFeedbackRequest(CommentText), CancellationToken.None);

        var command = SentRequest<AddFeedbackCommand>();
        Assert.Equal(Guid.Empty, command.UserId);
        Assert.Equal(Guid.Empty, command.UserCityId);
        Assert.False(command.BypassRestrictions);
    }
}
