using MediatR;
using Polls.Domain.Common;
using Polls.Domain.Polls.Enums;

namespace Polls.Application.Polls.Commands.ChangeStatus;

public sealed record ChangePollStatusCommand(
    Guid Id, 
    PollStatus NewStatus) : IRequest<Result<Unit>>;
