using Microsoft.Extensions.Logging;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Samples;

/// <summary>
/// Sample demonstrating finding the nearest road using the Locate endpoint.
/// </summary>
public static class NearestRoadSample
{
    /// <summary>
    /// Runs the nearest road sample.
    /// </summary>
    /// <param name="client">The Valhalla client.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task RunAsync(IValhallaClient client, ILogger logger)
    {
        logger.LogInformation(string.Empty);
        logger.LogInformation("=== Nearest Road Sample ===");

        try
        {
            // Find nearest road to a location
            var request = new LocateRequest
            {
                Locations = new List<Location>
                {
                    new() { Lat = 40.7128, Lon = -74.0060 },
                },
                Costing = CostingModel.Auto,
                Verbose = true,
            };

            logger.LogInformation(
                "Finding nearest road to ({Lat}, {Lon})",
                request.Locations[0].Lat,
                request.Locations[0].Lon);

            // Locate nearest road
            var response = await client.LocateAsync(request).ConfigureAwait(false);

            if (response.Results?.Any() == true && response.Results[0].Edges?.Any() == true)
            {
                var edges = response.Results[0].Edges!;
                logger.LogInformation("Found {Count} road candidate(s)", edges.Count);

                // Show details of the nearest road
                var nearest = edges[0];
                logger.LogInformation("Nearest road:");
                logger.LogInformation("  - Way ID: {Id}", nearest.WayId);
                logger.LogInformation("  - Distance: {Distance} meters", nearest.Distance);

                if (nearest.EdgeInfo != null)
                {
                    var roadName = "(unnamed)";
                    if (nearest.EdgeInfo.Names != null && nearest.EdgeInfo.Names.Count > 0)
                    {
                        roadName = nearest.EdgeInfo.Names[0];
                    }

                    logger.LogInformation("  - Road Name: {Name}", roadName);
                    logger.LogInformation("  - Road Class: {Class}", nearest.EdgeInfo.RoadClass);
                    logger.LogInformation("  - Speed Limit: {Speed} km/h", nearest.EdgeInfo.Speed);
                }
            }
            else
            {
                logger.LogInformation("No roads found near the location");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to locate nearest road");
            throw;
        }
    }
}
