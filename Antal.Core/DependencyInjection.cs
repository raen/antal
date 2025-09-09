using Antal.Core.Application;
using Antal.Core.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Antal.Core;

/// <summary>
/// Provides dependency injection extensions for the Antal core application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers core application services in the provided service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection for chaining.</returns>
    /// <remarks>
    /// Registers <see cref="IJokeFetcher"/> with its default implementation <see cref="JokeFetcher"/>.
    /// Consumers are expected to register <see cref="IJokeApi"/>, <see cref="IJokeRepository"/> and options for <see cref="Antal.Core.Application.Contracts.JokeFetcherOptions"/>.
    /// </remarks>
    public static IServiceCollection AddApplicationCore(this IServiceCollection services)
    {
        services.AddScoped<IJokeFetcher, JokeFetcher>();
        return services;
    }
}