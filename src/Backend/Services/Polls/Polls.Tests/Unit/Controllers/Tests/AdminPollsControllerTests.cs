using CityPulse.Contracts.Querying.Pagination;
using Polls.API.Controllers.Admin;
using Polls.API.Requests.Polls;
using Polls.Application.Common.Models;
using Polls.Application.Polls.Commands.ChangeStatus;
using Polls.Application.Polls.Commands.CreatePoll;
using Polls.Application.Polls.Commands.DeletePoll;
using Polls.Application.Polls.Commands.UpdatePoll;
using Polls.Application.Polls.DTOs;
using Polls.Application.Polls.Queries.GetPollById;
using Polls.Application.Polls.Queries.GetPolls;
using Polls.Application.Polls.Queries.GetPollWithIdeas;
using Polls.Domain.Common;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Xunit;
using MediatRUnit = MediatR.Unit;

namespace Polls.Tests.Unit.Controllers.Tests;

public sealed class AdminPollsControllerTests : ControllerTestsBase
{
    private const string Title = "Budget for the new playground";
    private const string Description = "Choose how to spend the city budget";
    private const decimal Budget = 1000;
    private const string FirstFile = "first.jpg";

    private static readonly Error Failure = Error.Failure("failure");
    private static readonly DateTimeOffset EndsAt = new(2027, 1, 15, 12, 0, 0, TimeSpan.Zero);

    private readonly AdminPollsController _sut;

    public AdminPollsControllerTests()
    {
        _sut = WithUser(new AdminPollsController(Sender.Object), UserId, UserCityId);
    }

    [Fact]
    public async Task GetPolls_Always_QueriesPollsOfAnyStatusWithSameFilterAndReturnsResult()
    {
        var filter = new PollFilter();
        var expected = Result<PagedList<PollDto>>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<GetPollsQuery, PagedList<PollDto>>(expected);

        var result = await _sut.GetPolls(filter, cts.Token);

        Assert.Same(expected, result);
        var query = SentRequest<GetPollsQuery>();
        Assert.Same(filter, query.Filter);
        Assert.False(query.IncludeOnlyActive);
        Assert.Equal(cts.Token, SentToken<GetPollsQuery>());
    }

    [Fact]
    public async Task GetPollById_Always_QueriesPollOfAnyStatusAndReturnsResult()
    {
        var id = Guid.NewGuid();
        var expected = Result<PollDto>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<GetPollByIdQuery, PollDto>(expected);

        var result = await _sut.GetPollById(id, cts.Token);

        Assert.Same(expected, result);
        var query = SentRequest<GetPollByIdQuery>();
        Assert.Equal(id, query.Id);
        Assert.False(query.IncludeOnlyActive);
        Assert.Equal(cts.Token, SentToken<GetPollByIdQuery>());
    }

    [Fact]
    public async Task GetPollWithIdeas_Always_QueriesPollOfAnyStatusAndReturnsResult()
    {
        var id = Guid.NewGuid();
        var expected = Result<PollWithIdeasDto>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<GetPollWithIdeasQuery, PollWithIdeasDto>(expected);

        var result = await _sut.GetPollWithIdeas(id, cts.Token);

        Assert.Same(expected, result);
        var query = SentRequest<GetPollWithIdeasQuery>();
        Assert.Equal(id, query.Id);
        Assert.False(query.IncludeOnlyActive);
        Assert.Equal(cts.Token, SentToken<GetPollWithIdeasQuery>());
    }

    [Fact]
    public async Task CreatePoll_Always_SendsCommandWithBypassedRestrictionsAndNoUserCity()
    {
        var cityId = Guid.NewGuid();
        var expected = Result<PollDto>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        var request = new CreatePollRequest
        {
            Title = Title,
            Description = Description,
            Type = PollType.ManagerVote,
            EndsAt = EndsAt,
            BudgetAmount = Budget,
            Images = FormFiles(FirstFile)
        };
        SetupSend<CreatePollCommand, PollDto>(expected);

        var result = await _sut.CreatePoll(cityId, request, cts.Token);

        Assert.Same(expected, result);
        var command = SentRequest<CreatePollCommand>();
        Assert.Equal(cityId, command.CityId);
        Assert.Equal(Title, command.Title);
        Assert.Equal(Description, command.Description);
        Assert.Equal(PollType.ManagerVote, command.Type);
        Assert.Equal(EndsAt, command.EndsAt);
        Assert.Equal(Budget, command.BudgetAmount);
        Assert.Equal([FirstFile], command.Images!.Select(image => image.FileName));
        Assert.Equal(Guid.Empty, command.UserCityId);
        Assert.True(command.BypassRestrictions);
        Assert.Equal(cts.Token, SentToken<CreatePollCommand>());
    }

    [Fact]
    public async Task UpdatePoll_Always_SendsCommandWithBypassedRestrictionsAndNoUserCity()
    {
        var id = Guid.NewGuid();
        var imageToDelete = Guid.NewGuid();
        var expected = Result<PollDto>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        var request = new UpdatePollRequest
        {
            Title = Title,
            Description = Description,
            EndsAt = EndsAt,
            BudgetAmount = Budget,
            ImagesToAdd = FormFiles(FirstFile),
            ImagesToDelete = [imageToDelete]
        };
        SetupSend<UpdatePollCommand, PollDto>(expected);

        var result = await _sut.UpdatePoll(id, request, cts.Token);

        Assert.Same(expected, result);
        var command = SentRequest<UpdatePollCommand>();
        Assert.Equal(id, command.Id);
        Assert.Equal(Title, command.Title);
        Assert.Equal(Description, command.Description);
        Assert.Equal(EndsAt, command.EndsAt);
        Assert.Equal(Budget, command.BudgetAmount);
        Assert.Equal([FirstFile], command.ImagesToAdd!.Select(image => image.FileName));
        Assert.Equal([imageToDelete], command.ImagesToDelete);
        Assert.Equal(Guid.Empty, command.UserCityId);
        Assert.True(command.BypassRestrictions);
        Assert.Equal(cts.Token, SentToken<UpdatePollCommand>());
    }

    [Fact]
    public async Task DeletePoll_Always_SendsCommandWithBypassedRestrictionsAndNoUserCity()
    {
        var id = Guid.NewGuid();
        var expected = Result<MediatRUnit>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<DeletePollCommand, MediatRUnit>(expected);

        var result = await _sut.DeletePoll(id, cts.Token);

        Assert.Same(expected, result);
        var command = SentRequest<DeletePollCommand>();
        Assert.Equal(id, command.Id);
        Assert.Equal(Guid.Empty, command.UserCityId);
        Assert.True(command.BypassRestrictions);
        Assert.Equal(cts.Token, SentToken<DeletePollCommand>());
    }

    [Theory]
    [InlineData(PollStatus.Active)]
    [InlineData(PollStatus.Suspended)]
    [InlineData(PollStatus.Finished)]
    public async Task ChangeStatus_Always_SendsCommandWithRequestedStatus(PollStatus status)
    {
        var id = Guid.NewGuid();
        var expected = Result<MediatRUnit>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<ChangePollStatusCommand, MediatRUnit>(expected);

        var result = await _sut.ChangeStatus(id, new ChangePollStatusRequest { NewStatus = status }, cts.Token);

        Assert.Same(expected, result);
        var command = SentRequest<ChangePollStatusCommand>();
        Assert.Equal(id, command.Id);
        Assert.Equal(status, command.NewStatus);
        Assert.Equal(cts.Token, SentToken<ChangePollStatusCommand>());
    }
}
