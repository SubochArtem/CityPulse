using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
using Polls.Domain.Authorization;
using Polls.Domain.Common;

namespace Polls.API.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/polls")]
[Authorize]
public class AdminPollsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.Polls.ReadAny)]
    public async Task<Result<PagedList<PollDto>>> GetPolls(
        [FromQuery] PollFilter filter,
        CancellationToken cancellationToken)
    {
        var query = new GetPollsQuery(
            Filter: filter,
            IncludeOnlyActive: false);
        
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.Polls.ReadAny)]
    public async Task<Result<PollDto>> GetPollById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetPollByIdQuery(
            Id: id,
            IncludeOnlyActive: false);
        
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id:guid}/ideas")]
    [Authorize(Policy = Permissions.Polls.ReadAny)]
    public async Task<Result<PollWithIdeasDto>> GetPollWithIdeas(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetPollWithIdeasQuery(
            Id: id,
            IncludeOnlyActive: false);
        
        return await sender.Send(query, cancellationToken);
    }

    [HttpPost("cities/{cityId:guid}")]
    [Authorize(Policy = Permissions.Polls.CreateAny)]
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
            BypassRestrictions: true);

        return await sender.Send(command, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.Polls.UpdateAny)]
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
            BypassRestrictions: true);

        return await sender.Send(command, cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.Polls.DeleteAny)]
    public async Task<Result<Unit>> DeletePoll(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeletePollCommand(
            Id: id,
            BypassRestrictions: true);
        
        return await sender.Send(command, cancellationToken);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = Permissions.Polls.ChangeStatusAny)]
    public async Task<Result<Unit>> ChangeStatus(
        Guid id,
        ChangePollStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangePollStatusCommand(
            Id: id,
            NewStatus: request.NewStatus);
        
        return await sender.Send(command, cancellationToken);
    }
}
