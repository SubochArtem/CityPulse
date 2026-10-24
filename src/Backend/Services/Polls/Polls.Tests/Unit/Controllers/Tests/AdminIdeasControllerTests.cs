using CityPulse.Contracts.Querying.Pagination;
using Polls.API.Controllers.Admin;
using Polls.API.Requests.Ideas;
using Polls.Application.Common.Models;
using Polls.Application.Ideas.Commands.ChangeAccessStatus;
using Polls.Application.Ideas.Commands.ChangeApprovalStatus;
using Polls.Application.Ideas.Commands.CreateIdea;
using Polls.Application.Ideas.Commands.DeleteIdea;
using Polls.Application.Ideas.Commands.UpdateIdea;
using Polls.Application.Ideas.DTOs;
using Polls.Application.Ideas.Queries.GetIdeaById;
using Polls.Application.Ideas.Queries.GetIdeas;
using Polls.Application.Ideas.Queries.GetIdeaWithPoll;
using Polls.Domain.Common;
using Polls.Domain.Ideas.Enums;
using Polls.Tests.Infrastructure;
using Xunit;
using MediatRUnit = MediatR.Unit;

namespace Polls.Tests.Unit.Controllers.Tests;

public sealed class AdminIdeasControllerTests : ControllerTestsBase
{
    private const string Title = "Park renovation";
    private const string Description = "Plant more trees in the central park";
    private const string FirstFile = "first.jpg";

    private static readonly Error Failure = Error.Failure("failure");

    private readonly AdminIdeasController _sut;

    public AdminIdeasControllerTests()
    {
        _sut = WithUser(new AdminIdeasController(Sender.Object), UserId, UserCityId);
    }

    [Fact]
    public async Task GetIdeas_Always_QueriesIdeasOfAnyStatusWithSameFilterAndReturnsResult()
    {
        var filter = new IdeaFilter();
        var expected = Result<PagedList<IdeaDto>>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<GetIdeasQuery, PagedList<IdeaDto>>(expected);

        var result = await _sut.GetIdeas(filter, cts.Token);

        Assert.Same(expected, result);
        var query = SentRequest<GetIdeasQuery>();
        Assert.Same(filter, query.Filter);
        Assert.False(query.IncludeOnlyActive);
        Assert.Equal(cts.Token, SentToken<GetIdeasQuery>());
    }

    [Fact]
    public async Task GetIdeaById_Always_QueriesIdeaOfAnyStatusAndReturnsResult()
    {
        var id = Guid.NewGuid();
        var expected = Result<IdeaDto>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<GetIdeaByIdQuery, IdeaDto>(expected);

        var result = await _sut.GetIdeaById(id, cts.Token);

        Assert.Same(expected, result);
        var query = SentRequest<GetIdeaByIdQuery>();
        Assert.Equal(id, query.Id);
        Assert.False(query.IncludeOnlyActive);
        Assert.Equal(cts.Token, SentToken<GetIdeaByIdQuery>());
    }

    [Fact]
    public async Task GetIdeaWithPoll_Always_QueriesIdeaOfAnyStatusAndReturnsResult()
    {
        var id = Guid.NewGuid();
        var expected = Result<IdeaWithPollDto>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<GetIdeaWithPollQuery, IdeaWithPollDto>(expected);

        var result = await _sut.GetIdeaWithPoll(id, cts.Token);

        Assert.Same(expected, result);
        var query = SentRequest<GetIdeaWithPollQuery>();
        Assert.Equal(id, query.Id);
        Assert.False(query.IncludeOnlyActive);
        Assert.Equal(cts.Token, SentToken<GetIdeaWithPollQuery>());
    }

    [Fact]
    public async Task CreateIdea_Always_SendsCommandWithUserFromClaimsAndBypassedRestrictions()
    {
        var pollId = Guid.NewGuid();
        var expected = Result<IdeaDto>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        var request = new CreateIdeaRequest
        {
            Title = Title,
            Description = Description,
            Images = FormFiles(FirstFile)
        };
        SetupSend<CreateIdeaCommand, IdeaDto>(expected);

        var result = await _sut.CreateIdea(pollId, request, cts.Token);

        Assert.Same(expected, result);
        var command = SentRequest<CreateIdeaCommand>();
        Assert.Equal(UserId, command.UserId);
        Assert.Equal(pollId, command.PollId);
        Assert.Equal(Title, command.Title);
        Assert.Equal(Description, command.Description);
        Assert.Equal([FirstFile], command.Images!.Select(image => image.FileName));
        Assert.Equal(Guid.Empty, command.UserCityId);
        Assert.True(command.BypassRestrictions);
        Assert.Equal(cts.Token, SentToken<CreateIdeaCommand>());
    }

