using Polls.Application.Common.CQRS;

namespace Polls.Application.Ideas.Commands.DeleteIdea;

public record DeleteIdeaCommand(Guid Id) : ICommand;
