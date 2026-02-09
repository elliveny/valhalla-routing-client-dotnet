# Interface Design Template

This document provides templates and examples for designing interfaces in the Valhalla .NET Routing Client project.

## Table of Contents

1. [Basic Interface Template](#basic-interface-template)
2. [Complete Interface Example](#complete-interface-example)
3. [Implementation Template](#implementation-template)
4. [Data Transfer Object Template](#data-transfer-object-template)
5. [Exception Template](#exception-template)
6. [Service Extension Template](#service-extension-template)

---

## Basic Interface Template

Use this template when creating new interfaces:

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Valhalla.Routing.Client;

/// <summary>
/// [Brief description of what this interface provides/does]
/// </summary>
/// <remarks>
/// <para>
/// [Additional context about the interface's purpose]
/// </para>
/// <para>
/// [Important notes about thread-safety, lifecycle, or usage]
/// </para>
/// </remarks>
public interface I[InterfaceName]
{
    /// <summary>
    /// [Brief description of what this method does]
    /// </summary>
    /// <param name="[parameterName]">
    /// [Description of the parameter and its constraints]
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the operation. Pass <see cref="CancellationToken.None"/> 
    /// to wait indefinitely.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// [description of return type and its contents].
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="[parameterName]"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when [description of validation failure conditions].
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when [description of state-related failure conditions].
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via <paramref name="cancellationToken"/>.
    /// </exception>
    /// <remarks>
    /// [Additional details about behavior, side effects, or important considerations]
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example usage
    /// var result = await service.[MethodName]([parameter]);
    /// </code>
    /// </example>
    Task<[ReturnType]> [MethodName]Async(
        [ParameterType] [parameterName],
        CancellationToken cancellationToken = default);
}
```

---

## Complete Interface Example

A fully documented interface for the Valhalla client:

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Valhalla.Routing.Client.Models.Route;
using Valhalla.Routing.Client.Models.MapMatching;
using Valhalla.Routing.Client.Models.Status;
using Valhalla.Routing.Client.Models.Locate;
using Valhalla.Routing.Client.Exceptions;

namespace Valhalla.Routing.Client;

/// <summary>
/// Provides methods for interacting with the Valhalla routing engine API.
/// </summary>
/// <remarks>
/// <para>
/// This client is thread-safe and designed for use with dependency injection.
/// All methods support cancellation via <see cref="CancellationToken"/>.
/// </para>
/// <para>
/// The client communicates with a Valhalla server over HTTP and handles
/// serialization, error responses, and timeout scenarios automatically.
/// API keys are applied per-request and never logged for security.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Using dependency injection
/// services.AddValhallaClient(options =>
/// {
///     options.BaseUri = new Uri("http://localhost:8002");
///     options.Timeout = TimeSpan.FromSeconds(30);
/// });
/// 
/// // Using the client
/// public class RouteService
/// {
///     private readonly IValhallaClient _client;
///     
///     public RouteService(IValhallaClient client)
///     {
///         _client = client;
///     }
///     
///     public async Task&lt;RouteResponse&gt; GetRouteAsync()
///     {
///         var request = new RouteRequest
///         {
///             Locations = new List&lt;Location&gt;
///             {
///                 new() { Lat = 40.7128, Lon = -74.0060 },
///                 new() { Lat = 40.7589, Lon = -73.9851 }
///             }
///         };
///         
///         return await _client.RouteAsync(request);
///     }
/// }
/// </code>
/// </example>
public interface IValhallaClient
{
    /// <summary>
    /// Calculates a route between two or more locations.
    /// </summary>
    /// <param name="request">
    /// The route request containing locations, costing options, and routing preferences.
    /// Must contain at least 2 locations.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the operation. Pass <see cref="CancellationToken.None"/> 
    /// to wait indefinitely.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// the route response with turn-by-turn directions, distance, and time estimates.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the request contains invalid data (e.g., fewer than 2 locations,
    /// coordinates out of range, or invalid costing model).
    /// </exception>
    /// <exception cref="ValhallaException">
    /// Thrown when the Valhalla API returns an error response. This includes cases
    /// where no route can be found, the server is overloaded, or the request format
    /// is invalid.
    /// </exception>
    /// <exception cref="TimeoutException">
    /// Thrown when the request exceeds the configured timeout duration.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via <paramref name="cancellationToken"/>.
    /// </exception>
    /// <remarks>
    /// The routing algorithm considers the specified costing model (e.g., auto, bicycle,
    /// pedestrian) and applies appropriate preferences for road types, turn restrictions,
    /// and travel time calculations.
    /// </remarks>
    /// <example>
    /// <code>
    /// var request = new RouteRequest
    /// {
    ///     Locations = new List&lt;Location&gt;
    ///     {
    ///         new() { Lat = 40.7128, Lon = -74.0060 },
    ///         new() { Lat = 40.7589, Lon = -73.9851 }
    ///     },
    ///     Costing = CostingModel.Auto
    /// };
    /// 
    /// var response = await client.RouteAsync(request);
    /// Console.WriteLine($"Distance: {response.Trip.Summary.Length} km");
    /// Console.WriteLine($"Duration: {response.Trip.Summary.Time} seconds");
    /// </code>
    /// </example>
    Task<RouteResponse> RouteAsync(
        RouteRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Matches a GPS trace to the road network and returns a route.
    /// </summary>
    /// <param name="request">
    /// The trace route request containing GPS trace points and matching options.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// the matched route with turn-by-turn directions.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is <c>null</c>.
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
    Task<TraceRouteResponse> TraceRouteAsync(
        TraceRouteRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Matches a GPS trace to the road network and returns detailed edge attributes.
    /// </summary>
    /// <param name="request">
    /// The trace attributes request containing GPS trace points and attribute filters.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// detailed attributes for each matched road segment.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is <c>null</c>.
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
    Task<TraceAttributesResponse> TraceAttributesAsync(
        TraceAttributesRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks the health and version information of the Valhalla service.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// status information including service version and availability.
    /// </returns>
    /// <exception cref="ValhallaException">
    /// Thrown when the Valhalla API returns an error response.
    /// </exception>
    /// <exception cref="TimeoutException">
    /// Thrown when the request exceeds the configured timeout duration.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via <paramref name="cancellationToken"/>.
    /// </exception>
    Task<StatusResponse> StatusAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the nearest roads and routing edges to a given location.
    /// </summary>
    /// <param name="request">
    /// The locate request containing the search location and filtering options.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// information about nearby edges and their routing connectivity.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is <c>null</c>.
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
    Task<LocateResponse> LocateAsync(
        LocateRequest request,
        CancellationToken cancellationToken = default);
}
```

---

## Implementation Template

Template for implementing an interface:

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Valhalla.Routing.Client;

/// <summary>
/// Default implementation of <see cref="I[InterfaceName]"/>.
/// </summary>
/// <remarks>
/// <para>
/// This implementation is thread-safe and designed for use with dependency injection.
/// </para>
/// <para>
/// [Additional implementation-specific notes]
/// </para>
/// </remarks>
public sealed class [ClassName] : I[InterfaceName]
{
    private readonly HttpClient _httpClient;
    private readonly [Options]Options _options;
    private readonly ILogger<[ClassName]> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="[ClassName]"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making requests.</param>
    /// <param name="options">The configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any parameter is <c>null</c>.
    /// </exception>
    public [ClassName](
        HttpClient httpClient,
        IOptions<[Options]Options> options,
        ILogger<[ClassName]> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<[ReturnType]> [MethodName]Async(
        [ParameterType] [parameter],
        CancellationToken cancellationToken = default)
    {
        if ([parameter] == null)
        {
            throw new ArgumentNullException(nameof([parameter]));
        }

        // Validation
        Validate[Parameter]([parameter]);

        try
        {
            _logger.LogDebug("Starting {Method} operation", nameof([MethodName]Async));

            // Implementation
            var result = await PerformOperationAsync([parameter], cancellationToken)
                .ConfigureAwait(false);

            _logger.LogDebug("Completed {Method} operation", nameof([MethodName]Async));

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed");
            throw new [Exception]Exception("Failed to communicate with service", ex);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Operation was cancelled");
            throw;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Request timed out");
            throw new TimeoutException("Request timed out", ex);
        }
    }

    private void Validate[Parameter]([ParameterType] [parameter])
    {
        // Validation logic
        if (/* invalid condition */)
        {
            throw new ArgumentException("Invalid parameter", nameof([parameter]));
        }
    }
}
```

---

## Data Transfer Object Template

Template for DTOs with JSON serialization:

```csharp
using System.Text.Json.Serialization;

namespace Valhalla.Routing.Client.Models.[Category];

/// <summary>
/// Represents [description of what this DTO represents].
/// </summary>
/// <remarks>
/// [Additional context about usage, validation rules, or relationships]
/// </remarks>
public class [ClassName]
{
    /// <summary>
    /// Gets or sets [property description].
    /// </summary>
    /// <value>
    /// [Detailed description of the value, including range, format, or constraints]
    /// </value>
    /// <remarks>
    /// [Additional notes about default values, validation, or special behavior]
    /// </remarks>
    [JsonPropertyName("[json_field_name]")]
    public [Type] [PropertyName] { get; set; }

    /// <summary>
    /// Gets or sets [property description].
    /// </summary>
    /// <value>
    /// [Value description]. If <c>null</c>, [default behavior].
    /// </value>
    [JsonPropertyName("[json_field_name]")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public [Type]? [OptionalProperty] { get; set; }
}
```

---

## Exception Template

Template for custom exceptions:

```csharp
using System;
using System.Runtime.Serialization;

namespace Valhalla.Routing.Client.Exceptions;

/// <summary>
/// The exception that is thrown when [description of when this exception occurs].
/// </summary>
/// <remarks>
/// <para>
/// [Detailed explanation of scenarios that trigger this exception]
/// </para>
/// <para>
/// [Additional context about error codes, recovery strategies, or related exceptions]
/// </para>
/// </remarks>
/// <example>
/// <code>
/// try
/// {
///     // Operation that might fail
/// }
/// catch ([ExceptionName] ex)
/// {
///     Console.WriteLine($"Error: {ex.Message}");
///     // Handle exception
/// }
/// </code>
/// </example>
[Serializable]
public class [ExceptionName] : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="[ExceptionName]"/> class.
    /// </summary>
    public [ExceptionName]()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="[ExceptionName]"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public [ExceptionName](string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="[ExceptionName]"/> class
    /// with a specified error message and a reference to the inner exception
    /// that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c>
    /// if no inner exception is specified.
    /// </param>
    public [ExceptionName](string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="[ExceptionName]"/> class
    /// with serialized data.
    /// </summary>
    /// <param name="info">
    /// The <see cref="SerializationInfo"/> that holds the serialized object data
    /// about the exception being thrown.
    /// </param>
    /// <param name="context">
    /// The <see cref="StreamingContext"/> that contains contextual information
    /// about the source or destination.
    /// </param>
    protected [ExceptionName](SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    /// <summary>
    /// Gets or sets [custom property description].
    /// </summary>
    /// <value>
    /// [Description of what this property contains]
    /// </value>
    public [Type]? [CustomProperty] { get; set; }
}
```

---

## Service Extension Template

Template for DI registration extensions:

```csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Valhalla.Routing.Client;

/// <summary>
/// Provides extension methods for registering [service name] with dependency injection.
/// </summary>
public static class [ServiceName]ServiceCollectionExtensions
{
    /// <summary>
    /// Adds [service description] to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">A delegate to configure the service options.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="configureOptions"/> is <c>null</c>.
    /// </exception>
    /// <example>
    /// <code>
    /// services.Add[ServiceName](options =>
    /// {
    ///     options.[Property] = value;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection Add[ServiceName](
        this IServiceCollection services,
        Action<[Options]Options> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);

        services.AddHttpClient<I[ServiceName], [ServiceName]>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<[Options]Options>>().Value;
            
            client.BaseAddress = options.BaseUri;
            client.Timeout = options.Timeout;
        });

        return services;
    }

    /// <summary>
    /// Adds [service description] to the dependency injection container with configuration binding.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration section to bind.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="configuration"/> is <c>null</c>.
    /// </exception>
    public static IServiceCollection Add[ServiceName](
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<[Options]Options>(configuration);

        services.AddHttpClient<I[ServiceName], [ServiceName]>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<[Options]Options>>().Value;
            
            client.BaseAddress = options.BaseUri;
            client.Timeout = options.Timeout;
        });

        return services;
    }
}
```

---

## Best Practices Summary

1. **Interface-First Design**
   - Define interfaces before implementations
   - Document thoroughly in the interface
   - Use `<inheritdoc/>` in implementations

2. **Comprehensive Documentation**
   - Document all public members
   - Include parameters, returns, and exceptions
   - Provide usage examples for complex scenarios

3. **Consistent Patterns**
   - Follow established naming conventions
   - Use standard parameter ordering (data parameters first, cancellation token last)
   - Apply consistent exception handling

4. **Thread Safety**
   - Design for concurrent usage
   - Document thread-safety guarantees
   - Use immutable types where appropriate

5. **Async/Await**
   - All I/O operations must be async
   - Use `ConfigureAwait(false)` in library code
   - Support cancellation tokens

## References

- `/docs/dotnet-best-practices.md` - Comprehensive best practices guide
- `/docs/specification/specification.md` - Project specification
- `.editorconfig` - Code formatting rules
