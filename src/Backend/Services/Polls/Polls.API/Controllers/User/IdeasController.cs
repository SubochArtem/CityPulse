using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polls.API.Common.Extensions;
using Polls.API.Requests.Ideas;
using Polls.Application.Common.Models;
using Polls.Application.Ideas.Commands.CreateIdea;
using Polls.Application.Ideas.Commands.DeleteIdea;
using Polls.Application.Ideas.Commands.UpdateIdea;
using Polls.Application.Ideas.DTOs;
using Polls.Application.Ideas.Queries.GetIdeaById;
using Polls.Application.Ideas.Queries.GetIdeas;
using Polls.Application.Ideas.Queries.GetIdeaWithPoll;
using Polls.Domain.Authorization;
using Polls.Domain.Common;

namespace Polls.API.Controllers.User;

[ApiController]
[Route("api/v1/ideas")]
public class IdeasController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<Result<PagedList<IdeaDto>>> GetIdeas(
        [FromQuery] IdeaFilter filter,
        CancellationToken cancellationToken)
    {
        var query = new GetIdeasQuery(
            Filter: filter, 
            IncludeOnlyActive: true);
        
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<Result<IdeaDto>> GetIdeaById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetIdeaByIdQuery(
            Id: id, 
            IncludeOnlyActive: true);
        
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id:guid}/poll")]
    public async Task<Result<IdeaWithPollDto>> GetIdeaWithPoll(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetIdeaWithPollQuery(
            Id: id, 
            IncludeOnlyActive: true);
        
        return await sender.Send(query, cancellationToken);
    }

    [HttpPost("polls/{pollId:guid}")]
    [Authorize(Policy = Permissions.Ideas.CreateCity)]
    public async Task<Result<IdeaDto>> CreateIdea(
        Guid pollId,
        [FromForm] CreateIdeaRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new CreateIdeaCommand(
            UserId: User.GetUserId(),
            PollId: pollId,
            Title: request.Title,
            Description: request.Description,
            Images: request.Images.ToImageFiles(), 
            UserCityId: User.GetCityId(),
            BypassRestrictions: false);

        return await sender.Send(command, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.Ideas.UpdateOwn)]
    public async Task<Result<IdeaDto>> UpdateIdea(
        Guid id,
        [FromForm] UpdateIdeaRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new UpdateIdeaCommand(
            Id: id,
            Title: request.Title,
            Description: request.Description,
            ImagesToAdd: request.ImagesToAdd.ToImageFiles(), 
            ImagesToDelete: request.ImagesToDelete,          
            UserId: User.GetUserId(),
            BypassRestrictions: false);

        return await sender.Send(command, cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.Ideas.DeleteOwn)]
    public async Task<Result<Unit>> DeleteIdea(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteIdeaCommand(
            Id: id,
            UserId: User.GetUserId(),
            BypassRestrictions: false);

        return await sender.Send(command, cancellationToken);
    }
}
