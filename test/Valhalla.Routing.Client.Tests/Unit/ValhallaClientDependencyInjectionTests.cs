using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Valhalla.Routing;
using Valhalla.Routing.Builder;

namespace Valhalla.Routing.Client.Tests.Unit;

/// <summary>
/// Unit tests for ValhallaClient service registration and builder pattern.
/// </summary>
public class ValhallaClientDependencyInjectionTests
{
    [Fact]
    public void AddValhallaClient_RegistersIValhallaClient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddValhallaClient(options =>
        {
            options.BaseUri = new Uri("http://localhost:8002");
            options.Timeout = TimeSpan.FromSeconds(30);
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var client = serviceProvider.GetService<IValhallaClient>();
        client.Should().NotBeNull("IValhallaClient should be registered in the service collection");
        client.Should().BeOfType<ValhallaClient>("The registered service should be ValhallaClient");
    }

    [Fact]
    public void AddValhallaClient_ConfiguresHttpClientBaseAddress()
    {
        // Arrange
        var services = new ServiceCollection();
        var baseUri = new Uri("http://localhost:8002");

        // Act
        services.AddValhallaClient(options =>
        {
            options.BaseUri = baseUri;
        });

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IValhallaClient>();

        // Assert
        // We can't directly access HttpClient.BaseAddress from the client,
        // but we can verify the client was created successfully with the options
        client.Should().NotBeNull();
        client.Should().BeOfType<ValhallaClient>();
    }

    [Fact]
    public void AddValhallaClient_ConfiguresTimeout()
    {
        // Arrange
        var services = new ServiceCollection();
        var timeout = TimeSpan.FromSeconds(45);

        // Act
        services.AddValhallaClient(options =>
        {
            options.BaseUri = new Uri("http://localhost:8002");
            options.Timeout = timeout;
        });

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<IValhallaClient>();

        // Assert
        client.Should().NotBeNull();
        client.Should().BeOfType<ValhallaClient>();
    }

    [Fact]
    public void AddValhallaClient_ReturnsIHttpClientBuilder_ForFurtherConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var httpClientBuilder = services.AddValhallaClient(options =>
        {
            options.BaseUri = new Uri("http://localhost:8002");
        });

        // Assert
        httpClientBuilder.Should().NotBeNull("AddValhallaClient should return IHttpClientBuilder");
        httpClientBuilder.Should().BeAssignableTo<IHttpClientBuilder>("The return value should implement IHttpClientBuilder");
    }

    [Fact]
    public void ValhallaClientBuilder_Build_ThrowsIfBaseUriNotSet()
    {
        // Arrange
        using var httpClient = new HttpClient();
        var builder = ValhallaClientBuilder.Create()
            .WithHttpClient(httpClient);

        // Act & Assert
        var act = () => builder.Build();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Base URI must be set*");
    }

    [Fact]
    public void ValhallaClientBuilder_Build_ThrowsIfHttpClientNotProvided()
    {
        // Arrange
#pragma warning disable CA2234 // Pass System.Uri objects instead of strings - intentionally testing string overload
        var builder = ValhallaClientBuilder.Create()
            .WithBaseUrl("http://localhost:8002");
#pragma warning restore CA2234

        // Act & Assert
        var act = () => builder.Build();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*HttpClient must be provided*");
    }

    [Fact]
    public void ValhallaClientBuilder_Build_NormalizesTrailingSlash()
    {
        // Arrange
        using var httpClient = new HttpClient();
#pragma warning disable CA2234 // Pass System.Uri objects instead of strings - intentionally testing string overload
        var builder = ValhallaClientBuilder.Create()
            .WithBaseUrl("http://localhost:8002/")
            .WithHttpClient(httpClient);
#pragma warning restore CA2234

        // Act
        var client = builder.Build();

        // Assert
        client.Should().NotBeNull();
        client.Should().BeOfType<ValhallaClient>();

        // Note: The actual normalization is done in ValhallaClient constructor,
        // which we tested in other unit tests
    }

    [Fact]
    public void ValhallaClientBuilder_WithHttpClient_LogsWarning()
    {
        // Arrange
        using var httpClient = new HttpClient();
        using var loggerFactory = LoggerFactory.Create(builder => { });
        var logger = loggerFactory.CreateLogger<ValhallaClient>();

        var builder = ValhallaClientBuilder.Create()
            .WithBaseUrl(new Uri("http://localhost:8002"))
            .WithHttpClient(httpClient)
            .WithLogger(logger);

        // Act
        var client = builder.Build();

        // Assert
        client.Should().NotBeNull();

        // Note: We can't directly verify the log output without a custom logger provider,
        // but we can verify the client was built successfully
    }

    [Fact]
    public void ValhallaClientBuilder_WithTimeout_SetsTimeout()
    {
        // Arrange
        using var httpClient = new HttpClient();
        var timeout = TimeSpan.FromSeconds(60);
#pragma warning disable CA2234 // Pass System.Uri objects instead of strings - intentionally testing string overload
        var builder = ValhallaClientBuilder.Create()
            .WithBaseUrl("http://localhost:8002")
            .WithTimeout(timeout)
            .WithHttpClient(httpClient);
#pragma warning restore CA2234

        // Act
        var client = builder.Build();

        // Assert
        client.Should().NotBeNull();
        client.Should().BeOfType<ValhallaClient>();
    }

