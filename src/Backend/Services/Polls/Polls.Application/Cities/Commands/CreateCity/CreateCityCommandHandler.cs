using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Polls.Application.Cities.DTOs;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Models;
using Polls.Application.Images.Helpers;
using Polls.Domain.Cities;
using Polls.Domain.Cities.Enums;
using Polls.Domain.Common;
using Polls.Domain.Images;

namespace Polls.Application.Cities.Commands.CreateCity;

public sealed class CreateCityCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IImageStorageService storageService,
    ILogger<CreateCityCommandHandler> logger)
    : IRequestHandler<CreateCityCommand, Result<CityDto>>
{
    public async Task<Result<CityDto>> Handle(
        CreateCityCommand command,
        CancellationToken cancellationToken)
    {
        var existingCity = await unitOfWork.Cities.GetByTitleAsync(
            command.Title,
            cancellationToken);

        if (existingCity is not null)
            return CityErrors.AlreadyExists;

        var city = new City
        {
            Title = command.Title,
            Coordinates = new Coordinates(
                command.Coordinates.Latitude,
                command.Coordinates.Longitude),
            Description = command.Description,
            Status = CityStatus.Active
        };

        unitOfWork.Cities.Create(city);
        
        var imageChanges = new ImageChanges(ToAdd: command.Images);

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
            imageChanges: imageChanges,
            cancellationToken: cancellationToken);

        if (!imageResult.IsSuccess)
            return imageResult.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<CityDto>(city);
    }
}
