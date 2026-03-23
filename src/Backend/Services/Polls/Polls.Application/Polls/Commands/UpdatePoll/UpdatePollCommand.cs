using Polls.Application.Common.CQRS;
using Polls.Application.Polls.DTOs;

namespace Polls.Application.Polls.Commands.UpdatePoll;

public record UpdatePollCommand(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset EndsAt,
    decimal BudgetAmount) : ICommand<PollDto>;
