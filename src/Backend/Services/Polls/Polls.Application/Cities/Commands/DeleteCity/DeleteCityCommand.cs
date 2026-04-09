using Polls.Application.Common.CQRS;

namespace Polls.Application.Cities.Commands.DeleteCity;

public record DeleteCityCommand(Guid Id) : ICommand;
