using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Polls.Application.Common.Interfaces;
using Polls.Application.Ideas.DTOs;
using Polls.Application.Ideas.Guards;
using Polls.Application.Images.Helpers;
using Polls.Domain.Common;
using Polls.Domain.Ideas;
using Polls.Domain.Images;

namespace Polls.Application.Ideas.Commands.UpdateIdea;

public sealed class UpdateIdeaCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IImageStorageService storageService,
    ILogger<UpdateIdeaCommandHandler> logger)
    : IRequestHandler<UpdateIdeaCommand, Result<IdeaDto>>
{
    public async Task<Result<IdeaDto>> Handle(
        UpdateIdeaCommand command,
        CancellationToken cancellationToken)
    {
        var idea = await unitOfWork.Ideas.GetWithPollAsync(command.Id, cancellationToken);
        if (idea is null)
            return IdeaErrors.NotFound(command.Id);

        if (!command.BypassRestrictions)
        {
            var validationResult = idea.ValidateIdeaAccess(command.UserId);
            if (!validationResult.IsSuccess)
                return validationResult.Error;
        }

        idea.Title = command.Title;
        idea.Description = command.Description;

        unitOfWork.Ideas.Update(idea);
        
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
            imagesToAdd: command.ImagesToAdd,
            imagesToDeleteIds: command.ImagesToDelete,
            cancellationToken: cancellationToken);

        if (!imageResult.IsSuccess)
            return imageResult.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<IdeaDto>(idea);
    }
}
