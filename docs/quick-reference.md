# .NET Best Practices Quick Reference

A quick reference guide for common patterns and practices in the Valhalla .NET Routing Client project.

## File Headers

No file headers required. StyleCop's file header requirement is disabled.

## Namespace Declaration

Use file-scoped namespaces (.NET 6+):

```csharp
namespace Valhalla.Routing.Client.Models;

public class Location
{
    // ...
}
```

## Interface Documentation

✅ **Complete interface documentation:**

```csharp
/// <summary>
/// Calculates a route between two or more locations.
/// </summary>
/// <param name="request">The route request. Must contain at least 2 locations.</param>
/// <param name="cancellationToken">A token to cancel the operation.</param>
/// <returns>
/// A task that represents the asynchronous operation. The task result contains
/// the route response with directions and distance estimates.
/// </returns>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="request"/> is <c>null</c>.
/// </exception>
/// <exception cref="ArgumentException">
/// Thrown when the request contains invalid data.
/// </exception>
/// <exception cref="ValhallaException">
/// Thrown when the API returns an error response.
/// </exception>
/// <exception cref="OperationCanceledException">
/// Thrown when cancelled via <paramref name="cancellationToken"/>.
/// </exception>
Task<RouteResponse> RouteAsync(
    RouteRequest request,
    CancellationToken cancellationToken = default);
```

## Implementation Documentation

✅ **Use inheritdoc:**

```csharp
/// <inheritdoc/>
public async Task<RouteResponse> RouteAsync(
    RouteRequest request,
    CancellationToken cancellationToken = default)
{
    // Implementation
}
```

## Property Documentation

✅ **Document properties with value descriptions:**

```csharp
/// <summary>
/// Gets or sets the latitude in decimal degrees.
/// </summary>
/// <value>A value between -90 and 90.</value>
[JsonPropertyName("lat")]
public double Lat { get; set; }
```

## Input Validation

✅ **Always validate inputs:**

```csharp
public async Task<RouteResponse> RouteAsync(
    RouteRequest request,
    CancellationToken cancellationToken = default)
{
    // 1. Null check
    if (request == null)
    {
        throw new ArgumentNullException(nameof(request));
    }

    // Or use modern syntax (C# 11+)
    ArgumentNullException.ThrowIfNull(request);

    // 2. Business rule validation
    if (request.Locations == null || request.Locations.Count < 2)
    {
        throw new ArgumentException(
            "Request must contain at least 2 locations.",
            nameof(request));
    }

    // 3. Implementation
}
```

## Async/Await

✅ **Use ConfigureAwait(false) in library code:**

```csharp
var response = await _httpClient
    .SendAsync(request, cancellationToken)
    .ConfigureAwait(false);
```

✅ **Pass cancellation tokens:**

```csharp
public async Task<T> MethodAsync(CancellationToken cancellationToken = default)
{
    return await SomeOperationAsync(cancellationToken)
        .ConfigureAwait(false);
}
```

## Exception Handling

✅ **Distinguish timeout from cancellation:**

```csharp
try
{
    await _httpClient.SendAsync(request, cancellationToken)
        .ConfigureAwait(false);
}
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "HTTP request failed");
    throw new ValhallaException("Failed to communicate with API", ex);
}
catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
{
    _logger.LogInformation("Request was cancelled");
    throw; // User cancellation - rethrow as-is
}
catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
{
    _logger.LogError(ex, "Request timed out");
    throw new TimeoutException("Request timed out", ex); // Timeout - wrap
}
```

## Dependency Injection

✅ **Constructor injection pattern:**

```csharp
public sealed class ValhallaClient : IValhallaClient
{
    private readonly HttpClient _httpClient;
    private readonly ValhallaClientOptions _options;
    private readonly ILogger<ValhallaClient> _logger;

    public ValhallaClient(
        HttpClient httpClient,
        IOptions<ValhallaClientOptions> options,
        ILogger<ValhallaClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

✅ **Service registration extension:**

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddValhallaClient(
        this IServiceCollection services,
        Action<ValhallaClientOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        services.AddHttpClient<IValhallaClient, ValhallaClient>();

        return services;
    }
}
```

## Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Namespace | PascalCase | `Valhalla.Routing.Client` |
| Class | PascalCase | `ValhallaClient` |
| Interface | I + PascalCase | `IValhallaClient` |
| Method | PascalCase | `RouteAsync` |
| Property | PascalCase | `BaseUri` |
| Private field | _camelCase | `_httpClient` |
| Parameter | camelCase | `cancellationToken` |
| Local variable | camelCase | `response` |
| Constant | PascalCase | `MaxLocations` |
| Async method | PascalCase + Async | `RouteAsync` |

## JSON Serialization

✅ **Use snake_case for Valhalla API:**

```csharp
public class Location
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }

    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }
}
```

## Testing

✅ **Descriptive test names:**

```csharp
[Fact]
public async Task RouteAsync_WithNullRequest_ThrowsArgumentNullException()
{
    // Arrange
    var client = CreateClient();

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentNullException>(
        () => client.RouteAsync(null!));
}

[Fact]
public async Task RouteAsync_WithValidRequest_ReturnsSuccessfulResponse()
{
    // Arrange
    var client = CreateClient();
    var request = CreateValidRequest();

    // Act
    var response = await client.RouteAsync(request);

    // Assert
    Assert.NotNull(response);
    Assert.NotNull(response.Trip);
}
```

## Code Organization

Order members in this sequence:

1. Constants
2. Private fields
3. Constructors
4. Public properties
5. Public methods
6. Private methods
7. Nested types

## Common Mistakes to Avoid

❌ **Don't duplicate interface documentation**
```csharp
// Bad - duplicates interface docs
public sealed class ValhallaClient : IValhallaClient
{
    /// <summary>
    /// Calculates a route...
    /// </summary>
    public async Task<RouteResponse> RouteAsync(...) { }
}
```

❌ **Don't swallow exceptions**
```csharp
// Bad
try { } catch { }
```

❌ **Don't mix sync and async**
```csharp
// Bad - can cause deadlocks
public RouteResponse GetRoute(RouteRequest request)
{
    return RouteAsync(request).Result;
}
```

❌ **Don't use var when type is not obvious**
```csharp
// Bad
var x = GetValue();

// Good
var client = new ValhallaClient();
RouteResponse response = GetValue();
```

❌ **Don't forget ConfigureAwait in library code**
```csharp
// Bad
var response = await _httpClient.SendAsync(request);

// Good
var response = await _httpClient.SendAsync(request)
    .ConfigureAwait(false);
```

## Build Enforcement

These practices are enforced by:

- **`.editorconfig`** - Formatting and style rules
- **`Directory.Build.props`** - Compiler warnings as errors, analyzers
- **`stylecop.json`** - StyleCop rules
- **CI pipeline** - Automated checks on every commit

## Quick Checklist Before Committing

- [ ] All public members have XML documentation
- [ ] Interface implementations use `<inheritdoc/>`
- [ ] All inputs are validated
- [ ] Async methods use `ConfigureAwait(false)`
- [ ] CancellationToken is passed through
- [ ] No compiler warnings
- [ ] Tests are written and passing
- [ ] Code follows naming conventions

## Need More Details?

See the complete guides:
- `/docs/dotnet-best-practices.md` - Comprehensive best practices
- `/docs/interface-design-template.md` - Interface templates
- `.github/agents/dotnet-developer.md` - Developer agent instructions
- `.github/agents/documentation-reviewer.md` - Documentation standards
