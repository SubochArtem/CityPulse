using CityPulse.Contracts.Events.Users;
using CityPulse.Contracts.Events.Users.Enums;
using MassTransit;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Ideas.Enums;

namespace Polls.Infrastructure.Messaging;

public sealed class UserStatusChangedConsumer(
    IIdeaRepository ideaRepository,
    IDateTimeProvider dateTimeProvider) : IConsumer<UserStatusChangedEvent>
{
    public async Task Consume(ConsumeContext<UserStatusChangedEvent> context)
    {
        var message = context.Message;

        if (message.UserLifecycleStatus == UserLifecycleStatus.Inactive)
            await ideaRepository.UpdateAccessStatusByUserIdAsync(
                message.UserId,
                IdeaAccessStatus.Active,
                IdeaAccessStatus.RestrictedByAuthor,
                context.CancellationToken);
        
        else if (message.UserLifecycleStatus == UserLifecycleStatus.Active)
            await ideaRepository.UpdateAccessStatusByUserIdAsync(
                message.UserId,
                IdeaAccessStatus.RestrictedByAuthor,
                IdeaAccessStatus.Active,
                context.CancellationToken);
    }
}
