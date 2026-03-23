using AutoMapper;
using MediatR;
using Polls.Application.Common.Constants;
using Polls.Application.Common.Extensions;
using Polls.Application.Common.Interfaces;
using Polls.Application.Polls.DTOs;
using Polls.Domain.Common;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;

namespace Polls.Application.Polls.Commands.UpdatePoll;

public sealed class UpdatePollCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<UpdatePollCommand, Result<PollDto>>
{
    public async Task<Result<PollDto>> Handle(
        UpdatePollCommand command,
        CancellationToken cancellationToken)
    {
        var poll = await unitOfWork.Polls.GetByIdAsync(command.Id, cancellationToken);

        if (poll is null)
            return PollErrors.NotFound(command.Id);

        poll.EnsureStatusConsistency();

        if (poll.Status == PollStatus.Finished)
            return PollErrors.AlreadyFinished(command.Id);

        var totalDurationFromStart = command.EndsAt - poll.CreatedAt;

        if (totalDurationFromStart.TotalDays > ValidationConstants.Poll.MaxDurationDays)
            return PollErrors.MaxDurationExceeded(ValidationConstants.Poll.MaxDurationDays);

        poll.Title = command.Title;
        poll.Description = command.Description;
        poll.EndsAt = command.EndsAt;
        poll.BudgetAmount = command.BudgetAmount;

        unitOfWork.Polls.Update(poll);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<PollDto>(poll);
    }
}
