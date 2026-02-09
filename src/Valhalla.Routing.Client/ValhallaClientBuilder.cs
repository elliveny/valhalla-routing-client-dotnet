using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Valhalla.Routing.Builder;

/// <summary>
/// Builder for creating <see cref="IValhallaClient"/> instances without dependency injection.
/// </summary>
public class ValhallaClientBuilder
{
    private Uri? baseUri;
    private TimeSpan timeout = TimeSpan.FromSeconds(15);
    private string? apiKeyHeaderName;
    private string? apiKeyHeaderValue;
    private bool enableSensitiveLogging;
    private HttpClient? httpClient;
    private ILogger<ValhallaClient>? logger;

    /// <summary>
    /// Creates a new instance of <see cref="ValhallaClientBuilder"/>.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public static ValhallaClientBuilder Create() => new();

    /// <summary>
    /// Sets the base URI for the Valhalla server.
    /// </summary>
    /// <param name="baseUrl">The base URL of the Valhalla server (e.g., "http://localhost:8002").</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="baseUrl"/> is null, empty, consists only of whitespace, or is not a valid absolute URI.</exception>
    public ValhallaClientBuilder WithBaseUrl(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException("Base URL cannot be null or empty.", nameof(baseUrl));
        }

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"Base URL '{baseUrl}' is not a valid absolute URI.", nameof(baseUrl));
        }

        this.baseUri = uri;
        return this;
    }

    /// <summary>
    /// Sets the base URI for the Valhalla server.
    /// </summary>
    /// <param name="baseUri">The base URI of the Valhalla server.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="baseUri"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="baseUri"/> is not an absolute URI.</exception>
    public ValhallaClientBuilder WithBaseUrl(Uri baseUri)
    {
        ArgumentNullException.ThrowIfNull(baseUri);

        if (!baseUri.IsAbsoluteUri)
        {
            throw new ArgumentException("Base URI must be an absolute URI.", nameof(baseUri));
        }

        this.baseUri = baseUri;
        return this;
    }

    /// <summary>
    /// Sets the timeout for HTTP requests.
    /// </summary>
    /// <param name="timeout">The timeout duration.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="timeout"/> is zero or negative.</exception>
    public ValhallaClientBuilder WithTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be greater than zero.");
        }

        this.timeout = timeout;
        return this;
    }

    /// <summary>
    /// Configures an API key header for authentication.
    /// </summary>
    /// <param name="headerName">The name of the API key header (e.g., "X-Api-Key").</param>
    /// <param name="headerValue">The value of the API key.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="headerName"/> or <paramref name="headerValue"/> is null, empty, or consists only of whitespace.</exception>
    public ValhallaClientBuilder WithApiKey(string headerName, string headerValue)
    {
        if (string.IsNullOrWhiteSpace(headerName))
        {
            throw new ArgumentException("API key header name cannot be null or empty.", nameof(headerName));
        }

        if (string.IsNullOrWhiteSpace(headerValue))
        {
            throw new ArgumentException("API key header value cannot be null or empty.", nameof(headerValue));
        }

        this.apiKeyHeaderName = headerName;
        this.apiKeyHeaderValue = headerValue;
        return this;
    }

    /// <summary>
    /// Enables logging of request and response bodies.
    /// WARNING: This may log sensitive data. Use only for debugging.
    /// </summary>
    /// <returns>The builder instance for fluent chaining.</returns>
    public ValhallaClientBuilder WithSensitiveLogging()
    {
        this.enableSensitiveLogging = true;
        return this;
    }

    /// <summary>
    /// Provides a custom <see cref="HttpClient"/> instance.
    /// Note: The HttpClient's BaseAddress and Timeout will be overwritten by the builder settings.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> is null.</exception>
    public ValhallaClientBuilder WithHttpClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        this.httpClient = httpClient;
        return this;
    }

    /// <summary>
    /// Provides a custom logger instance.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger"/> is null.</exception>
    public ValhallaClientBuilder WithLogger(ILogger<ValhallaClient> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        this.logger = logger;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="IValhallaClient"/> instance.
    /// </summary>
    /// <returns>A configured instance of <see cref="IValhallaClient"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the base URI has not been set or if a custom HttpClient was not provided.</exception>
    /// <remarks>
    /// The builder requires a custom HttpClient to be provided via <see cref="WithHttpClient(HttpClient)"/> to ensure proper resource management.
    /// The caller is responsible for managing the lifetime and disposal of the provided HttpClient.
    /// </remarks>
    public IValhallaClient Build()
    {
        if (this.baseUri == null)
        {
            throw new InvalidOperationException("Base URI must be set before building the client. Call WithBaseUrl() first.");
        }

        if (this.httpClient == null)
        {
            throw new InvalidOperationException("HttpClient must be provided via WithHttpClient() before building the client. The caller is responsible for managing the HttpClient lifetime.");
        }

        // Create logger if not provided
        var loggerInstance = this.logger ?? NullLogger<ValhallaClient>.Instance;

        // Log warning if custom HttpClient was provided
        Log.CustomHttpClientWarning(loggerInstance);

        // Create options
        var options = Options.Create(new ValhallaClientOptions
        {
            BaseUri = this.baseUri,
            Timeout = this.timeout,
            ApiKeyHeaderName = this.apiKeyHeaderName,
            ApiKeyHeaderValue = this.apiKeyHeaderValue,
            EnableSensitiveLogging = this.enableSensitiveLogging,
        });

        // Build and return client
        return new ValhallaClient(this.httpClient, options, loggerInstance);
    }
}
