using CityPulse.Contracts.Querying.Pagination;
using Polls.API.Controllers.User;
using Polls.API.Requests.Polls;
using Polls.Application.Common.Models;
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

public sealed class PollsControllerTests : ControllerTestsBase
{
    private const string Title = "Budget for the new playground";
    private const string Description = "Choose how to spend the city budget";
    private const decimal Budget = 1000;
    private const string FirstFile = "first.jpg";

    private static readonly Error Failure = Error.Failure("failure");
    private static readonly DateTimeOffset EndsAt = new(2027, 1, 15, 12, 0, 0, TimeSpan.Zero);

    private readonly PollsController _sut;

    public PollsControllerTests()
    {
        _sut = WithUser(new PollsController(Sender.Object), UserId, UserCityId);
    }

    [Fact]
    public async Task GetPolls_Always_QueriesOnlyActivePollsWithSameFilterAndReturnsResult()
    {
        var filter = new PollFilter();
        var expected = Result<PagedList<PollDto>>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<GetPollsQuery, PagedList<PollDto>>(expected);

        var result = await _sut.GetPolls(filter, cts.Token);

        Assert.Same(expected, result);
        var query = SentRequest<GetPollsQuery>();
        Assert.Same(filter, query.Filter);
        Assert.True(query.IncludeOnlyActive);
        Assert.Equal(cts.Token, SentToken<GetPollsQuery>());
    }

    [Fact]
    public async Task GetPollById_Always_QueriesOnlyActivePollAndReturnsResult()
    {
        var id = Guid.NewGuid();
        var expected = Result<PollDto>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<GetPollByIdQuery, PollDto>(expected);

        var result = await _sut.GetPollById(id, cts.Token);

        Assert.Same(expected, result);
        var query = SentRequest<GetPollByIdQuery>();
        Assert.Equal(id, query.Id);
        Assert.True(query.IncludeOnlyActive);
        Assert.Equal(cts.Token, SentToken<GetPollByIdQuery>());
    }

    [Fact]
    public async Task GetPollWithIdeas_Always_QueriesOnlyActivePollAndReturnsResult()
    {
        var id = Guid.NewGuid();
        var expected = Result<PollWithIdeasDto>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<GetPollWithIdeasQuery, PollWithIdeasDto>(expected);

        var result = await _sut.GetPollWithIdeas(id, cts.Token);

        Assert.Same(expected, result);
        var query = SentRequest<GetPollWithIdeasQuery>();
        Assert.Equal(id, query.Id);
        Assert.True(query.IncludeOnlyActive);
        Assert.Equal(cts.Token, SentToken<GetPollWithIdeasQuery>());
    }

    [Fact]
    public async Task CreatePoll_Always_SendsRestrictedCommandWithCityFromClaims()
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
        Assert.Equal(UserCityId, command.UserCityId);
        Assert.Equal(Title, command.Title);
        Assert.Equal(Description, command.Description);
        Assert.Equal(PollType.ManagerVote, command.Type);
        Assert.Equal(EndsAt, command.EndsAt);
        Assert.Equal(Budget, command.BudgetAmount);
        Assert.Equal([FirstFile], command.Images!.Select(image => image.FileName));
        Assert.False(command.BypassRestrictions);
        Assert.Equal(cts.Token, SentToken<CreatePollCommand>());
    }

    [Fact]
    public async Task CreatePoll_ClaimsAreMissing_SendsCommandWithEmptyUserCity()
    {
        var sut = WithUser(new PollsController(Sender.Object));
        var request = new CreatePollRequest
        {
            Title = Title,
            Type = PollType.UserVote,
            EndsAt = EndsAt,
            BudgetAmount = Budget
        };
        SetupSend<CreatePollCommand, PollDto>(Result<PollDto>.Failure(Failure));

        await sut.CreatePoll(Guid.NewGuid(), request, CancellationToken.None);

        var command = SentRequest<CreatePollCommand>();
        Assert.Equal(Guid.Empty, command.UserCityId);
        Assert.False(command.BypassRestrictions);
    }

    [Fact]
    public async Task UpdatePoll_Always_SendsRestrictedCommandWithCityFromClaims()
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
        Assert.Equal(UserCityId, command.UserCityId);
        Assert.Equal(Title, command.Title);
        Assert.Equal(Description, command.Description);
        Assert.Equal(EndsAt, command.EndsAt);
        Assert.Equal(Budget, command.BudgetAmount);
        Assert.Equal([FirstFile], command.ImagesToAdd!.Select(image => image.FileName));
        Assert.Equal([imageToDelete], command.ImagesToDelete);
        Assert.False(command.BypassRestrictions);
        Assert.Equal(cts.Token, SentToken<UpdatePollCommand>());
    }

    [Fact]
    public async Task DeletePoll_Always_SendsRestrictedCommandWithCityFromClaims()
    {
        var id = Guid.NewGuid();
        var expected = Result<MediatRUnit>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<DeletePollCommand, MediatRUnit>(expected);

        var result = await _sut.DeletePoll(id, cts.Token);

        Assert.Same(expected, result);
        var command = SentRequest<DeletePollCommand>();
        Assert.Equal(id, command.Id);
        Assert.Equal(UserCityId, command.UserCityId);
        Assert.False(command.BypassRestrictions);
        Assert.Equal(cts.Token, SentToken<DeletePollCommand>());
    }

    [Fact]
    public async Task DeletePoll_ClaimsAreMissing_SendsCommandWithEmptyUserCity()
    {
        var sut = WithUser(new PollsController(Sender.Object));
        SetupSend<DeletePollCommand, MediatRUnit>(Result<MediatRUnit>.Failure(Failure));

        await sut.DeletePoll(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(Guid.Empty, SentRequest<DeletePollCommand>().UserCityId);
    }
}
