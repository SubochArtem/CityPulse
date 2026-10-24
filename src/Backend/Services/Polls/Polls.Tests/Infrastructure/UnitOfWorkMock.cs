using Moq;
using Polls.Application.Common.Interfaces;

namespace Polls.Tests.Infrastructure;

public sealed class UnitOfWorkMock
{
    public Mock<IUnitOfWork> Mock { get; } = new(MockBehavior.Strict);

    public Mock<ICityRepository> Cities { get; } = new(MockBehavior.Strict);

    public Mock<IPollRepository> Polls { get; } = new(MockBehavior.Strict);

    public Mock<IIdeaRepository> Ideas { get; } = new(MockBehavior.Strict);

    public Mock<IDeletedImageRepository> DeletedImages { get; } = new(MockBehavior.Strict);

    public IUnitOfWork Object => Mock.Object;

    public UnitOfWorkMock()
    {
        Mock.Setup(x => x.Cities).Returns(Cities.Object);
        Mock.Setup(x => x.Polls).Returns(Polls.Object);
        Mock.Setup(x => x.Ideas).Returns(Ideas.Object);
        Mock.Setup(x => x.DeletedImages).Returns(DeletedImages.Object);
    }

    public void SetupSaveChanges() =>
        Mock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

    public Mock<IUnitOfWorkTransaction> SetupTransaction()
    {
        var transaction = new Mock<IUnitOfWorkTransaction>(MockBehavior.Strict);
        transaction.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

        Mock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction.Object);

        return transaction;
    }
}
