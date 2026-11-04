using ErrorOr;
using Feedback.API.Features.Activity.GetActivity;
using Feedback.Tests.Infrastructure;
using Xunit;

namespace Feedback.Tests.Unit.Controllers.Tests;

public sealed class GetActivityControllerTests : ControllerTestsBase
{
    private readonly GetActivityController _sut;

    public GetActivityControllerTests()
    {
        _sut = new GetActivityController(Sender.Object);
    }

    [Fact]
    public async Task GetActivity_Always_QueriesWithSameFilterAndReturnsResult()
    {
        var filter = new ActivityFilter { CityId = Guid.NewGuid(), PageSize = 5 };
        ErrorOr<ActivityPage> expected = Error.Failure("Activity.Test", "failure");
        using var cts = new CancellationTokenSource();
        SetupSend<GetActivityQuery, ActivityPage>(expected);

        var result = await _sut.GetActivity(filter, cts.Token);

        Assert.Equal(expected, result);
        Assert.Same(filter, SentRequest<GetActivityQuery>().Filter);
        Assert.Equal(cts.Token, SentToken<GetActivityQuery>());
    }

    [Fact]
    public async Task GetActivity_PageFound_ReturnsPageFromSender()
    {
        var page = new ActivityPage([], null);
        ErrorOr<ActivityPage> expected = page;
        SetupSend<GetActivityQuery, ActivityPage>(expected);

        var result = await _sut.GetActivity(new ActivityFilter(), CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Same(page, result.Value);
    }
}
