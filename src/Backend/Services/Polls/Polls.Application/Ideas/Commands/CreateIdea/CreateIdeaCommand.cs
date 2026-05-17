using Polls.Application.Common.CQRS;
using Polls.Application.Common.Models;
using Polls.Application.Ideas.DTOs;

namespace Polls.Application.Ideas.Commands.CreateIdea;

public record CreateIdeaCommand(
    Guid UserId,
    Guid PollId,
    string Title,
    string? Description,
    Guid UserCityId = default,
    bool BypassRestrictions = false,
    IReadOnlyList<ImageFile>? Images = null) : ICommand<IdeaDto>;
