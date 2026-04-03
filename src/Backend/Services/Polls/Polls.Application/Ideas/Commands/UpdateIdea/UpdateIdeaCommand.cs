using Polls.Application.Common.CQRS;
using Polls.Application.Ideas.DTOs;

namespace Polls.Application.Ideas.Commands.UpdateIdea;

public record UpdateIdeaCommand(
    Guid Id,
    string Title,
    string? Description,
    Guid UserId = default,
    bool BypassRestrictions = false) : ICommand<IdeaDto>;
