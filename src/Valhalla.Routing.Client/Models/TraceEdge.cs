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
    /// Gets the routing/traversal speed in kilometers per hour.
    /// </summary>
    public double? Speed { get; init; }

    /// <summary>
    /// Gets the posted speed limit in kilometers per hour.
    /// </summary>
    public double? SpeedLimit { get; init; }

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

    /// <summary>
    /// Gets the OpenStreetMap way identifier for this edge.
    /// Useful for correlating edges back to OSM data.
    /// </summary>
    public long? WayId { get; init; }

    /// <summary>
    /// Gets Valhalla's internal edge identifier.
    /// </summary>
    public long? Id { get; init; }

    /// <summary>
    /// Gets the use classification of the road (e.g., road, ramp, ferry, cycleway).
    /// </summary>
    public string? Use { get; init; }

    /// <summary>
    /// Gets the surface type (e.g., paved, gravel).
    /// </summary>
    public string? Surface { get; init; }

    /// <summary>
    /// Gets a value indicating whether the edge has a toll.
    /// </summary>
    public bool? Toll { get; init; }

    /// <summary>
    /// Gets a value indicating whether the edge is a tunnel.
    /// </summary>
    public bool? Tunnel { get; init; }

    /// <summary>
    /// Gets a value indicating whether the edge is a bridge.
    /// </summary>
    public bool? Bridge { get; init; }
}
