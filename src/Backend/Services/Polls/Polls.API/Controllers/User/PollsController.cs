using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polls.API.Common.Extensions;
using Polls.API.Requests.Polls;
using Polls.Application.Common.Models;
using Polls.Application.Polls.Commands.CreatePoll;
using Polls.Application.Polls.Commands.DeletePoll;
using Polls.Application.Polls.Commands.UpdatePoll;
using Polls.Application.Polls.DTOs;
using Polls.Application.Polls.Queries.GetPollById;
using Polls.Application.Polls.Queries.GetPolls;
using Polls.Application.Polls.Queries.GetPollWithIdeas;
using Polls.Domain.Authorization;
using Polls.Domain.Common;

namespace Polls.API.Controllers.User;

[ApiController]
[Route("api/v1/polls")]
public class PollsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<Result<PagedList<PollDto>>> GetPolls(
        [FromQuery] PollFilter filter,
        CancellationToken cancellationToken)
    {
        var query = new GetPollsQuery(
            Filter: filter, 
            IncludeOnlyActive: true);
        
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<Result<PollDto>> GetPollById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetPollByIdQuery(
            Id: id, 
            IncludeOnlyActive: true);
        
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id:guid}/ideas")]
    public async Task<Result<PollWithIdeasDto>> GetPollWithIdeas(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetPollWithIdeasQuery(
            Id: id, 
            IncludeOnlyActive: true);
        
        return await sender.Send(query, cancellationToken);
    }

    [HttpPost("cities/{cityId:guid}")]
    [Authorize(Policy = Permissions.Polls.CreateCity)]
    public async Task<Result<PollDto>> CreatePoll(
        Guid cityId,
        CreatePollRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePollCommand(
            CityId: cityId,
            Title: request.Title,
            Description: request.Description,
            Type: request.Type,
            EndsAt: request.EndsAt,
            BudgetAmount: request.BudgetAmount,
            UserCityId: User.GetCityId(),
            BypassRestrictions: false);

        return await sender.Send(command, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.Polls.UpdateCity)]
    public async Task<Result<PollDto>> UpdatePoll(
        Guid id,
        UpdatePollRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePollCommand(
            Id: id,
            Title: request.Title,
            Description: request.Description,
            EndsAt: request.EndsAt,
            BudgetAmount: request.BudgetAmount,
            UserCityId: User.GetCityId(),
            BypassRestrictions: false);

        return await sender.Send(command, cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.Polls.DeleteCity)]
    public async Task<Result<Unit>> DeletePoll(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeletePollCommand(
            Id: id,
            UserCityId: User.GetCityId(),
            BypassRestrictions: false);

        return await sender.Send(command, cancellationToken);
    }
}
