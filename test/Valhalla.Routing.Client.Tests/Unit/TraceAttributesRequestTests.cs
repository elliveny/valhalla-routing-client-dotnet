using System.Text.Json;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Tests.Unit;

/// <summary>
/// Unit tests for TraceAttributesRequest serialization and validation.
/// </summary>
public class TraceAttributesRequestTests
{
    [Fact]
    public void TraceAttributesRequest_SerializesToSnakeCase()
    {
        // Arrange
        var request = new TraceAttributesRequest
        {
            Shape = new[]
            {
                new TracePoint { Lat = 49.6116, Lon = 6.1319 },
                new TracePoint { Lat = 49.6233, Lon = 6.2044 },
            },
            Costing = CostingModel.Auto,
            ShapeMatch = "map_snap",
            UseTimestamps = true,
            Id = "test-trace-attributes",
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
        json.Should().Contain("\"id\":\"test-trace-attributes\"");
    }

    [Fact]
    public void TraceAttributesRequest_Validation_FilterActionMustBeIncludeOrExclude()
    {
        // Arrange - invalid filter action
        var request = new TraceAttributesRequest
        {
            Shape = new[]
            {
                new TracePoint { Lat = 49.6116, Lon = 6.1319 },
                new TracePoint { Lat = 49.6233, Lon = 6.2044 },
            },
            Costing = CostingModel.Auto,
            Filters = new FilterAttributes
            {
                Action = "invalid_action",
                Attributes = new[] { "edge.id", "edge.length" },
            },
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("*FilterAttributes.Action must be 'include' or 'exclude'*")
            .And.ParamName.Should().Be("Action");
    }

    [Fact]
    public void TraceAttributesRequest_Validation_AcceptsIncludeAction()
    {
        // Arrange
        var request = new TraceAttributesRequest
        {
            Shape = new[]
            {
                new TracePoint { Lat = 49.6116, Lon = 6.1319 },
                new TracePoint { Lat = 49.6233, Lon = 6.2044 },
            },
            Costing = CostingModel.Auto,
            Filters = new FilterAttributes
            {
                Action = "include",
                Attributes = new[] { "edge.id" },
            },
        };

        // Act & Assert
        request.Invoking(r => r.Validate()).Should().NotThrow();
    }

    [Fact]
    public void TraceAttributesRequest_Validation_AcceptsExcludeAction()
    {
        // Arrange
        var request = new TraceAttributesRequest
        {
            Shape = new[]
            {
                new TracePoint { Lat = 49.6116, Lon = 6.1319 },
                new TracePoint { Lat = 49.6233, Lon = 6.2044 },
            },
            Costing = CostingModel.Auto,
            Filters = new FilterAttributes
            {
                Action = "exclude",
                Attributes = new[] { "edge.id" },
            },
        };

        // Act & Assert
        request.Invoking(r => r.Validate()).Should().NotThrow();
    }

    [Fact]
    public void TraceAttributesRequest_Validation_AcceptsValidRequestWithoutFilters()
    {
        // Arrange
        var request = new TraceAttributesRequest
        {
            Shape = new[]
            {
                new TracePoint { Lat = 49.6116, Lon = 6.1319 },
                new TracePoint { Lat = 49.6233, Lon = 6.2044 },
            },
            Costing = CostingModel.Auto,
        };

        // Act & Assert
        request.Invoking(r => r.Validate()).Should().NotThrow();
    }
}
