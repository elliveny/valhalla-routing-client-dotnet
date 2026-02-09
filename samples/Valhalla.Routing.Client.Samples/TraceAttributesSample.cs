using Microsoft.Extensions.Logging;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Samples;

/// <summary>
/// Sample demonstrating trace attributes extraction using the TraceAttributes endpoint.
/// </summary>
public static class TraceAttributesSample
{
    /// <summary>
    /// Runs the trace attributes sample.
    /// </summary>
    /// <param name="client">The Valhalla client.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task RunAsync(IValhallaClient client, ILogger logger)
    {
        logger.LogInformation(string.Empty);
        logger.LogInformation("=== Trace Attributes Sample ===");

        try
        {
            // Create a GPS trace
            var tracePoints = new List<TracePoint>
            {
                new() { Lat = 40.7128, Lon = -74.0060 },
                new() { Lat = 40.7189, Lon = -74.0020 },
                new() { Lat = 40.7249, Lon = -73.9980 },
                new() { Lat = 40.7309, Lon = -73.9940 },
            };

            var request = new TraceAttributesRequest
            {
                Shape = tracePoints,
                Costing = CostingModel.Auto,
                ShapeMatch = "map_snap",
                Filters = new FilterAttributes
                {
                    Attributes = new List<string> { "edge.names", "edge.id", "edge.speed", "edge.length" },
                    Action = "include",
                },
            };

            logger.LogInformation("Extracting attributes for GPS trace with {Count} points", tracePoints.Count);

            // Get trace attributes
            var response = await client.TraceAttributesAsync(request).ConfigureAwait(false);

            if (response.MatchedPoints?.Any() == true)
            {
                logger.LogInformation("Trace attributes extracted successfully!");
                logger.LogInformation("Matched {Count} points", response.MatchedPoints.Count);

                // Show details of matched points
                foreach (var point in response.MatchedPoints.Take(3))
                {
                    logger.LogInformation("Matched Point:");
                    logger.LogInformation("  - Match Type: {Type}", point.Type);
                    logger.LogInformation("  - Edge Index: {Index}", point.EdgeIndex);
                }
            }

            if (response.Edges?.Any() == true)
            {
                logger.LogInformation("Found {Count} edges in the matched trace", response.Edges.Count);

                // Show details of matched edges
                foreach (var edge in response.Edges.Take(3))
                {
                    logger.LogInformation("Edge:");
                    logger.LogInformation("  - Names: {Names}", edge.Names?.Any() == true ? string.Join(", ", edge.Names) : "(unnamed)");
                    logger.LogInformation("  - Speed: {Speed} km/h", edge.Speed);
                    logger.LogInformation("  - Length: {Length} km", edge.Length);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to extract trace attributes");
            throw;
        }
    }
}
