namespace Valhalla.Routing;

/// <summary>
/// Represents a matched GPS trace point in the trace attributes response.
/// </summary>
public record MatchedPoint
{
    /// <summary>
    /// Gets the latitude coordinate of the matched point.
    /// </summary>
    public double? Lat { get; init; }

    /// <summary>
    /// Gets the longitude coordinate of the matched point.
    /// </summary>
    public double? Lon { get; init; }

    /// <summary>
    /// Gets the match type. Common values: "matched", "interpolated", "unmatched".
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Gets the index into the edges array for this matched point.
    /// </summary>
    public int? EdgeIndex { get; init; }

    /// <summary>
    /// Gets the distance along the matched edge in the specified units.
    /// </summary>
    public double? DistanceAlongEdge { get; init; }

    /// <summary>
    /// Gets the distance from the original trace point to the matched location.
    /// </summary>
    public double? DistanceFromTracePoint { get; init; }
}
