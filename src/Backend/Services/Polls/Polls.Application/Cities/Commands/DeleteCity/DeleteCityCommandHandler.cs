using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Cities;
using Polls.Domain.Common;

namespace Polls.Application.Cities.Commands.DeleteCity;

public sealed class DeleteCityCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCityCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteCityCommand command, CancellationToken ct)
    {
        var city = await unitOfWork.Cities.GetByIdAsync(command.Id, ct);

        if (city is null)
            return CityErrors.NotFound(command.Id);

        unitOfWork.Cities.Delete(city);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<Unit>.Success(Unit.Value);
    }
}
