using AutoMapper;
using Moq;
using Polls.Application.Common.Interfaces;

namespace Polls.Tests.Infrastructure;

public abstract class HandlerTestsBase
{
    protected const string StorageUrlPrefix = "https://storage.example.com/";

    protected UnitOfWorkMock UnitOfWork { get; } = new();

    protected Mock<IImageStorageService> Storage { get; } = new(MockBehavior.Strict);

    protected Mock<IIntegrationEventPublisher> EventPublisher { get; } = new(MockBehavior.Strict);

    protected Mock<IDateTimeProvider> Clock { get; } = new(MockBehavior.Strict);

    protected static DateTimeOffset Now { get; } = new(2026, 11, 5, 12, 0, 0, TimeSpan.Zero);

    protected IMapper Mapper { get; }

    protected HandlerTestsBase()
    {
        Storage
            .Setup(x => x.GetUrl(It.IsAny<string>()))
            .Returns<string>(fileName => StorageUrlPrefix + fileName);

        EventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<It.IsAnyType>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Clock.Setup(x => x.UtcNow).Returns(Now);

        Mapper = TestMapper.Create(Storage.Object);
    }
}
