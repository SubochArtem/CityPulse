using Moq;
using Polls.Application.Ideas.Queries.GetIdeaById;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Handlers.TestData;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Tests;

public sealed class GetIdeaByIdQueryHandlerTests : HandlerTestsBase
{
    private readonly GetIdeaByIdQueryHandler _sut;

    public GetIdeaByIdQueryHandlerTests()
    {
        _sut = new GetIdeaByIdQueryHandler(UnitOfWork.Object, Mapper);
    }

    private void SetupIdea(Idea? idea, Guid id) =>
        UnitOfWork.Ideas
            .Setup(x => x.GetByIdWithImagesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(idea);

    [Fact]
    public async Task Handle_IdeaIsActive_ReturnsMappedIdea()
    {
        var idea = IdeaFakers.Ideas().Generate();
        SetupIdea(idea, idea.Id);

        var result = await _sut.Handle(new GetIdeaByIdQuery(idea.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(idea.Id, result.Value!.Id);
        Assert.Equal(idea.Title, result.Value.Title);
    }

    [Fact]
    public async Task Handle_IdeaDoesNotExist_ReturnsNotFoundError()
    {
        var id = Guid.NewGuid();
        SetupIdea(null, id);

        var result = await _sut.Handle(new GetIdeaByIdQuery(id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(IdeaErrors.NotFound(id), result.Error);
    }

    [Theory]
    [MemberData(nameof(IdeaHandlerTestData.NotActiveStatuses), MemberType = typeof(IdeaHandlerTestData))]
    public async Task Handle_IdeaIsNotActiveAndOnlyActiveRequested_ReturnsNotFoundError(IdeaAccessStatus status)
    {
        var idea = IdeaFakers.Ideas().Generate();
        idea.AccessStatus = status;
        SetupIdea(idea, idea.Id);

        var result = await _sut.Handle(new GetIdeaByIdQuery(idea.Id, IncludeOnlyActive: true), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(IdeaErrors.NotFound(idea.Id), result.Error);
    }

    [Theory]
    [MemberData(nameof(IdeaHandlerTestData.NotActiveStatuses), MemberType = typeof(IdeaHandlerTestData))]
    public async Task Handle_IdeaIsNotActiveAndAllRequested_ReturnsIdea(IdeaAccessStatus status)
    {
        var idea = IdeaFakers.Ideas().Generate();
        idea.AccessStatus = status;
        SetupIdea(idea, idea.Id);

        var result = await _sut.Handle(new GetIdeaByIdQuery(idea.Id, IncludeOnlyActive: false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal((int)status, result.Value!.AccessStatus);
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesItToRepository()
    {
        var idea = IdeaFakers.Ideas().Generate();
        using var cts = new CancellationTokenSource();
        UnitOfWork.Ideas
            .Setup(x => x.GetByIdWithImagesAsync(idea.Id, cts.Token))
            .ReturnsAsync(idea);

        await _sut.Handle(new GetIdeaByIdQuery(idea.Id), cts.Token);

        UnitOfWork.Ideas.Verify(x => x.GetByIdWithImagesAsync(idea.Id, cts.Token), Times.Once);
    }
}
