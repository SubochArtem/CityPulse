using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Security;
using Polls.Application.Ideas.DTOs;
using Polls.Application.Images.Helpers;
using Polls.Application.Polls.Guards;
using Polls.Domain.Common;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Images;
using Polls.Domain.Polls;

namespace Polls.Application.Ideas.Commands.CreateIdea;

public sealed class CreateIdeaCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IImageStorageService storageService,
    ILogger<CreateIdeaCommandHandler> logger)
    : IRequestHandler<CreateIdeaCommand, Result<IdeaDto>>
{
    public async Task<Result<IdeaDto>> Handle(
        CreateIdeaCommand command,
        CancellationToken cancellationToken)
    {
        var poll = await unitOfWork.Polls.GetByIdAsync(command.PollId, cancellationToken);
        if (poll is null)
            return PollErrors.NotFound(command.PollId);

        if (!command.BypassRestrictions)
        {
            var accessResult = CityAccessPolicy.Check(command.UserCityId, poll.CityId);
            if (!accessResult.IsSuccess)
                return accessResult.Error;

            var guardResult = PollGuard.For(poll)
                .IsNotFinished()
                .Validate();

            if (!guardResult.IsSuccess)
                return guardResult.Error;
        }

        var idea = new Idea
        {
            UserId = command.UserId,
            PollId = command.PollId,
            Title = command.Title,
            Description = command.Description,
            Status = IdeaStatus.Active
        };

        unitOfWork.Ideas.Create(idea);

        var imageResult = await ImageProcessingHelper.ProcessChangesAsync<IdeaImage>(
            currentImages: idea.Images,
            unitOfWork: unitOfWork,
            storageService: storageService,
            logger: logger,
            createImageFactory: (fileName, order) => new IdeaImage
            {
                FileName = fileName,
                IdeaId = idea.Id,
                Order = order
            },
            imagesToAdd: command.Images,
            cancellationToken: cancellationToken);

        if (!imageResult.IsSuccess)
            return imageResult.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<IdeaDto>(idea);
    }
}
