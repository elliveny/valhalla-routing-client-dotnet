namespace Valhalla.Routing;

/// <summary>
/// Detailed information about an edge. Only included in verbose mode.
/// </summary>
public class EdgeInfo
{
    /// <summary>
    /// Gets the list of street names for this edge.
    /// </summary>
    public IReadOnlyList<string>? Names { get; init; }

    /// <summary>
    /// Gets the road class (e.g., motorway, primary, residential).
    /// </summary>
    public string? RoadClass { get; init; }

    /// <summary>
    /// Gets the speed in kilometers per hour.
    /// </summary>
    public int? Speed { get; init; }

    /// <summary>
    /// Gets the use classification of the road (e.g., road, ramp, ferry).
    /// </summary>
    public string? Use { get; init; }

    /// <summary>
    /// Gets the length of the edge in kilometers.
    /// </summary>
    public double? Length { get; init; }

    /// <summary>
    /// Gets a value indicating whether the edge is a bridge.
    /// </summary>
    public bool? Bridge { get; init; }

    /// <summary>
    /// Gets a value indicating whether the edge is a tunnel.
    /// </summary>
    public bool? Tunnel { get; init; }

    /// <summary>
    /// Gets a value indicating whether the edge is a toll road.
    /// </summary>
    public bool? Toll { get; init; }
}
