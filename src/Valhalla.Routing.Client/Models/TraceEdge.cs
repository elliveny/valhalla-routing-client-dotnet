namespace Valhalla.Routing;

/// <summary>
/// Represents an edge (road segment) in the trace attributes response.
/// </summary>
public record TraceEdge
{
    /// <summary>
    /// Gets the list of street names for this edge.
    /// </summary>
    public IReadOnlyList<string>? Names { get; init; }

    /// <summary>
    /// Gets the length of the edge in the specified units.
    /// </summary>
    public double? Length { get; init; }

    /// <summary>
    /// Gets the speed limit or average speed in kilometers per hour.
    /// </summary>
    public double? Speed { get; init; }

    /// <summary>
    /// Gets the road classification. Examples: "motorway", "primary", "secondary", "residential".
    /// </summary>
    public string? RoadClass { get; init; }

    /// <summary>
    /// Gets the starting index in the shape polyline for this edge.
    /// </summary>
    public int? BeginShapeIndex { get; init; }

    /// <summary>
    /// Gets the ending index in the shape polyline for this edge.
    /// </summary>
    public int? EndShapeIndex { get; init; }

    /// <summary>
    /// Gets the number of traffic segments for this edge.
    /// </summary>
    public int? TrafficSegments { get; init; }
}
