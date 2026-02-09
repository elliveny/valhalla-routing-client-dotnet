using System.Text.Json;

namespace Valhalla.Routing;

/// <summary>
/// Represents a request to calculate a route between multiple locations.
/// </summary>
public class RouteRequest
{
    /// <summary>
    /// Gets the list of locations (waypoints) for the route.
    /// At least 2 locations are required.
    /// </summary>
    public required IReadOnlyList<Location> Locations { get; init; }

    /// <summary>
    /// Gets the costing model for route calculation.
    /// See <see cref="CostingModel"/> for common values.
    /// </summary>
    public required string Costing { get; init; }

    /// <summary>
    /// Gets the distance units for the response.
    /// Valid values: "miles", "mi", "kilometers", "km".
    /// </summary>
    public string? Units { get; init; }

    /// <summary>
    /// Gets the language for turn-by-turn instructions.
    /// IETF BCP 47 language tag (e.g., "en-US", "de-DE").
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// Gets the directions type.
    /// Valid values: "none", "maneuvers", "instructions".
    /// </summary>
    public string? DirectionsType { get; init; }

    /// <summary>
    /// Gets the response format.
    /// Valid values: "json", "gpx", "osrm", "pbf".
    /// </summary>
    public string? Format { get; init; }

    /// <summary>
    /// Gets the costing-specific options as a JSON element.
    /// Structure varies by costing model.
    /// </summary>
    public JsonElement? CostingOptions { get; init; }

    /// <summary>
    /// Gets the date/time options for time-dependent routing.
    /// </summary>
    public DateTimeOptions? DateTime { get; init; }

    /// <summary>
    /// Gets the list of locations to exclude from the route.
    /// </summary>
    public IReadOnlyList<Location>? ExcludeLocations { get; init; }

    /// <summary>
    /// Gets the GeoJSON polygon(s) defining areas to exclude from the route.
    /// </summary>
    public JsonElement? ExcludePolygons { get; init; }

    /// <summary>
    /// Gets the request identifier that will be echoed in the response.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the number of alternate routes to calculate.
    /// Must be greater than or equal to 0 if provided.
    /// A value of 0 or null means no alternate routes will be calculated.
    /// Note: Valhalla only supports alternates for 2-location routes (origin + destination).
    /// The client does not enforce this constraint to remain forward-compatible.
    /// Requests with 3+ locations and alternates greater than 0 will be sent to Valhalla,
    /// which will ignore the alternates parameter and return only the primary route.
    /// </summary>
    public int? Alternates { get; init; }

    /// <summary>
    /// Gets the elevation sampling interval in meters.
    /// </summary>
    public int? ElevationInterval { get; init; }

    /// <summary>
    /// Gets a value indicating whether to include roundabout exit instructions.
    /// Default: true.
    /// </summary>
    public bool? RoundaboutExits { get; init; }

    /// <summary>
    /// Gets a value indicating whether to include OpenLR linear references.
    /// </summary>
    public bool? LinearReferences { get; init; }

    /// <summary>
    /// Validates the route request.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public void Validate()
    {
        // Validate locations count
        if (this.Locations == null || this.Locations.Count < 2)
        {
            throw new ArgumentException(
                "At least 2 locations are required for routing.",
                nameof(this.Locations));
        }

        // Validate costing
        if (string.IsNullOrWhiteSpace(this.Costing))
        {
            throw new ArgumentException(
                "Costing is required.",
                nameof(this.Costing));
        }

        // Validate each location
        foreach (var location in this.Locations)
        {
            location.Validate();

            // Route-specific validation for heading
            if (location.Heading.HasValue &&
                (location.Heading.Value < 0 || location.Heading.Value > 360))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(location.Heading),
                    location.Heading.Value,
                    "Heading must be between 0 and 360 degrees.");
            }

            // Route-specific validation for heading tolerance
            if (location.HeadingTolerance.HasValue &&
                (location.HeadingTolerance.Value < 0 || location.HeadingTolerance.Value > 180))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(location.HeadingTolerance),
                    location.HeadingTolerance.Value,
                    "HeadingTolerance must be between 0 and 180 degrees.");
            }

            // Route-specific validation for radius
            if (location.Radius.HasValue && location.Radius.Value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(location.Radius),
                    location.Radius.Value,
                    "Radius must be greater than or equal to 0.");
            }
        }

        // Validate date/time options
        this.DateTime?.Validate();

        // Validate alternates
        if (this.Alternates.HasValue && this.Alternates.Value < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(this.Alternates),
                this.Alternates.Value,
                "Alternates must be greater than or equal to 0.");
        }
    }
}
