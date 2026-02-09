using System.Text.Json.Serialization;

namespace Valhalla.Routing;

/// <summary>
/// Represents a GPS trace point for map matching.
/// </summary>
public record TracePoint
{
    /// <summary>
    /// Gets the latitude coordinate in decimal degrees.
    /// Valid range: -90 to 90.
    /// </summary>
    public required double Lat { get; init; }

    /// <summary>
    /// Gets the longitude coordinate in decimal degrees.
    /// Valid range: -180 to 180.
    /// </summary>
    public required double Lon { get; init; }

    /// <summary>
    /// Gets the optional type for trace points.
    /// Common values: "break", "via", "through", "break_through".
    /// If not specified, defaults to "via" behavior.
    /// Note: Type semantics for trace points may differ from route locations.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Gets the timestamp for temporal matching. Serializes to/from epoch seconds in JSON.
    /// </summary>
    [JsonConverter(typeof(EpochSecondsConverter))]
    public DateTimeOffset? Time { get; init; }

    /// <summary>
    /// Gets the per-point search radius in meters.
    /// Overrides the global TraceOptions.SearchRadius for this specific point.
    /// </summary>
    public double? Radius { get; init; }

    /// <summary>
    /// Validates that the trace point has valid latitude and longitude coordinates.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when latitude, longitude, or radius is out of valid range.</exception>
    public void Validate()
    {
        if (this.Lat < -90 || this.Lat > 90)
        {
            throw new ArgumentOutOfRangeException(
                nameof(this.Lat),
                this.Lat,
                "Latitude must be between -90 and 90 degrees.");
        }

        if (this.Lon < -180 || this.Lon > 180)
        {
            throw new ArgumentOutOfRangeException(
                nameof(this.Lon),
                this.Lon,
                "Longitude must be between -180 and 180 degrees.");
        }

        if (this.Radius.HasValue && (this.Radius.Value < 0 || this.Radius.Value > 100))
        {
            throw new ArgumentOutOfRangeException(
                nameof(this.Radius),
                this.Radius.Value,
                "Radius must be between 0 and 100 meters.");
        }
    }
}
