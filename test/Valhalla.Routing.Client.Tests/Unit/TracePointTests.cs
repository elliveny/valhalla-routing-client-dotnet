using System.Text.Json;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Tests.Unit;

/// <summary>
/// Unit tests for TracePoint serialization.
/// </summary>
public class TracePointTests
{
    [Fact]
    public void TracePoint_SerializesToSnakeCase()
    {
        // Arrange
        var tracePoint = new TracePoint
        {
            Lat = 49.6116,
            Lon = 6.1319,
            Type = "via",
            Time = DateTimeOffset.FromUnixTimeSeconds(1672531200),
            Radius = 25.5,
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Converters = { new EpochSecondsConverter() },
        };

        // Act
        var json = JsonSerializer.Serialize(tracePoint, options);

        // Assert
        json.Should().Contain("\"lat\":49.6116");
        json.Should().Contain("\"lon\":6.1319");
        json.Should().Contain("\"type\":\"via\"");
        json.Should().Contain("\"time\":1672531200");
        json.Should().Contain("\"radius\":25.5");
    }

    [Fact]
    public void TracePoint_SerializesMinimalFields()
    {
        // Arrange
        var tracePoint = new TracePoint
        {
            Lat = 49.6116,
            Lon = 6.1319,
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };

        // Act
        var json = JsonSerializer.Serialize(tracePoint, options);

        // Assert
        json.Should().Contain("\"lat\":49.6116");
        json.Should().Contain("\"lon\":6.1319");
        json.Should().NotContain("\"type\"");
        json.Should().NotContain("\"time\"");
        json.Should().NotContain("\"radius\"");
    }

    [Fact]
    public void TracePoint_Validation_ThrowsIfInvalidLatitude()
    {
        // Arrange
        var tracePoint = new TracePoint
        {
            Lat = 91,
            Lon = 6.1319,
        };

        // Act & Assert
        var act = () => tracePoint.Validate();
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Latitude must be between -90 and 90 degrees*");
    }

    [Fact]
    public void TracePoint_Validation_ThrowsIfInvalidLongitude()
    {
        // Arrange
        var tracePoint = new TracePoint
        {
            Lat = 49.6116,
            Lon = 181,
        };

        // Act & Assert
        var act = () => tracePoint.Validate();
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Longitude must be between -180 and 180 degrees*");
    }

    [Fact]
    public void TracePoint_Validation_ThrowsIfRadiusExceeds100()
    {
        // Arrange
        var tracePoint = new TracePoint
        {
            Lat = 49.6116,
            Lon = 6.1319,
            Radius = 150,
        };

        // Act & Assert
        var act = () => tracePoint.Validate();
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Radius must be between 0 and 100 meters*");
    }

    [Fact]
    public void TracePoint_Validation_ThrowsIfRadiusIsNegative()
    {
        // Arrange
        var tracePoint = new TracePoint
        {
            Lat = 49.6116,
            Lon = 6.1319,
            Radius = -10,
        };

        // Act & Assert
        var act = () => tracePoint.Validate();
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Radius must be between 0 and 100 meters*");
    }

    [Fact]
    public void TracePoint_Validation_AcceptsValidRadius()
    {
        // Arrange
        var tracePoint = new TracePoint
        {
            Lat = 49.6116,
            Lon = 6.1319,
            Radius = 50,
        };

        // Act & Assert
        var act = () => tracePoint.Validate();
        act.Should().NotThrow();
    }
}
