namespace Valhalla.Routing;

/// <summary>
/// Represents date/time options for time-dependent routing.
/// </summary>
public record DateTimeOptions
{
    /// <summary>
    /// Gets the date/time type for the routing request.
    /// </summary>
    public required DateTimeType Type { get; init; }

    /// <summary>
    /// Gets the date and time in ISO 8601 format: YYYY-MM-DDTHH:mm (e.g., "2026-02-07T14:30").
    /// Seconds are optional and typically ignored by Valhalla (minute precision).
    /// The time is interpreted in the local timezone of the departure or arrival location.
    /// Required when Type is DepartAt or ArriveBy.
    /// </summary>
    public string? Value { get; init; }

    /// <summary>
    /// Validates the date/time options.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when Type is invalid or Value is missing for DepartAt/ArriveBy.</exception>
    public void Validate()
    {
        if ((int)this.Type < 0 || (int)this.Type > 3)
        {
            throw new ArgumentException(
                $"DateTimeType must be between 0 and 3, but was {(int)this.Type}.",
                nameof(this.Type));
        }

        if ((this.Type == DateTimeType.DepartAt || this.Type == DateTimeType.ArriveBy) &&
            string.IsNullOrWhiteSpace(this.Value))
        {
            throw new ArgumentException(
                $"Value is required when Type is {this.Type}.",
                nameof(this.Value));
        }
    }
}
