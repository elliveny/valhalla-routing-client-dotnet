using System.Text.Json;

namespace Valhalla.Routing;

/// <summary>
/// Represents a response from the trace_route endpoint containing a matched route.
/// </summary>
public class TraceRouteResponse
{
    /// <summary>
    /// Gets the raw JSON response for forward compatibility with Valhalla API changes.
    /// Access this property for fields not yet exposed as strongly-typed properties.
    /// </summary>
    public required JsonElement Raw { get; init; }

    /// <summary>
    /// Gets the request identifier that was provided in the request.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the matched trip. Unlike RouteResponse.Trips, trace_route always returns
    /// a single matched route (no alternates support for map matching).
    /// </summary>
    public Trip? Trip { get; init; }
}
