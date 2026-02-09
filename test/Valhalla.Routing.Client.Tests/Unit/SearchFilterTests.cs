using System.Text.Json;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Tests.Unit;

/// <summary>
/// Unit tests for SearchFilter serialization.
/// </summary>
public class SearchFilterTests
{
    [Fact]
    public void SearchFilter_SerializesToSnakeCase()
    {
        // Arrange
        var filter = new SearchFilter
        {
            ExcludeTunnel = true,
            ExcludeBridge = false,
            ExcludeToll = true,
            MinRoadClass = "secondary",
            MaxRoadClass = "motorway",
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
        };

        // Act
        var json = JsonSerializer.Serialize(filter, options);

        // Assert
        json.Should().Contain("\"exclude_tunnel\":true");
        json.Should().Contain("\"exclude_bridge\":false");
        json.Should().Contain("\"exclude_toll\":true");
        json.Should().Contain("\"min_road_class\":\"secondary\"");
        json.Should().Contain("\"max_road_class\":\"motorway\"");
    }
}
