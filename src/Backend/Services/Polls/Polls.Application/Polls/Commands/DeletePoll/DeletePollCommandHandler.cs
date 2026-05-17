using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Security;
using Polls.Application.Polls.Guards;
using Polls.Domain.Common;
using Polls.Domain.Images;
using Polls.Domain.Polls;

namespace Polls.Application.Polls.Commands.DeletePoll;

public sealed class DeletePollCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePollCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(
        DeletePollCommand command,
        CancellationToken cancellationToken)
    {
        var poll = await unitOfWork.Polls.GetByIdWithImagesAsync(command.Id, cancellationToken);
        if (poll is null)
            return PollErrors.NotFound(command.Id);

        if (!command.BypassRestrictions)
        {
            var accessResult = CityAccessPolicy.Check(command.UserCityId, poll.CityId);
            if (!accessResult.IsSuccess)
                return accessResult.Error;

            var guardResult = PollGuard.For(poll)
                .IsNotFinished()
                .EditWindowNotExpired()
                .Validate();

            if (!guardResult.IsSuccess)
                return guardResult.Error;
        }
        
        if (poll.Images.Count > 0)
        {
            var deletedImages = poll.Images
                .Select(i => new DeletedImage { FileName = i.FileName })
                .ToList();

            unitOfWork.DeletedImages.AddRange(deletedImages);
        }

        unitOfWork.Polls.Delete(poll);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
