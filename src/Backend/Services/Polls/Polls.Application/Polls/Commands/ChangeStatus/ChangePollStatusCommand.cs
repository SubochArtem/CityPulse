using Polls.Application.Common.CQRS;
using Polls.Domain.Polls.Enums;

namespace Polls.Application.Polls.Commands.ChangeStatus;

public sealed record ChangePollStatusCommand(
    Guid Id, 
    PollStatus NewStatus) : ICommand;
