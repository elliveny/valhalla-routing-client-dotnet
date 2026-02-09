using System.Text.Json;

namespace Valhalla.Routing;

/// <summary>
/// Response from the /status endpoint.
/// </summary>
public class StatusResponse
{
    /// <summary>
    /// Gets or initializes the raw JSON response.
    /// This property contains the complete JSON document for forward compatibility.
    /// </summary>
    public JsonElement Raw { get; init; }

    /// <summary>
    /// Gets or initializes the Valhalla version string.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Gets or initializes the UNIX timestamp when the tileset was last modified.
    /// </summary>
    public long? TilesetLastModified { get; init; }

    /// <summary>
    /// Gets a value indicating whether a valid tileset is loaded.
    /// </summary>
    public bool? HasTiles { get; init; }

    /// <summary>
    /// Gets a value indicating whether the tileset was built with admin database.
    /// </summary>
    public bool? HasAdmins { get; init; }

    /// <summary>
    /// Gets a value indicating whether the tileset was built with timezone database.
    /// </summary>
    public bool? HasTimezones { get; init; }

    /// <summary>
    /// Gets a value indicating whether live traffic data is available.
    /// </summary>
    public bool? HasLiveTraffic { get; init; }

    /// <summary>
    /// Gets or initializes the bounding box object representing the tileset extent.
    /// This is an object with fields such as min_lat, min_lon, max_lat, and max_lon.
    /// </summary>
    public JsonElement? Bbox { get; init; }
}
