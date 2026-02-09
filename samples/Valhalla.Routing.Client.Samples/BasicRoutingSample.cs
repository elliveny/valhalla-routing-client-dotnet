using Microsoft.Extensions.Logging;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Samples;

/// <summary>
/// Sample demonstrating basic routing between two locations.
/// </summary>
public static class BasicRoutingSample
{
    /// <summary>
    /// Runs the basic routing sample.
    /// </summary>
    /// <param name="client">The Valhalla client.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task RunAsync(IValhallaClient client, ILogger logger)
    {
        logger.LogInformation(string.Empty);
        logger.LogInformation("=== Basic Routing Sample ===");

        try
        {
            // Create a simple route request between two locations
            var request = new RouteRequest
            {
                Locations = new List<Location>
                {
                    // Start: Near downtown area (example coordinates)
                    new() { Lat = 40.7128, Lon = -74.0060 },

                    // End: A few blocks away
                    new() { Lat = 40.7589, Lon = -73.9851 },
                },
                Costing = CostingModel.Auto,
                Units = "km",
            };

            logger.LogInformation(
                "Calculating route from ({Lat1}, {Lon1}) to ({Lat2}, {Lon2})",
                request.Locations[0].Lat,
                request.Locations[0].Lon,
                request.Locations[1].Lat,
                request.Locations[1].Lon);

            // Calculate the route
            var response = await client.RouteAsync(request).ConfigureAwait(false);

            if (response.Trips?.Any() == true)
            {
                var trip = response.Trips[0];
                logger.LogInformation("Route found!");
                logger.LogInformation("Distance: {Distance} km", trip.Summary?.Length ?? 0);
                logger.LogInformation(
                    "Duration: {Duration} seconds ({Minutes:F1} minutes)",
                    trip.Summary?.Time ?? 0,
                    (trip.Summary?.Time ?? 0) / 60.0);
                logger.LogInformation("Number of legs: {Legs}", trip.Legs?.Count ?? 0);

                // Show first few maneuvers
                if (trip.Legs?.Any() == true)
                {
                    var firstLeg = trip.Legs[0];
                    logger.LogInformation("First leg has {Count} maneuvers", firstLeg.Maneuvers?.Count ?? 0);

                    if (firstLeg.Maneuvers?.Any() == true)
                    {
                        logger.LogInformation("First 3 maneuvers:");
                        foreach (var maneuver in firstLeg.Maneuvers.Take(3))
                        {
                            logger.LogInformation("  - {Instruction}", maneuver.Instruction);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to calculate route");
            throw;
        }
    }
}
