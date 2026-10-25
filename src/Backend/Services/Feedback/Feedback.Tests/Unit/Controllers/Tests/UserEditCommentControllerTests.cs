using ErrorOr;
using Feedback.API.Features.Feedback;
using Feedback.API.Features.Feedback.EditComment;
using Feedback.Tests.Infrastructure;
using Xunit;

namespace Feedback.Tests.Unit.Controllers.Tests;

public sealed class UserEditCommentControllerTests : ControllerTestsBase
{
    private const string NewText = "Edited comment";

    private readonly UserEditCommentController _sut;

    public UserEditCommentControllerTests()
    {
        _sut = WithUser(new UserEditCommentController(Sender.Object), UserId, UserCityId);
    }

    private static ErrorOr<FeedbackResponse> Failure() => Error.Failure("Feedback.Test", "failure");

    [Fact]
    public async Task EditComment_Always_SendsRestrictedCommandWithUserFromClaims()
    {
        var id = Guid.NewGuid();
        var expected = Failure();
        using var cts = new CancellationTokenSource();
        SetupSend<EditCommentCommand, FeedbackResponse>(expected);

        var result = await _sut.EditComment(id, new EditCommentRequest(NewText), cts.Token);

        Assert.Equal(expected, result);
        var command = SentRequest<EditCommentCommand>();
        Assert.Equal(UserId, command.UserId);
        Assert.Equal(id, command.Id);
        Assert.Equal(NewText, command.Text);
        Assert.False(command.BypassRestrictions);
        Assert.Equal(cts.Token, SentToken<EditCommentCommand>());
    }

    [Fact]
    public async Task EditComment_ClaimsAreMissing_SendsCommandWithEmptyUser()
    {
        var sut = WithUser(new UserEditCommentController(Sender.Object));
        SetupSend<EditCommentCommand, FeedbackResponse>(Failure());

        await sut.EditComment(Guid.NewGuid(), new EditCommentRequest(NewText), CancellationToken.None);

        var command = SentRequest<EditCommentCommand>();
        Assert.Equal(Guid.Empty, command.UserId);
        Assert.False(command.BypassRestrictions);
    }
}
