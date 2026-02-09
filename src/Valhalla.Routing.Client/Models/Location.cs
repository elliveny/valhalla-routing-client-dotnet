namespace Valhalla.Routing;

/// <summary>
/// Represents a geographic location for routing or map matching.
/// Validation requirements vary by endpoint:
/// - Route: Heading (0-360°), HeadingTolerance (0-180°), Radius (≥ 0) are validated.
/// - Locate: Only Lat/Lon are validated; other fields are optional without validation.
/// - Common: Lat must be -90 to 90, Lon must be -180 to 180 (inclusive).
/// </summary>
public record Location
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
    /// Gets the location type for routing.
    /// Valid values: "break", "via", "through", "break_through".
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Gets the location name for narration.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the heading in degrees from north (0-360).
    /// For Route endpoint: validated to be 0-360 if provided.
    /// </summary>
    public double? Heading { get; init; }

    /// <summary>
    /// Gets the heading tolerance in degrees.
    /// Default: 60 degrees.
    /// For Route endpoint: validated to be 0-180 if provided.
    /// </summary>
    public double? HeadingTolerance { get; init; }

    /// <summary>
    /// Gets the street name for address display.
    /// </summary>
    public string? Street { get; init; }

    /// <summary>
    /// Gets the city name for address display.
    /// </summary>
    public string? City { get; init; }

    /// <summary>
    /// Gets the state name for address display.
    /// </summary>
    public string? State { get; init; }

    /// <summary>
    /// Gets the postal code for address display.
    /// </summary>
    public string? PostalCode { get; init; }

    /// <summary>
    /// Gets the country name for address display.
    /// </summary>
    public string? Country { get; init; }

    /// <summary>
    /// Gets the display latitude coordinate for side-of-street positioning.
    /// </summary>
    public double? DisplayLat { get; init; }

    /// <summary>
    /// Gets the display longitude coordinate for side-of-street positioning.
    /// </summary>
    public double? DisplayLon { get; init; }

    /// <summary>
    /// Gets the preferred side of street.
    /// Valid values: "same", "opposite", "either".
    /// </summary>
    public string? PreferredSide { get; init; }

    /// <summary>
    /// Gets the search radius in meters.
    /// Default: 0.
    /// For Route endpoint: validated to be ≥ 0 if provided.
    /// </summary>
    public double? Radius { get; init; }

    /// <summary>
    /// Gets the minimum reachability for road selection.
    /// Default: 50.
    /// </summary>
    public int? MinimumReachability { get; init; }

    /// <summary>
    /// Gets a value indicating whether to rank edge candidates.
    /// </summary>
    public bool? RankCandidates { get; init; }

    /// <summary>
    /// Gets the maximum search distance in meters.
    /// Default: 35000m.
    /// </summary>
    public double? SearchCutoff { get; init; }

    /// <summary>
    /// Gets the node snap tolerance in meters.
    /// Default: 5m.
    /// </summary>
    public double? NodeSnapTolerance { get; init; }

    /// <summary>
    /// Gets the street side tolerance in meters.
    /// Default: 5m.
    /// </summary>
    public double? StreetSideTolerance { get; init; }

    /// <summary>
    /// Gets the search filter for excluding specific road types.
    /// </summary>
    public SearchFilter? SearchFilter { get; init; }

    /// <summary>
    /// Gets the preferred layer for multi-level routing.
    /// </summary>
    public int? PreferredLayer { get; init; }

    /// <summary>
    /// Gets the waiting time at this location in seconds.
    /// </summary>
    public int? Waiting { get; init; }

    /// <summary>
    /// Validates that the location has valid latitude and longitude coordinates.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when latitude or longitude is out of valid range.</exception>
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
    }
}
