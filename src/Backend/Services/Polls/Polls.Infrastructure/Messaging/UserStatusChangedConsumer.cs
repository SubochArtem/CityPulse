using CityPulse.Contracts.Events.Users;
using CityPulse.Contracts.Events.Users.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Ideas.Enums;

namespace Polls.Infrastructure.Messaging;

public sealed class UserStatusChangedConsumer(
    IIdeaRepository ideaRepository,
    ILogger<UserStatusChangedConsumer> logger) : IConsumer<UserStatusChangedEvent>
{
    public async Task Consume(ConsumeContext<UserStatusChangedEvent> context)
    {
        var message = context.Message;
        var (sourceIdeaAccessStatus, targetIdeaAccessStatus) = GetStatusTransition(message.UserLifecycleStatus);

        if (sourceIdeaAccessStatus == IdeaAccessStatus.Undefined || targetIdeaAccessStatus == IdeaAccessStatus.Undefined)
        {
            logger.LogWarning(
                "Unsupported user lifecycle status transition for user {UserId}: {Status}",
                message.UserId, message.UserLifecycleStatus);
            return;
        }

        await ideaRepository.UpdateAccessStatusByUserIdAsync(
            message.UserId,
            sourceIdeaAccessStatus,
            targetIdeaAccessStatus,
            context.CancellationToken);
    }

    private static (IdeaAccessStatus SourceIdeaAccessStatus, IdeaAccessStatus TargetIdeaAccessStatus) GetStatusTransition(
        UserLifecycleStatus status) => status switch
    {
        UserLifecycleStatus.Inactive => (IdeaAccessStatus.Active, IdeaAccessStatus.RestrictedByAuthor),
        UserLifecycleStatus.Active => (IdeaAccessStatus.RestrictedByAuthor, IdeaAccessStatus.Active),
        _ => (IdeaAccessStatus.Undefined, IdeaAccessStatus.Undefined)
    };
}
