using CityPulse.Contracts.Querying.Pagination;
using Moq;
using Polls.Application.Common.Models;
using Polls.Application.Ideas.Queries.GetIdeas;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Handlers.Ideas.TestData;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Ideas.Tests;

public sealed class GetIdeasQueryHandlerTests : HandlerTestsBase
{
    private const int Page = 2;
    private const int PageSize = 3;
    private const int TotalCount = 8;

    private readonly GetIdeasQueryHandler _sut;

    public GetIdeasQueryHandlerTests()
    {
        _sut = new GetIdeasQueryHandler(UnitOfWork.Object, Mapper);
    }

    private void SetupRepository(List<Idea>? ideas = null) =>
        UnitOfWork.Ideas
            .Setup(x => x.GetFilteredAsync(It.IsAny<IdeaFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Idea>(ideas ?? [], Page, PageSize, TotalCount));

    [Theory]
    [MemberData(nameof(IdeaHandlerTestData.NonActiveFilterStatuses), MemberType = typeof(IdeaHandlerTestData))]
    public async Task Handle_OnlyActiveRequested_ForcesActiveAccessStatusInFilter(IdeaAccessStatus? requestedStatus)
    {
        var filter = new IdeaFilter { AccessStatus = requestedStatus };
        SetupRepository();

        await _sut.Handle(new GetIdeasQuery(filter, IncludeOnlyActive: true), CancellationToken.None);

        Assert.Equal(IdeaAccessStatus.Active, filter.AccessStatus);
        UnitOfWork.Ideas.Verify(
            x => x.GetFilteredAsync(filter, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [MemberData(nameof(IdeaHandlerTestData.AnyFilterStatuses), MemberType = typeof(IdeaHandlerTestData))]
    public async Task Handle_AllRequested_KeepsFilterAccessStatusUnchanged(IdeaAccessStatus? requestedStatus)
    {
        var filter = new IdeaFilter { AccessStatus = requestedStatus };
        SetupRepository();

        await _sut.Handle(new GetIdeasQuery(filter, IncludeOnlyActive: false), CancellationToken.None);

        Assert.Equal(requestedStatus, filter.AccessStatus);
    }

    [Fact]
    public async Task Handle_IdeasFound_ReturnsMappedPageKeepingPaging()
    {
        var ideas = IdeaFakers.Ideas().Generate(PageSize);
        SetupRepository(ideas);

        var result = await _sut.Handle(new GetIdeasQuery(new IdeaFilter()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ideas.Select(i => i.Id), result.Value!.Items.Select(i => i.Id));
        Assert.Equal(Page, result.Value.Page);
        Assert.Equal(PageSize, result.Value.PageSize);
        Assert.Equal(TotalCount, result.Value.TotalCount);
    }

    [Fact]
    public async Task Handle_NoIdeasFound_ReturnsEmptyItems()
    {
        SetupRepository();

        var result = await _sut.Handle(new GetIdeasQuery(new IdeaFilter()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
    }
}
