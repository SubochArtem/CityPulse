using Polls.Application.Polls.DTOs;

namespace Polls.Application.Ideas.DTOs;

public record IdeaWithPollDto(
    Guid Id,
    Guid UserId,
    Guid PollId,
    string Title,
    string? Description,
    int Status,
    PollDto Poll);
