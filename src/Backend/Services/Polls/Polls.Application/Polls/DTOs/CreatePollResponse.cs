namespace Polls.Application.Polls.DTOs;

public record CreatePollResponse(
    PollDto UserPoll,
    PollDto ManagerPoll
);
