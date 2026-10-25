using CityPulse.Contracts.Querying.Pagination;
using ErrorOr;
using Feedback.API.Features.Feedback;
using Feedback.API.Features.Feedback.GetFilteredFeedback;
using Feedback.Tests.Infrastructure;
using Xunit;

namespace Feedback.Tests.Unit.Controllers.Tests;

public sealed class GetFilteredFeedbackControllerTests : ControllerTestsBase
{
    private readonly GetFilteredFeedbackController _sut;

    public GetFilteredFeedbackControllerTests()
    {
        _sut = WithUser(new GetFilteredFeedbackController(Sender.Object), UserId, UserCityId);
    }

    [Fact]
    public async Task GetFilteredFeedback_Always_QueriesWithSameFilterAndReturnsResult()
    {
        var filter = new FeedbackFilter { IdeaId = Guid.NewGuid(), Kind = FeedbackKind.Comment };
        ErrorOr<PagedList<FeedbackResponse>> expected = Error.Failure("Feedback.Test", "failure");
        using var cts = new CancellationTokenSource();
        SetupSend<GetFilteredFeedbackQuery, PagedList<FeedbackResponse>>(expected);

        var result = await _sut.GetFilteredFeedback(filter, cts.Token);

        Assert.Equal(expected, result);
        Assert.Same(filter, SentRequest<GetFilteredFeedbackQuery>().Filter);
        Assert.Equal(cts.Token, SentToken<GetFilteredFeedbackQuery>());
    }

    [Fact]
    public async Task GetFilteredFeedback_PageFound_ReturnsPageFromSender()
    {
        var page = new PagedList<FeedbackResponse>([], 1, 10, 0);
        ErrorOr<PagedList<FeedbackResponse>> expected = page;
        SetupSend<GetFilteredFeedbackQuery, PagedList<FeedbackResponse>>(expected);

        var result = await _sut.GetFilteredFeedback(new FeedbackFilter(), CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Same(page, result.Value);
    }
}
