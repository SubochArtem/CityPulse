using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Cities;
using Polls.Domain.Common;
using Polls.Domain.Images;

namespace Polls.Application.Cities.Commands.DeleteCity;

public sealed class DeleteCityCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCityCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(
        DeleteCityCommand command,
        CancellationToken cancellationToken)
    {
        var city = await unitOfWork.Cities.GetByIdWithImagesAsync(command.Id, cancellationToken);
        if (city is null)
            return CityErrors.NotFound(command.Id);

        if (city.Images.Count > 0)
        {
            var deletedImages = city.Images
                .Select(i => new DeletedImage { FileName = i.FileName })
                .ToList();

            unitOfWork.DeletedImages.AddRange(deletedImages);
        }

        unitOfWork.Cities.Delete(city);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
