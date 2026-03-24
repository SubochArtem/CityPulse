using AutoMapper;
using MediatR;
using Polls.Application.Common.Extensions;
using Polls.Application.Common.Interfaces;
using Polls.Application.Ideas.DTOs;
using Polls.Domain.Common;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls;

namespace Polls.Application.Ideas.Commands.UpdateIdea;

public sealed class UpdateIdeaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<UpdateIdeaCommand, Result<IdeaDto>>
{
    public async Task<Result<IdeaDto>> Handle(
        UpdateIdeaCommand command,
        CancellationToken cancellationToken)
    {
        var idea = await unitOfWork.Ideas.GetWithPollAsync(command.Id, cancellationToken);
        if (idea is null)
            return IdeaErrors.NotFound(command.Id);

        if (idea.Status == IdeaStatus.Approved)
            return IdeaErrors.AlreadyApproved(command.Id);

        if (!idea.Poll.IsOpen())
        {
            return PollErrors.AlreadyFinished(idea.PollId);
        }

        idea.Title = command.Title;
        idea.Description = command.Description;

        unitOfWork.Ideas.Update(idea);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<IdeaDto>(idea);
    }
}
