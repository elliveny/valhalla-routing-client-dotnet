namespace Valhalla.Routing;

/// <summary>
/// Represents a geographic bounding box defined by minimum and maximum latitude/longitude coordinates.
/// </summary>
public record BoundingBox
{
    /// <summary>
    /// Gets the minimum longitude coordinate in decimal degrees.
    /// </summary>
    public required double MinLon { get; init; }

    /// <summary>
    /// Gets the minimum latitude coordinate in decimal degrees.
    /// </summary>
    public required double MinLat { get; init; }

    /// <summary>
    /// Gets the maximum longitude coordinate in decimal degrees.
    /// </summary>
    public required double MaxLon { get; init; }

    /// <summary>
    /// Gets the maximum latitude coordinate in decimal degrees.
    /// </summary>
    public required double MaxLat { get; init; }
}
