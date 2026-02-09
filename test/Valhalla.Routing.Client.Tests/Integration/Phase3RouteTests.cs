using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Valhalla.Routing;
using Valhalla.Routing.Client.Tests.Helpers;

namespace Valhalla.Routing.Client.Tests.Integration;

/// <summary>
/// Integration tests for Phase 3 - Route endpoint.
/// These tests run against a real Valhalla instance in Docker.
/// </summary>
[Trait("Category", "Integration")]
public class Phase3RouteTests
{
    private const string ValhallaBaseUrl = "http://localhost:8002";

    [Fact]
    public async Task RouteAsync_TwoLocations_ReturnsTripWithOneLeg()
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
        response.Trips.Should().HaveCount(1, "Two-location route should return one trip");

        var trip = response.Trips![0];
        trip.Legs.Should().HaveCount(1, "Two-location route should have one leg");
        trip.Summary.Should().NotBeNull("Trip should have a summary");
        trip.Summary!.Length.Should().BeGreaterThan(0, "Trip should have non-zero length");
        trip.Summary.Time.Should().BeGreaterThan(0, "Trip should have non-zero time");
        trip.Units.Should().Be("kilometers");

        var leg = trip.Legs![0];
        leg.Maneuvers.Should().NotBeNull("Leg should have maneuvers");
        leg.Maneuvers.Should().NotBeEmpty("Leg should have at least one maneuver");
        leg.Summary.Should().NotBeNull("Leg should have a summary");
        leg.Shape.Should().NotBeNullOrEmpty("Leg should have a shape (encoded polyline)");

        // Verify Raw JSON is preserved
        response.Raw.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Object);
        response.Raw.TryGetProperty("trip", out var tripElement).Should().BeTrue();
    }

    [Fact]
    public async Task RouteAsync_ThreeLocations_ReturnsTripWithTwoLegs()
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
                new Location { Lat = TestLocations.KirchbergLatitude, Lon = TestLocations.KirchbergLongitude },
                new Location { Lat = TestLocations.LuxembourgAirportLatitude, Lon = TestLocations.LuxembourgAirportLongitude },
            },
            Costing = CostingModel.Auto,
            Units = "kilometers",
        };

        // Act
        var response = await client.RouteAsync(request);

        // Assert
        response.Should().NotBeNull("Route endpoint should return a response");
        response.Trips.Should().HaveCount(1, "Three-location route should return one trip");

        var trip = response.Trips![0];
        trip.Legs.Should().HaveCount(2, "Three-location route should have two legs");
        trip.Summary.Should().NotBeNull("Trip should have a summary");

        foreach (var leg in trip.Legs!)
        {
            leg.Maneuvers.Should().NotBeNull("Each leg should have maneuvers");
            leg.Maneuvers.Should().NotBeEmpty("Each leg should have at least one maneuver");
            leg.Summary.Should().NotBeNull("Each leg should have a summary");
            leg.Shape.Should().NotBeNullOrEmpty("Each leg should have a shape");
        }
    }

    [Fact]
    public async Task RouteAsync_WithAlternates_ReturnsMultipleTrips()
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
            Alternates = 2,
            Units = "kilometers",
        };

        // Act
        var response = await client.RouteAsync(request);

        // Assert
        response.Should().NotBeNull("Route endpoint should return a response");

        // Note: Valhalla may return fewer alternates than requested if suitable alternatives don't exist
        // We just verify that we get at least the primary trip, and potentially more
        response.Trips.Should().NotBeEmpty("Route should return at least the primary trip");
        response.Trips!.Count.Should().BeGreaterThanOrEqualTo(1, "Route should return at least one trip");

        // If alternates were found, verify they're different routes
        if (response.Trips.Count > 1)
        {
            var primaryLength = response.Trips[0].Summary?.Length;
            var hasAlternate = response.Trips.Skip(1).Any(t => t.Summary?.Length != primaryLength);
            hasAlternate.Should().BeTrue("Alternate routes should have different lengths");
        }

        // Verify Raw JSON structure
        response.Raw.TryGetProperty("trip", out var _).Should().BeTrue("Response should have primary trip");
    }

    [Fact]
    public async Task RouteAsync_NoRoutePossible_ThrowsValhallaException()
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

        // Use locations that are very far apart in different continents
        // to increase likelihood of route failure
        var request = new RouteRequest
        {
            Locations = new[]
            {
                new Location { Lat = 90.0, Lon = 0.0 }, // North Pole (inaccessible)
                new Location { Lat = -90.0, Lon = 0.0 }, // South Pole (inaccessible)
            },
            Costing = CostingModel.Auto,
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValhallaException>(
            async () => await client.RouteAsync(request));

        exception.Should().NotBeNull("Inaccessible locations should throw ValhallaException");
        exception.Message.Should().NotBeNullOrEmpty("Exception should have a descriptive message");
    }

    [Fact]
    public async Task RouteAsync_DifferentCostingModels_ReturnsValidRoutes()
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

        var locations = new[]
        {
            new Location { Lat = TestLocations.LuxembourgCityCenterLatitude, Lon = TestLocations.LuxembourgCityCenterLongitude },
            new Location { Lat = TestLocations.LuxembourgAirportLatitude, Lon = TestLocations.LuxembourgAirportLongitude },
        };

        // Test auto costing
        var autoRequest = new RouteRequest
        {
            Locations = locations,
            Costing = CostingModel.Auto,
        };

        var autoResponse = await client.RouteAsync(autoRequest);
        autoResponse.Should().NotBeNull();
        autoResponse.Trips.Should().HaveCount(1);

        // Test bicycle costing
        var bicycleRequest = new RouteRequest
        {
            Locations = locations,
            Costing = CostingModel.Bicycle,
        };

        var bicycleResponse = await client.RouteAsync(bicycleRequest);
        bicycleResponse.Should().NotBeNull();
        bicycleResponse.Trips.Should().HaveCount(1);

        // Test pedestrian costing
        var pedestrianRequest = new RouteRequest
        {
            Locations = locations,
            Costing = CostingModel.Pedestrian,
        };

        var pedestrianResponse = await client.RouteAsync(pedestrianRequest);
        pedestrianResponse.Should().NotBeNull();
        pedestrianResponse.Trips.Should().HaveCount(1);

        // Verify that different costing models may produce different routes
        // (though for very short distances they might be similar)
        autoResponse.Trips![0].Summary.Should().NotBeNull();
        bicycleResponse.Trips![0].Summary.Should().NotBeNull();
        pedestrianResponse.Trips![0].Summary.Should().NotBeNull();
    }
}
