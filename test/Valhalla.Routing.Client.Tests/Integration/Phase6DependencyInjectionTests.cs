using Microsoft.Extensions.DependencyInjection;
using Valhalla.Routing;
using Valhalla.Routing.Builder;

namespace Valhalla.Routing.Client.Tests.Integration;

/// <summary>
/// Integration tests for Phase 6 - DI and Builder pattern.
/// These tests run against a real Valhalla instance in Docker.
/// </summary>
[Trait("Category", "Integration")]
public class Phase6DependencyInjectionTests
{
    private const string ValhallaBaseUrl = "http://localhost:8002";

    [Fact]
    public async Task ServiceProvider_ResolvesValhallaClient_AndCallsStatus()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddValhallaClient(options =>
        {
            options.BaseUri = new Uri(ValhallaBaseUrl);
            options.Timeout = TimeSpan.FromSeconds(30);
        });

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IValhallaClient>();

        // Act
        var response = await client.StatusAsync();

        // Assert
        response.Should().NotBeNull("Status endpoint should return a response");
        response.Version.Should().NotBeNullOrEmpty("Valhalla version should be present");
        response.Raw.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Object, "Raw JSON should be preserved");
    }

    [Fact]
    public async Task ValhallaClientBuilder_Build_CanCallStatus()
    {
        // Arrange
        using var httpClient = new HttpClient();
#pragma warning disable CA2234 // Pass System.Uri objects instead of strings - intentionally testing string overload
        var client = ValhallaClientBuilder
            .Create()
            .WithBaseUrl(ValhallaBaseUrl)
            .WithTimeout(TimeSpan.FromSeconds(30))
            .WithHttpClient(httpClient)
            .Build();
#pragma warning restore CA2234

        // Act
        var response = await client.StatusAsync();

        // Assert
        response.Should().NotBeNull("Status endpoint should return a response");
        response.Version.Should().NotBeNullOrEmpty("Valhalla version should be present");
        response.Raw.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Object, "Raw JSON should be preserved");
    }

    [Fact]
    public async Task ValhallaClientBuilder_WithUriOverload_CanCallStatus()
    {
        // Arrange
        using var httpClient = new HttpClient();
        var client = ValhallaClientBuilder
            .Create()
            .WithBaseUrl(new Uri(ValhallaBaseUrl))
            .WithTimeout(TimeSpan.FromSeconds(30))
            .WithHttpClient(httpClient)
            .Build();

        // Act
        var response = await client.StatusAsync();

        // Assert
        response.Should().NotBeNull("Status endpoint should return a response");
        response.Version.Should().NotBeNullOrEmpty("Valhalla version should be present");
    }

    [Fact]
    public async Task ValhallaClientBuilder_WithTrailingSlash_NormalizesAndWorks()
    {
        // Arrange - Base URL with trailing slash
        using var httpClient = new HttpClient();
#pragma warning disable CA2234 // Pass System.Uri objects instead of strings - intentionally testing string overload
        var client = ValhallaClientBuilder
            .Create()
            .WithBaseUrl("http://localhost:8002/")
            .WithTimeout(TimeSpan.FromSeconds(30))
            .WithHttpClient(httpClient)
            .Build();
#pragma warning restore CA2234

        // Act
        var response = await client.StatusAsync();

        // Assert
        response.Should().NotBeNull("Status endpoint should return a response even with trailing slash");
        response.Version.Should().NotBeNullOrEmpty("Valhalla version should be present");
    }

    [Fact]
    public async Task ServiceProvider_WithMultipleClients_EachGetsSeparateInstance()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddValhallaClient(options =>
        {
            options.BaseUri = new Uri(ValhallaBaseUrl);
            options.Timeout = TimeSpan.FromSeconds(30);
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var client1 = serviceProvider.GetRequiredService<IValhallaClient>();
        var client2 = serviceProvider.GetRequiredService<IValhallaClient>();

        // Assert
        client1.Should().NotBeSameAs(client2, "Each resolution should create a new instance (transient lifetime)");

        // Both should work
        var response1 = await client1.StatusAsync();
        var response2 = await client2.StatusAsync();

        response1.Should().NotBeNull();
        response2.Should().NotBeNull();
    }
}
