using AutoMapper;
using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Application.Ideas.DTOs;
using Polls.Application.Polls.Guards;
using Polls.Domain.Authorization;
using Polls.Domain.Common;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls;

namespace Polls.Application.Ideas.Commands.CreateIdea;

public sealed class CreateIdeaCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IUserContextService userContext)
    : IRequestHandler<CreateIdeaCommand, Result<IdeaDto>>
{
    public async Task<Result<IdeaDto>> Handle(
        CreateIdeaCommand command,
        CancellationToken cancellationToken)
    {
        var poll = await unitOfWork.Polls.GetByIdAsync(command.PollId, cancellationToken);
        if (poll is null)
            return PollErrors.NotFound(command.PollId);

        var canCreateAny = userContext.UserPermissions.Contains(Permissions.Ideas.CreateAny);

        if (!canCreateAny)
        {
            if (poll.CityId != userContext.UserId)
                return PollErrors.NotFound(command.PollId);
            
            var guardResult = PollGuard.For(poll)
                .IsNotFinished()
                .Validate();

            if (!guardResult.IsSuccess)
                return guardResult.Errors[0];
        }

        var idea = new Idea
        {
            UserId = command.UserId,
            PollId = command.PollId,
            Title = command.Title,
            Description = command.Description,
            Status = IdeaStatus.InPoll
        };

        unitOfWork.Ideas.Create(idea);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<IdeaDto>(idea);
    }
}
