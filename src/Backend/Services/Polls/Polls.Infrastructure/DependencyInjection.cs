using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polls.Application.Common.Interfaces;
using Polls.Infrastructure.Persistence;
using Polls.Infrastructure.Persistence.Interceptors;
using Polls.Infrastructure.Persistence.Repositories;

namespace Polls.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'DefaultConnection' is not configured.");

        services.AddTransient(typeof(Lazy<>), typeof(LazyResolver<>));

        services
            .AddScoped<SaveChangesInterceptor>()
            .AddScoped<AuditInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var updateTimestampsInterceptor =
                serviceProvider.GetRequiredService<SaveChangesInterceptor>();
            var auditInterceptor =
                serviceProvider.GetRequiredService<AuditInterceptor>();

            options
                .UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        5,
                        TimeSpan.FromSeconds(30),
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
            .AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
