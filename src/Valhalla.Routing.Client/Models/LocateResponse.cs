using System.Text.Json;

namespace Valhalla.Routing;

/// <summary>
/// Response from the /locate endpoint.
/// </summary>
public class LocateResponse
{
    /// <summary>
    /// Gets the array of locate results, one per input location.
    /// </summary>
    public IReadOnlyList<LocateResult>? Results { get; init; }

    /// <summary>
    /// Gets the raw JSON response.
    /// This property contains the complete JSON document for forward compatibility.
    /// </summary>
    public JsonElement Raw { get; init; }

    /// <summary>
    /// Gets any warnings returned by the API.
    /// </summary>
    public IReadOnlyList<string>? Warnings { get; init; }

    /// <summary>
    /// Gets the optional ID echoed back from the request.
    /// </summary>
    public string? Id { get; init; }
}
