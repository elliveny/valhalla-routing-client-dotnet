using System.Text.Json;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Tests.Unit;

/// <summary>
/// Unit tests for TraceAttributesResponse deserialization.
/// </summary>
public class TraceAttributesResponseTests
{
    [Fact]
    public void TraceAttributesResponse_Deserializes_MatchedPointsAndEdges()
    {
        // Arrange
        var json = """
        {
          "matched_points": [
            {
              "lat": 49.6117,
              "lon": 6.1320,
              "type": "matched",
              "edge_index": 0,
              "distance_along_edge": 0.123
            },
            {
              "lat": 49.6234,
              "lon": 6.2045,
              "type": "matched",
              "edge_index": 1,
              "distance_along_edge": 0.456
            }
          ],
          "edges": [
            {
              "length": 0.543,
              "speed": 50,
              "road_class": "secondary",
              "begin_shape_index": 0,
              "end_shape_index": 12,
              "names": ["Main Street"]
            },
            {
              "length": 1.234,
              "speed": 40,
              "road_class": "tertiary",
              "begin_shape_index": 12,
              "end_shape_index": 28,
              "names": ["Oak Avenue"]
            }
          ],
          "shape": "encoded_polyline_for_matched_route",
          "units": "kilometers",
          "id": "trace-attrs-123"
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

        string? id = null;
        if (root.TryGetProperty("id", out var idElement))
        {
            id = idElement.GetString();
        }

        string? units = null;
        if (root.TryGetProperty("units", out var unitsElement))
        {
            units = unitsElement.GetString();
        }

        string? shape = null;
        if (root.TryGetProperty("shape", out var shapeElement))
        {
            shape = shapeElement.GetString();
        }

        IReadOnlyList<MatchedPoint>? matchedPoints = null;
        if (root.TryGetProperty("matched_points", out var matchedPointsElement))
        {
            matchedPoints = JsonSerializer.Deserialize<List<MatchedPoint>>(matchedPointsElement.GetRawText(), options);
        }

        IReadOnlyList<TraceEdge>? edges = null;
        if (root.TryGetProperty("edges", out var edgesElement))
        {
            edges = JsonSerializer.Deserialize<List<TraceEdge>>(edgesElement.GetRawText(), options);
        }

        var response = new TraceAttributesResponse
        {
            Raw = root.Clone(),
            Id = id,
            Units = units,
            MatchedPoints = matchedPoints,
            Edges = edges,
            Shape = shape,
        };

        // Assert
        response.Id.Should().Be("trace-attrs-123");
        response.Units.Should().Be("kilometers");
        response.Shape.Should().Be("encoded_polyline_for_matched_route");

        // Verify matched points
        response.MatchedPoints.Should().NotBeNull();
        response.MatchedPoints.Should().HaveCount(2);

        var point1 = response.MatchedPoints![0];
        point1.Lat.Should().Be(49.6117);
        point1.Lon.Should().Be(6.1320);
        point1.Type.Should().Be("matched");
        point1.EdgeIndex.Should().Be(0);
        point1.DistanceAlongEdge.Should().Be(0.123);

        var point2 = response.MatchedPoints[1];
        point2.Lat.Should().Be(49.6234);
        point2.Lon.Should().Be(6.2045);
        point2.EdgeIndex.Should().Be(1);

        // Verify edges
        response.Edges.Should().NotBeNull();
        response.Edges.Should().HaveCount(2);

        var edge1 = response.Edges![0];
        edge1.Length.Should().Be(0.543);
        edge1.Speed.Should().Be(50);
        edge1.RoadClass.Should().Be("secondary");
        edge1.BeginShapeIndex.Should().Be(0);
        edge1.EndShapeIndex.Should().Be(12);
        edge1.Names.Should().ContainSingle().Which.Should().Be("Main Street");

        var edge2 = response.Edges[1];
        edge2.Length.Should().Be(1.234);
        edge2.Speed.Should().Be(40);
        edge2.RoadClass.Should().Be("tertiary");
        edge2.Names.Should().ContainSingle().Which.Should().Be("Oak Avenue");

        // Verify Raw JSON is preserved
        response.Raw.ValueKind.Should().Be(JsonValueKind.Object);
        response.Raw.TryGetProperty("id", out var rawId).Should().BeTrue();
        rawId.GetString().Should().Be("trace-attrs-123");
    }

    [Fact]
    public void TraceAttributesResponse_Deserializes_MinimalResponse()
    {
        // Arrange
        var json = """
        {
          "matched_points": [],
          "edges": [],
          "units": "miles"
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

        string? units = null;
        if (root.TryGetProperty("units", out var unitsElement))
        {
            units = unitsElement.GetString();
        }

        IReadOnlyList<MatchedPoint>? matchedPoints = null;
        if (root.TryGetProperty("matched_points", out var matchedPointsElement))
        {
            matchedPoints = JsonSerializer.Deserialize<List<MatchedPoint>>(matchedPointsElement.GetRawText(), options);
        }

        IReadOnlyList<TraceEdge>? edges = null;
        if (root.TryGetProperty("edges", out var edgesElement))
        {
            edges = JsonSerializer.Deserialize<List<TraceEdge>>(edgesElement.GetRawText(), options);
        }

        var response = new TraceAttributesResponse
        {
            Raw = root.Clone(),
            Units = units,
            MatchedPoints = matchedPoints,
            Edges = edges,
        };

        // Assert
        response.Units.Should().Be("miles");
        response.MatchedPoints.Should().NotBeNull();
        response.MatchedPoints.Should().BeEmpty();
        response.Edges.Should().NotBeNull();
        response.Edges.Should().BeEmpty();
    }

    [Fact]
    public void TraceAttributesResponse_Deserializes_ExtendedEdgeProperties()
    {
        // Arrange
        var json = """
        {
          "edges": [
            {
              "length": 0.543,
              "speed": 50,
              "speed_limit": 60,
              "road_class": "secondary",
              "begin_shape_index": 0,
              "end_shape_index": 12,
              "names": ["Main Street"],
              "way_id": 123456789,
              "id": 987654321,
              "use": "road",
              "surface": "paved",
              "toll": false,
              "tunnel": false,
              "bridge": true
            },
            {
              "length": 0.2,
              "speed": 30,
              "speed_limit": 40,
              "road_class": "service",
              "use": "ramp",
              "surface": "gravel",
              "toll": true,
              "tunnel": true,
              "bridge": false,
              "way_id": 111222333
            }
          ],
          "units": "kilometers"
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

        IReadOnlyList<TraceEdge>? edges = null;
        if (root.TryGetProperty("edges", out var edgesElement))
        {
            edges = JsonSerializer.Deserialize<List<TraceEdge>>(edgesElement.GetRawText(), options);
        }

        var response = new TraceAttributesResponse
        {
            Raw = root.Clone(),
            Edges = edges,
        };

        // Assert
        response.Edges.Should().NotBeNull();
        response.Edges.Should().HaveCount(2);

        var edge1 = response.Edges![0];
        edge1.Length.Should().Be(0.543);
        edge1.Speed.Should().Be(50);
        edge1.SpeedLimit.Should().Be(60);
        edge1.RoadClass.Should().Be("secondary");
        edge1.BeginShapeIndex.Should().Be(0);
        edge1.EndShapeIndex.Should().Be(12);
        edge1.Names.Should().ContainSingle().Which.Should().Be("Main Street");
        edge1.WayId.Should().Be(123456789);
        edge1.Id.Should().Be(987654321);
        edge1.Use.Should().Be("road");
        edge1.Surface.Should().Be("paved");
        edge1.Toll.Should().BeFalse();
        edge1.Tunnel.Should().BeFalse();
        edge1.Bridge.Should().BeTrue();

        var edge2 = response.Edges[1];
        edge2.Length.Should().Be(0.2);
        edge2.Speed.Should().Be(30);
        edge2.SpeedLimit.Should().Be(40);
        edge2.RoadClass.Should().Be("service");
        edge2.Use.Should().Be("ramp");
        edge2.Surface.Should().Be("gravel");
        edge2.Toll.Should().BeTrue();
        edge2.Tunnel.Should().BeTrue();
        edge2.Bridge.Should().BeFalse();
        edge2.WayId.Should().Be(111222333);
    }
}
