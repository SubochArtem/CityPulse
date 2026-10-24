using Grpc.Core;
using Moq;
using Moq.Protected;

namespace Polls.Tests.Infrastructure;

internal static class TestServerCallContext
{
    private const string CancellationTokenMember = "CancellationTokenCore";

    public static ServerCallContext Create(CancellationToken cancellationToken = default)
    {
        var context = new Mock<ServerCallContext>();
        context
            .Protected()
            .Setup<CancellationToken>(CancellationTokenMember)
            .Returns(cancellationToken);

        return context.Object;
    }
}
