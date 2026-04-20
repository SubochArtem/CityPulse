using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Security;
using Polls.Application.Images.Helpers;
using Polls.Application.Polls.DTOs;
using Polls.Application.Polls.Guards;
using Polls.Domain.Common;
using Polls.Domain.Images;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;

namespace Polls.Application.Polls.Commands.UpdatePoll;

public sealed class UpdatePollCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IPollScheduler pollScheduler,
    IImageStorageService storageService,
    ILogger<UpdatePollCommandHandler> logger) 
    : IRequestHandler<UpdatePollCommand, Result<PollDto>>
{
    public async Task<Result<PollDto>> Handle(
        UpdatePollCommand command,
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
                .DurationIsValid(command.EndsAt)
                .Validate();

            if (!guardResult.IsSuccess)
                return guardResult.Error;
        }

        var endsAtChanged = poll.EndsAt != command.EndsAt; 

        poll.Title = command.Title;
        poll.Description = command.Description;
        poll.EndsAt = command.EndsAt;
        poll.BudgetAmount = command.BudgetAmount;

        unitOfWork.Polls.Update(poll);
        
        var imageResult = await ImageProcessingHelper.ProcessChangesAsync<PollImage>(
            currentImages: poll.Images,
            unitOfWork: unitOfWork,
            storageService: storageService,
            logger: logger,
            createImageFactory: (fileName, order) => new PollImage
            {
                FileName = fileName,
                PollId = poll.Id,
                Order = order
            },
            imagesToAdd: command.ImagesToAdd,
            imagesToDeleteIds: command.ImagesToDelete,
            cancellationToken: cancellationToken);

        if (!imageResult.IsSuccess)
            return imageResult.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (endsAtChanged && poll.Status == PollStatus.Active) 
            await pollScheduler.ScheduleAsync(poll.Id, poll.EndsAt, cancellationToken);

        return mapper.Map<PollDto>(poll);
    }
}
