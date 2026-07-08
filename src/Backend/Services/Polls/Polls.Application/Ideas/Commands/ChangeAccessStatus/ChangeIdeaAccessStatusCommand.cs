using Polls.Application.Common.CQRS;
using Polls.Domain.Ideas.Enums;

namespace Polls.Application.Ideas.Commands.ChangeAccessStatus;

public sealed record ChangeIdeaAccessStatusCommand(
    Guid Id, 
    IdeaAccessStatus NewIdeaAccessStatus) : ICommand;
