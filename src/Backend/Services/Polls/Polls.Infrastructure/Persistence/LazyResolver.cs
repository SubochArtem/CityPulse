using Microsoft.Extensions.DependencyInjection;

namespace Polls.Infrastructure.Persistence;

internal sealed class LazyResolver<T>(IServiceProvider provider)
    : Lazy<T>(provider.GetRequiredService<T>) where T : notnull;
