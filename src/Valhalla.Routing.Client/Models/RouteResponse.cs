using System.Text.Json;

namespace Valhalla.Routing;

/// <summary>
/// Represents the response from a route calculation request.
/// </summary>
public class RouteResponse
{
    /// <summary>
    /// Gets the raw JSON response from the Valhalla API.
    /// Provides access to any additional fields not explicitly mapped in this DTO.
    /// </summary>
    public required JsonElement Raw { get; init; }

    /// <summary>
    /// Gets the request identifier that was provided in the request.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the list of trips.
    /// Contains one trip for a standard route request.
    /// When Alternates &gt; 0 is specified, may contain multiple alternate routes.
    /// </summary>
    public IReadOnlyList<Trip>? Trips { get; init; }
}
