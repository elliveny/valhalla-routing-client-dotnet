using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Valhalla.Routing;
using Valhalla.Routing.Client.Tests.Helpers;

namespace Valhalla.Routing.Client.Tests.Integration;

/// <summary>
/// Integration tests for Phase 5 - Polyline utilities.
/// These tests run against a real Valhalla instance in Docker.
/// </summary>
[Trait("Category", "Integration")]
public class Phase5PolylineTests
{
    private const string ValhallaBaseUrl = "http://localhost:8002";

    [Fact]
    public async Task RouteAsync_Shape_CanBeDecodedByPolylineEncoder()
    {
        // Arrange
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30),
        };

        var options = Options.Create(new ValhallaClientOptions
        {
            BaseUri = new Uri(ValhallaBaseUrl),
            Timeout = TimeSpan.FromSeconds(30),
        });

        var logger = NullLogger<ValhallaClient>.Instance;
        var client = new ValhallaClient(httpClient, options, logger);

        var request = new RouteRequest
        {
            Locations = new[]
            {
                new Location { Lat = TestLocations.LuxembourgCityCenterLatitude, Lon = TestLocations.LuxembourgCityCenterLongitude },
                new Location { Lat = TestLocations.LuxembourgAirportLatitude, Lon = TestLocations.LuxembourgAirportLongitude },
            },
            Costing = CostingModel.Auto,
            Units = "kilometers",
        };

        // Act
        var response = await client.RouteAsync(request);

        // Assert
        response.Should().NotBeNull("Route endpoint should return a response");
        response.Trips.Should().HaveCount(1, "Route should return one trip");

        var trip = response.Trips![0];
        trip.Legs.Should().HaveCount(1, "Two-location route should have one leg");

        // Extract shape from the raw JSON response
        var shape = response.Raw.GetProperty("trip").GetProperty("legs")[0].GetProperty("shape").GetString();
        shape.Should().NotBeNullOrEmpty("Leg should have a shape polyline");

        // Decode the shape polyline
        var coordinates = PolylineEncoder.Decode(shape!);

        // Verify decoded coordinates
        coordinates.Should().NotBeNull("Decoded coordinates should not be null");
        coordinates.Should().NotBeEmpty("Decoded coordinates should not be empty");
        coordinates.Should().HaveCountGreaterThan(2, "Route should have multiple coordinate points");

        // Verify first coordinate is near the start location
        coordinates[0].latitude.Should().BeApproximately(TestLocations.LuxembourgCityCenterLatitude, 0.01, "First coordinate should be near start location");
        coordinates[0].longitude.Should().BeApproximately(TestLocations.LuxembourgCityCenterLongitude, 0.01, "First coordinate should be near start location");

        // Verify last coordinate is near the end location
        var lastIndex = coordinates.Count - 1;
        coordinates[lastIndex].latitude.Should().BeApproximately(TestLocations.LuxembourgAirportLatitude, 0.01, "Last coordinate should be near end location");
        coordinates[lastIndex].longitude.Should().BeApproximately(TestLocations.LuxembourgAirportLongitude, 0.01, "Last coordinate should be near end location");

        // Test round-trip encode/decode
        var reencoded = PolylineEncoder.Encode(coordinates);
        reencoded.Should().NotBeNullOrEmpty("Re-encoded polyline should not be empty");

        var redecoded = PolylineEncoder.Decode(reencoded);
        redecoded.Should().HaveCount(coordinates.Count, "Round-trip should preserve coordinate count");

        // Verify round-trip coordinates match within tolerance
        for (var i = 0; i < coordinates.Count; i++)
        {
            redecoded[i].latitude.Should().BeApproximately(coordinates[i].latitude, 0.000001, $"Latitude at index {i} should match after round-trip");
            redecoded[i].longitude.Should().BeApproximately(coordinates[i].longitude, 0.000001, $"Longitude at index {i} should match after round-trip");
        }
    }
}
