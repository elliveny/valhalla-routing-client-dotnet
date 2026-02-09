using System.Text.Json;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Tests.Unit;

/// <summary>
/// Unit tests for DateTimeOptions serialization and validation.
/// </summary>
public class DateTimeOptionsTests
{
    [Fact]
    public void DateTimeOptions_SerializesToSnakeCase()
    {
        // Arrange
        var options = new DateTimeOptions
        {
            Type = DateTimeType.DepartAt,
            Value = "2026-02-08T14:30",
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
        };

        // Act
        var json = JsonSerializer.Serialize(options, jsonOptions);

        // Assert
        json.Should().Contain("\"type\":1");
        json.Should().Contain("\"value\":\"2026-02-08T14:30\"");
    }

    [Fact]
    public void DateTimeOptions_Validation_AcceptsCurrentType()
    {
        // Arrange
        var options = new DateTimeOptions
        {
            Type = DateTimeType.Current,
        };

        // Act & Assert
        options.Invoking(o => o.Validate()).Should().NotThrow();
    }

    [Fact]
    public void DateTimeOptions_Validation_AcceptsInvariantType()
    {
        // Arrange
        var options = new DateTimeOptions
        {
            Type = DateTimeType.Invariant,
        };

        // Act & Assert
        options.Invoking(o => o.Validate()).Should().NotThrow();
    }

    [Fact]
    public void DateTimeOptions_Validation_RequiresValueForDepartAt()
    {
        // Arrange
        var options = new DateTimeOptions
        {
            Type = DateTimeType.DepartAt,
            Value = null,
        };

        // Act & Assert
        options.Invoking(o => o.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("*Value is required when Type is DepartAt*");
    }

    [Fact]
    public void DateTimeOptions_Validation_RequiresValueForArriveBy()
    {
        // Arrange
        var options = new DateTimeOptions
        {
            Type = DateTimeType.ArriveBy,
            Value = null,
        };

        // Act & Assert
        options.Invoking(o => o.Validate())
            .Should().Throw<ArgumentException>()
            .WithMessage("*Value is required when Type is ArriveBy*");
    }
}
