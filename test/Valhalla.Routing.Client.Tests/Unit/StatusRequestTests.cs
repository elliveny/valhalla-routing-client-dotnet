using System.Text.Json;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Tests.Unit;

/// <summary>
/// Unit tests for StatusRequest serialization.
/// </summary>
public class StatusRequestTests
{
    [Fact]
    public void StatusRequest_Verbose_SerializesToSnakeCase()
    {
        // Arrange
        var request = new StatusRequest { Verbose = true };
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
        };

        // Act
        var json = JsonSerializer.Serialize(request, options);

        // Assert
        json.Should().Contain("\"verbose\":true");
    }

    [Fact]
    public void StatusRequest_VerboseFalse_SerializesToSnakeCase()
    {
        // Arrange
        var request = new StatusRequest { Verbose = false };
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
        };

        // Act
        var json = JsonSerializer.Serialize(request, options);

        // Assert
        json.Should().Contain("\"verbose\":false");
    }

    [Fact]
    public void StatusRequest_Default_HasVerboseFalse()
    {
        // Arrange & Act
        var request = new StatusRequest();

        // Assert
        request.Verbose.Should().BeFalse();
    }
}
