namespace Valhalla.Routing.Client.Tests.Integration;

/// <summary>
/// Integration tests for Phase 0 that verify the test infrastructure is set up correctly.
/// These tests validate Docker connectivity and Valhalla container responsiveness.
/// </summary>
[Trait("Category", "Integration")]
public class Phase0InfrastructureTests
{
    private const string ValhallaBaseUrl = "http://localhost:8002";

    /// <summary>
    /// Verifies that the Valhalla container starts successfully and responds to HTTP status requests.
    /// This test confirms the Docker infrastructure is properly configured.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Valhalla_Container_RespondsToStatusRequest()
    {
        // Arrange
        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri(ValhallaBaseUrl),
            Timeout = TimeSpan.FromSeconds(30),
        };

        // Act
        var response = await httpClient.GetAsync(new Uri("/status", UriKind.Relative));

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue(
            "Valhalla container should respond successfully to /status endpoint");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrWhiteSpace("Status response should contain data");

        // Basic validation that this looks like a Valhalla response
        content.Should().Contain("version", "Status response should include version information");
    }
}
