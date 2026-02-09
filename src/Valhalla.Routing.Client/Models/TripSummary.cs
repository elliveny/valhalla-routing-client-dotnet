namespace Valhalla.Routing;

/// <summary>
/// Represents summary information for an entire trip.
/// </summary>
public record TripSummary
{
    /// <summary>
    /// Gets the total distance of the trip in the specified units.
    /// </summary>
    public double? Length { get; init; }

    /// <summary>
    /// Gets the estimated duration of the trip in seconds.
    /// </summary>
    public double? Time { get; init; }

    /// <summary>
    /// Gets the minimum latitude coordinate of the trip bounding box.
    /// </summary>
    public double? MinLat { get; init; }

    /// <summary>
    /// Gets the minimum longitude coordinate of the trip bounding box.
    /// </summary>
    public double? MinLon { get; init; }

    /// <summary>
    /// Gets the maximum latitude coordinate of the trip bounding box.
    /// </summary>
    public double? MaxLat { get; init; }

    /// <summary>
    /// Gets the maximum longitude coordinate of the trip bounding box.
    /// </summary>
    public double? MaxLon { get; init; }

    /// <summary>
    /// Gets a value indicating whether the route has time restrictions.
    /// </summary>
    public bool? HasTimeRestrictions { get; init; }

    /// <summary>
    /// Gets a value indicating whether the route includes toll roads.
    /// </summary>
    public bool? HasToll { get; init; }

    /// <summary>
    /// Gets a value indicating whether the route includes highways.
    /// </summary>
    public bool? HasHighway { get; init; }

    /// <summary>
    /// Gets a value indicating whether the route includes ferry crossings.
    /// </summary>
    public bool? HasFerry { get; init; }
}
