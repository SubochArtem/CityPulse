using Polls.Application.Common.CQRS;
using Polls.Domain.Ideas.Enums;

namespace Polls.Application.Ideas.Commands.ChangeStatus;

public sealed record ChangeIdeaAccessStatusCommand(
    Guid Id, 
    AccessStatus NewAccessStatus) : ICommand;
