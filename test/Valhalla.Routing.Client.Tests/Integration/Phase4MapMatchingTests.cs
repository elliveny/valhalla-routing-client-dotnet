using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Valhalla.Routing;
using Valhalla.Routing.Client.Tests.Helpers;

namespace Valhalla.Routing.Client.Tests.Integration;

/// <summary>
/// Integration tests for Phase 4 - Map Matching endpoints (trace_route and trace_attributes).
/// These tests run against a real Valhalla instance in Docker.
/// </summary>
[Trait("Category", "Integration")]
public class Phase4MapMatchingTests
{
    private const string ValhallaBaseUrl = "http://localhost:8002";

    [Fact]
    public async Task TraceRouteAsync_WithShape_ReturnsMatchedRoute()
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

        // Create a trace with closely-spaced GPS points (300-500m apart)
        // This simulates a realistic GPS trace that can be matched to roads
        var request = new TraceRouteRequest
        {
            Shape = TestLocations.GetShortGpsTrace(),
            Costing = CostingModel.Auto,
            Units = "kilometers",
            ShapeMatch = "map_snap", // Use map_snap for better GPS trace matching
        };

        // Act
        var response = await client.TraceRouteAsync(request);

        // Assert
        response.Should().NotBeNull("TraceRoute endpoint should return a response");
        response.Trip.Should().NotBeNull("TraceRoute should return a matched trip");

        var trip = response.Trip!;
        trip.Legs.Should().NotBeNull("Trip should have legs");
        trip.Legs.Should().NotBeEmpty("Trip should have at least one leg");
        trip.Summary.Should().NotBeNull("Trip should have a summary");
        trip.Summary!.Length.Should().BeGreaterThan(0, "Matched trip should have non-zero length");
        trip.Summary.Time.Should().BeGreaterThan(0, "Matched trip should have non-zero time");
        trip.Units.Should().Be("kilometers");

        foreach (var leg in trip.Legs!)
        {
            leg.Maneuvers.Should().NotBeNull("Each leg should have maneuvers");
            leg.Maneuvers.Should().NotBeEmpty("Each leg should have at least one maneuver");
            leg.Summary.Should().NotBeNull("Each leg should have a summary");
            leg.Shape.Should().NotBeNullOrEmpty("Each leg should have a shape (encoded polyline)");
        }

        // Verify Raw JSON is preserved
        response.Raw.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Object);
        response.Raw.TryGetProperty("trip", out var tripElement).Should().BeTrue();
    }

    [Fact]
    public async Task TraceRouteAsync_WithEncodedPolyline_ReturnsMatchedRoute()
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

        // Use a valid encoded polyline from actual Luxembourg roads
        // This polyline represents a path along roads in Luxembourg with proper spacing
        var request = new TraceRouteRequest
        {
            EncodedPolyline = TestLocations.GetValidEncodedPolyline(),
            Costing = CostingModel.Auto,
            Units = "kilometers",
            ShapeMatch = "map_snap", // Use map_snap for better GPS trace matching
        };

        // Act
        var response = await client.TraceRouteAsync(request);

        // Assert
        response.Should().NotBeNull("TraceRoute endpoint should return a response");
        response.Trip.Should().NotBeNull("TraceRoute should return a matched trip");

        var trip = response.Trip!;
        trip.Legs.Should().NotBeNull("Trip should have legs");
        trip.Legs.Should().NotBeEmpty("Trip should have at least one leg");
        trip.Summary.Should().NotBeNull("Trip should have a summary");
        trip.Summary!.Length.Should().BeGreaterThan(0, "Matched trip should have non-zero length");
        trip.Summary.Time.Should().BeGreaterThan(0, "Matched trip should have non-zero time");

        var leg = trip.Legs![0];
        leg.Maneuvers.Should().NotBeNull("Leg should have maneuvers");
        leg.Maneuvers.Should().NotBeEmpty("Leg should have at least one maneuver");
        leg.Summary.Should().NotBeNull("Leg should have a summary");
        leg.Shape.Should().NotBeNullOrEmpty("Leg should have a shape");

        // Verify Raw JSON is preserved
        response.Raw.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Object);
        response.Raw.TryGetProperty("trip", out var _).Should().BeTrue();
    }

    [Fact]
    public async Task TraceAttributesAsync_ReturnsMatchedPointsAndEdges()
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

        // Create a trace with closely-spaced GPS points for attribute extraction
        // Using realistic GPS trace points that can be matched to actual roads
        var request = new TraceAttributesRequest
        {
            Shape = TestLocations.GetShortGpsTrace(),
            Costing = CostingModel.Auto,
            ShapeMatch = "map_snap", // Use map_snap for better GPS trace matching
        };

        // Act
        var response = await client.TraceAttributesAsync(request);

        // Assert
        response.Should().NotBeNull("TraceAttributes endpoint should return a response");
        response.MatchedPoints.Should().NotBeNull("Response should have matched points");
        response.MatchedPoints.Should().NotBeEmpty("Response should have at least one matched point");
        response.Edges.Should().NotBeNull("Response should have edges");
        response.Edges.Should().NotBeEmpty("Response should have at least one edge");
        response.Shape.Should().NotBeNullOrEmpty("Response should have a shape (encoded polyline)");

        // Verify matched points structure
        var firstPoint = response.MatchedPoints![0];
        firstPoint.Lat.Should().BeGreaterThan(0, "Matched point should have a valid latitude");
        firstPoint.Lon.Should().BeGreaterThan(0, "Matched point should have a valid longitude");
        firstPoint.EdgeIndex.Should().BeGreaterThanOrEqualTo(0, "Matched point should reference an edge index");

        // Verify edges structure
        var firstEdge = response.Edges![0];
        firstEdge.Length.Should().BeGreaterThan(0, "Edge should have a positive length");

        // Verify Raw JSON is preserved
        response.Raw.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Object);
        response.Raw.TryGetProperty("matched_points", out var _).Should().BeTrue();
        response.Raw.TryGetProperty("edges", out var _).Should().BeTrue();
    }

    [Fact]
    public async Task TraceRouteAsync_NoMatch_ThrowsValhallaException()
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

        // Use GPS points that are very far apart in the ocean (no road network)
        // This should result in no match being possible
        var request = new TraceRouteRequest
        {
            Shape = new[]
            {
                new TracePoint { Lat = 0.0, Lon = 0.0 }, // Middle of the Atlantic Ocean
                new TracePoint { Lat = 0.1, Lon = 0.1 }, // Still in the ocean
            },
            Costing = CostingModel.Auto,
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValhallaException>(
            async () => await client.TraceRouteAsync(request));

        exception.Should().NotBeNull("Invalid trace should throw ValhallaException");
        exception.Message.Should().NotBeNullOrEmpty("Exception should have a descriptive message");
    }
}
