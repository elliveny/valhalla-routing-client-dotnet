using System.Text.Json;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Tests.Unit;

/// <summary>
/// Unit tests for TraceRouteResponse deserialization.
/// </summary>
public class TraceRouteResponseTests
{
    [Fact]
    public void TraceRouteResponse_Deserializes_TripData()
    {
        // Arrange
        var json = """
        {
          "trip": {
            "locations": [
              {"lat": 49.6116, "lon": 6.1319, "type": "break"},
              {"lat": 49.6233, "lon": 6.2044, "type": "break"}
            ],
            "legs": [
              {
                "maneuvers": [
                  {
                    "type": 1,
                    "instruction": "Drive northwest on Main Street.",
                    "length": 1.234,
                    "time": 180,
                    "begin_shape_index": 0,
                    "end_shape_index": 25,
                    "street_names": ["Main Street"]
                  }
                ],
                "summary": {
                  "length": 8.5,
                  "time": 720
                },
                "shape": "encoded_polyline_string"
              }
            ],
            "summary": {
              "length": 8.5,
              "time": 720,
              "min_lat": 49.6116,
              "min_lon": 6.1319,
              "max_lat": 49.6233,
              "max_lon": 6.2044,
              "has_toll": false,
              "has_highway": true,
              "has_ferry": false
            },
            "units": "kilometers",
            "language": "en-US"
          },
          "id": "trace-request-123"
        }
        """;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
            PropertyNameCaseInsensitive = true,
        };

        // Act
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Trip? trip = null;
        if (root.TryGetProperty("trip", out var tripElement))
        {
            trip = JsonSerializer.Deserialize<Trip>(tripElement.GetRawText(), options);
        }

        var response = new TraceRouteResponse
        {
            Raw = root.Clone(),
            Id = root.TryGetProperty("id", out var idElement) ? idElement.GetString() : null,
            Trip = trip,
        };

        // Assert
        response.Id.Should().Be("trace-request-123");
        response.Trip.Should().NotBeNull();

        var resultTrip = response.Trip!;
        resultTrip.Units.Should().Be("kilometers");
        resultTrip.Language.Should().Be("en-US");
        resultTrip.Legs.Should().HaveCount(1);

        var leg = resultTrip.Legs![0];
        leg.Summary.Should().NotBeNull();
        leg.Summary!.Length.Should().Be(8.5);
        leg.Summary.Time.Should().Be(720);
        leg.Shape.Should().Be("encoded_polyline_string");

        leg.Maneuvers.Should().HaveCount(1);
        var maneuver = leg.Maneuvers![0];
        maneuver.Type.Should().Be(1);
        maneuver.Instruction.Should().Be("Drive northwest on Main Street.");
        maneuver.Length.Should().Be(1.234);
        maneuver.Time.Should().Be(180);

        var tripSummary = resultTrip.Summary!;
        tripSummary.Length.Should().Be(8.5);
        tripSummary.Time.Should().Be(720);
        tripSummary.MinLat.Should().Be(49.6116);
        tripSummary.MaxLon.Should().Be(6.2044);
        tripSummary.HasToll.Should().BeFalse();
        tripSummary.HasHighway.Should().BeTrue();
        tripSummary.HasFerry.Should().BeFalse();

        // Verify Raw JSON is preserved
        response.Raw.ValueKind.Should().Be(JsonValueKind.Object);
        response.Raw.TryGetProperty("id", out var rawId).Should().BeTrue();
        rawId.GetString().Should().Be("trace-request-123");
    }

    [Fact]
    public void TraceRouteResponse_Deserializes_MinimalResponse()
    {
        // Arrange
        var json = """
        {
          "trip": {
            "summary": {
              "length": 5.2,
              "time": 360
            }
          }
        }
        """;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
            PropertyNameCaseInsensitive = true,
        };

        // Act
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Trip? trip = null;
        if (root.TryGetProperty("trip", out var tripElement))
        {
            trip = JsonSerializer.Deserialize<Trip>(tripElement.GetRawText(), options);
        }

        var response = new TraceRouteResponse
        {
            Raw = root.Clone(),
            Trip = trip,
        };

        // Assert
        response.Trip.Should().NotBeNull();
        response.Trip!.Summary.Should().NotBeNull();
        response.Trip.Summary!.Length.Should().Be(5.2);
        response.Trip.Summary.Time.Should().Be(360);
    }
}
