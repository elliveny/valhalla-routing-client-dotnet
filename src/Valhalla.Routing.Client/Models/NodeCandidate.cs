namespace Valhalla.Routing;

/// <summary>
/// Represents a candidate node (intersection) near a location.
/// </summary>
public class NodeCandidate
{
    /// <summary>
    /// Gets the latitude of the node.
    /// </summary>
    public double? Lat { get; init; }

    /// <summary>
    /// Gets the longitude of the node.
    /// </summary>
    public double? Lon { get; init; }

    /// <summary>
    /// Gets the distance in meters from the input location to this node.
    /// Only present in verbose mode.
    /// </summary>
    public double? Distance { get; init; }
}
