using System.Text.Json;

namespace Valhalla.Routing;

/// <summary>
/// Represents a request to match a GPS trace to the road network and return a route.
/// </summary>
public class TraceRouteRequest
{
    /// <summary>
    /// Gets the costing model for map matching.
    /// See <see cref="CostingModel"/> for common values.
    /// </summary>
    public required string Costing { get; init; }

    /// <summary>
    /// Gets the list of GPS trace points. Provide either Shape or EncodedPolyline, not both.
    /// Must contain at least 2 points if provided.
    /// </summary>
    public IReadOnlyList<TracePoint>? Shape { get; init; }

    /// <summary>
    /// Gets the encoded polyline alternative to Shape array.
    /// Provide either Shape or EncodedPolyline, not both.
    /// </summary>
    public string? EncodedPolyline { get; init; }

    /// <summary>
    /// Gets the start timestamp in epoch seconds for encoded polyline timestamps.
    /// Used with EncodedPolyline to establish temporal matching.
    /// </summary>
    public long? BeginTime { get; init; }

    /// <summary>
    /// Gets the delta times between points in seconds when using EncodedPolyline.
    /// </summary>
    public IReadOnlyList<int>? Durations { get; init; }

    /// <summary>
    /// Gets a value indicating whether to use input timestamps for elapsed time calculation.
    /// </summary>
    public bool? UseTimestamps { get; init; }

    /// <summary>
    /// Gets the matching algorithm. Valid values: "edge_walk", "map_snap", "walk_or_snap".
    /// </summary>
    public string? ShapeMatch { get; init; }

    /// <summary>
    /// Gets the trace matching options.
    /// </summary>
    public TraceOptions? TraceOptions { get; init; }

    /// <summary>
    /// Gets the costing-specific options as a JSON element.
    /// Structure varies by costing model.
    /// </summary>
    public JsonElement? CostingOptions { get; init; }

    /// <summary>
    /// Gets the response format. Valid values: "json", "gpx", "osrm", "pbf".
    /// </summary>
    public string? Format { get; init; }

    /// <summary>
    /// Gets the distance units for the response. Valid values: "miles", "mi", "kilometers", "km".
    /// </summary>
    public string? Units { get; init; }

    /// <summary>
    /// Gets the language for turn-by-turn instructions. IETF BCP 47 language tag (e.g., "en-US", "de-DE").
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// Gets the directions type. Valid values: "none", "maneuvers", "instructions".
    /// </summary>
    public string? DirectionsType { get; init; }

    /// <summary>
    /// Gets a value indicating whether to include OpenLR linear references.
    /// </summary>
    public bool? LinearReferences { get; init; }

    /// <summary>
    /// Gets the request identifier that will be echoed in the response.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Validates the trace route request.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public void Validate()
    {
        // Validate costing
        if (string.IsNullOrWhiteSpace(this.Costing))
        {
            throw new ArgumentException(
                "Costing is required.",
                nameof(this.Costing));
        }

        // Validate Shape vs EncodedPolyline mutual exclusivity
        if (this.Shape == null && string.IsNullOrWhiteSpace(this.EncodedPolyline))
        {
            throw new ArgumentException(
                "Either Shape or EncodedPolyline must be provided.",
                nameof(this.Shape));
        }

        if (this.Shape != null && !string.IsNullOrWhiteSpace(this.EncodedPolyline))
        {
            throw new ArgumentException(
                "Cannot provide both Shape and EncodedPolyline. Provide only one.",
                nameof(this.Shape));
        }

        // Validate Shape if provided
        if (this.Shape != null)
        {
            if (this.Shape.Count < 2)
            {
                throw new ArgumentException(
                    "Shape must contain at least 2 trace points.",
                    nameof(this.Shape));
            }

            // Validate each trace point
            foreach (var point in this.Shape)
            {
                point.Validate();
            }
        }

        // Validate TraceOptions.SearchRadius if provided
        if (this.TraceOptions?.SearchRadius.HasValue == true)
        {
            var searchRadius = this.TraceOptions.SearchRadius.Value;
            if (searchRadius < 0 || searchRadius > 100)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(this.TraceOptions.SearchRadius),
                    searchRadius,
                    "SearchRadius must be between 0 and 100 meters.");
            }
        }
    }
}
