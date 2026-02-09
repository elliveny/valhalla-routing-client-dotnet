namespace Valhalla.Routing;

/// <summary>
/// Configuration options for the Valhalla routing client.
/// </summary>
public class ValhallaClientOptions
{
    /// <summary>
    /// Gets or sets the base URI for the Valhalla server.
    /// This URI will be normalized to remove trailing slashes.
    /// </summary>
    public required Uri BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the timeout for HTTP requests.
    /// Default is 15 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Gets or sets the name of the API key header (e.g., "X-Api-Key").
    /// </summary>
    public string? ApiKeyHeaderName { get; set; }

    /// <summary>
    /// Gets or sets the value of the API key header.
    /// </summary>
    public string? ApiKeyHeaderValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable logging of request/response bodies.
    /// Default is false. When enabled, sensitive data may be logged.
    /// API keys are never logged even when this is enabled.
    /// </summary>
    public bool EnableSensitiveLogging { get; set; }
}
