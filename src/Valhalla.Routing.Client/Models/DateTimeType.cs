namespace Valhalla.Routing;

/// <summary>
/// Specifies the date/time type for routing requests.
/// </summary>
public enum DateTimeType
{
    /// <summary>
    /// Use current date/time (default behavior).
    /// </summary>
    Current = 0,

    /// <summary>
    /// Depart at the specified date/time.
    /// </summary>
    DepartAt = 1,

    /// <summary>
    /// Arrive by the specified date/time.
    /// </summary>
    ArriveBy = 2,

    /// <summary>
    /// Invariant routing (ignore time-dependent routing).
    /// </summary>
    Invariant = 3,
}
