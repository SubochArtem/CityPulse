using Polls.Application.Common.CQRS;

namespace Polls.Application.Ideas.Commands.DeleteIdea;

public record DeleteIdeaCommand(
    Guid Id,
    Guid UserId = default,
    bool BypassRestrictions = false) : ICommand;
