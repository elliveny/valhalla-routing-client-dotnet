using System.Text.Json;

namespace Valhalla.Routing;

/// <summary>
/// Represents a response from the trace_attributes endpoint containing matched points and edge details.
/// </summary>
public class TraceAttributesResponse
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
    /// Gets the distance units used in the response. Examples: "kilometers", "miles".
    /// </summary>
    public string? Units { get; init; }

    /// <summary>
    /// Gets the list of matched GPS trace points.
    /// </summary>
    public IReadOnlyList<MatchedPoint>? MatchedPoints { get; init; }

    /// <summary>
    /// Gets the list of road edges (segments) that were matched.
    /// </summary>
    public IReadOnlyList<TraceEdge>? Edges { get; init; }

    /// <summary>
    /// Gets the encoded polyline representing the matched shape.
    /// </summary>
    public string? Shape { get; init; }
}
