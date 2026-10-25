using System.Security.Claims;
using CityPulse.Contracts.Auth.Claims;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Feedback.Tests.Infrastructure;

public abstract class ControllerTestsBase
{
    private const string AuthenticationType = "test";

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

    protected void SetupSend<TRequest, TValue>(ErrorOr<TValue> result)
        where TRequest : class, IRequest<ErrorOr<TValue>> =>
        Sender
            .Setup(x => x.Send<ErrorOr<TValue>>(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

    protected TRequest SentRequest<TRequest>() where TRequest : class =>
        Sender.Invocations
            .Select(invocation => invocation.Arguments[0])
            .OfType<TRequest>()
            .Single();

    protected CancellationToken SentToken<TRequest>() where TRequest : class =>
        Sender.Invocations
            .Where(invocation => invocation.Arguments[0] is TRequest)
            .Select(invocation => (CancellationToken)invocation.Arguments[1])
            .Single();
}
