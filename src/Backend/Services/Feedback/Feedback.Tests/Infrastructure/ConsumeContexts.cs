using MassTransit;
using Moq;

namespace Feedback.Tests.Infrastructure;

internal static class ConsumeContexts
{
    public static ConsumeContext<T> For<T>(T message, CancellationToken cancellationToken = default)
        where T : class
    {
        var context = new Mock<ConsumeContext<T>>();
        context.Setup(x => x.Message).Returns(message);
        context.Setup(x => x.CancellationToken).Returns(cancellationToken);

        return context.Object;
    }
}
