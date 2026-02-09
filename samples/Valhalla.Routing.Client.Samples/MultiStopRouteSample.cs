using Microsoft.Extensions.Logging;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Samples;

/// <summary>
/// Sample demonstrating multi-stop routing (more than 2 waypoints).
/// </summary>
public static class MultiStopRouteSample
{
    /// <summary>
    /// Runs the multi-stop route sample.
    /// </summary>
    /// <param name="client">The Valhalla client.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task RunAsync(IValhallaClient client, ILogger logger)
    {
        logger.LogInformation(string.Empty);
        logger.LogInformation("=== Multi-Stop Route Sample ===");

        try
        {
            // Create a route with multiple stops
            var request = new RouteRequest
            {
                Locations = new List<Location>
                {
                    // Stop 1: Start location
                    new() { Lat = 40.7128, Lon = -74.0060 },

                    // Stop 2: Intermediate stop
                    new() { Lat = 40.7489, Lon = -73.9680 },

                    // Stop 3: Another stop
                    new() { Lat = 40.7589, Lon = -73.9851 },

                    // Stop 4: Final destination
                    new() { Lat = 40.7829, Lon = -73.9654 },
                },
                Costing = CostingModel.Auto,
                Units = "km",
            };

            logger.LogInformation("Calculating route with {Count} stops", request.Locations.Count);

            // Calculate the route
            var response = await client.RouteAsync(request).ConfigureAwait(false);

            if (response.Trips?.Any() == true)
            {
                var trip = response.Trips[0];
                logger.LogInformation("Multi-stop route found!");
                logger.LogInformation("Total Distance: {Distance} km", trip.Summary?.Length ?? 0);
                logger.LogInformation(
                    "Total Duration: {Duration} seconds ({Minutes:F1} minutes)",
                    trip.Summary?.Time ?? 0,
                    (trip.Summary?.Time ?? 0) / 60.0);
                logger.LogInformation("Number of legs: {Legs}", trip.Legs?.Count ?? 0);

                // Show details for each leg
                if (trip.Legs?.Any() == true)
                {
                    for (int i = 0; i < trip.Legs.Count; i++)
                    {
                        var leg = trip.Legs[i];
                        logger.LogInformation(
                            "Leg {Number}: {Distance} km, {Duration} seconds",
                            i + 1,
                            leg.Summary?.Length ?? 0,
                            leg.Summary?.Time ?? 0);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to calculate multi-stop route");
            throw;
        }
    }
}
