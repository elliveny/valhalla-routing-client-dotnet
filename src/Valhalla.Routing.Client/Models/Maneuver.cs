namespace Valhalla.Routing;

/// <summary>
/// Represents a single turn-by-turn maneuver instruction in a route.
/// </summary>
public record Maneuver
{
    /// <summary>
    /// Gets the maneuver type code.
    /// See Valhalla API documentation for maneuver type codes.
    /// </summary>
    public int? Type { get; init; }

    /// <summary>
    /// Gets the human-readable instruction text for this maneuver.
    /// </summary>
    public string? Instruction { get; init; }

    /// <summary>
    /// Gets the distance of this maneuver in the specified units.
    /// </summary>
    public double? Length { get; init; }

    /// <summary>
    /// Gets the estimated duration of this maneuver in seconds.
    /// </summary>
    public double? Time { get; init; }

    /// <summary>
    /// Gets the index into the shape (polyline) array where this maneuver begins.
    /// </summary>
    public int? BeginShapeIndex { get; init; }

    /// <summary>
    /// Gets the index into the shape (polyline) array where this maneuver ends.
    /// </summary>
    public int? EndShapeIndex { get; init; }

    /// <summary>
    /// Gets the list of street names associated with this maneuver.
    /// </summary>
    public IReadOnlyList<string>? StreetNames { get; init; }
}
