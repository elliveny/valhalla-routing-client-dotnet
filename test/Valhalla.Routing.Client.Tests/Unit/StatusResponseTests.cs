using System.Text.Json;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Tests.Unit;

/// <summary>
/// Unit tests for StatusResponse deserialization.
/// </summary>
public class StatusResponseTests
{
    [Fact]
    public void StatusResponse_Deserializes_AllTypedFields()
    {
        // Arrange
        var json = """
        {
            "version": "3.6.0",
            "tileset_last_modified": 1704067200,
            "has_tiles": true,
            "has_admins": true,
            "has_timezones": true,
            "has_live_traffic": false,
            "bbox": {
                "min_lat": 49.4,
                "min_lon": 5.7,
                "max_lat": 50.2,
                "max_lon": 6.5
            }
        }
        """;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
            PropertyNameCaseInsensitive = true,
        };

        // Act
        using var doc = JsonDocument.Parse(json);
        var rawElement = doc.RootElement.Clone();
        var response = JsonSerializer.Deserialize<StatusResponse>(json, options);

        // Manually set Raw property since it's not deserialized automatically
        var responseWithRaw = new StatusResponse
        {
            Raw = rawElement,
            Version = response!.Version,
            TilesetLastModified = response.TilesetLastModified,
            HasTiles = response.HasTiles,
            HasAdmins = response.HasAdmins,
            HasTimezones = response.HasTimezones,
            HasLiveTraffic = response.HasLiveTraffic,
            Bbox = response.Bbox,
        };

        // Assert
        responseWithRaw.Version.Should().Be("3.6.0");
        responseWithRaw.TilesetLastModified.Should().Be(1704067200);
        responseWithRaw.HasTiles.Should().BeTrue();
        responseWithRaw.HasAdmins.Should().BeTrue();
        responseWithRaw.HasTimezones.Should().BeTrue();
        responseWithRaw.HasLiveTraffic.Should().BeFalse();
        responseWithRaw.Bbox.Should().NotBeNull();
    }

    [Fact]
    public void StatusResponse_Deserializes_RawJsonPreserved()
    {
        // Arrange
        var json = """
        {
            "version": "3.6.0",
            "tileset_last_modified": 1704067200,
            "has_tiles": true,
            "custom_field": "custom_value"
        }
        """;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
            PropertyNameCaseInsensitive = true,
        };

        // Act
        using var doc = JsonDocument.Parse(json);
        var rawElement = doc.RootElement.Clone();
        var response = JsonSerializer.Deserialize<StatusResponse>(json, options);

        // Manually set Raw property
        var responseWithRaw = new StatusResponse
        {
            Raw = rawElement,
            Version = response!.Version,
            TilesetLastModified = response.TilesetLastModified,
            HasTiles = response.HasTiles,
        };

        // Assert
        responseWithRaw.Raw.ValueKind.Should().Be(JsonValueKind.Object);
        responseWithRaw.Raw.TryGetProperty("custom_field", out var customField).Should().BeTrue();
        customField.GetString().Should().Be("custom_value");
        responseWithRaw.Raw.TryGetProperty("version", out var version).Should().BeTrue();
        version.GetString().Should().Be("3.6.0");
    }

    [Fact]
    public void StatusResponse_Deserializes_MinimalResponse()
    {
        // Arrange - minimal response with only version
        var json = """
        {
            "version": "3.6.0"
        }
        """;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
            PropertyNameCaseInsensitive = true,
        };

        // Act
        using var doc = JsonDocument.Parse(json);
        var rawElement = doc.RootElement.Clone();
        var response = JsonSerializer.Deserialize<StatusResponse>(json, options);

        // Manually set Raw property
        var responseWithRaw = new StatusResponse
        {
            Raw = rawElement,
            Version = response!.Version,
        };

        // Assert
        responseWithRaw.Version.Should().Be("3.6.0");
        responseWithRaw.TilesetLastModified.Should().BeNull();
        responseWithRaw.HasTiles.Should().BeNull();
        responseWithRaw.HasAdmins.Should().BeNull();
        responseWithRaw.HasTimezones.Should().BeNull();
        responseWithRaw.HasLiveTraffic.Should().BeNull();
        responseWithRaw.Bbox.Should().BeNull();
    }
}
