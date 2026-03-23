namespace Polls.Application.Ideas.DTOs;

public record IdeaDto(
    Guid Id,
    Guid UserId,
    Guid PollId,
    string Title,
    string? Description,
    int Status);
