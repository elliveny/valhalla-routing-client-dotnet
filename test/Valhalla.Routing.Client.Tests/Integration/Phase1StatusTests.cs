using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Tests.Integration;

/// <summary>
/// Integration tests for Phase 1 - Status endpoint.
/// These tests run against a real Valhalla instance in Docker.
/// </summary>
[Trait("Category", "Integration")]
public class Phase1StatusTests
{
    private const string ValhallaBaseUrl = "http://localhost:8002";

    [Fact]
    public async Task StatusAsync_ReturnsVersionAndTileInfo()
    {
        // Arrange
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30),
        };

        var options = Options.Create(new ValhallaClientOptions
        {
            BaseUri = new Uri(ValhallaBaseUrl),
            Timeout = TimeSpan.FromSeconds(30),
        });

        var logger = NullLogger<ValhallaClient>.Instance;
        var client = new ValhallaClient(httpClient, options, logger);

        // Act
        var response = await client.StatusAsync();

        // Assert
        response.Should().NotBeNull("Status endpoint should return a response");
        response.Version.Should().NotBeNullOrEmpty("Valhalla version should be present");
        response.Raw.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Object, "Raw JSON should be preserved");

        // Verify that Raw contains the version field
        response.Raw.TryGetProperty("version", out var versionElement).Should().BeTrue();
        versionElement.GetString().Should().Be(response.Version);
    }

    [Fact]
    public async Task StatusAsync_Verbose_ReturnsExtendedInfo()
    {
        // Arrange
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30),
        };

        var options = Options.Create(new ValhallaClientOptions
        {
            BaseUri = new Uri(ValhallaBaseUrl),
            Timeout = TimeSpan.FromSeconds(30),
        });

        var logger = NullLogger<ValhallaClient>.Instance;
        var client = new ValhallaClient(httpClient, options, logger);
        var request = new StatusRequest { Verbose = true };

        // Act
        var response = await client.StatusAsync(request);

        // Assert
        response.Should().NotBeNull("Status endpoint should return a response");
        response.Version.Should().NotBeNullOrEmpty("Valhalla version should be present");

        // Verbose mode should include additional information
        // Note: The exact fields depend on Valhalla configuration, but version is always present
        response.Raw.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Object);

        // Check that tileset information is present (if tiles are loaded)
        if (response.HasTiles == true)
        {
            response.TilesetLastModified.Should().NotBeNull("Tileset last modified should be present when tiles are loaded");
        }
    }

    [Fact]
    public async Task StatusAsync_WithNullRequest_UsesDefaults()
    {
        // Arrange
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30),
        };

        var options = Options.Create(new ValhallaClientOptions
        {
            BaseUri = new Uri(ValhallaBaseUrl),
            Timeout = TimeSpan.FromSeconds(30),
        });

        var logger = NullLogger<ValhallaClient>.Instance;
        var client = new ValhallaClient(httpClient, options, logger);

        // Act
        var response = await client.StatusAsync(null);

        // Assert
        response.Should().NotBeNull("Status endpoint should return a response");
        response.Version.Should().NotBeNullOrEmpty("Valhalla version should be present");
    }

    [Fact]
    public async Task StatusAsync_WithCancellationToken_Succeeds()
    {
        // Arrange
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30),
        };

        var options = Options.Create(new ValhallaClientOptions
        {
            BaseUri = new Uri(ValhallaBaseUrl),
            Timeout = TimeSpan.FromSeconds(30),
        });

        var logger = NullLogger<ValhallaClient>.Instance;
        var client = new ValhallaClient(httpClient, options, logger);
        using var cts = new CancellationTokenSource();

        // Act
        var response = await client.StatusAsync(cancellationToken: cts.Token);

        // Assert
        response.Should().NotBeNull("Status endpoint should return a response");
        response.Version.Should().NotBeNullOrEmpty("Valhalla version should be present");
    }
}
