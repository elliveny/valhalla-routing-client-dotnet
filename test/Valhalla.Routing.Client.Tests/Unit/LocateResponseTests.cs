using System.Text.Json;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Tests.Unit;

/// <summary>
/// Unit tests for LocateResponse deserialization.
/// </summary>
public class LocateResponseTests
{
    [Fact]
    public void LocateResponse_Deserializes_EdgeCandidates()
    {
        // Arrange
        var json = """
            [
                {
                    "input_lat": 49.6116,
                    "input_lon": 6.1319,
                    "edges": [
                        {
                            "way_id": 12345678,
                            "correlated_lat": 49.6117,
                            "correlated_lon": 6.1320,
                            "side_of_street": "left",
                            "percent_along": 0.42
                        }
                    ],
                    "nodes": [
                        {
                            "lat": 49.6118,
                            "lon": 6.1321
                        }
                    ]
                }
            ]
            """;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
            PropertyNameCaseInsensitive = true,
        };

        // Act
        var results = JsonSerializer.Deserialize<List<LocateResult>>(json, options);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);

        var result = results![0];
        result.InputLat.Should().Be(49.6116);
        result.InputLon.Should().Be(6.1319);

        result.Edges.Should().NotBeNull();
        result.Edges.Should().HaveCount(1);

        var edge = result.Edges![0];
        edge.WayId.Should().Be(12345678);
        edge.CorrelatedLat.Should().Be(49.6117);
        edge.CorrelatedLon.Should().Be(6.1320);
        edge.SideOfStreet.Should().Be("left");
        edge.PercentAlong.Should().Be(0.42);

        result.Nodes.Should().NotBeNull();
        result.Nodes.Should().HaveCount(1);

        var node = result.Nodes![0];
        node.Lat.Should().Be(49.6118);
        node.Lon.Should().Be(6.1321);
    }

    [Fact]
    public void LocateResponse_CanStoreRawJson()
    {
        // Arrange
        var json = """
            [
                {
                    "input_lat": 49.6116,
                    "input_lon": 6.1319,
                    "edges": [],
                    "custom_field": "should_be_preserved"
                }
            ]
            """;

        // Act
        using var doc = JsonDocument.Parse(json);
        var rawElement = doc.RootElement.Clone();

        // Create a LocateResponse with the Raw field
        var response = new LocateResponse
        {
            Raw = rawElement,
        };

        // Assert - Raw property can preserve arbitrary JSON including custom fields
        response.Raw.ValueKind.Should().Be(JsonValueKind.Array);
        response.Raw.GetArrayLength().Should().Be(1);

        var firstElement = response.Raw[0];
        firstElement.TryGetProperty("custom_field", out var customField).Should().BeTrue();
        customField.GetString().Should().Be("should_be_preserved");
    }

    [Fact]
    public void LocateResponse_Deserializes_VerboseEdgeInfo()
    {
        // Arrange
        var json = """
            [
                {
                    "input_lat": 49.6116,
                    "input_lon": 6.1319,
                    "edges": [
                        {
                            "way_id": 12345678,
                            "correlated_lat": 49.6117,
                            "correlated_lon": 6.1320,
                            "side_of_street": "neither",
                            "percent_along": 0.5,
                            "distance": 25.3,
                            "edge_info": {
                                "names": ["Main Street", "Route 66"],
                                "road_class": "primary",
                                "speed": 50,
                                "use": "road",
                                "length": 1.234,
                                "bridge": true,
                                "tunnel": false,
                                "toll": false
                            }
                        }
                    ]
                }
            ]
            """;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
            PropertyNameCaseInsensitive = true,
        };

        // Act
        var results = JsonSerializer.Deserialize<List<LocateResult>>(json, options);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(1);

        var edge = results![0].Edges![0];
        edge.Distance.Should().Be(25.3);

        var edgeInfo = edge.EdgeInfo;
        edgeInfo.Should().NotBeNull();
        edgeInfo!.Names.Should().ContainInOrder("Main Street", "Route 66");
        edgeInfo.RoadClass.Should().Be("primary");
        edgeInfo.Speed.Should().Be(50);
        edgeInfo.Use.Should().Be("road");
        edgeInfo.Length.Should().Be(1.234);
        edgeInfo.Bridge.Should().BeTrue();
        edgeInfo.Tunnel.Should().BeFalse();
        edgeInfo.Toll.Should().BeFalse();
    }

    [Fact]
    public void LocateResponse_Deserializes_MultipleLocations()
    {
        // Arrange
        var json = """
            [
                {
                    "input_lat": 49.6116,
                    "input_lon": 6.1319,
                    "edges": [
                        {
                            "way_id": 111,
                            "correlated_lat": 49.6117,
                            "correlated_lon": 6.1320,
                            "side_of_street": "left",
                            "percent_along": 0.1
                        }
                    ]
                },
                {
                    "input_lat": 49.6233,
                    "input_lon": 6.2044,
                    "edges": [
                        {
                            "way_id": 222,
                            "correlated_lat": 49.6234,
                            "correlated_lon": 6.2045,
                            "side_of_street": "right",
                            "percent_along": 0.9
                        }
                    ]
                }
            ]
            """;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
            PropertyNameCaseInsensitive = true,
        };

        // Act
        var results = JsonSerializer.Deserialize<List<LocateResult>>(json, options);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(2);

        results![0].InputLat.Should().Be(49.6116);
        results[0].Edges![0].WayId.Should().Be(111);

        results[1].InputLat.Should().Be(49.6233);
        results[1].Edges![0].WayId.Should().Be(222);
    }
}
