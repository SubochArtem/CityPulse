using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Users.DataAccess.Interceptors;
using Users.DataAccess.Interfaces;
using Users.DataAccess.Repositories;

namespace Users.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw
            new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddScoped<SaveChangesInterceptor>();
        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var saveChangesInterceptor = serviceProvider.GetRequiredService<SaveChangesInterceptor>();
            var auditInterceptor = serviceProvider.GetRequiredService<AuditInterceptor>();

            options.UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            5,
                            TimeSpan.FromSeconds(30),
                            null
                        );
                    }
                )
                .AddInterceptors(saveChangesInterceptor, auditInterceptor);
        });

        services.AddScoped<IUserUnitOfWorkRepository, UserUnitOfWorkRepository>();

        return services;
    }
}
