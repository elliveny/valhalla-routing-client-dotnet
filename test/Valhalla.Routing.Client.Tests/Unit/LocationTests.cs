using System.Text.Json;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Tests.Unit;

/// <summary>
/// Unit tests for Location serialization and validation.
/// </summary>
public class LocationTests
{
    [Fact]
    public void Location_SerializesToSnakeCase_AllFields()
    {
        // Arrange
        var location = new Location { Lat = 49.6116, Lon = 6.1319 };
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
        };

        // Act
        var json = JsonSerializer.Serialize(location, options);

        // Assert
        json.Should().Contain("\"lat\":49.6116");
        json.Should().Contain("\"lon\":6.1319");
    }

    [Fact]
    public void Location_Validate_AcceptsValidCoordinates()
    {
        // Arrange
        var location = new Location { Lat = 49.6116, Lon = 6.1319 };

        // Act & Assert
        location.Invoking(l => l.Validate()).Should().NotThrow();
    }

    [Fact]
    public void Location_Validate_ThrowsForLatitudeTooLow()
    {
        // Arrange
        var location = new Location { Lat = -91.0, Lon = 6.1319 };

        // Act & Assert
        location.Invoking(l => l.Validate())
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Latitude must be between -90 and 90 degrees*");
    }

    [Fact]
    public void Location_Validate_ThrowsForLatitudeTooHigh()
    {
        // Arrange
        var location = new Location { Lat = 91.0, Lon = 6.1319 };

        // Act & Assert
        location.Invoking(l => l.Validate())
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Latitude must be between -90 and 90 degrees*");
    }

    [Fact]
    public void Location_Validate_ThrowsForLongitudeTooLow()
    {
        // Arrange
        var location = new Location { Lat = 49.6116, Lon = -181.0 };

        // Act & Assert
        location.Invoking(l => l.Validate())
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Longitude must be between -180 and 180 degrees*");
    }

    [Fact]
    public void Location_Validate_ThrowsForLongitudeTooHigh()
    {
        // Arrange
        var location = new Location { Lat = 49.6116, Lon = 181.0 };

        // Act & Assert
        location.Invoking(l => l.Validate())
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Longitude must be between -180 and 180 degrees*");
    }

    [Fact]
    public void Location_Validate_AcceptsBoundaryValues()
    {
        // Arrange & Act & Assert
        var minLat = new Location { Lat = -90.0, Lon = 0.0 };
        minLat.Invoking(l => l.Validate()).Should().NotThrow();

        var maxLat = new Location { Lat = 90.0, Lon = 0.0 };
        maxLat.Invoking(l => l.Validate()).Should().NotThrow();

        var minLon = new Location { Lat = 0.0, Lon = -180.0 };
        minLon.Invoking(l => l.Validate()).Should().NotThrow();

        var maxLon = new Location { Lat = 0.0, Lon = 180.0 };
        maxLon.Invoking(l => l.Validate()).Should().NotThrow();
    }
}
