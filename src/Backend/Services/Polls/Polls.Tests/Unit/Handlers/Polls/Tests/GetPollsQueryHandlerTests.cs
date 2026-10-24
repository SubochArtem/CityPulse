using CityPulse.Contracts.Querying.Pagination;
using Moq;
using Polls.Application.Common.Models;
using Polls.Application.Polls.Queries.GetPolls;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Handlers.Polls.TestData;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Polls.Tests;

public sealed class GetPollsQueryHandlerTests : HandlerTestsBase
{
    private const int Page = 2;
    private const int PageSize = 3;
    private const int TotalCount = 8;

    private readonly GetPollsQueryHandler _sut;

    public GetPollsQueryHandlerTests()
    {
        _sut = new GetPollsQueryHandler(UnitOfWork.Object, Mapper);
    }

    private void SetupRepository(List<Poll>? polls = null) =>
        UnitOfWork.Polls
            .Setup(x => x.GetFilteredAsync(It.IsAny<PollFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Poll>(polls ?? [], Page, PageSize, TotalCount));

    [Theory]
    [MemberData(nameof(PollHandlerTestData.NonActiveFilterStatuses), MemberType = typeof(PollHandlerTestData))]
    public async Task Handle_OnlyActiveRequested_ForcesActiveStatusInFilter(PollStatus? requestedStatus)
    {
        var filter = new PollFilter { Status = requestedStatus };
        SetupRepository();

        await _sut.Handle(new GetPollsQuery(filter, IncludeOnlyActive: true), CancellationToken.None);

        Assert.Equal(PollStatus.Active, filter.Status);
        UnitOfWork.Polls.Verify(
            x => x.GetFilteredAsync(filter, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [MemberData(nameof(PollHandlerTestData.AnyFilterStatuses), MemberType = typeof(PollHandlerTestData))]
    public async Task Handle_AllRequested_KeepsFilterStatusUnchanged(PollStatus? requestedStatus)
    {
        var filter = new PollFilter { Status = requestedStatus };
        SetupRepository();

        await _sut.Handle(new GetPollsQuery(filter, IncludeOnlyActive: false), CancellationToken.None);

        Assert.Equal(requestedStatus, filter.Status);
    }

    [Fact]
    public async Task Handle_PollsFound_ReturnsMappedPageKeepingPaging()
    {
        var polls = PollFakers.Polls().Generate(PageSize);
        SetupRepository(polls);

        var result = await _sut.Handle(new GetPollsQuery(new PollFilter()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(polls.Select(p => p.Id), result.Value!.Items.Select(p => p.Id));
        Assert.Equal(Page, result.Value.Page);
        Assert.Equal(PageSize, result.Value.PageSize);
        Assert.Equal(TotalCount, result.Value.TotalCount);
    }

    [Fact]
    public async Task Handle_NoPollsFound_ReturnsEmptyItems()
    {
        SetupRepository();

        var result = await _sut.Handle(new GetPollsQuery(new PollFilter()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
    }
}
