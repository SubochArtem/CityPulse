using Polls.Application.Common.CQRS;

namespace Polls.Application.Polls.Commands.DeletePoll;

public record DeletePollCommand(Guid Id) : ICommand;
