# .NET Best Practices for Valhalla Routing Client

## Overview

This document defines the coding standards, best practices, and conventions for the Valhalla .NET Routing Client project. These guidelines ensure consistency, maintainability, and quality throughout the codebase.

## Table of Contents

1. [Coding Style](#coding-style)
2. [XML Documentation](#xml-documentation)
3. [Interface Design](#interface-design)
4. [Naming Conventions](#naming-conventions)
5. [Error Handling](#error-handling)
6. [Asynchronous Programming](#asynchronous-programming)
7. [Dependency Injection](#dependency-injection)
8. [Testing](#testing)
9. [Code Quality](#code-quality)

---

## Coding Style

### General Guidelines

- Follow the [.NET Runtime Coding Style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md)
- Use C# language features appropriately for the target frameworks (.NET 6.0 and .NET 8.0)
- Prefer clarity and readability over cleverness
- Keep methods focused and concise (Single Responsibility Principle)

### Formatting

- **Indentation:** 4 spaces (no tabs)
- **Line length:** Aim for 120 characters maximum
- **Braces:** Allman style (opening brace on new line)
- **Access modifiers:** Always explicit (e.g., `public`, `private`, `internal`)
- **File-scoped namespaces:** Use file-scoped namespace declarations in .NET 6+ projects

```csharp
// Good - file-scoped namespace
namespace Valhalla.Routing.Client.Models;

public class Location
{
    // Implementation
}
```

### Code Organization

- One class per file (except for closely related nested classes)
- File name must match the primary type name
- Order members logically:
  1. Constants
  2. Fields
  3. Constructors
  4. Properties
  5. Methods
  6. Nested types

---

## XML Documentation

### Required Documentation

**ALL public types and members MUST have XML documentation comments.** This is enforced via compiler warnings (CS1591).

### Documentation Standards

#### Interfaces

Interfaces are the primary contracts and must have comprehensive documentation:

```csharp
/// <summary>
/// Provides methods for interacting with the Valhalla routing engine API.
/// </summary>
/// <remarks>
/// This client is thread-safe and designed for use with dependency injection.
/// All methods support cancellation via <see cref="CancellationToken"/>.
/// </remarks>
public interface IValhallaClient
{
    /// <summary>
    /// Calculates a route between two or more locations.
    /// </summary>
    /// <param name="request">The route request containing locations and routing options.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// the route response with turn-by-turn directions, distance, and time estimates.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the request contains invalid data (e.g., fewer than 2 locations).
    /// </exception>
    /// <exception cref="ValhallaException">
    /// Thrown when the Valhalla API returns an error response.
    /// </exception>
    /// <exception cref="TimeoutException">
    /// Thrown when the request exceeds the configured timeout duration.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via <paramref name="cancellationToken"/>.
    /// </exception>
    Task<RouteResponse> RouteAsync(
        RouteRequest request,
        CancellationToken cancellationToken = default);
}
```

#### Concrete Implementations

Use `<inheritdoc/>` to inherit documentation from interfaces:

```csharp
/// <summary>
/// Default implementation of <see cref="IValhallaClient"/>.
/// </summary>
public sealed class ValhallaClient : IValhallaClient
{
    /// <inheritdoc/>
    public async Task<RouteResponse> RouteAsync(
        RouteRequest request,
        CancellationToken cancellationToken = default)
    {
        // Implementation
    }
}
```

**Important:** Always use `<inheritdoc/>` for interface implementations to:
- Avoid documentation duplication
- Ensure consistency when interfaces change
- Reduce maintenance burden

#### Data Transfer Objects (DTOs)

Document all properties with clear descriptions:

```csharp
/// <summary>
/// Represents a geographic location with coordinates.
/// </summary>
public class Location
{
    /// <summary>
    /// Gets or sets the latitude in decimal degrees.
    /// </summary>
    /// <value>
    /// A value between -90 and 90, where positive values represent
    /// north of the equator and negative values represent south.
    /// </value>
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    /// <summary>
    /// Gets or sets the longitude in decimal degrees.
    /// </summary>
    /// <value>
    /// A value between -180 and 180, where positive values represent
    /// east of the prime meridian and negative values represent west.
    /// </value>
    [JsonPropertyName("lon")]
    public double Lon { get; set; }

    /// <summary>
    /// Gets or sets the location type for routing preferences.
    /// </summary>
    /// <value>
    /// One of: "break", "through", "via", or "break_through".
    /// If <c>null</c>, defaults to "break".
    /// </value>
    /// <remarks>
    /// "break" indicates a stop where the route can change direction,
    /// "through" indicates the route should pass through this point,
    /// "via" is a location that must be visited but can be reordered.
    /// </remarks>
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
```

#### Exceptions

Document custom exceptions thoroughly:

```csharp
/// <summary>
/// The exception that is thrown when the Valhalla API returns an error response.
/// </summary>
/// <remarks>
/// This exception provides detailed information about API failures including
/// HTTP status codes, Valhalla-specific error codes, and the raw response body
/// for debugging purposes.
/// </remarks>
public class ValhallaException : Exception
{
    /// <summary>
    /// Gets the HTTP status code returned by the Valhalla API.
    /// </summary>
    /// <value>
    /// The HTTP status code, or <c>null</c> if no response was received.
    /// </value>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Gets the Valhalla error code from the response body.
    /// </summary>
    /// <value>
    /// The numeric error code from the "error_code" field in the response,
    /// or <c>null</c> if not present.
    /// </value>
    public int? ValhallaErrorCode { get; }
}
```

### Documentation Tags Reference

- `<summary>` - Brief description (required for all public members)
- `<remarks>` - Additional details, usage notes, or important warnings
- `<param>` - Parameter description (required for all parameters)
- `<returns>` - Return value description (required for non-void methods)
- `<value>` - Property value description (recommended for properties)
- `<exception>` - Document all exceptions that can be thrown
- `<example>` - Code examples for complex usage patterns
- `<see cref=""/>` - References to other types or members
- `<c>` - Inline code elements
- `<code>` - Multi-line code blocks
- `<inheritdoc/>` - Inherit documentation from base class or interface

### Common Patterns

#### Null Parameters

```csharp
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="request"/> is <c>null</c>.
/// </exception>
```

#### Cancellation

```csharp
/// <param name="cancellationToken">
/// A token to cancel the operation. Pass <see cref="CancellationToken.None"/> 
/// to wait indefinitely.
/// </param>
/// <exception cref="OperationCanceledException">
/// Thrown when the operation is cancelled via <paramref name="cancellationToken"/>.
/// </exception>
```

#### Thread Safety

```csharp
/// <remarks>
/// This type is thread-safe and can be used concurrently from multiple threads.
/// </remarks>
```

---

## Interface Design

### Interface-First Development

1. **Define interfaces before implementations**
   - Interfaces represent contracts and should be stable
   - Design interfaces with extensibility in mind
   - Consider backward compatibility

2. **Interface naming**
   - Prefix with `I` (e.g., `IValhallaClient`)
   - Use clear, descriptive names

3. **Keep interfaces focused**
   - Single Responsibility Principle applies to interfaces
   - Avoid "god interfaces" with too many methods
   - Consider Interface Segregation Principle

### Example: Good Interface Design

```csharp
/// <summary>
/// Provides methods for interacting with the Valhalla routing engine API.
/// </summary>
public interface IValhallaClient
{
    /// <summary>
    /// Calculates a route between two or more locations.
    /// </summary>
    Task<RouteResponse> RouteAsync(
        RouteRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Matches a GPS trace to the road network.
    /// </summary>
    Task<TraceRouteResponse> TraceRouteAsync(
        TraceRouteRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets detailed attributes for a GPS trace matched to the road network.
    /// </summary>
    Task<TraceAttributesResponse> TraceAttributesAsync(
        TraceAttributesRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks the health and version information of the Valhalla service.
    /// </summary>
    Task<StatusResponse> StatusAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the nearest roads and routing edges to a given location.
    /// </summary>
    Task<LocateResponse> LocateAsync(
        LocateRequest request, 
        CancellationToken cancellationToken = default);
}
```

### Implementation Pattern

```csharp
/// <summary>
/// Default implementation of <see cref="IValhallaClient"/>.
/// </summary>
/// <remarks>
/// This implementation is thread-safe and designed for use with dependency injection.
/// </remarks>
public sealed class ValhallaClient : IValhallaClient
{
    private readonly HttpClient _httpClient;
    private readonly ValhallaClientOptions _options;
    private readonly ILogger<ValhallaClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValhallaClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making requests.</param>
    /// <param name="options">The configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any parameter is <c>null</c>.
    /// </exception>
    public ValhallaClient(
        HttpClient httpClient,
        IOptions<ValhallaClientOptions> options,
        ILogger<ValhallaClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<RouteResponse> RouteAsync(
        RouteRequest request,
        CancellationToken cancellationToken = default)
    {
        // Implementation
    }
}
```

---

## Naming Conventions

### General Rules

- Use **PascalCase** for:
  - Class names
  - Method names
  - Property names
  - Public fields (avoid public fields, prefer properties)
  - Namespace names

- Use **camelCase** for:
  - Local variables
  - Method parameters
  - Private fields (with `_` prefix)

- Use **UPPER_SNAKE_CASE** for:
  - Constants (if using C# 12+ consider `const` with PascalCase)

### Examples

```csharp
namespace Valhalla.Routing.Client.Models;

public class RouteRequest
{
    // Private field with underscore prefix
    private readonly ILogger _logger;
    
    // Constant in PascalCase (preferred in modern C#)
    public const int MaxLocations = 20;
    
    // Property in PascalCase
    public List<Location> Locations { get; set; }
    
    // Method in PascalCase
    public void ValidateRequest()
    {
        // Local variable in camelCase
        int locationCount = Locations.Count;
    }
}
```

### Abbreviations and Acronyms

- Two-letter acronyms: Both uppercase (e.g., `IO`, `UI`)
- Three+ letter acronyms: PascalCase (e.g., `HttpClient`, `JsonSerializer`)

```csharp
public class HttpApiClient { } // Good
public class HTTPAPIClient { } // Bad

public string ToJson() { } // Good
public string ToJSON() { } // Bad
```

### Boolean Properties and Methods

- Prefix with `Is`, `Has`, `Can`, or `Should`

```csharp
public bool IsValid { get; set; }
public bool HasError { get; set; }
public bool CanRetry { get; set; }
public bool ShouldRedactApiKey { get; set; }
```

---

## Error Handling

### Exception Guidelines

1. **Use specific exception types**
   - `ArgumentNullException` for null arguments
   - `ArgumentException` for invalid arguments
   - `InvalidOperationException` for invalid state
   - Custom `ValhallaException` for API errors
   - `TimeoutException` for timeouts
   - `OperationCanceledException` for cancellations

2. **Always validate input**
   - Check for null references
   - Validate ranges and constraints
   - Provide clear error messages

3. **Don't swallow exceptions**
   - Log exceptions before re-throwing
   - Wrap exceptions with context when needed

### Example: Proper Validation

```csharp
/// <inheritdoc/>
public async Task<RouteResponse> RouteAsync(
    RouteRequest request,
    CancellationToken cancellationToken = default)
{
    if (request == null)
    {
        throw new ArgumentNullException(nameof(request));
    }

    if (request.Locations == null || request.Locations.Count < 2)
    {
        throw new ArgumentException(
            "Request must contain at least 2 locations.", 
            nameof(request));
    }

    try
    {
        // Implementation
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "HTTP request failed for route endpoint");
        throw new ValhallaException(
            "Failed to communicate with Valhalla API", 
            ex);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        _logger.LogInformation("Route request was cancelled");
        throw;
    }
}
```

### Custom Exception Design

```csharp
/// <summary>
/// The exception that is thrown when the Valhalla API returns an error response.
/// </summary>
public class ValhallaException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValhallaException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ValhallaException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValhallaException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public ValhallaException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    // Additional properties...
}
```

---

## Asynchronous Programming

### Async/Await Best Practices

1. **Always use async/await for I/O operations**
   - All HTTP calls must be async
   - File I/O must be async
   - Database operations must be async

2. **Naming convention**
   - Suffix async methods with `Async`
   - Example: `RouteAsync`, `ValidateAsync`

3. **ConfigureAwait**
   - Use `ConfigureAwait(false)` in library code
   - Avoid capturing synchronization context unnecessarily

4. **Cancellation support**
   - Always accept `CancellationToken` as last parameter
   - Default to `CancellationToken.None` or `default`
   - Pass cancellation token to downstream operations

### Example

```csharp
public async Task<RouteResponse> RouteAsync(
    RouteRequest request,
    CancellationToken cancellationToken = default)
{
    ArgumentNullException.ThrowIfNull(request);

    var jsonContent = SerializeRequest(request);
    
    using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "route")
    {
        Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
    };

    // Pass cancellation token and use ConfigureAwait(false)
    using var response = await _httpClient
        .SendAsync(httpRequest, cancellationToken)
        .ConfigureAwait(false);

    var responseBody = await response.Content
        .ReadAsStringAsync(cancellationToken)
        .ConfigureAwait(false);

    return ParseResponse(responseBody);
}
```

---

## Dependency Injection

### Registration Pattern

Provide extension methods for service registration:

```csharp
/// <summary>
/// Provides extension methods for registering Valhalla client services.
/// </summary>
public static class ValhallaClientServiceCollectionExtensions
{
    /// <summary>
    /// Adds Valhalla routing client services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">A delegate to configure client options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> is <c>null</c>.
    /// </exception>
    public static IServiceCollection AddValhallaClient(
        this IServiceCollection services,
        Action<ValhallaClientOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        
        services.AddHttpClient<IValhallaClient, ValhallaClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ValhallaClientOptions>>().Value;
            client.BaseAddress = options.BaseUri;
            client.Timeout = options.Timeout;
        });

        return services;
    }
}
```

### Non-DI Builder Pattern

For scenarios without DI, provide a builder:

```csharp
/// <summary>
/// Provides a fluent API for building <see cref="IValhallaClient"/> instances.
/// </summary>
/// <remarks>
/// Use this builder when not using dependency injection. For DI scenarios,
/// use <see cref="ValhallaClientServiceCollectionExtensions.AddValhallaClient"/>.
/// </remarks>
public class ValhallaClientBuilder
{
    /// <summary>
    /// Creates a new instance of <see cref="ValhallaClientBuilder"/>.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public static ValhallaClientBuilder Create() => new();

    /// <summary>
    /// Sets the base URL for the Valhalla API.
    /// </summary>
    /// <param name="baseUrl">The base URL (e.g., "http://localhost:8002").</param>
    /// <returns>This builder instance for chaining.</returns>
    public ValhallaClientBuilder WithBaseUrl(string baseUrl)
    {
        // Implementation
        return this;
    }

    /// <summary>
    /// Builds the configured <see cref="IValhallaClient"/> instance.
    /// </summary>
    /// <returns>A fully configured client instance.</returns>
    public IValhallaClient Build()
    {
        // Implementation
    }
}
```

---

## Testing

### Test Organization

- **Unit tests:** Test individual components in isolation
- **Integration tests:** Test against real Valhalla instance
- **Test fixtures:** Store test data in separate JSON files

### Test Naming

Use descriptive names that explain what is being tested:

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
    var request = new RouteRequest
    {
        Locations = new List<Location>
        {
            new() { Lat = 40.7128, Lon = -74.0060 },
            new() { Lat = 40.7589, Lon = -73.9851 }
        }
    };

    // Act
    var response = await client.RouteAsync(request);

    // Assert
    Assert.NotNull(response);
    Assert.NotNull(response.Trip);
}
```

### Code Coverage

- Aim for ≥80% code coverage
- Focus on critical paths and error handling
- Don't sacrifice code quality for coverage metrics

---

## Code Quality

### Static Analysis

The project enforces code quality through:

1. **Compiler warnings as errors** - Zero tolerance for warnings
2. **Code analyzers** - StyleCop, Microsoft.CodeAnalysis.NetAnalyzers
3. **XML documentation warnings** - CS1591 enabled

### Configuration Files

- **`.editorconfig`** - Enforces formatting rules
- **`Directory.Build.props`** - Project-wide settings
- **`stylecop.json`** - StyleCop configuration

### Pre-commit Checklist

Before committing code:

1. ✅ All compiler warnings resolved
2. ✅ All public members documented
3. ✅ Code formatted per `.editorconfig`
4. ✅ Tests pass locally
5. ✅ No sensitive data in code or logs

### Code Review Focus Areas

When reviewing code:

1. **Correctness** - Does it work as intended?
2. **Security** - Any vulnerabilities or data leaks?
3. **Performance** - Any obvious inefficiencies?
4. **Maintainability** - Is it clear and well-documented?
5. **Testability** - Can it be tested effectively?
6. **Consistency** - Does it follow project conventions?

---

## Summary

These best practices ensure:

- ✅ **Consistency** across the codebase
- ✅ **Quality** through automated enforcement
- ✅ **Maintainability** via clear documentation
- ✅ **Reliability** through proper error handling
- ✅ **Professionalism** suitable for open source release

When in doubt, refer to:
1. This document
2. The specification document (`docs/specification/specification.md`)
3. .NET Runtime coding guidelines
4. Microsoft's framework design guidelines

**Remember:** These are guidelines, not absolute rules. Use judgment, and when exceptions are needed, document them clearly.
