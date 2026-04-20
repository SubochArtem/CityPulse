using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Polls.Application.Cities.DTOs;
using Polls.Application.Common.Interfaces;
using Polls.Application.Images.Helpers;
using Polls.Domain.Cities;
using Polls.Domain.Common;
using Polls.Domain.Images;

namespace Polls.Application.Cities.Commands.UpdateCity;

public sealed class UpdateCityCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IImageStorageService storageService,
    ILogger<UpdateCityCommandHandler> logger)
    : IRequestHandler<UpdateCityCommand, Result<CityDto>>
{
    public async Task<Result<CityDto>> Handle(
        UpdateCityCommand command,
        CancellationToken cancellationToken)
    {
        var city = await unitOfWork.Cities.GetByIdWithImagesAsync(command.Id, cancellationToken);
        if (city is null)
            return CityErrors.NotFound(command.Id);

        var existingCity = await unitOfWork.Cities.GetByTitleAsync(
            command.Title,
            cancellationToken);

        if (existingCity is not null && existingCity.Id != command.Id)
            return CityErrors.AlreadyExists;

        city.Title = command.Title;
        city.Coordinates = new Coordinates(
            command.Coordinates.Latitude,
            command.Coordinates.Longitude);
        city.Description = command.Description;
        unitOfWork.Cities.Update(city);

        var imageResult = await ImageProcessingHelper.ProcessChangesAsync<CityImage>(
            currentImages: city.Images,
            unitOfWork: unitOfWork,
            storageService: storageService,
            logger: logger,
            createImageFactory: (fileName, order) => new CityImage
            {
                FileName = fileName,
                CityId = city.Id,
                Order = order
            },
            imagesToAdd: command.ImagesToAdd,
            imagesToDeleteIds: command.ImagesToDelete,
            cancellationToken: cancellationToken);

        if (!imageResult.IsSuccess)
            return imageResult.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<CityDto>(city);
    }
}
