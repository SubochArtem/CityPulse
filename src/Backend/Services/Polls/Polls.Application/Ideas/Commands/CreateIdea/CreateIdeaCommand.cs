using Polls.Application.Common.CQRS;
using Polls.Application.Ideas.DTOs;

namespace Polls.Application.Ideas.Commands.CreateIdea;

public record CreateIdeaCommand(
    Guid UserId,
    Guid PollId,
    string Title,
    string? Description) : ICommand<IdeaDto>;
