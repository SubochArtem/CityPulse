using CityPulse.Contracts.Querying.Pagination;
using Polls.API.Controllers.User;
using Polls.API.Requests.Ideas;
using Polls.Application.Common.Models;
using Polls.Application.Ideas.Commands.CreateIdea;
using Polls.Application.Ideas.Commands.DeleteIdea;
using Polls.Application.Ideas.Commands.UpdateIdea;
using Polls.Application.Ideas.DTOs;
using Polls.Application.Ideas.Queries.GetIdeaById;
using Polls.Application.Ideas.Queries.GetIdeas;
using Polls.Application.Ideas.Queries.GetIdeaWithPoll;
using Polls.Domain.Common;
using Polls.Tests.Infrastructure;
using Xunit;
using MediatRUnit = MediatR.Unit;

namespace Polls.Tests.Unit.Controllers.Tests;

public sealed class IdeasControllerTests : ControllerTestsBase
{
    private const string Title = "Park renovation";
    private const string Description = "Plant more trees in the central park";
    private const string FirstFile = "first.jpg";
    private const string SecondFile = "second.png";

    private static readonly Error Failure = Error.Failure("failure");

    private readonly IdeasController _sut;

    public IdeasControllerTests()
    {
        _sut = WithUser(new IdeasController(Sender.Object), UserId, UserCityId);
    }

    [Fact]
    public async Task GetIdeas_Always_QueriesOnlyActiveIdeasWithSameFilterAndReturnsResult()
    {
        var filter = new IdeaFilter();
        var expected = Result<PagedList<IdeaDto>>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<GetIdeasQuery, PagedList<IdeaDto>>(expected);

        var result = await _sut.GetIdeas(filter, cts.Token);

        Assert.Same(expected, result);
        var query = SentRequest<GetIdeasQuery>();
        Assert.Same(filter, query.Filter);
        Assert.True(query.IncludeOnlyActive);
        Assert.Equal(cts.Token, SentToken<GetIdeasQuery>());
    }

    [Fact]
    public async Task GetIdeaById_Always_QueriesOnlyActiveIdeaAndReturnsResult()
    {
        var id = Guid.NewGuid();
        var expected = Result<IdeaDto>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<GetIdeaByIdQuery, IdeaDto>(expected);

        var result = await _sut.GetIdeaById(id, cts.Token);

        Assert.Same(expected, result);
        var query = SentRequest<GetIdeaByIdQuery>();
        Assert.Equal(id, query.Id);
        Assert.True(query.IncludeOnlyActive);
        Assert.Equal(cts.Token, SentToken<GetIdeaByIdQuery>());
    }

    [Fact]
    public async Task GetIdeaWithPoll_Always_QueriesOnlyActiveIdeaAndReturnsResult()
    {
        var id = Guid.NewGuid();
        var expected = Result<IdeaWithPollDto>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<GetIdeaWithPollQuery, IdeaWithPollDto>(expected);

        var result = await _sut.GetIdeaWithPoll(id, cts.Token);

        Assert.Same(expected, result);
        var query = SentRequest<GetIdeaWithPollQuery>();
        Assert.Equal(id, query.Id);
        Assert.True(query.IncludeOnlyActive);
        Assert.Equal(cts.Token, SentToken<GetIdeaWithPollQuery>());
    }

    [Fact]
    public async Task CreateIdea_Always_SendsRestrictedCommandWithUserAndCityFromClaims()
    {
        var pollId = Guid.NewGuid();
        var expected = Result<IdeaDto>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        var request = new CreateIdeaRequest
        {
            Title = Title,
            Description = Description,
            Images = FormFiles(FirstFile, SecondFile)
        };
        SetupSend<CreateIdeaCommand, IdeaDto>(expected);

        var result = await _sut.CreateIdea(pollId, request, cts.Token);

        Assert.Same(expected, result);
        var command = SentRequest<CreateIdeaCommand>();
        Assert.Equal(UserId, command.UserId);
        Assert.Equal(UserCityId, command.UserCityId);
        Assert.Equal(pollId, command.PollId);
        Assert.Equal(Title, command.Title);
        Assert.Equal(Description, command.Description);
        Assert.Equal([FirstFile, SecondFile], command.Images!.Select(image => image.FileName));
        Assert.False(command.BypassRestrictions);
        Assert.Equal(cts.Token, SentToken<CreateIdeaCommand>());
    }

    [Fact]
    public async Task CreateIdea_ClaimsAreMissing_SendsCommandWithEmptyUserAndCity()
    {
        var sut = WithUser(new IdeasController(Sender.Object));
        SetupSend<CreateIdeaCommand, IdeaDto>(Result<IdeaDto>.Failure(Failure));

        await sut.CreateIdea(Guid.NewGuid(), new CreateIdeaRequest { Title = Title }, CancellationToken.None);

        var command = SentRequest<CreateIdeaCommand>();
        Assert.Equal(Guid.Empty, command.UserId);
        Assert.Equal(Guid.Empty, command.UserCityId);
        Assert.False(command.BypassRestrictions);
    }

    [Fact]
    public async Task UpdateIdea_Always_SendsRestrictedCommandWithUserFromClaims()
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
        Assert.Equal(UserId, command.UserId);
        Assert.Equal(Title, command.Title);
        Assert.Equal(Description, command.Description);
        Assert.Equal([FirstFile], command.ImagesToAdd!.Select(image => image.FileName));
        Assert.Equal([imageToDelete], command.ImagesToDelete);
        Assert.False(command.BypassRestrictions);
        Assert.Equal(cts.Token, SentToken<UpdateIdeaCommand>());
    }

    [Fact]
    public async Task DeleteIdea_Always_SendsRestrictedCommandWithUserFromClaims()
    {
        var id = Guid.NewGuid();
        var expected = Result<MediatRUnit>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<DeleteIdeaCommand, MediatRUnit>(expected);

        var result = await _sut.DeleteIdea(id, cts.Token);

        Assert.Same(expected, result);
        var command = SentRequest<DeleteIdeaCommand>();
        Assert.Equal(id, command.Id);
        Assert.Equal(UserId, command.UserId);
        Assert.False(command.BypassRestrictions);
        Assert.Equal(cts.Token, SentToken<DeleteIdeaCommand>());
    }

    [Fact]
    public async Task DeleteIdea_ClaimsAreMissing_SendsCommandWithEmptyUser()
    {
        var sut = WithUser(new IdeasController(Sender.Object));
        SetupSend<DeleteIdeaCommand, MediatRUnit>(Result<MediatRUnit>.Failure(Failure));

        await sut.DeleteIdea(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(Guid.Empty, SentRequest<DeleteIdeaCommand>().UserId);
    }
}