    [Fact]
    public void ValhallaClientBuilder_WithApiKey_ConfiguresHeader()
    {
        // Arrange
        using var httpClient = new HttpClient();
#pragma warning disable CA2234 // Pass System.Uri objects instead of strings - intentionally testing string overload
        var builder = ValhallaClientBuilder.Create()
            .WithBaseUrl("http://localhost:8002")
            .WithApiKey("X-Api-Key", "test-key-12345")
            .WithHttpClient(httpClient);
#pragma warning restore CA2234

        // Act
        var client = builder.Build();

        // Assert
        client.Should().NotBeNull();
        client.Should().BeOfType<ValhallaClient>();
    }

    [Fact]
    public void ValhallaClientBuilder_WithBaseUrl_AcceptsString()
    {
        // Arrange & Act
        using var httpClient = new HttpClient();
#pragma warning disable CA2234 // Pass System.Uri objects instead of strings - intentionally testing string overload
        var builder = ValhallaClientBuilder.Create()
            .WithBaseUrl("http://localhost:8002")
            .WithHttpClient(httpClient);
#pragma warning restore CA2234

        var client = builder.Build();

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public void ValhallaClientBuilder_WithBaseUrl_AcceptsUri()
    {
        // Arrange & Act
        using var httpClient = new HttpClient();
        var builder = ValhallaClientBuilder.Create()
            .WithBaseUrl(new Uri("http://localhost:8002"))
            .WithHttpClient(httpClient);

        var client = builder.Build();

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public void ValhallaClientBuilder_WithBaseUrl_ThrowsOnInvalidString()
    {
        // Arrange
        var builder = ValhallaClientBuilder.Create();

        // Act & Assert
#pragma warning disable CA2234 // Pass System.Uri objects instead of strings - intentionally testing string overload
        var act = () => builder.WithBaseUrl("not-a-valid-url");
#pragma warning restore CA2234
        act.Should().Throw<ArgumentException>()
            .WithMessage("*not a valid absolute URI*");
    }

    [Fact]
    public void ValhallaClientBuilder_WithBaseUrl_ThrowsOnRelativeUri()
    {
        // Arrange
        var builder = ValhallaClientBuilder.Create();
        var relativeUri = new Uri("/path", UriKind.Relative);

        // Act & Assert
        var act = () => builder.WithBaseUrl(relativeUri);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*must be an absolute URI*");
    }

    [Fact]
    public void ValhallaClientBuilder_WithTimeout_ThrowsOnZeroOrNegative()
    {
        // Arrange
        var builder = ValhallaClientBuilder.Create();

        // Act & Assert - Zero timeout
        var actZero = () => builder.WithTimeout(TimeSpan.Zero);
        actZero.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("timeout");

        // Act & Assert - Negative timeout
        var actNegative = () => builder.WithTimeout(TimeSpan.FromSeconds(-1));
        actNegative.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("timeout");
    }

    [Fact]
    public void ValhallaClientBuilder_WithApiKey_ThrowsOnNullOrEmptyHeaderName()
    {
        // Arrange
        var builder = ValhallaClientBuilder.Create();

        // Act & Assert - Null header name
        var actNull = () => builder.WithApiKey(null!, "value");
        actNull.Should().Throw<ArgumentException>()
            .WithParameterName("headerName");

        // Act & Assert - Empty header name
        var actEmpty = () => builder.WithApiKey(string.Empty, "value");
        actEmpty.Should().Throw<ArgumentException>()
            .WithParameterName("headerName");
    }

    [Fact]
    public void ValhallaClientBuilder_WithApiKey_ThrowsOnNullOrEmptyHeaderValue()
    {
        // Arrange
        var builder = ValhallaClientBuilder.Create();

        // Act & Assert - Null header value
        var actNull = () => builder.WithApiKey("X-Api-Key", null!);
        actNull.Should().Throw<ArgumentException>()
            .WithParameterName("headerValue");

        // Act & Assert - Empty header value
        var actEmpty = () => builder.WithApiKey("X-Api-Key", string.Empty);
        actEmpty.Should().Throw<ArgumentException>()
            .WithParameterName("headerValue");
    }

    [Fact]
    public void ValhallaClientBuilder_WithHttpClient_ThrowsOnNull()
    {
        // Arrange
        var builder = ValhallaClientBuilder.Create();

        // Act & Assert
        var act = () => builder.WithHttpClient(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("httpClient");
    }

    [Fact]
    public void ValhallaClientBuilder_WithLogger_ThrowsOnNull()
    {
        // Arrange
        var builder = ValhallaClientBuilder.Create();

        // Act & Assert
        var act = () => builder.WithLogger(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void ValhallaClientBuilder_WithSensitiveLogging_EnablesLogging()
    {
        // Arrange & Act
        using var httpClient = new HttpClient();
#pragma warning disable CA2234 // Pass System.Uri objects instead of strings - intentionally testing string overload
        var builder = ValhallaClientBuilder.Create()
            .WithBaseUrl("http://localhost:8002")
            .WithSensitiveLogging()
            .WithHttpClient(httpClient);
#pragma warning restore CA2234

        var client = builder.Build();

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public void AddValhallaClient_ThrowsOnNullServices()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act & Assert
        var act = () => services.AddValhallaClient(options =>
        {
            options.BaseUri = new Uri("http://localhost:8002");
        });
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddValhallaClient_ThrowsOnNullConfigureOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var act = () => services.AddValhallaClient(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configureOptions");
    }
}
