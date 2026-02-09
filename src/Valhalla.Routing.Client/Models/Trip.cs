namespace Valhalla.Routing;

/// <summary>
/// Represents a trip with one or more legs (segments between locations).
/// </summary>
public record Trip
{
    /// <summary>
    /// Gets the list of legs in this trip.
    /// Each leg represents travel between two consecutive locations.
    /// </summary>
    public IReadOnlyList<Leg>? Legs { get; init; }

    /// <summary>
    /// Gets the summary information for the entire trip.
    /// </summary>
    public TripSummary? Summary { get; init; }

    /// <summary>
    /// Gets the distance units used in this trip.
    /// Possible values: "miles", "mi", "kilometers", "km".
    /// </summary>
    public string? Units { get; init; }

    /// <summary>
    /// Gets the language used for instructions in this trip.
    /// IETF BCP 47 language tag (e.g., "en-US", "de-DE").
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// Gets the locations (waypoints) for this trip.
    /// </summary>
    public IReadOnlyList<Location>? Locations { get; init; }
}
