using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polls.API.Common.Extensions;
using Polls.API.Requests.Ideas;
using Polls.Application.Common.Models;
using Polls.Application.Ideas.Commands.ChangeStatus;
using Polls.Application.Ideas.Commands.CreateIdea;
using Polls.Application.Ideas.Commands.DeleteIdea;
using Polls.Application.Ideas.Commands.UpdateIdea;
using Polls.Application.Ideas.DTOs;
using Polls.Application.Ideas.Queries.GetIdeaById;
using Polls.Application.Ideas.Queries.GetIdeas;
using Polls.Application.Ideas.Queries.GetIdeaWithPoll;
using Polls.Domain.Authorization;
using Polls.Domain.Common;
using Polls.Domain.Ideas.Enums;

namespace Polls.API.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/ideas")]
[Authorize]
public class AdminIdeasController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.Ideas.ReadAny)]
    public async Task<Result<PagedList<IdeaDto>>> GetIdeas(
        [FromQuery] IdeaFilter filter,
        CancellationToken cancellationToken)
    {
        var command = new GetIdeasQuery(
            Filter: filter,
            IncludeOnlyActive: false);
        
        return await sender.Send(command, cancellationToken);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.Ideas.ReadAny)]
    public async Task<Result<IdeaDto>> GetIdeaById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new GetIdeaByIdQuery(
            Id: id,
            IncludeOnlyActive: false);
        
        return await sender.Send(command, cancellationToken);
    }

    [HttpGet("{id:guid}/poll")]
    [Authorize(Policy = Permissions.Ideas.ReadAny)]
    public async Task<Result<IdeaWithPollDto>> GetIdeaWithPoll(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new GetIdeaWithPollQuery(
            Id: id,
            IncludeOnlyActive: false);
        
        return await sender.Send(command, cancellationToken);
    }
    
    [HttpPost("{pollId:guid}")]
    [Authorize(Policy = Permissions.Ideas.CreateAny)]
    public async Task<Result<IdeaDto>> CreateIdea(
        Guid pollId,
        CreateIdeaRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateIdeaCommand(
            UserId: User.GetUserId(),
            PollId: pollId,
            Title: request.Title,
            Description: request.Description,
            BypassRestrictions: true);
        
        return await sender.Send(command, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.Ideas.UpdateAny)]
    public async Task<Result<IdeaDto>> UpdateIdea(
        Guid id,
        UpdateIdeaRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateIdeaCommand(
            Id: id,
            Title: request.Title,
            Description: request.Description,
            BypassRestrictions: true);

        return await sender.Send(command, cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.Ideas.DeleteAny)]
    public async Task<Result<Unit>> DeleteIdea(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteIdeaCommand(
            Id: id,
            BypassRestrictions: true);
        
        return await sender.Send(command, cancellationToken);
    }

    [HttpPost("{id:guid}/status")]
    [Authorize(Policy = Permissions.Ideas.ChangeStatusAny)]
    public async Task<Result<Unit>> ChangeStatus(
        Guid id,
        [FromBody] IdeaStatus newStatus,
        CancellationToken cancellationToken)
    {
        var command = new ChangeIdeaStatusCommand(
            Id: id,
            NewStatus: newStatus);
        
        return await sender.Send(command, cancellationToken);
    }
}
