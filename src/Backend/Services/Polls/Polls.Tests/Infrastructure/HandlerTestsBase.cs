using AutoMapper;
using Moq;
using Polls.Application.Common.Interfaces;

namespace Polls.Tests.Infrastructure;

public abstract class HandlerTestsBase
{
    protected const string StorageUrlPrefix = "https://storage.example.com/";

    protected UnitOfWorkMock UnitOfWork { get; } = new();

    protected Mock<IImageStorageService> Storage { get; } = new(MockBehavior.Strict);

    protected IMapper Mapper { get; }

    protected HandlerTestsBase()
    {
        Storage
            .Setup(x => x.GetUrl(It.IsAny<string>()))
            .Returns<string>(fileName => StorageUrlPrefix + fileName);

        Mapper = TestMapper.Create(Storage.Object);
    }
}
