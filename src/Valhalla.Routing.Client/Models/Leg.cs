namespace Valhalla.Routing;

/// <summary>
/// Represents one leg (segment) of a trip between two consecutive locations.
/// </summary>
public record Leg
{
    /// <summary>
    /// Gets the list of turn-by-turn maneuvers for this leg.
    /// </summary>
    public IReadOnlyList<Maneuver>? Maneuvers { get; init; }

    /// <summary>
    /// Gets the summary information for this leg.
    /// </summary>
    public LegSummary? Summary { get; init; }

    /// <summary>
    /// Gets the encoded polyline shape representing the path of this leg.
    /// Use PolylineEncoder to decode this into latitude/longitude coordinates.
    /// </summary>
    public string? Shape { get; init; }
}
