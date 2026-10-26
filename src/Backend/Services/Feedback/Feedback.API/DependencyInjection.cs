using CityPulse.Common.Api.Auth;
using CityPulse.Common.Api.Extensions;
using CityPulse.Common.Api.Middleware;
using CityPulse.Common.Api.Swagger;
using CityPulse.Contracts.Grpc.Protos;
using Feedback.API.Common.Behaviors;
using Feedback.API.Common.Filters;
using Feedback.API.Common.Interfaces;
using Feedback.API.Common.Services;
using Feedback.API.Infrastructure.Grpc;
using Feedback.API.Infrastructure.Mongo;
using Feedback.API.Infrastructure.Redis;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Feedback.API;

public static class DependencyInjection
{
    private const string SwaggerTitle = "CityPulse Feedback API";
    private const string SwaggerDescription = "Feedback management service with Auth0 authentication";

    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers(options =>
            options.Filters.Add<ErrorOrResultFilter>());

        services.AddUnifiedApiErrors();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));
        services.AddSingleton<MongoDbContext>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddOptions<RedisSettings>()
            .Bind(configuration.GetSection(RedisSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddStackExchangeRedisCache(options =>
        {
            var settings = configuration.GetSection(RedisSettings.SectionName).Get<RedisSettings>()!;
            options.Configuration = settings.ConnectionString;
            options.InstanceName = settings.InstanceName;
        });

        services.AddOptions<GrpcSettings>()
            .Bind(configuration.GetSection(GrpcSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddGrpcClient<IdeasService.IdeasServiceClient>((sp, options) =>
        {
            var settings = sp.GetRequiredService<IOptions<GrpcSettings>>().Value;
            options.Address = new Uri(settings.IdeasServiceUrl);
        });

        services.AddScoped<IdeaGrpcClient>();
        services.AddScoped<IIdeaService>(sp => new CachedIdeaService(
            sp.GetRequiredService<IdeaGrpcClient>(),
            sp.GetRequiredService<IDistributedCache>(),
            sp.GetRequiredService<ILogger<CachedIdeaService>>()));

        services.AddCityPulseAuthentication(configuration);

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddAuthorization();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerConfiguration(SwaggerTitle, SwaggerDescription);

        return services;
    }

    public static void UsePresentation(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlerMiddleware>();
        app.UseUnifiedStatusCodePages();
        app.UseSwaggerConfiguration(SwaggerTitle);
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }

    public static async Task InitializeMongoAsync(this WebApplication app)
    {
        var context = app.Services.GetRequiredService<MongoDbContext>();
        await context.CreateIndexesAsync();
    }
}
