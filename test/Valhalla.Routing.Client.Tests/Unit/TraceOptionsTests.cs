using System.Text.Json;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Tests.Unit;

/// <summary>
/// Unit tests for TraceOptions serialization.
/// </summary>
public class TraceOptionsTests
{
    [Fact]
    public void TraceOptions_SerializesToSnakeCase()
    {
        // Arrange
        var traceOptions = new TraceOptions
        {
            SearchRadius = 50,
            GpsAccuracy = 10,
            BreakageDistance = 2500,
            InterpolationDistance = 50,
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };

        // Act
        var json = JsonSerializer.Serialize(traceOptions, options);

        // Assert
        json.Should().Contain("\"search_radius\":50");
        json.Should().Contain("\"gps_accuracy\":10");
        json.Should().Contain("\"breakage_distance\":2500");
        json.Should().Contain("\"interpolation_distance\":50");
    }

    [Fact]
    public void TraceOptions_SerializesPartialFields()
    {
        // Arrange
        var traceOptions = new TraceOptions
        {
            SearchRadius = 40,
            GpsAccuracy = 5,
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };

        // Act
        var json = JsonSerializer.Serialize(traceOptions, options);

        // Assert
        json.Should().Contain("\"search_radius\":40");
        json.Should().Contain("\"gps_accuracy\":5");
        json.Should().NotContain("\"breakage_distance\"");
        json.Should().NotContain("\"interpolation_distance\"");
    }
}
