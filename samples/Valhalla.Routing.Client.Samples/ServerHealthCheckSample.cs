using Microsoft.Extensions.Logging;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Samples;

/// <summary>
/// Sample demonstrating server health check using the Status endpoint.
/// </summary>
public static class ServerHealthCheckSample
{
    /// <summary>
    /// Runs the server health check sample.
    /// </summary>
    /// <param name="client">The Valhalla client.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task RunAsync(IValhallaClient client, ILogger logger)
    {
        logger.LogInformation(string.Empty);
        logger.LogInformation("=== Server Health Check Sample ===");

        try
        {
            // Check server status
            var status = await client.StatusAsync().ConfigureAwait(false);

            logger.LogInformation("Server Status: OK");
            logger.LogInformation("Valhalla Version: {Version}", status.Version);
            logger.LogInformation("Has Tiles: {HasTiles}", status.HasTiles ?? false);
            logger.LogInformation("Has Admins: {HasAdmins}", status.HasAdmins ?? false);
            logger.LogInformation("Has Timezones: {HasTimezones}", status.HasTimezones ?? false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get server status");
            throw;
        }
    }
}
