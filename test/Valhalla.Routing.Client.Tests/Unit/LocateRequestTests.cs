using System.Text.Json;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Tests.Unit;

/// <summary>
/// Unit tests for LocateRequest serialization and validation.
/// </summary>
public class LocateRequestTests
{
    [Fact]
    public void LocateRequest_SerializesToSnakeCase()
    {
        // Arrange
        var request = new LocateRequest
        {
            Locations = new[]
            {
                new Location { Lat = 49.6116, Lon = 6.1319 },
            },
            Costing = CostingModel.Auto,
            Verbose = true,
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
        };

        // Act
        var json = JsonSerializer.Serialize(request, options);

        // Assert
        json.Should().Contain("\"locations\":");
        json.Should().Contain("\"costing\":\"auto\"");
        json.Should().Contain("\"verbose\":true");
        json.Should().Contain("\"lat\":49.6116");
        json.Should().Contain("\"lon\":6.1319");
    }

    [Fact]
    public void LocateRequest_Validation_ThrowsIfNoLocations()
    {
        // Arrange
        var request = new LocateRequest
        {
            Locations = Array.Empty<Location>(),
            Costing = CostingModel.Auto,
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("At least one location is required.*")
            .And.ParamName.Should().Be("Locations");
    }

    [Fact]
    public void LocateRequest_Validation_ThrowsIfLocationsNull()
    {
        // Arrange - using null suppression for testing invalid state
        var request = new LocateRequest
        {
            Locations = null!,
            Costing = CostingModel.Auto,
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("At least one location is required.*")
            .And.ParamName.Should().Be("Locations");
    }

    [Fact]
    public void LocateRequest_Validation_ThrowsIfInvalidLatitude()
    {
        // Arrange
        var request = new LocateRequest
        {
            Locations = new[]
            {
                new Location { Lat = 95.0, Lon = 6.1319 },
            },
            Costing = CostingModel.Auto,
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Latitude must be between -90 and 90 degrees*");
    }

    [Fact]
    public void LocateRequest_Validation_ThrowsIfInvalidLongitude()
    {
        // Arrange
        var request = new LocateRequest
        {
            Locations = new[]
            {
                new Location { Lat = 49.6116, Lon = 190.0 },
            },
            Costing = CostingModel.Auto,
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Longitude must be between -180 and 180 degrees*");
    }

    [Fact]
    public void LocateRequest_Validation_ThrowsIfMissingCosting()
    {
        // Arrange
        var request = new LocateRequest
        {
            Locations = new[]
            {
                new Location { Lat = 49.6116, Lon = 6.1319 },
            },
            Costing = string.Empty,
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("Costing model is required.*")
            .And.ParamName.Should().Be("Costing");
    }

    [Fact]
    public void LocateRequest_Validation_ThrowsIfCostingWhitespace()
    {
        // Arrange
        var request = new LocateRequest
        {
            Locations = new[]
            {
                new Location { Lat = 49.6116, Lon = 6.1319 },
            },
            Costing = "   ",
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("Costing model is required.*")
            .And.ParamName.Should().Be("Costing");
    }

    [Fact]
    public void LocateRequest_Validation_AcceptsValidRequest()
    {
        // Arrange
        var request = new LocateRequest
        {
            Locations = new[]
            {
                new Location { Lat = 49.6116, Lon = 6.1319 },
                new Location { Lat = 49.6233, Lon = 6.2044 },
            },
            Costing = CostingModel.Bicycle,
        };

        // Act & Assert
        request.Invoking(r => r.Validate()).Should().NotThrow();
    }
}
