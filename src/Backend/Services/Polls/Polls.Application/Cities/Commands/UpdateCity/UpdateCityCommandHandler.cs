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

        var isNameTaken = await unitOfWork.Cities.GetFirstByPredicateAsync(
            c => c.Name == command.Name
                 && c.Id != command.Id,
            cancellationToken) is not null;

        if (isNameTaken)
            return CityErrors.AlreadyExists;

        city.Name = command.Name;
        city.Coordinates = new Coordinates(
            command.Coordinates.Latitude,
            command.Coordinates.Longitude);
        city.Description = command.Description;

        unitOfWork.Cities.Update(city);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<CityDto>(city);
    }
}
