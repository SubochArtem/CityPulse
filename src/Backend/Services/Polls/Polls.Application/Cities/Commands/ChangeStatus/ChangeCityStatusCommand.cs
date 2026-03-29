using MediatR;
using Polls.Domain.Cities.Enums;
using Polls.Domain.Common;

namespace Polls.Application.Cities.Commands.ChangeStatus;

public sealed record ChangeCityStatusCommand(
    Guid Id, 
    CityStatus NewStatus) : IRequest<Result<Unit>>;
