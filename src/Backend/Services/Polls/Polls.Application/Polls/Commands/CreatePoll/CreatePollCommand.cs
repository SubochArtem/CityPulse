using Polls.Application.Common.CQRS;
using Polls.Application.Common.Models;
using Polls.Application.Polls.DTOs;
using Polls.Domain.Polls.Enums;

namespace Polls.Application.Polls.Commands.CreatePoll;

public record CreatePollCommand(
    Guid CityId,
    string Title,
    string? Description,
    PollType Type,
    DateTimeOffset EndsAt,
    decimal BudgetAmount,
    Guid UserCityId = default,
    bool BypassRestrictions = false,
    IReadOnlyList<ImageFile>? Images = null) : ICommand<PollDto>;
