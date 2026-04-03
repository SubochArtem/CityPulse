using MediatR;
using Polls.Domain.Common;
using Polls.Domain.Ideas.Enums;

namespace Polls.Application.Ideas.Commands.ChangeStatus;

public sealed record ChangeIdeaStatusCommand(
    Guid Id, 
    IdeaStatus NewStatus) : IRequest<Result<Unit>>;
