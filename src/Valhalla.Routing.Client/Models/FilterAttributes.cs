namespace Valhalla.Routing;

/// <summary>
/// Defines attribute filtering for trace attributes requests.
/// </summary>
public record FilterAttributes
{
    /// <summary>
    /// Gets the list of attributes to include or exclude.
    /// </summary>
    public IReadOnlyList<string>? Attributes { get; init; }

    /// <summary>
    /// Gets the filter action. Valid values: "include" or "exclude".
    /// </summary>
    public string? Action { get; init; }
}
