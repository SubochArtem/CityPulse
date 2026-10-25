using Microsoft.Extensions.Logging;
using Moq;
using Polls.Application.Cities.Commands.ChangeStatus;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Cities;
using Polls.Domain.Cities.Enums;
using Polls.Domain.Common;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Handlers.Cities.TestData;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Cities.Tests;

public sealed class ChangeCityStatusCommandHandlerTests : HandlerTestsBase
{
    private const string UpdateStep = "update";
    private const string SaveStep = "save";
    private const string PollsStep = "polls";
    private const string IdeasStep = "ideas";
    private const string DatabaseFailureMessage = "database failure";

    private static readonly DateTimeOffset Now = new(2026, 10, 24, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new(MockBehavior.Strict);
    private readonly Mock<ILogger<ChangeCityStatusCommandHandler>> _logger = new();
    private readonly ChangeCityStatusCommandHandler _sut;

    public ChangeCityStatusCommandHandlerTests()
    {
        _sut = new ChangeCityStatusCommandHandler(UnitOfWork.Object, _dateTimeProvider.Object, _logger.Object);
    }

    private void SetupCity(City? city, Guid id) =>
        UnitOfWork.Cities
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

    private void SetupFlow(
        City city,
        PollStatus pollSource,
        PollStatus pollTarget,
        IdeaAccessStatus ideaSource,
        IdeaAccessStatus ideaTarget,
        List<string>? calls = null,
        FailingStep? failingStep = null,
        Exception? failure = null)
    {
        calls ??= [];
        SetupCity(city, city.Id);
        _dateTimeProvider.Setup(x => x.UtcNow).Returns(Now);

        UnitOfWork.Cities
            .Setup(x => x.Update(city))
            .Callback(() => calls.Add(UpdateStep));

        var save = UnitOfWork.Mock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()));
        if (failingStep == FailingStep.SaveCity)
            save.ThrowsAsync(failure!);
        else
            save.Callback(() => calls.Add(SaveStep)).Returns(Task.CompletedTask);

        var polls = UnitOfWork.Polls.Setup(x => x.UpdateStatusByCityAsync(
            city.Id,
            pollSource,
            pollTarget,
            Now,
            It.IsAny<CancellationToken>()));
        if (failingStep == FailingStep.UpdatePolls)
            polls.ThrowsAsync(failure!);
        else
            polls.Callback(() => calls.Add(PollsStep)).Returns(Task.CompletedTask);

        var ideas = UnitOfWork.Ideas.Setup(x => x.UpdateAccessStatusByCityAsync(
            city.Id,
            ideaSource,
            ideaTarget,
            It.IsAny<CancellationToken>()));
        if (failingStep == FailingStep.UpdateIdeas)
            ideas.ThrowsAsync(failure!);
        else
            ideas.Callback(() => calls.Add(IdeasStep)).ReturnsAsync(0);

        UnitOfWork.SetupExecuteInTransaction();
    }

    [Fact]
    public async Task Handle_CityDoesNotExist_ReturnsNotFoundErrorWithoutTransaction()
    {
        var id = Guid.NewGuid();
        SetupCity(null, id);

        var result = await _sut.Handle(new ChangeCityStatusCommand(id, CityStatus.Active), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(CityErrors.NotFound(id), result.Error);
        UnitOfWork.Mock.Verify(
            x => x.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_StatusIsAlreadySet_ReturnsSuccessWithoutChanges()
    {
        var city = CityFakers.Cities().Generate();
        city.Status = CityStatus.Active;
        SetupCity(city, city.Id);

        var result = await _sut.Handle(new ChangeCityStatusCommand(city.Id, CityStatus.Active), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CityStatus.Active, city.Status);
        UnitOfWork.Mock.Verify(
            x => x.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [MemberData(nameof(ChangeCityStatusTestData.UnsupportedStatuses), MemberType = typeof(ChangeCityStatusTestData))]
    public async Task Handle_StatusIsNotSupported_ReturnsInvalidStatusErrorAndLogsWarning(CityStatus status)
    {
        var city = CityFakers.Cities().Generate();
        city.Status = CityStatus.Active;
        SetupCity(city, city.Id);

        var result = await _sut.Handle(new ChangeCityStatusCommand(city.Id, status), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(CityErrors.InvalidStatus(status), result.Error);
        Assert.Equal(CityStatus.Active, city.Status);
        UnitOfWork.Mock.Verify(
            x => x.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [MemberData(nameof(ChangeCityStatusTestData.Transitions), MemberType = typeof(ChangeCityStatusTestData))]
    public async Task Handle_StatusChanges_UpdatesCityPollsAndIdeas(
        CityStatus currentStatus,
        CityStatus newStatus,
        PollStatus pollSource,
        PollStatus pollTarget,
        IdeaAccessStatus ideaSource,
        IdeaAccessStatus ideaTarget)
    {
        var city = CityFakers.Cities().Generate();
        city.Status = currentStatus;
        SetupFlow(city, pollSource, pollTarget, ideaSource, ideaTarget);

        var result = await _sut.Handle(new ChangeCityStatusCommand(city.Id, newStatus), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(newStatus, city.Status);
        UnitOfWork.Polls.Verify(
            x => x.UpdateStatusByCityAsync(city.Id, pollSource, pollTarget, Now, It.IsAny<CancellationToken>()),
            Times.Once);
        UnitOfWork.Ideas.Verify(
            x => x.UpdateAccessStatusByCityAsync(city.Id, ideaSource, ideaTarget, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_StatusChanges_RunsStepsInOrder()
    {
        var city = CityFakers.Cities().Generate();
        city.Status = CityStatus.Active;
        var calls = new List<string>();
        SetupFlow(
            city,
            PollStatus.Active,
            PollStatus.Suspended,
            IdeaAccessStatus.Active,
            IdeaAccessStatus.RestrictedByContext,
            calls);

        await _sut.Handle(new ChangeCityStatusCommand(city.Id, CityStatus.Inactive), CancellationToken.None);

        Assert.Equal([UpdateStep, SaveStep, PollsStep, IdeasStep], calls);
    }

    [Theory]
    [MemberData(nameof(ChangeCityStatusTestData.FailingSteps), MemberType = typeof(ChangeCityStatusTestData))]
    public async Task Handle_StepFails_LogsErrorAndReturnsDatabaseError(FailingStep failingStep)
    {
        var city = CityFakers.Cities().Generate();
        city.Status = CityStatus.Active;
        var failure = new InvalidOperationException(DatabaseFailureMessage);
        SetupFlow(
            city,
            PollStatus.Active,
            PollStatus.Suspended,
            IdeaAccessStatus.Active,
            IdeaAccessStatus.RestrictedByContext,
            failingStep: failingStep,
            failure: failure);

        var result = await _sut.Handle(new ChangeCityStatusCommand(city.Id, CityStatus.Inactive), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(CommonErrors.DatabaseError, result.Error);
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                failure,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
