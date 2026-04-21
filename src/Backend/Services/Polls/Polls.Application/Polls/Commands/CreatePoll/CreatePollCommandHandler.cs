using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Models;
using Polls.Application.Common.Security;
using Polls.Application.Images.Helpers;
using Polls.Application.Polls.DTOs;
using Polls.Domain.Cities;
using Polls.Domain.Common;
using Polls.Domain.Images;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;

namespace Polls.Application.Polls.Commands.CreatePoll;

public sealed class CreatePollCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IPollScheduler pollScheduler,
    IImageStorageService storageService,
    ILogger<CreatePollCommandHandler> logger)
    : IRequestHandler<CreatePollCommand, Result<PollDto>>
{
    public async Task<Result<PollDto>> Handle(
        CreatePollCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.BypassRestrictions)
        {
            var accessResult = CityAccessPolicy.Check(command.UserCityId, command.CityId);
            if (!accessResult.IsSuccess)
                return accessResult.Error;
        }

        var city = await unitOfWork.Cities.GetByIdAsync(
            command.CityId,
            cancellationToken);
        if (city is null)
            return CityErrors.NotFound(command.CityId);

        var filter = new PollFilter
        {
            CityId = command.CityId,
            Type = command.Type,
            Status = PollStatus.Active
        };
        var activePolls = await unitOfWork.Polls.GetFilteredAsync(
            filter,
            cancellationToken);
        if (activePolls.TotalCount > 0)
            return PollErrors.AlreadyExists(command.CityId);

        var poll = new Poll
        {
            CityId = command.CityId,
            Title = command.Title,
            Description = command.Description,
            EndsAt = command.EndsAt,
            Type = command.Type,
            BudgetAmount = command.BudgetAmount,
            Status = PollStatus.Active
        };

        unitOfWork.Polls.Create(poll);
        
        var imageChanges = new ImageChanges(ToAdd: command.Images);

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
            imageChanges: imageChanges,
            cancellationToken: cancellationToken);

        if (!imageResult.IsSuccess)
            return imageResult.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await pollScheduler.ScheduleAsync(poll.Id, poll.EndsAt, cancellationToken);

        return mapper.Map<PollDto>(poll);
    }
}
