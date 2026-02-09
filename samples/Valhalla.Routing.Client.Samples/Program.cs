using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Valhalla.Routing;
using Valhalla.Routing.Client.Samples;

// Configure services
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Add Valhalla client
// Update this URL to point to your Valhalla server
var valhallaBaseUrl = Environment.GetEnvironmentVariable("VALHALLA_URL") ?? "http://localhost:8002";
services.AddValhallaClient(options =>
{
    options.BaseUri = new Uri(valhallaBaseUrl);
    options.Timeout = TimeSpan.FromSeconds(30);
});

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Get the client
var client = serviceProvider.GetRequiredService<IValhallaClient>();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Valhalla Routing Client Samples");
logger.LogInformation("Using Valhalla server at: {Url}", valhallaBaseUrl);

const string separator = "==================================================";
logger.LogInformation(separator);

try
{
    // Run all samples
    await ServerHealthCheckSample.RunAsync(client, logger).ConfigureAwait(false);
    await BasicRoutingSample.RunAsync(client, logger).ConfigureAwait(false);
    await MultiStopRouteSample.RunAsync(client, logger).ConfigureAwait(false);
    await NearestRoadSample.RunAsync(client, logger).ConfigureAwait(false);
    await GpsTraceMatchingSample.RunAsync(client, logger).ConfigureAwait(false);
    await TraceAttributesSample.RunAsync(client, logger).ConfigureAwait(false);

    logger.LogInformation(separator);
    logger.LogInformation("All samples completed successfully!");
}
catch (HttpRequestException ex)
{
    logger.LogError(ex, "Error running samples. Make sure Valhalla server is running.");
    logger.LogInformation("Start Valhalla with: docker-compose -f docker-compose.integration.yml up -d");
    return 1;
}

return 0;
