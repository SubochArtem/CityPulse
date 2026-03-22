using AutoMapper;
using MediatR;
using Polls.Application.Cities.DTOs;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Cities;
using Polls.Domain.Common;

namespace Polls.Application.Cities.Commands.UpdateCity;

public sealed class UpdateCityCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<UpdateCityCommand, Result<CityDto>>
{
    public async Task<Result<CityDto>> Handle(
        UpdateCityCommand command,
        CancellationToken cancellationToken)
    {
        var city = await unitOfWork.Cities.GetByIdAsync(command.Id, cancellationToken);

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
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<CityDto>(city);
    }
}
