using MediatR;
using Polls.Application.Common.Extensions;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Common;
using Polls.Domain.Polls;

namespace Polls.Application.Polls.Commands.DeletePoll;

public sealed class DeletePollCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePollCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(
        DeletePollCommand command,
        CancellationToken cancellationToken)
    {
        var poll = await unitOfWork.Polls.GetByIdAsync(command.Id, cancellationToken);
        if (poll is null)
            return PollErrors.NotFound(command.Id);

        if (!poll.IsOpen())
            return PollErrors.AlreadyFinished(command.Id);

        unitOfWork.Polls.Delete(poll);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
