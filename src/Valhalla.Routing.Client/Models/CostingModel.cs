namespace Valhalla.Routing;

/// <summary>
/// Constants for Valhalla costing models (travel modes).
/// </summary>
public static class CostingModel
{
    /// <summary>
    /// Standard costing for driving routes in an automobile.
    /// </summary>
    public const string Auto = "auto";

    /// <summary>
    /// Bicycle routing that provides routes suitable for cycling.
    /// </summary>
    public const string Bicycle = "bicycle";

    /// <summary>
    /// Pedestrian routing that provides walking directions.
    /// </summary>
    public const string Pedestrian = "pedestrian";

    /// <summary>
    /// Motorcycle routing optimized for motorcycle travel.
    /// </summary>
    public const string Motorcycle = "motorcycle";

    /// <summary>
    /// Motor scooter routing optimized for motor scooter travel.
    /// </summary>
    public const string MotorScooter = "motor_scooter";

    /// <summary>
    /// Routing for buses that includes bus-specific roads.
    /// </summary>
    public const string Bus = "bus";

    /// <summary>
    /// Truck routing that considers height, weight, and hazmat restrictions.
    /// </summary>
    public const string Truck = "truck";

    /// <summary>
    /// Taxi routing optimized for taxi services with access to taxi lanes.
    /// </summary>
    public const string Taxi = "taxi";

    /// <summary>
    /// Multimodal routing that combines pedestrian and transit modes.
    /// </summary>
    public const string Multimodal = "multimodal";

    /// <summary>
    /// Bikeshare routing that accounts for pedestrian and shared bicycle systems.
    /// </summary>
    public const string Bikeshare = "bikeshare";
}
