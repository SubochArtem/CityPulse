using ErrorOr;
using Feedback.API.Features.Feedback.DeleteFeedback;
using Feedback.Tests.Infrastructure;
using Xunit;

namespace Feedback.Tests.Unit.Controllers.Tests;

public sealed class AdminDeleteFeedbackControllerTests : ControllerTestsBase
{
    private readonly AdminDeleteFeedbackController _sut;

    public AdminDeleteFeedbackControllerTests()
    {
        _sut = WithUser(new AdminDeleteFeedbackController(Sender.Object), UserId, UserCityId);
    }

    private static ErrorOr<Deleted> Failure() => Error.Failure("Feedback.Test", "failure");

    [Fact]
    public async Task DeleteFeedback_Always_SendsCommandWithBypassedRestrictionsAndUserFromClaims()
    {
        var id = Guid.NewGuid();
        var expected = Failure();
        using var cts = new CancellationTokenSource();
        SetupSend<DeleteFeedbackCommand, Deleted>(expected);

        var result = await _sut.DeleteFeedback(id, cts.Token);

        Assert.Equal(expected, result);
        var command = SentRequest<DeleteFeedbackCommand>();
        Assert.Equal(UserId, command.UserId);
        Assert.Equal(id, command.Id);
        Assert.True(command.BypassRestrictions);
        Assert.Equal(cts.Token, SentToken<DeleteFeedbackCommand>());
    }

    [Fact]
    public async Task DeleteFeedback_ClaimsAreMissing_SendsCommandWithEmptyUser()
    {
        var sut = WithUser(new AdminDeleteFeedbackController(Sender.Object));
        SetupSend<DeleteFeedbackCommand, Deleted>(Failure());

        await sut.DeleteFeedback(Guid.NewGuid(), CancellationToken.None);

        var command = SentRequest<DeleteFeedbackCommand>();
        Assert.Equal(Guid.Empty, command.UserId);
        Assert.True(command.BypassRestrictions);
    }
}
