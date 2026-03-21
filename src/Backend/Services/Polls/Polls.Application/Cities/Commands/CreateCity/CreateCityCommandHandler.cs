using AutoMapper;
using MediatR;
using Polls.Application.Cities.DTOs;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Cities;
using Polls.Domain.Cities.Enums;
using Polls.Domain.Common;

namespace Polls.Application.Cities.Commands.CreateCity;

public sealed class CreateCityCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<CreateCityCommand, Result<CityDto>>
{
    public async Task<Result<CityDto>> Handle(
        CreateCityCommand command,
        CancellationToken cancellationToken)
    {
        var existingCity = await unitOfWork.Cities.GetFirstByPredicateAsync(
            с => с.Name == command.Name,
            cancellationToken);

        if (existingCity is not null)
            return CityErrors.AlreadyExists;

        var city = new City
        {
            Name = command.Name,
            Coordinates = new Coordinates(
                command.Coordinates.Latitude,
                command.Coordinates.Longitude),
            Description = command.Description,
            Status = CityStatus.Active
        };

        unitOfWork.Cities.Create(city);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<CityDto>(city);
    }
}
