namespace Valhalla.Routing;

/// <summary>
/// Represents a single location result from the locate endpoint.
/// </summary>
public class LocateResult
{
    /// <summary>
    /// Gets the input latitude that was used for this result.
    /// </summary>
    public double? InputLat { get; init; }

    /// <summary>
    /// Gets the input longitude that was used for this result.
    /// </summary>
    public double? InputLon { get; init; }

    /// <summary>
    /// Gets the edges (road segments) that were found near this location.
    /// </summary>
    public IReadOnlyList<EdgeCandidate>? Edges { get; init; }

    /// <summary>
    /// Gets the nodes (intersections) that were found near this location.
    /// </summary>
    public IReadOnlyList<NodeCandidate>? Nodes { get; init; }

    /// <summary>
    /// Gets any warnings specific to this location.
    /// </summary>
    public IReadOnlyList<string>? Warnings { get; init; }
}
