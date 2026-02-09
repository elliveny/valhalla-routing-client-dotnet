namespace Valhalla.Routing;

/// <summary>
/// Represents a candidate edge (road segment) near a location.
/// </summary>
public class EdgeCandidate
{
    /// <summary>
    /// Gets the OpenStreetMap way ID for this edge.
    /// </summary>
    public long? WayId { get; init; }

    /// <summary>
    /// Gets the correlated (snapped) latitude on this edge.
    /// </summary>
    public double? CorrelatedLat { get; init; }

    /// <summary>
    /// Gets the correlated (snapped) longitude on this edge.
    /// </summary>
    public double? CorrelatedLon { get; init; }

    /// <summary>
    /// Gets the side of the street relative to the input location.
    /// Possible values: "left", "right", "neither".
    /// </summary>
    public string? SideOfStreet { get; init; }

    /// <summary>
    /// Gets the percent along the edge where the correlated point is located.
    /// Range: 0.0 (start of edge) to 1.0 (end of edge).
    /// </summary>
    public double? PercentAlong { get; init; }

    /// <summary>
    /// Gets the distance in meters from the input location to the correlated point.
    /// Only present in verbose mode.
    /// </summary>
    public double? Distance { get; init; }

    /// <summary>
    /// Gets detailed edge information.
    /// Only present in verbose mode.
    /// </summary>
    public EdgeInfo? EdgeInfo { get; init; }
}
