namespace Valhalla.Routing;

/// <summary>
/// Represents search filters for excluding or including specific road types in routing.
/// </summary>
public record SearchFilter
{
    /// <summary>
    /// Gets a value indicating whether to exclude tunnels from the route.
    /// </summary>
    public bool? ExcludeTunnel { get; init; }

    /// <summary>
    /// Gets a value indicating whether to exclude bridges from the route.
    /// </summary>
    public bool? ExcludeBridge { get; init; }

    /// <summary>
    /// Gets a value indicating whether to exclude ramps from the route.
    /// </summary>
    public bool? ExcludeRamp { get; init; }

    /// <summary>
    /// Gets a value indicating whether to exclude road closures from the route.
    /// </summary>
    public bool? ExcludeClosures { get; init; }

    /// <summary>
    /// Gets a value indicating whether to exclude toll roads from the route.
    /// </summary>
    public bool? ExcludeToll { get; init; }

    /// <summary>
    /// Gets a value indicating whether to exclude ferries from the route.
    /// </summary>
    public bool? ExcludeFerry { get; init; }

    /// <summary>
    /// Gets a value indicating whether to exclude cash-only tolls from the route.
    /// </summary>
    public bool? ExcludeCashOnlyTolls { get; init; }

    /// <summary>
    /// Gets the minimum road class to use for routing.
    /// Valid values (ordered from highest to lowest):
    /// motorway, trunk, primary, secondary, tertiary, unclassified, residential, service_other.
    /// </summary>
    public string? MinRoadClass { get; init; }

    /// <summary>
    /// Gets the maximum road class to use for routing.
    /// Valid values (ordered from highest to lowest):
    /// motorway, trunk, primary, secondary, tertiary, unclassified, residential, service_other.
    /// </summary>
    public string? MaxRoadClass { get; init; }
}
