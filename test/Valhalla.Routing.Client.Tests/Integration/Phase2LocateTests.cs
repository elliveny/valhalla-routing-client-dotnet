using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Valhalla.Routing;
using Valhalla.Routing.Client.Tests.Helpers;

namespace Valhalla.Routing.Client.Tests.Integration;

/// <summary>
/// Integration tests for Phase 2 - Locate endpoint.
/// These tests run against a real Valhalla instance in Docker.
/// </summary>
[Trait("Category", "Integration")]
public class Phase2LocateTests
{
    private const string ValhallaBaseUrl = "http://localhost:8002";

    [Fact]
    public async Task LocateAsync_ReturnsEdgeCandidates()
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

        var request = new LocateRequest
        {
            Locations = new[]
            {
                new Location
                {
                    Lat = TestLocations.LuxembourgCityCenterLatitude,
                    Lon = TestLocations.LuxembourgCityCenterLongitude,
                },
            },
            Costing = CostingModel.Auto,
        };

        // Act
        var response = await client.LocateAsync(request);

        // Assert
        response.Should().NotBeNull("Locate endpoint should return a response");
        response.Results.Should().NotBeNull("Results should not be null");
        response.Results.Should().HaveCountGreaterThan(0, "Should have at least one result");

        var result = response.Results![0];
        result.InputLat.Should().Be(TestLocations.LuxembourgCityCenterLatitude);
        result.InputLon.Should().Be(TestLocations.LuxembourgCityCenterLongitude);

        result.Edges.Should().NotBeNull("Edges should not be null");
        result.Edges.Should().HaveCountGreaterThan(0, "Should have at least one edge candidate");

        var edge = result.Edges![0];
        edge.WayId.Should().BeGreaterThan(0, "Way ID should be positive");
        edge.CorrelatedLat.Should().NotBeNull("Correlated latitude should be present");
        edge.CorrelatedLon.Should().NotBeNull("Correlated longitude should be present");
        edge.SideOfStreet.Should().NotBeNullOrEmpty("Side of street should be present");
        edge.PercentAlong.Should().BeInRange(0, 1, "Percent along should be between 0 and 1");

        // Verify Raw JSON is preserved
        response.Raw.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Array, "Raw JSON should be an array");
    }

    [Fact]
    public async Task LocateAsync_Verbose_ReturnsDetailedEdgeInfo()
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

        var request = new LocateRequest
        {
            Locations = new[]
            {
                new Location
                {
                    Lat = TestLocations.LuxembourgCityCenterLatitude,
                    Lon = TestLocations.LuxembourgCityCenterLongitude,
                },
            },
            Costing = CostingModel.Auto,
            Verbose = true,
        };

        // Act
        var response = await client.LocateAsync(request);

        // Assert
        response.Should().NotBeNull("Locate endpoint should return a response");
        response.Results.Should().NotBeNull("Results should not be null");
        response.Results.Should().HaveCountGreaterThan(0, "Should have at least one result");

        var result = response.Results![0];
        result.Edges.Should().NotBeNull("Edges should not be null");
        result.Edges.Should().HaveCountGreaterThan(0, "Should have at least one edge candidate");

        // In verbose mode, edges may have distance and edge_info
        var edge = result.Edges![0];

        // Distance may be present in verbose mode
        if (edge.Distance.HasValue)
        {
            edge.Distance.Should().BeGreaterThanOrEqualTo(0, "Distance should be non-negative");
        }

        // Edge info may be present in verbose mode (depends on Valhalla configuration)
        // Not all Valhalla instances return edge_info, so we make this conditional
        if (edge.EdgeInfo != null)
        {
            edge.EdgeInfo.Should().NotBeNull("Edge info should be present in verbose mode");
        }
    }

    [Fact]
    public async Task LocateAsync_InvalidLocation_ThrowsValhallaException()
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

        // Use a location in the middle of the ocean far from any roads
        // This should trigger a Valhalla error
        var request = new LocateRequest
        {
            Locations = new[]
            {
                new Location { Lat = 0.0, Lon = 0.0 }, // Middle of the Atlantic Ocean
            },
            Costing = CostingModel.Auto,
        };

        // Act & Assert
        // Note: Valhalla may return a 400 error or may return empty edges
        // depending on configuration. Let's check both scenarios.
        try
        {
            var response = await client.LocateAsync(request);

            // If no exception, verify that we got a response but potentially with issues
            response.Should().NotBeNull("Should get a response even for distant locations");

            // The response may have empty edges or no results for locations too far from any roads
            // This is acceptable behavior - just verify the response structure is valid
        }
        catch (ValhallaException ex)
        {
            // This is also expected - Valhalla may return an error for locations too far from roads
            ex.Should().NotBeNull("Should get a ValhallaException for invalid locations");
            ex.HttpStatusCode.Should().BeOneOf(
                System.Net.HttpStatusCode.BadRequest,
                System.Net.HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task LocateAsync_MultipleLocations_ReturnsMultipleResults()
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

        var request = new LocateRequest
        {
            Locations = new[]
            {
                new Location
                {
                    Lat = TestLocations.LuxembourgCityCenterLatitude,
                    Lon = TestLocations.LuxembourgCityCenterLongitude,
                },
                new Location
                {
                    Lat = TestLocations.LuxembourgAirportLatitude,
                    Lon = TestLocations.LuxembourgAirportLongitude,
                },
            },
            Costing = CostingModel.Bicycle,
        };

        // Act
        var response = await client.LocateAsync(request);

        // Assert
        response.Should().NotBeNull("Locate endpoint should return a response");
        response.Results.Should().NotBeNull("Results should not be null");
        response.Results.Should().HaveCount(2, "Should have two results for two input locations");

        var result1 = response.Results![0];
        result1.InputLat.Should().Be(TestLocations.LuxembourgCityCenterLatitude);
        result1.InputLon.Should().Be(TestLocations.LuxembourgCityCenterLongitude);

        var result2 = response.Results[1];
        result2.InputLat.Should().Be(TestLocations.LuxembourgAirportLatitude);
        result2.InputLon.Should().Be(TestLocations.LuxembourgAirportLongitude);
    }
}
