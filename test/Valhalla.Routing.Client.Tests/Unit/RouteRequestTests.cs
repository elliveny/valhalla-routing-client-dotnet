using System.Text.Json;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Tests.Unit;

/// <summary>
/// Unit tests for RouteRequest serialization and validation.
/// </summary>
public class RouteRequestTests
{
    [Fact]
    public void RouteRequest_SerializesToSnakeCase_AllFields()
    {
        // Arrange
        var request = new RouteRequest
        {
            Locations = new List<Location>
            {
                new() { Lat = 52.517037, Lon = 13.388860 },
                new() { Lat = 52.529407, Lon = 13.397634 },
            },
            Costing = CostingModel.Auto,
            Units = "kilometers",
            Language = "en-US",
            DirectionsType = "instructions",
            Alternates = 2,
            Id = "test-route",
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
        };

        // Act
        var json = JsonSerializer.Serialize(request, options);

        // Assert
        json.Should().Contain("\"costing\":\"auto\"");
        json.Should().Contain("\"units\":\"kilometers\"");
        json.Should().Contain("\"language\":\"en-US\"");
        json.Should().Contain("\"directions_type\":\"instructions\"");
        json.Should().Contain("\"alternates\":2");
        json.Should().Contain("\"id\":\"test-route\"");
    }

    [Fact]
    public void RouteRequest_Validation_ThrowsIfFewerThanTwoLocations()
    {
        // Arrange - one location
        var request = new RouteRequest
        {
            Locations = new List<Location>
            {
                new() { Lat = 52.517037, Lon = 13.388860 },
            },
            Costing = CostingModel.Auto,
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("*At least 2 locations are required*");
    }

    [Fact]
    public void RouteRequest_Validation_ThrowsIfMissingCosting()
    {
        // Arrange
        var request = new RouteRequest
        {
            Locations = new List<Location>
            {
                new() { Lat = 52.517037, Lon = 13.388860 },
                new() { Lat = 52.529407, Lon = 13.397634 },
            },
            Costing = string.Empty,
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("*Costing is required*");
    }

    [Fact]
    public void RouteRequest_Validation_ThrowsIfInvalidHeading()
    {
        // Arrange - heading > 360
        var request = new RouteRequest
        {
            Locations = new List<Location>
            {
                new() { Lat = 52.517037, Lon = 13.388860, Heading = 400 },
                new() { Lat = 52.529407, Lon = 13.397634 },
            },
            Costing = CostingModel.Auto,
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Heading must be between 0 and 360 degrees*");
    }

    [Fact]
    public void RouteRequest_Validation_ThrowsIfInvalidHeadingTolerance()
    {
        // Arrange - heading tolerance > 180
        var request = new RouteRequest
        {
            Locations = new List<Location>
            {
                new() { Lat = 52.517037, Lon = 13.388860, HeadingTolerance = 200 },
                new() { Lat = 52.529407, Lon = 13.397634 },
            },
            Costing = CostingModel.Auto,
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*HeadingTolerance must be between 0 and 180 degrees*");
    }

    [Fact]
    public void RouteRequest_Validation_ThrowsIfInvalidRadius()
    {
        // Arrange - negative radius
        var request = new RouteRequest
        {
            Locations = new List<Location>
            {
                new() { Lat = 52.517037, Lon = 13.388860, Radius = -10 },
                new() { Lat = 52.529407, Lon = 13.397634 },
            },
            Costing = CostingModel.Auto,
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Radius must be greater than or equal to 0*");
    }

    [Fact]
    public void RouteRequest_Validation_ThrowsIfInvalidDateTimeType()
    {
        // Arrange - invalid date/time type
        var request = new RouteRequest
        {
            Locations = new List<Location>
            {
                new() { Lat = 52.517037, Lon = 13.388860 },
                new() { Lat = 52.529407, Lon = 13.397634 },
            },
            Costing = CostingModel.Auto,
            DateTime = new DateTimeOptions
            {
                Type = (DateTimeType)999,
            },
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("*DateTimeType must be between 0 and 3*");
    }

    [Fact]
    public void RouteRequest_Validation_ThrowsIfDateTimeValueMissingForDepartAt()
    {
        // Arrange - DepartAt without Value
        var request = new RouteRequest
        {
            Locations = new List<Location>
            {
                new() { Lat = 52.517037, Lon = 13.388860 },
                new() { Lat = 52.529407, Lon = 13.397634 },
            },
            Costing = CostingModel.Auto,
            DateTime = new DateTimeOptions
            {
                Type = DateTimeType.DepartAt,
                Value = null,
            },
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("*Value is required when Type is DepartAt*");
    }

    [Fact]
    public void RouteRequest_Validation_ThrowsIfInvalidAlternates()
    {
        // Arrange - negative alternates
        var request = new RouteRequest
        {
            Locations = new List<Location>
            {
                new() { Lat = 52.517037, Lon = 13.388860 },
                new() { Lat = 52.529407, Lon = 13.397634 },
            },
            Costing = CostingModel.Auto,
            Alternates = -1,
        };

        // Act & Assert
        request.Invoking(r => r.Validate())
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Alternates must be greater than or equal to 0*");
    }

    [Fact]
    public void RouteRequest_Validation_AcceptsValidRequest()
    {
        // Arrange
        var request = new RouteRequest
        {
            Locations = new List<Location>
            {
                new() { Lat = 52.517037, Lon = 13.388860 },
                new() { Lat = 52.529407, Lon = 13.397634 },
            },
            Costing = CostingModel.Auto,
        };

        // Act & Assert
        request.Invoking(r => r.Validate()).Should().NotThrow();
    }
}
