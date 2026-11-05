using System.Security.Claims;
using Gateway.API.Bff;
using Gateway.API.Configuration;
using Gateway.API.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.StackExchangeRedis;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using StackExchange.Redis;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace Gateway.API.Extensions;

public static class BffExtensions
{
    public static IServiceCollection AddGatewayBff(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<BffSettings>()
            .Bind(configuration.GetSection(BffSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<ForwardedHeadersSettings>()
            .Bind(configuration.GetSection(ForwardedHeadersSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<ForwardedHeadersOptions>()
            .Configure<IOptions<ForwardedHeadersSettings>>((options, settings) =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
                foreach (var network in settings.Value.KnownNetworks)
                {
                    options.KnownNetworks.Add(IPNetwork.Parse(network));
                }
            });

        services.AddOptions<RedisSettings>()
            .Bind(configuration.GetSection(RedisSettings.SectionName));

        services.AddAuthentication()
            .AddCookie(BffDefaults.CookieScheme)
            .AddOpenIdConnect(BffDefaults.OidcScheme, _ => { });

        services.AddOptions<CookieAuthenticationOptions>(BffDefaults.CookieScheme)
            .Configure<IOptions<BffSettings>, ITicketStore>((options, settings, store) =>
                ConfigureCookie(options, settings.Value, store));

        services.AddOptions<OpenIdConnectOptions>(BffDefaults.OidcScheme)
            .Configure<IOptions<BffSettings>>((options, settings) => ConfigureOpenIdConnect(options, settings.Value));

        AddSessionStorage(services);
        AddTokenForwarding(services);

        return services;
    }

    public static void UseGatewayBff(this WebApplication app)
    {
        if (app.Services.GetRequiredService<IOptions<BffSettings>>().Value.Enabled)
        {
            app.MapBffEndpoints();
        }
    }

    public static void UseGatewayBffTokenForwarding(this IReverseProxyApplicationBuilder proxy)
    {
        if (proxy.ApplicationServices.GetRequiredService<IOptions<BffSettings>>().Value.Enabled)
        {
            proxy.UseMiddleware<BffTokenForwardingMiddleware>();
        }
    }

    private static void ConfigureCookie(CookieAuthenticationOptions options, BffSettings settings, ITicketStore store)
    {
        options.Cookie.Name = BffDefaults.CookieName;
        options.Cookie.Path = BffDefaults.RootPath;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromDays(settings.SessionLifetimeDays);
        options.SlidingExpiration = true;
        options.SessionStore = store;
        options.Events.OnRedirectToLogin = context => WriteStatus(context.Response, StatusCodes.Status401Unauthorized);
        options.Events.OnRedirectToAccessDenied = context => WriteStatus(context.Response, StatusCodes.Status403Forbidden);
        options.Events.OnSigningIn = context =>
        {
            if (context.Principal?.Identity is ClaimsIdentity identity
                && !identity.HasClaim(claim => claim.Type == BffDefaults.SessionIdClaim))
            {
                identity.AddClaim(new Claim(BffDefaults.SessionIdClaim, Guid.NewGuid().ToString("N")));
            }

            return Task.CompletedTask;
        };
    }

    private static void ConfigureOpenIdConnect(OpenIdConnectOptions options, BffSettings settings)
    {
        options.SignInScheme = BffDefaults.CookieScheme;
        options.CallbackPath = BffDefaults.CallbackPath;

        if (!settings.Enabled)
        {
            options.ClientId = BffDefaults.DisabledClientId;
            options.Configuration = new OpenIdConnectConfiguration();
            return;
        }

        options.Authority = BffDefaults.Authority(settings.Domain);
        options.ClientId = settings.ClientId;
        options.ClientSecret = settings.ClientSecret;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.ResponseMode = OpenIdConnectResponseMode.Query;
        options.UsePkce = true;
        options.SaveTokens = true;
        options.MapInboundClaims = false;
        options.GetClaimsFromUserInfoEndpoint = false;
        options.TokenValidationParameters.NameClaimType = BffDefaults.SubjectClaim;
        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.NonceCookie.SameSite = SameSiteMode.Lax;
        options.NonceCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

        options.Scope.Clear();
        options.Scope.Add(BffDefaults.OpenIdScope);
        options.Scope.Add(BffDefaults.ProfileScope);
        options.Scope.Add(BffDefaults.EmailScope);
        options.Scope.Add(BffDefaults.OfflineAccessScope);

        options.Events.OnRedirectToIdentityProvider = context =>
        {
            context.ProtocolMessage.SetParameter(BffDefaults.AudienceParam, settings.Audience);
            if (context.Properties.Items.TryGetValue(BffDefaults.UiLocalesParam, out var locales)
                && !string.IsNullOrWhiteSpace(locales))
            {
                context.ProtocolMessage.SetParameter(BffDefaults.UiLocalesParam, locales);
            }

            return Task.CompletedTask;
        };
        options.Events.OnRedirectToIdentityProviderForSignOut = context =>
        {
            var request = context.Request;
            var returnTo = $"{request.Scheme}://{request.Host}{request.PathBase}";
            context.Response.Redirect(BffRedirects.Auth0Logout(settings.Domain, settings.ClientId, returnTo));
            context.HandleResponse();

            return Task.CompletedTask;
        };
        options.Events.OnRemoteFailure = context =>
        {
            var description = context.Request.Query[BffDefaults.ErrorDescriptionParam].FirstOrDefault();
            context.Response.Redirect(BffRedirects.LoginFailed(description));
            context.HandleResponse();

            return Task.CompletedTask;
        };
    }

    private static void AddSessionStorage(IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
            ConnectionMultiplexer.Connect(
                serviceProvider.GetRequiredService<IOptions<RedisSettings>>().Value.ConnectionString));

        services.AddSingleton<IDistributedCache>(serviceProvider =>
        {
            var redis = serviceProvider.GetRequiredService<IOptions<RedisSettings>>().Value;
            if (string.IsNullOrWhiteSpace(redis.ConnectionString))
            {
                return new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            }

            var connection = serviceProvider.GetRequiredService<IConnectionMultiplexer>();

            return new RedisCache(Options.Create(new RedisCacheOptions
            {
                InstanceName = BffDefaults.CacheInstanceName,
                ConnectionMultiplexerFactory = () => Task.FromResult(connection)
            }));
        });

        services.AddDataProtection().SetApplicationName(BffDefaults.DataProtectionApplicationName);
        services.AddOptions<KeyManagementOptions>()
            .Configure<IOptions<RedisSettings>, IServiceProvider>((options, redis, serviceProvider) =>
            {
                if (!string.IsNullOrWhiteSpace(redis.Value.ConnectionString))
                {
                    options.XmlRepository = new RedisXmlRepository(
                        () => serviceProvider.GetRequiredService<IConnectionMultiplexer>().GetDatabase(),
                        BffDefaults.DataProtectionKeysKey);
                }
            });

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<ITicketStore, DistributedTicketStore>();
    }

    private static void AddTokenForwarding(IServiceCollection services)
    {
        services.AddHttpClient(BffDefaults.TokenClientName);
        services.AddSingleton<SessionKeyReader>();
        services.AddSingleton<SessionLocks>();
        services.AddSingleton<TokenRefreshClient>();
        services.AddSingleton<SessionTokenProvider>();
    }

    private static Task WriteStatus(HttpResponse response, int statusCode)
    {
        response.StatusCode = statusCode;

        return Task.CompletedTask;
    }
}
