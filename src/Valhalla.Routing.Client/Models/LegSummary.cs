namespace Valhalla.Routing;

/// <summary>
/// Represents summary information for one leg of a trip.
/// </summary>
public record LegSummary
{
    /// <summary>
    /// Gets the distance of this leg in the specified units.
    /// </summary>
    public double? Length { get; init; }

    /// <summary>
    /// Gets the estimated duration of this leg in seconds.
    /// </summary>
    public double? Time { get; init; }

    /// <summary>
    /// Gets the minimum latitude coordinate of the leg bounding box.
    /// </summary>
    public double? MinLat { get; init; }

    /// <summary>
    /// Gets the minimum longitude coordinate of the leg bounding box.
    /// </summary>
    public double? MinLon { get; init; }

    /// <summary>
    /// Gets the maximum latitude coordinate of the leg bounding box.
    /// </summary>
    public double? MaxLat { get; init; }

    /// <summary>
    /// Gets the maximum longitude coordinate of the leg bounding box.
    /// </summary>
    public double? MaxLon { get; init; }
}
