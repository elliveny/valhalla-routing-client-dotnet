namespace Valhalla.Routing;

/// <summary>
/// Options for GPS trace map matching.
/// </summary>
public record TraceOptions
{
    /// <summary>
    /// Gets the search radius for map matching in meters.
    /// Must be between 0 and 100 meters. Default: 40 meters.
    /// Per-point Radius on TracePoint overrides this global value.
    /// </summary>
    public double? SearchRadius { get; init; }

    /// <summary>
    /// Gets the GPS accuracy in meters. Default: 5 meters.
    /// </summary>
    public double? GpsAccuracy { get; init; }

    /// <summary>
    /// Gets the distance to break trace in meters. Default: 2000 meters.
    /// </summary>
    public double? BreakageDistance { get; init; }

    /// <summary>
    /// Gets the merge threshold in meters for interpolation distance.
    /// </summary>
    public double? InterpolationDistance { get; init; }
}
