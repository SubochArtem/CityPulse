using Polls.Application.Common.CQRS;
using Polls.Application.Common.Models;
using Polls.Application.Polls.DTOs;

namespace Polls.Application.Polls.Commands.UpdatePoll;

public record UpdatePollCommand(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset EndsAt,
    decimal BudgetAmount,
    Guid UserCityId = default,
    bool BypassRestrictions = false,
    IReadOnlyList<ImageFile>? ImagesToAdd = null,
    IReadOnlyList<Guid>? ImagesToDelete = null) : ICommand<PollDto>;
