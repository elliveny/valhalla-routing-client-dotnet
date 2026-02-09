# Valhalla Routing Client Samples

This directory contains practical examples demonstrating how to use the Valhalla Routing Client library.

## Prerequisites

Before running these samples, you need a running Valhalla server. The easiest way is using Docker:

```bash
# From the repository root
docker-compose -f docker-compose.integration.yml up -d
```

The samples expect the Valhalla server to be available at `http://localhost:8002` by default. You can override this by setting the `VALHALLA_URL` environment variable:

```bash
export VALHALLA_URL=http://your-valhalla-server:8002
dotnet run
```

## Running the Samples

```bash
cd samples/Valhalla.Routing.Client.Samples
dotnet run
```

This will run all the samples in sequence, demonstrating various features of the client library.

## Samples Included

### 1. Server Health Check
**File:** `ServerHealthCheckSample.cs`

Demonstrates how to check the Valhalla server status and retrieve version information.

```csharp
var status = await client.StatusAsync();
Console.WriteLine($"Valhalla Version: {status.Version}");
```

### 2. Basic Routing
**File:** `BasicRoutingSample.cs`

Shows how to calculate a simple route between two locations.

```csharp
var request = new RouteRequest
{
    Locations = new List<Location>
    {
        new() { Lat = 40.7128, Lon = -74.0060 },
        new() { Lat = 40.7589, Lon = -73.9851 },
    },
    Costing = CostingModel.Auto,
};

var response = await client.RouteAsync(request);
```

### 3. Multi-Stop Route
**File:** `MultiStopRouteSample.cs`

Demonstrates routing with multiple waypoints (more than 2 stops).

```csharp
var request = new RouteRequest
{
    Locations = new List<Location>
    {
        new() { Lat = 40.7128, Lon = -74.0060 },
        new() { Lat = 40.7489, Lon = -73.9680 },
        new() { Lat = 40.7589, Lon = -73.9851 },
        new() { Lat = 40.7829, Lon = -73.9654 },
    },
    Costing = CostingModel.Auto,
};
```

### 4. Nearest Road (Locate)
**File:** `NearestRoadSample.cs`

Shows how to find the nearest road to a given location using the Locate endpoint.

```csharp
var request = new LocateRequest
{
    Locations = new List<Location>
    {
        new() { Lat = 40.7128, Lon = -74.0060 },
    },
    Costing = CostingModel.Auto,
    Verbose = true,
};

var response = await client.LocateAsync(request);
```

### 5. GPS Trace Matching
**File:** `GpsTraceMatchingSample.cs`

Demonstrates how to match a GPS trace to the road network using the TraceRoute endpoint.

```csharp
var tracePoints = new List<TracePoint>
{
    new() { Lat = 40.7128, Lon = -74.0060, Time = DateTimeOffset.UtcNow.AddMinutes(-5) },
    new() { Lat = 40.7189, Lon = -74.0020, Time = DateTimeOffset.UtcNow.AddMinutes(-4) },
    // ... more points
};

var request = new TraceRouteRequest
{
    Shape = tracePoints,
    Costing = CostingModel.Auto,
    ShapeMatch = "map_snap",
};

var response = await client.TraceRouteAsync(request);
```

### 6. Trace Attributes
**File:** `TraceAttributesSample.cs`

Shows how to extract detailed edge attributes from a GPS trace using the TraceAttributes endpoint.

```csharp
var request = new TraceAttributesRequest
{
    Shape = tracePoints,
    Costing = CostingModel.Auto,
    ShapeMatch = "map_snap",
    Filters = new FilterAttributes
    {
        Attributes = new List<string> { "edge.names", "edge.id", "edge.speed" },
        Action = "include",
    },
};

var response = await client.TraceAttributesAsync(request);
```

## Dependency Injection Setup

The `Program.cs` file demonstrates how to set up the Valhalla client with dependency injection:

```csharp
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Add Valhalla client
services.AddValhallaClient(options =>
{
    options.BaseUri = new Uri("http://localhost:8002");
    options.Timeout = TimeSpan.FromSeconds(30);
});

var serviceProvider = services.BuildServiceProvider();
var client = serviceProvider.GetRequiredService<IValhallaClient>();
```

## Alternative: Builder Pattern (No DI)

You can also use the client without dependency injection:

```csharp
var client = ValhallaClientBuilder
    .Create()
    .WithBaseUrl("http://localhost:8002")
    .WithTimeout(TimeSpan.FromSeconds(30))
    .Build();
```

## Learn More

- [Main README](../../README.md) - Project overview and documentation
- [Valhalla API Documentation](https://valhalla.github.io/valhalla/api/) - Official Valhalla API reference
- [.NET Best Practices](../../docs/dotnet-best-practices.md) - Coding standards and guidelines

## Troubleshooting

### "Connection refused" errors

Make sure the Valhalla server is running:

```bash
docker-compose -f docker-compose.integration.yml ps
```

If not running, start it:

```bash
docker-compose -f docker-compose.integration.yml up -d
```

### "No route found" errors

The sample coordinates are examples and may not work with your specific Valhalla tile data. Update the coordinates in the sample files to match locations within your tile coverage area.
