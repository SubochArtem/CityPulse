using Polls.Application.Common.CQRS;
using Polls.Domain.Cities.Enums;

namespace Polls.Application.Cities.Commands.ChangeStatus;

public sealed record ChangeCityStatusCommand(
    Guid Id,
    CityStatus NewStatus) : ICommand;
