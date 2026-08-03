using Polls.Application.Common.CQRS;
using Polls.Domain.Ideas.Enums;

namespace Polls.Application.Ideas.Commands.ChangeApprovalStatus;

public sealed record ChangeIdeaApprovalStatusCommand(
    Guid Id,
    IdeaApprovalStatus NewApprovalStatus) : ICommand;
