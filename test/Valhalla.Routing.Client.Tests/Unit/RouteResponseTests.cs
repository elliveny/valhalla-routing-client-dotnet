using System.Text.Json;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Tests.Unit;

/// <summary>
/// Unit tests for RouteResponse deserialization.
/// </summary>
public class RouteResponseTests
{
    [Fact]
    public void RouteResponse_Deserializes_TripWithLegsAndManeuvers()
    {
        // Arrange
        var json = """
        {
          "trip": {
            "locations": [
              {"lat": 52.517037, "lon": 13.388860, "type": "break"},
              {"lat": 52.529407, "lon": 13.397634, "type": "break"}
            ],
            "legs": [
              {
                "maneuvers": [
                  {
                    "type": 1,
                    "instruction": "Drive north on Friedrichstraße.",
                    "length": 0.543,
                    "time": 65,
                    "begin_shape_index": 0,
                    "end_shape_index": 12,
                    "street_names": ["Friedrichstraße"]
                  },
                  {
                    "type": 4,
                    "instruction": "You have arrived at your destination.",
                    "length": 0.0,
                    "time": 0,
                    "begin_shape_index": 12,
                    "end_shape_index": 12
                  }
                ],
                "summary": {
                  "length": 1.842,
                  "time": 312
                },
                "shape": "yzq~IcvxpA..."
              }
            ],
            "summary": {
              "length": 1.842,
              "time": 312,
              "min_lat": 52.517037,
              "min_lon": 13.388860,
              "max_lat": 52.529407,
              "max_lon": 13.397634,
              "has_toll": false,
              "has_highway": false,
              "has_ferry": false
            },
            "units": "kilometers",
            "language": "en-US"
          },
          "id": "my-request-id"
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

        var trips = new List<Trip>();
        if (root.TryGetProperty("trip", out var tripElement))
        {
            var primaryTrip = JsonSerializer.Deserialize<Trip>(tripElement.GetRawText(), options);
            if (primaryTrip != null)
            {
                trips.Add(primaryTrip);
            }
        }

        var response = new RouteResponse
        {
            Raw = root.Clone(),
            Id = root.TryGetProperty("id", out var id) ? id.GetString() : null,
            Trips = trips,
        };

        // Assert
        response.Id.Should().Be("my-request-id");
        response.Trips.Should().HaveCount(1);

        var trip = response.Trips![0];
        trip.Units.Should().Be("kilometers");
        trip.Language.Should().Be("en-US");
        trip.Legs.Should().HaveCount(1);

        var leg = trip.Legs![0];
        leg.Summary.Should().NotBeNull();
        leg.Summary!.Length.Should().Be(1.842);
        leg.Summary.Time.Should().Be(312);
        leg.Shape.Should().Be("yzq~IcvxpA...");

        leg.Maneuvers.Should().HaveCount(2);
        var maneuver = leg.Maneuvers![0];
        maneuver.Type.Should().Be(1);
        maneuver.Instruction.Should().Be("Drive north on Friedrichstraße.");
        maneuver.Length.Should().Be(0.543);
        maneuver.Time.Should().Be(65);
        maneuver.BeginShapeIndex.Should().Be(0);
        maneuver.EndShapeIndex.Should().Be(12);
        maneuver.StreetNames.Should().ContainSingle().Which.Should().Be("Friedrichstraße");
    }

    [Fact]
    public void RouteResponse_Deserializes_TripSummary()
    {
        // Arrange
        var json = """
        {
          "trip": {
            "summary": {
              "length": 1.842,
              "time": 312,
              "min_lat": 52.517037,
              "min_lon": 13.388860,
              "max_lat": 52.529407,
              "max_lon": 13.397634,
              "has_time_restrictions": false,
              "has_toll": true,
              "has_highway": true,
              "has_ferry": false
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

        var trips = new List<Trip>();
        if (root.TryGetProperty("trip", out var tripElement))
        {
            var primaryTrip = JsonSerializer.Deserialize<Trip>(tripElement.GetRawText(), options);
            if (primaryTrip != null)
            {
                trips.Add(primaryTrip);
            }
        }

        var response = new RouteResponse
        {
            Raw = root.Clone(),
            Trips = trips,
        };

        // Assert
        response.Trips.Should().HaveCount(1);
        var summary = response.Trips![0].Summary;
        summary.Should().NotBeNull();
        summary!.Length.Should().Be(1.842);
        summary.Time.Should().Be(312);
        summary.MinLat.Should().Be(52.517037);
        summary.MinLon.Should().Be(13.388860);
        summary.MaxLat.Should().Be(52.529407);
        summary.MaxLon.Should().Be(13.397634);
        summary.HasTimeRestrictions.Should().BeFalse();
        summary.HasToll.Should().BeTrue();
        summary.HasHighway.Should().BeTrue();
        summary.HasFerry.Should().BeFalse();
    }

    [Fact]
    public void RouteResponse_Deserializes_RawJsonPreserved()
    {
        // Arrange
        var json = """
        {
          "trip": {
            "summary": {
              "length": 1.842,
              "time": 312
            }
          },
          "id": "test-id",
          "custom_field": "custom_value"
        }
        """;

        // Act
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var response = new RouteResponse
        {
            Raw = root.Clone(),
            Id = root.TryGetProperty("id", out var id) ? id.GetString() : null,
        };

        // Assert
        response.Raw.TryGetProperty("custom_field", out var customField).Should().BeTrue();
        customField.GetString().Should().Be("custom_value");
        response.Id.Should().Be("test-id");
    }

    [Fact]
    public void RouteResponse_Deserializes_WithAlternates()
    {
        // Arrange
        var json = """
        {
          "trip": {
            "summary": {
              "length": 1.842,
              "time": 312
            }
          },
          "alternates": [
            {
              "trip": {
                "summary": {
                  "length": 2.156,
                  "time": 378
                }
              }
            },
            {
              "trip": {
                "summary": {
                  "length": 2.521,
                  "time": 425
                }
              }
            }
          ]
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

        var trips = new List<Trip>();

        // Primary trip
        if (root.TryGetProperty("trip", out var tripElement))
        {
            var primaryTrip = JsonSerializer.Deserialize<Trip>(tripElement.GetRawText(), options);
            if (primaryTrip != null)
            {
                trips.Add(primaryTrip);
            }
        }

        // Alternate trips
        if (root.TryGetProperty("alternates", out var alternatesElement))
        {
            foreach (var alt in alternatesElement.EnumerateArray().Where(a => a.TryGetProperty("trip", out _)))
            {
                // Where clause guarantees 'trip' property exists
                var altTripElement = alt.GetProperty("trip");
                var altTrip = JsonSerializer.Deserialize<Trip>(altTripElement.GetRawText(), options);
                if (altTrip != null)
                {
                    trips.Add(altTrip);
                }
            }
        }

        var response = new RouteResponse
        {
            Raw = root.Clone(),
            Trips = trips,
        };

        // Assert
        response.Trips.Should().HaveCount(3);
        response.Trips![0].Summary!.Length.Should().Be(1.842);
        response.Trips[1].Summary!.Length.Should().Be(2.156);
        response.Trips[2].Summary!.Length.Should().Be(2.521);
    }
}
