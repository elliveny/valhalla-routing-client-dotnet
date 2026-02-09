using System.Text.Json;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Tests.Unit;

/// <summary>
/// Unit tests for TraceRouteRequest serialization and validation.
/// </summary>
public class TraceRouteRequestTests
{
    [Fact]
    public void TraceRouteRequest_SerializesToSnakeCase()
    {
        // Arrange
        var request = new TraceRouteRequest
        {
            Shape = new[]
            {
                new TracePoint { Lat = 49.6116, Lon = 6.1319 },
                new TracePoint { Lat = 49.6233, Lon = 6.2044 },
            },
            Costing = CostingModel.Auto,
            ShapeMatch = "map_snap",
            UseTimestamps = true,
            LinearReferences = false,
            Units = "kilometers",
            Language = "en-US",
            DirectionsType = "instructions",
            Id = "test-trace-route",
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };

        // Act
        var json = JsonSerializer.Serialize(request, options);

        // Assert
        json.Should().Contain("\"costing\":\"auto\"");
        json.Should().Contain("\"shape\":");
        json.Should().Contain("\"shape_match\":\"map_snap\"");
        json.Should().Contain("\"use_timestamps\":true");
        json.Should().Contain("\"linear_references\":false");
        json.Should().Contain("\"units\":\"kilometers\"");
        json.Should().Contain("\"language\":\"en-US\"");
        json.Should().Contain("\"directions_type\":\"instructions\"");
        json.Should().Contain("\"id\":\"test-trace-route\"");
    }

    [Fact]
    public void TraceRouteRequest_Validation_ThrowsIfNoShapeOrPolyline()
    {
        // Arrange - neither Shape nor EncodedPolyline provided
        var request = new TraceRouteRequest
        {
            Costing = CostingModel.Auto,
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("*Either Shape or EncodedPolyline must be provided*")
            .And.ParamName.Should().Be("Shape");
    }

    [Fact]
    public void TraceRouteRequest_Validation_ThrowsIfBothShapeAndPolyline()
    {
        // Arrange - both Shape and EncodedPolyline provided
        var request = new TraceRouteRequest
        {
            Shape = new[]
            {
                new TracePoint { Lat = 49.6116, Lon = 6.1319 },
                new TracePoint { Lat = 49.6233, Lon = 6.2044 },
            },
            EncodedPolyline = "test_polyline",
            Costing = CostingModel.Auto,
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("*Cannot provide both Shape and EncodedPolyline*")
            .And.ParamName.Should().Be("Shape");
    }

    [Fact]
    public void TraceRouteRequest_Validation_ThrowsIfShapeFewerThanTwoPoints()
    {
        // Arrange - Shape with only one point
        var request = new TraceRouteRequest
        {
            Shape = new[]
            {
                new TracePoint { Lat = 49.6116, Lon = 6.1319 },
            },
            Costing = CostingModel.Auto,
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("*Shape must contain at least 2 trace points*")
            .And.ParamName.Should().Be("Shape");
    }

    [Fact]
    public void TraceRouteRequest_Validation_ThrowsIfMissingCosting()
    {
        // Arrange
        var request = new TraceRouteRequest
        {
            Shape = new[]
            {
                new TracePoint { Lat = 49.6116, Lon = 6.1319 },
                new TracePoint { Lat = 49.6233, Lon = 6.2044 },
            },
            Costing = string.Empty,
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("*Costing is required*")
            .And.ParamName.Should().Be("Costing");
    }

    [Fact]
    public void TraceRouteRequest_Validation_ThrowsIfSearchRadiusExceeds100()
    {
        // Arrange - SearchRadius > 100
        var request = new TraceRouteRequest
        {
            Shape = new[]
            {
                new TracePoint { Lat = 49.6116, Lon = 6.1319 },
                new TracePoint { Lat = 49.6233, Lon = 6.2044 },
            },
            Costing = CostingModel.Auto,
            TraceOptions = new TraceOptions
            {
                SearchRadius = 150,
            },
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*SearchRadius must be between 0 and 100 meters*");
    }

    [Fact]
    public void TraceRouteRequest_Validation_AcceptsValidRequest()
    {
        // Arrange
        var request = new TraceRouteRequest
        {
            Shape = new[]
            {
                new TracePoint { Lat = 49.6116, Lon = 6.1319 },
                new TracePoint { Lat = 49.6233, Lon = 6.2044 },
            },
            Costing = CostingModel.Auto,
            TraceOptions = new TraceOptions
            {
                SearchRadius = 50,
            },
        };

        // Act & Assert
        request.Invoking(r => r.Validate()).Should().NotThrow();
    }
}
