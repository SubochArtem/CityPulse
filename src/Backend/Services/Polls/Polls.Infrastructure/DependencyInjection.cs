using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polls.Application.Common.Interfaces;
using Polls.Infrastructure.Persistence;
using Polls.Infrastructure.Persistence.Interceptors;
using Polls.Infrastructure.Persistence.Options;
using Polls.Infrastructure.Persistence.Repositories;

namespace Polls.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddTransient(typeof(Lazy<>), typeof(LazyResolver<>));

        services
            .AddScoped<SaveChangesInterceptor>()
            .AddScoped<AuditInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var dbOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

            var updateTimestampsInterceptor =
                serviceProvider.GetRequiredService<SaveChangesInterceptor>();
            var auditInterceptor =
                serviceProvider.GetRequiredService<AuditInterceptor>();

            options
                .UseNpgsql(dbOptions.ConnectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        dbOptions.MaxRetryCount,
                        TimeSpan.FromSeconds(dbOptions.MaxRetryDelaySeconds),
                        null);
                })
                .AddInterceptors(
                    updateTimestampsInterceptor,
                    auditInterceptor);
        });

        services
            .AddScoped<ICityRepository, CityRepository>()
            .AddScoped<IPollRepository, PollRepository>()
            .AddScoped<IIdeaRepository, IdeaRepository>()
            .AddScoped<IPollScheduleJobRepository, PollScheduleJobRepository>()
            .AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
