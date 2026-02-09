using Microsoft.Extensions.Logging;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Samples;

/// <summary>
/// Sample demonstrating GPS trace matching using the TraceRoute endpoint.
/// </summary>
public static class GpsTraceMatchingSample
{
    /// <summary>
    /// Runs the GPS trace matching sample.
    /// </summary>
    /// <param name="client">The Valhalla client.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task RunAsync(IValhallaClient client, ILogger logger)
    {
        logger.LogInformation(string.Empty);
        logger.LogInformation("=== GPS Trace Matching Sample ===");

        try
        {
            // Create a GPS trace (simulating recorded GPS points)
            var tracePoints = new List<TracePoint>
            {
                new() { Lat = 40.7128, Lon = -74.0060, Time = DateTimeOffset.UtcNow.AddMinutes(-5) },
                new() { Lat = 40.7189, Lon = -74.0020, Time = DateTimeOffset.UtcNow.AddMinutes(-4) },
                new() { Lat = 40.7249, Lon = -73.9980, Time = DateTimeOffset.UtcNow.AddMinutes(-3) },
                new() { Lat = 40.7309, Lon = -73.9940, Time = DateTimeOffset.UtcNow.AddMinutes(-2) },
                new() { Lat = 40.7369, Lon = -73.9900, Time = DateTimeOffset.UtcNow.AddMinutes(-1) },
            };

            var request = new TraceRouteRequest
            {
                Shape = tracePoints,
                Costing = CostingModel.Auto,
                ShapeMatch = "map_snap",
            };

            logger.LogInformation("Matching GPS trace with {Count} points", tracePoints.Count);

            // Match the trace to the road network
            var response = await client.TraceRouteAsync(request).ConfigureAwait(false);

            if (response.Trip != null)
            {
                logger.LogInformation("GPS trace matched successfully!");
                logger.LogInformation("Matched Distance: {Distance} km", response.Trip.Summary?.Length ?? 0);
                logger.LogInformation(
                    "Matched Duration: {Duration} seconds ({Minutes:F1} minutes)",
                    response.Trip.Summary?.Time ?? 0,
                    (response.Trip.Summary?.Time ?? 0) / 60.0);

                if (response.Trip.Legs?.Any() == true)
                {
                    var firstLeg = response.Trip.Legs[0];
                    logger.LogInformation("Matched route has {Count} maneuvers", firstLeg.Maneuvers?.Count ?? 0);

                    if (firstLeg.Shape != null)
                    {
                        logger.LogInformation(
                            "Matched route shape (encoded polyline): {Shape}",
                            firstLeg.Shape.Length > 50 ? string.Concat(firstLeg.Shape.AsSpan(0, 50), "...") : firstLeg.Shape);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to match GPS trace");
            throw;
        }
    }
}
