using System.Linq.Expressions;
using System.Security.Claims;
using CityPulse.Contracts.Auth.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Polls.Domain.Common;

namespace Polls.Tests.Infrastructure;

public abstract class ControllerTestsBase
{
    private const string AuthenticationType = "test";
    private const string FormFieldName = "files";
    private const string ImageContentType = "image/jpeg";

    protected static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    protected static readonly Guid UserCityId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    protected Mock<ISender> Sender { get; } = new(MockBehavior.Strict);

    protected static TController WithUser<TController>(
        TController controller,
        Guid? userId = null,
        Guid? cityId = null)
        where TController : ControllerBase
    {
        var claims = new List<Claim>();

        if (userId.HasValue)
            claims.Add(new Claim(CityPulseClaims.InternalUserId, userId.Value.ToString()));

        if (cityId.HasValue)
            claims.Add(new Claim(CityPulseClaims.CityId, cityId.Value.ToString()));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, AuthenticationType))
            }
        };

        return controller;
    }

    protected void SetupSend<TRequest, TValue>(Result<TValue> result)
        where TRequest : class, IRequest<Result<TValue>> =>
        Sender
            .Setup(x => x.Send<Result<TValue>>(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

    protected void VerifySent<TRequest, TValue>(
        Expression<Func<TRequest, bool>> match,
        CancellationToken cancellationToken)
        where TRequest : class, IRequest<Result<TValue>> =>
        Sender.Verify(
            x => x.Send<Result<TValue>>(It.Is(match), cancellationToken),
            Times.Once);

    protected static FormFileCollection FormFiles(params string[] fileNames)
    {
        var collection = new FormFileCollection();

        foreach (var fileName in fileNames)
        {
            collection.Add(new FormFile(new MemoryStream([1, 2, 3]), 0, 3, FormFieldName, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = ImageContentType
            });
        }

        return collection;
    }
}