    [Fact]
    public async Task UpdateIdea_Always_SendsCommandWithBypassedRestrictionsAndNoUser()
    {
        var id = Guid.NewGuid();
        var imageToDelete = Guid.NewGuid();
        var expected = Result<IdeaDto>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        var request = new UpdateIdeaRequest
        {
            Title = Title,
            Description = Description,
            ImagesToAdd = FormFiles(FirstFile),
            ImagesToDelete = [imageToDelete]
        };
        SetupSend<UpdateIdeaCommand, IdeaDto>(expected);

        var result = await _sut.UpdateIdea(id, request, cts.Token);

        Assert.Same(expected, result);
        var command = SentRequest<UpdateIdeaCommand>();
        Assert.Equal(id, command.Id);
        Assert.Equal(Title, command.Title);
        Assert.Equal(Description, command.Description);
        Assert.Equal([FirstFile], command.ImagesToAdd!.Select(image => image.FileName));
        Assert.Equal([imageToDelete], command.ImagesToDelete);
        Assert.Equal(Guid.Empty, command.UserId);
        Assert.True(command.BypassRestrictions);
        Assert.Equal(cts.Token, SentToken<UpdateIdeaCommand>());
    }

    [Fact]
    public async Task DeleteIdea_Always_SendsCommandWithBypassedRestrictionsAndNoUser()
    {
        var id = Guid.NewGuid();
        var expected = Result<MediatRUnit>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<DeleteIdeaCommand, MediatRUnit>(expected);

        var result = await _sut.DeleteIdea(id, cts.Token);

        Assert.Same(expected, result);
        var command = SentRequest<DeleteIdeaCommand>();
        Assert.Equal(id, command.Id);
        Assert.Equal(Guid.Empty, command.UserId);
        Assert.True(command.BypassRestrictions);
        Assert.Equal(cts.Token, SentToken<DeleteIdeaCommand>());
    }

    [Theory]
    [InlineData(IdeaAccessStatus.Active)]
    [InlineData(IdeaAccessStatus.Inactive)]
    [InlineData(IdeaAccessStatus.RestrictedByAuthor)]
    public async Task ChangeAccessStatus_Always_SendsCommandWithRequestedStatus(IdeaAccessStatus status)
    {
        var id = Guid.NewGuid();
        var expected = Result<MediatRUnit>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<ChangeIdeaAccessStatusCommand, MediatRUnit>(expected);

        var result = await _sut.ChangeAccessStatus(
            id,
            new ChangeIdeaAccessStatusRequest { NewIdeaAccessStatus = status },
            cts.Token);

        Assert.Same(expected, result);
        var command = SentRequest<ChangeIdeaAccessStatusCommand>();
        Assert.Equal(id, command.Id);
        Assert.Equal(status, command.NewIdeaAccessStatus);
        Assert.Equal(cts.Token, SentToken<ChangeIdeaAccessStatusCommand>());
    }

    [Theory]
    [InlineData(IdeaApprovalStatus.Approved)]
    [InlineData(IdeaApprovalStatus.Rejected)]
    [InlineData(IdeaApprovalStatus.Pending)]
    public async Task ChangeApprovalStatus_Always_SendsCommandWithRequestedStatus(IdeaApprovalStatus status)
    {
        var id = Guid.NewGuid();
        var expected = Result<MediatRUnit>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<ChangeIdeaApprovalStatusCommand, MediatRUnit>(expected);

        var result = await _sut.ChangeApprovalStatus(
            id,
            new ChangeIdeaApprovalStatusRequest { NewApprovalStatus = status },
            cts.Token);

        Assert.Same(expected, result);
        var command = SentRequest<ChangeIdeaApprovalStatusCommand>();
        Assert.Equal(id, command.Id);
        Assert.Equal(status, command.NewApprovalStatus);
        Assert.Equal(cts.Token, SentToken<ChangeIdeaApprovalStatusCommand>());
    }
}
