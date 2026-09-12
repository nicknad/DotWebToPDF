using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotWebToPdf;

/// <summary>
/// Extension methods for registering the PDF generator with an <see cref="IServiceCollection"/>.
/// </summary>
public static class DotWebToPdfGeneratorExtensions
{
    /// <summary>
    /// Adds the PDF generator to the service collection as a singleton.
    /// The shared Chromium browser is launched on first use and disposed with the service provider.
    /// </summary>
    /// <param name="services">The service collection to add the generator to.</param>
    /// <param name="configure">An optional delegate used to configure the generator and browser options.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddDotWebToPdfGenerator(
        this IServiceCollection services,
        Action<GeneratorOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddLogging();
        services.AddOptions<GeneratorOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton<DotWebToPdfGenerator>();
        services.TryAddSingleton<IWebToPdfGenerator>(static provider => provider.GetRequiredService<DotWebToPdfGenerator>());

        return services;
    }
}
