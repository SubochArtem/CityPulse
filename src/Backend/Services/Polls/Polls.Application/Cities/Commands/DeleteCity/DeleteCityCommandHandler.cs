using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Cities;
using Polls.Domain.Common;

namespace Polls.Application.Cities.Commands.DeleteCity;

public sealed class DeleteCityCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCityCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteCityCommand command, CancellationToken cancellationToken)
    {
        var city = await unitOfWork.Cities.GetByIdAsync(command.Id, cancellationToken);

        if (city is null)
            return CityErrors.NotFound(command.Id);

        unitOfWork.Cities.Delete(city);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
