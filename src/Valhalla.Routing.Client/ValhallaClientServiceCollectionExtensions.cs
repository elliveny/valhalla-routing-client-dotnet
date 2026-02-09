using Microsoft.Extensions.DependencyInjection;

namespace Valhalla.Routing;

/// <summary>
/// Extension methods for configuring Valhalla routing client services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ValhallaClientServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Valhalla routing client services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureOptions">A delegate to configure the <see cref="ValhallaClientOptions"/>.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to further configure the HTTP client.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> or <paramref name="configureOptions"/> is null.</exception>
    public static IHttpClientBuilder AddValhallaClient(
        this IServiceCollection services,
        Action<ValhallaClientOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        // Configure options
        services.Configure(configureOptions);

        // Register the typed HTTP client with ValhallaClient
        // HttpClient configuration is done in ValhallaClient constructor based on ValhallaClientOptions
        return services.AddHttpClient<IValhallaClient, ValhallaClient>();
    }
}
