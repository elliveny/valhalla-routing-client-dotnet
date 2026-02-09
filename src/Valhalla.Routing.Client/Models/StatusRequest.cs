namespace Valhalla.Routing;

/// <summary>
/// Request options for the /status endpoint.
/// </summary>
public record StatusRequest
{
    /// <summary>
    /// Gets a value indicating whether to return verbose status information.
    /// Default is false.
    /// </summary>
    public bool Verbose { get; init; }
}
