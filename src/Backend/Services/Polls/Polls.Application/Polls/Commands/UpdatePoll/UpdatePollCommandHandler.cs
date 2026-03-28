using AutoMapper;
using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Application.Polls.DTOs;
using Polls.Application.Polls.Guards;
using Polls.Domain.Authorization;
using Polls.Domain.Common;
using Polls.Domain.Polls;

namespace Polls.Application.Polls.Commands.UpdatePoll;

public sealed class UpdatePollCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IUserContextService userContext)
    : IRequestHandler<UpdatePollCommand, Result<PollDto>>
{
    public async Task<Result<PollDto>> Handle(
        UpdatePollCommand command,
        CancellationToken cancellationToken)
    {
        var poll = await unitOfWork.Polls.GetByIdAsync(command.Id, cancellationToken);

        if (poll is null)
            return PollErrors.NotFound(command.Id);

        var canUpdateAny = userContext.UserPermissions.Contains(Permissions.Polls.UpdateAny);

        if (!canUpdateAny)
        {
            var guardResult = PollGuard.For(poll)
                .IsNotFinished()
                .EditWindowNotExpired()
                .DurationIsValid(command.EndsAt)
                .Validate();

            if (!guardResult.IsSuccess) return guardResult.Errors[0];
        }

        poll.Title = command.Title;
        poll.Description = command.Description;
        poll.EndsAt = command.EndsAt;
        poll.BudgetAmount = command.BudgetAmount;

        unitOfWork.Polls.Update(poll);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<PollDto>(poll);
    }
}
