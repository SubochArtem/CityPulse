using Moq;
using Polls.Application.Common.Interfaces;
using Polls.Application.Cities.Commands.DeleteCity;
using Polls.Domain.Cities;
using Polls.Domain.Images;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Cities.Tests;

public sealed class DeleteCityCommandHandlerTests : HandlerTestsBase
{
    private const string CancelStep = "cancel";
    private const string DeleteStep = "delete";
    private const string SaveStep = "save";

    private readonly Mock<IPollScheduler> _scheduler = new(MockBehavior.Strict);
    private readonly DeleteCityCommandHandler _sut;

    public DeleteCityCommandHandlerTests()
    {
        _sut = new DeleteCityCommandHandler(UnitOfWork.Object, _scheduler.Object);
    }

    private void SetupPollIds(Guid cityId, params Guid[] pollIds)
    {
        UnitOfWork.Polls
            .Setup(x => x.GetIdsByCityAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pollIds);

        foreach (var pollId in pollIds)
            _scheduler
                .Setup(x => x.CancelAsync(pollId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
    }

    private void VerifyCancelCalls(Times times) =>
        _scheduler.Verify(x => x.CancelAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), times);

    private void SetupCity(City? city, Guid id) =>
        UnitOfWork.Cities
            .Setup(x => x.GetByIdWithImagesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

    [Fact]
    public async Task Handle_CityDoesNotExist_ReturnsNotFoundErrorAndDeletesNothing()
    {
        var id = Guid.NewGuid();
        SetupCity(null, id);

        var result = await _sut.Handle(new DeleteCityCommand(id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(CityErrors.NotFound(id), result.Error);
        UnitOfWork.Cities.Verify(x => x.Delete(It.IsAny<City>()), Times.Never);
        VerifyCancelCalls(Times.Never());
    }

    [Fact]
    public async Task Handle_CityWithoutImages_DeletesCityAndSavesWithoutQueueingFiles()
    {
        var city = CityFakers.Cities().Generate();
        SetupCity(city, city.Id);
        SetupPollIds(city.Id);
        UnitOfWork.Cities.Setup(x => x.Delete(city));
        UnitOfWork.SetupSaveChanges();

        var result = await _sut.Handle(new DeleteCityCommand(city.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        UnitOfWork.Cities.Verify(x => x.Delete(city), Times.Once);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        UnitOfWork.DeletedImages.Verify(x => x.AddRange(It.IsAny<IEnumerable<DeletedImage>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CityHasPolls_CancelsEachScheduledJobBeforeDeletingAndSaving()
    {
        var city = CityFakers.Cities().Generate();
        var pollIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var calls = new List<string>();
        SetupCity(city, city.Id);
        UnitOfWork.Polls
            .Setup(x => x.GetIdsByCityAsync(city.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pollIds);
        foreach (var pollId in pollIds)
            _scheduler
                .Setup(x => x.CancelAsync(pollId, It.IsAny<CancellationToken>()))
                .Callback(() => calls.Add(CancelStep))
                .Returns(Task.CompletedTask);
        UnitOfWork.Cities
            .Setup(x => x.Delete(city))
            .Callback(() => calls.Add(DeleteStep));
        UnitOfWork.Mock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add(SaveStep))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(new DeleteCityCommand(city.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal([CancelStep, CancelStep, DeleteStep, SaveStep], calls);
        foreach (var pollId in pollIds)
            _scheduler.Verify(x => x.CancelAsync(pollId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CityHasNoPolls_CancelsNothing()
    {
        var city = CityFakers.Cities().Generate();
        SetupCity(city, city.Id);
        SetupPollIds(city.Id);
        UnitOfWork.Cities.Setup(x => x.Delete(city));
        UnitOfWork.SetupSaveChanges();

        await _sut.Handle(new DeleteCityCommand(city.Id), CancellationToken.None);

        VerifyCancelCalls(Times.Never());
    }

    [Fact]
    public async Task Handle_CityWithImages_QueuesImageFilesForCleanupBeforeDeleting()
    {
        var city = CityFakers.CityWithImages("first.jpg", "second.png");
        List<DeletedImage>? queued = null;
        SetupCity(city, city.Id);
        SetupPollIds(city.Id);
        UnitOfWork.DeletedImages
            .Setup(x => x.AddRange(It.IsAny<IEnumerable<DeletedImage>>()))
            .Callback<IEnumerable<DeletedImage>>(entries => queued = entries.ToList());
        UnitOfWork.Cities.Setup(x => x.Delete(city));
        UnitOfWork.SetupSaveChanges();

        var result = await _sut.Handle(new DeleteCityCommand(city.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(queued);
        Assert.Equal(["first.jpg", "second.png"], queued.Select(entry => entry.FileName));
        UnitOfWork.Cities.Verify(x => x.Delete(city), Times.Once);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesItToRepositoryAndSave()
    {
        var city = CityFakers.Cities().Generate();
        var pollId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        UnitOfWork.Cities
            .Setup(x => x.GetByIdWithImagesAsync(city.Id, cts.Token))
            .ReturnsAsync(city);
        UnitOfWork.Polls
            .Setup(x => x.GetIdsByCityAsync(city.Id, cts.Token))
            .ReturnsAsync([pollId]);
        _scheduler
            .Setup(x => x.CancelAsync(pollId, cts.Token))
            .Returns(Task.CompletedTask);
        UnitOfWork.Cities.Setup(x => x.Delete(city));
        UnitOfWork.Mock.Setup(x => x.SaveChangesAsync(cts.Token)).Returns(Task.CompletedTask);

        await _sut.Handle(new DeleteCityCommand(city.Id), cts.Token);

        UnitOfWork.Cities.Verify(x => x.GetByIdWithImagesAsync(city.Id, cts.Token), Times.Once);
        UnitOfWork.Polls.Verify(x => x.GetIdsByCityAsync(city.Id, cts.Token), Times.Once);
        _scheduler.Verify(x => x.CancelAsync(pollId, cts.Token), Times.Once);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(cts.Token), Times.Once);
    }
}
