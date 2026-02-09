using System.Text.Json;

namespace Valhalla.Routing;

/// <summary>
/// Request options for the /locate endpoint.
/// </summary>
public record LocateRequest
{
    /// <summary>
    /// Gets the locations to find nearest roads for.
    /// At least one location is required.
    /// </summary>
    public required IReadOnlyList<Location> Locations { get; init; }

    /// <summary>
    /// Gets the costing model to use for the request.
    /// This determines which roads are considered accessible.
    /// </summary>
    public required string Costing { get; init; }

    /// <summary>
    /// Gets costing-specific options to customize the costing model behavior.
    /// This should contain a property matching the costing model name (e.g., "auto", "bicycle").
    /// </summary>
    public JsonElement? CostingOptions { get; init; }

    /// <summary>
    /// Gets a value indicating whether to return verbose information about edges and nodes.
    /// Default is false for minimal output.
    /// </summary>
    public bool? Verbose { get; init; }

    /// <summary>
    /// Gets the distance units for the response ("miles", "mi", "kilometers", "km").
    /// If not specified, the default is kilometers.
    /// </summary>
    public string? Units { get; init; }

    /// <summary>
    /// Gets an optional identifier that will be echoed back in the response.
    /// This can be used to match responses to requests in asynchronous scenarios.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Validates the request parameters.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public void Validate()
    {
        if (this.Locations == null || this.Locations.Count == 0)
        {
            throw new ArgumentException("At least one location is required.", nameof(this.Locations));
        }

        if (string.IsNullOrWhiteSpace(this.Costing))
        {
            throw new ArgumentException("Costing model is required.", nameof(this.Costing));
        }

        // Validate each location
        foreach (var location in this.Locations)
        {
            location.Validate();
        }
    }
}
