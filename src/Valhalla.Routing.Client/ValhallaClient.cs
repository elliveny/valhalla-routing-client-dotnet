using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Valhalla.Routing;

/// <summary>
/// Client for interacting with the Valhalla routing engine HTTP API.
/// </summary>
public class ValhallaClient : IValhallaClient
{
    private const int MaxResponseSizeBytes = 10 * 1024 * 1024; // 10MB
    private const int MaxRawResponseSizeBytes = 8 * 1024; // 8KB for exception raw response

    private readonly HttpClient httpClient;
    private readonly ValhallaClientOptions options;
    private readonly ILogger<ValhallaClient> logger;
    private readonly JsonSerializerOptions jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValhallaClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests.</param>
    /// <param name="options">The configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public ValhallaClient(
        HttpClient httpClient,
        IOptions<ValhallaClientOptions> options,
        ILogger<ValhallaClient> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options?.Value);
        ArgumentNullException.ThrowIfNull(logger);

        this.httpClient = httpClient;
        this.options = options.Value;
        this.logger = logger;

        // Normalize BaseUri to remove trailing slash
        var normalizedBaseUri = NormalizeBaseUri(this.options.BaseUri);

        // Configure HttpClient - always use the options BaseUri as the source of truth
        this.httpClient.BaseAddress = normalizedBaseUri;
        this.httpClient.Timeout = this.options.Timeout;

        // Configure JSON serialization
        this.jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
        };

        // Warn if using HTTP with API key
        if (!string.IsNullOrEmpty(this.options.ApiKeyHeaderName) &&
            !string.IsNullOrEmpty(this.options.ApiKeyHeaderValue) &&
            normalizedBaseUri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase))
        {
            Log.InsecureTransportWarning(this.logger);
        }
    }

    /// <inheritdoc/>
    public async Task<StatusResponse> StatusAsync(StatusRequest? request = null, CancellationToken cancellationToken = default)
    {
        // Use default instance when request is null to ensure valid JSON object
        var requestBody = request ?? new StatusRequest { Verbose = false };

        // No validation required for StatusRequest (all fields are optional)
        return await this.SendAsync<StatusResponse>("status", requestBody, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<LocateResponse> LocateAsync(LocateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate the request
        request.Validate();

        return await this.SendAsync<LocateResponse>("locate", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<RouteResponse> RouteAsync(RouteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate the request
        request.Validate();

        // For RouteAsync, we need custom deserialization to handle the trip/alternates transformation
        return await this.SendRouteRequestAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<TraceRouteResponse> TraceRouteAsync(TraceRouteRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate the request
        request.Validate();

        return await this.SendAsync<TraceRouteResponse>("trace_route", request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<TraceAttributesResponse> TraceAttributesAsync(TraceAttributesRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate the request
        request.Validate();

        return await this.SendAsync<TraceAttributesResponse>("trace_attributes", request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Normalizes the base URI to ensure correct composition with relative endpoints.
    /// For root paths (e.g. "https://host/") a trailing slash is removed; for URIs with
    /// a non-root path (e.g. "https://host/valhalla/") a trailing slash is ensured so that
    /// relative paths are appended instead of replacing the last segment.
    /// </summary>
    /// <param name="uri">The URI to normalize.</param>
    /// <returns>The normalized URI.</returns>
    private static Uri NormalizeBaseUri(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        // If the URI only has the root path, normalize by trimming a trailing slash (if any).
        if (uri.AbsolutePath == "/")
        {
            var rootUriString = uri.AbsoluteUri;
            return rootUriString.EndsWith('/')
                ? new Uri(rootUriString.TrimEnd('/'))
                : uri;
        }

        // For URIs with a non-root path, ensure a trailing slash on the path component so that
        // HttpClient correctly appends relative request URIs (e.g. "status") instead of
        // replacing the last path segment.
        var pathOnly = uri.GetLeftPart(UriPartial.Path);
        if (!pathOnly.EndsWith('/'))
        {
            pathOnly += "/";
        }

        return new Uri(pathOnly);
    }

    /// <summary>
    /// Tries to get a boolean property from a JSON element.
    /// </summary>
    /// <param name="element">The JSON element to search in.</param>
    /// <param name="propertyName">The name of the property to get.</param>
    /// <returns>True if the property exists and is true, false if the property exists and is false, null if the property doesn't exist.</returns>
    private static bool? TryGetBooleanProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var prop))
        {
            return null;
        }

        return prop.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null,
        };
    }

    /// <summary>
    /// Sends a route request and handles the custom deserialization for trip/alternates.
    /// </summary>
    /// <param name="request">The route request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The route response.</returns>
    private async Task<RouteResponse> SendRouteRequestAsync(RouteRequest request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        // Serialize request body
        var jsonBody = JsonSerializer.Serialize(request, this.jsonOptions);

        if (this.options.EnableSensitiveLogging)
        {
            Log.RequestBody(this.logger, jsonBody);
        }

        // Create HTTP request
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "route")
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json"),
        };

        // Add Accept header
        httpRequest.Headers.Accept.Clear();
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Add API key header if configured
        if (!string.IsNullOrEmpty(this.options.ApiKeyHeaderName) &&
            !string.IsNullOrEmpty(this.options.ApiKeyHeaderValue))
        {
            httpRequest.Headers.Add(this.options.ApiKeyHeaderName, this.options.ApiKeyHeaderValue);
        }

        Log.RequestStarting(this.logger, HttpMethod.Post, "route");

        HttpResponseMessage? response = null;
        try
        {
            response = await this.httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            // Caller requested cancellation
            throw new OperationCanceledException("Request was cancelled.", ex, cancellationToken);
        }
        catch (TaskCanceledException ex)
        {
            // Timeout occurred (not caller cancellation)
            stopwatch.Stop();
            Log.RequestTimedOut(this.logger, "route", (long)this.options.Timeout.TotalMilliseconds);
            throw new TimeoutException($"Request to route timed out after {this.options.Timeout}.", ex);
        }

        using (response)
        {
            stopwatch.Stop();

            // Check Content-Length header for size limit
            if (response.Content.Headers.ContentLength.HasValue &&
                response.Content.Headers.ContentLength.Value > MaxResponseSizeBytes)
            {
                Log.RequestFailed(this.logger, "route", (int)response.StatusCode, "Response size exceeds 10MB limit");
                throw new ValhallaException(
                    response.StatusCode,
                    $"Response size ({response.Content.Headers.ContentLength.Value} bytes) exceeds maximum allowed size ({MaxResponseSizeBytes} bytes)");
            }

            // Read response body with size limit enforcement
            string responseBody;
            try
            {
#if NET6_0
#pragma warning disable CA2016 // Forward the cancellationToken parameter - ReadAsStreamAsync doesn't support it in NET6.0
                using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#pragma warning restore CA2016
#else
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#endif
                using var memoryStream = new MemoryStream();
                var buffer = new byte[8192];
                var totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(
                    new Memory<byte>(buffer),
                    cancellationToken).ConfigureAwait(false)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead > MaxResponseSizeBytes)
                    {
                        Log.RequestFailed(this.logger, "route", (int)response.StatusCode, "Response size exceeds 10MB limit");
                        throw new ValhallaException(
                            response.StatusCode,
                            $"Response size exceeds maximum allowed size ({MaxResponseSizeBytes} bytes)");
                    }

                    await memoryStream.WriteAsync(
                        new ReadOnlyMemory<byte>(buffer, 0, bytesRead),
                        cancellationToken).ConfigureAwait(false);
                }

                responseBody = Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            catch (ValhallaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.DeserializationError(this.logger, "route", ex.Message);
                throw new ValhallaException(
                    response.StatusCode,
                    $"Failed to read response body: {ex.Message}");
            }

            if (this.options.EnableSensitiveLogging)
            {
                var truncatedBody = responseBody.Length > MaxRawResponseSizeBytes
                    ? responseBody[..MaxRawResponseSizeBytes] + "... (truncated)"
                    : responseBody;
                Log.ResponseBody(this.logger, truncatedBody);
            }

            // Handle error responses
            if (!response.IsSuccessStatusCode)
            {
                var truncatedResponse = responseBody.Length > MaxRawResponseSizeBytes
                    ? responseBody[..MaxRawResponseSizeBytes]
                    : responseBody;

                // Try to parse Valhalla error response
                try
                {
                    using var errorDoc = JsonDocument.Parse(responseBody);
                    var root = errorDoc.RootElement;

                    var errorCode = root.TryGetProperty("error_code", out var errorCodeElement) &&
                        errorCodeElement.TryGetInt32(out var ec)
                        ? ec
                        : (int?)null;

                    var errorMessage = root.TryGetProperty("error", out var errorElement) &&
                        errorElement.ValueKind == JsonValueKind.String
                        ? errorElement.GetString()
                        : null;

                    var status = root.TryGetProperty("status", out var statusElement) &&
                        statusElement.ValueKind == JsonValueKind.String
                        ? statusElement.GetString()
                        : null;

                    var message = errorMessage ?? $"Request failed with status {(int)response.StatusCode}";

                    Log.RequestFailed(this.logger, "route", (int)response.StatusCode, message);

                    throw new ValhallaException(
                        response.StatusCode,
                        message,
                        truncatedResponse,
                        errorCode,
                        status);
                }
                catch (JsonException)
                {
                    // Not a valid JSON error response, use generic error
                    var message = $"Request failed with status {(int)response.StatusCode}";
                    Log.RequestFailed(this.logger, "route", (int)response.StatusCode, message);
                    throw new ValhallaException(
                        response.StatusCode,
                        message,
                        truncatedResponse);
                }
            }

            Log.RequestCompleted(this.logger, "route", stopwatch.ElapsedMilliseconds, (int)response.StatusCode);

            // Deserialize route response with custom trip/alternates handling
            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                var rootElement = doc.RootElement;

                return this.DeserializeRouteResponse(rootElement);
            }
            catch (JsonException ex)
            {
                Log.DeserializationError(this.logger, "route", ex.Message);
                var errorResponse = responseBody.Length > MaxRawResponseSizeBytes
                    ? responseBody[..MaxRawResponseSizeBytes]
                    : responseBody;
                throw new ValhallaException(
                    HttpStatusCode.OK,
                    $"Failed to deserialize response: {ex.Message}",
                    errorResponse);
            }
        }
    }

    /// <summary>
    /// Deserializes a route response, transforming the trip/alternates structure.
    /// </summary>
    /// <param name="root">The root JSON element.</param>
    /// <returns>The deserialized route response.</returns>
    /// <exception cref="ValhallaException">Thrown when the response is missing the required 'trip' property or trip deserialization fails.</exception>
    private RouteResponse DeserializeRouteResponse(JsonElement root)
    {
        var trips = new List<Trip>();

        // Primary trip (always present in valid responses)
        if (!root.TryGetProperty("trip", out var tripElement) ||
            tripElement.ValueKind == JsonValueKind.Null)
        {
            throw new ValhallaException(
                HttpStatusCode.OK,
                "Route response is missing required 'trip' property.");
        }

        Trip? primaryTrip;
        try
        {
            primaryTrip = JsonSerializer.Deserialize<Trip>(tripElement.GetRawText(), this.jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new ValhallaException(
                HttpStatusCode.OK,
                $"Failed to deserialize primary 'trip' from route response: {ex.Message}");
        }

        if (primaryTrip == null)
        {
            throw new ValhallaException(
                HttpStatusCode.OK,
                "Route response contained an invalid primary 'trip'.");
        }

        trips.Add(primaryTrip);

        // Alternate trips (present when alternates > 0 was requested)
        if (root.TryGetProperty("alternates", out var alternatesElement) &&
            alternatesElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var alt in alternatesElement.EnumerateArray().Where(a => a.TryGetProperty("trip", out _)))
            {
                // Where clause guarantees 'trip' property exists
                var altTripElement = alt.GetProperty("trip");

                try
                {
                    var altTrip = JsonSerializer.Deserialize<Trip>(altTripElement.GetRawText(), this.jsonOptions);
                    if (altTrip != null)
                    {
                        trips.Add(altTrip);
                    }
                }
                catch (JsonException ex)
                {
                    // Log and skip invalid alternate trips, but don't fail the entire request
                    // since the primary trip is valid
                    Log.DeserializationError(this.logger, "route", $"Failed to deserialize alternate trip: {ex.Message}");
                }
            }
        }

        return new RouteResponse
        {
            Raw = root.Clone(),  // MUST clone to detach from parent JsonDocument
            Id = root.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String
                ? id.GetString()
                : null,
            Trips = trips,
        };
    }

    /// <summary>
    /// Sends a request to the Valhalla API and returns the deserialized response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="endpoint">The API endpoint path.</param>
    /// <param name="requestBody">The request body object.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    private async Task<TResponse> SendAsync<TResponse>(
        string endpoint,
        object requestBody,
        CancellationToken cancellationToken)
        where TResponse : class
    {
        var stopwatch = Stopwatch.StartNew();

        // Serialize request body
        var jsonBody = JsonSerializer.Serialize(requestBody, this.jsonOptions);

        if (this.options.EnableSensitiveLogging)
        {
            Log.RequestBody(this.logger, jsonBody);
        }

        // Create HTTP request
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json"),
        };

        // Add Accept header
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Add API key header if configured
        if (!string.IsNullOrEmpty(this.options.ApiKeyHeaderName) &&
            !string.IsNullOrEmpty(this.options.ApiKeyHeaderValue))
        {
            request.Headers.Add(this.options.ApiKeyHeaderName, this.options.ApiKeyHeaderValue);
        }

        Log.RequestStarting(this.logger, HttpMethod.Post, endpoint);

        HttpResponseMessage? response = null;
        try
        {
            response = await this.httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            // Caller requested cancellation
            throw new OperationCanceledException("Request was cancelled.", ex, cancellationToken);
        }
        catch (TaskCanceledException ex)
        {
            // Timeout occurred (not caller cancellation)
            stopwatch.Stop();
            Log.RequestTimedOut(this.logger, endpoint, (long)this.options.Timeout.TotalMilliseconds);
            throw new TimeoutException($"Request to {endpoint} timed out after {this.options.Timeout}.", ex);
        }

        using (response)
        {
            stopwatch.Stop();

            // Check Content-Length header for size limit
            if (response.Content.Headers.ContentLength.HasValue &&
                response.Content.Headers.ContentLength.Value > MaxResponseSizeBytes)
            {
                Log.RequestFailed(this.logger, endpoint, (int)response.StatusCode, "Response size exceeds 10MB limit");
                throw new ValhallaException(
                    response.StatusCode,
                    $"Response size ({response.Content.Headers.ContentLength.Value} bytes) exceeds maximum allowed size ({MaxResponseSizeBytes} bytes)");
            }

            // Read response body with size limit enforcement
            string responseBody;
            try
            {
#if NET6_0
#pragma warning disable CA2016 // Forward the cancellationToken parameter - ReadAsStreamAsync doesn't support it in NET6.0
                using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#pragma warning restore CA2016
#else
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#endif
                using var memoryStream = new MemoryStream();
                var buffer = new byte[8192];
                var totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(
                    new Memory<byte>(buffer),
                    cancellationToken).ConfigureAwait(false)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead > MaxResponseSizeBytes)
                    {
                        Log.RequestFailed(this.logger, endpoint, (int)response.StatusCode, "Response size exceeds 10MB limit");
                        throw new ValhallaException(
                            response.StatusCode,
                            $"Response size exceeds maximum allowed size ({MaxResponseSizeBytes} bytes)");
                    }

                    await memoryStream.WriteAsync(
                        new ReadOnlyMemory<byte>(buffer, 0, bytesRead),
                        cancellationToken).ConfigureAwait(false);
                }

                responseBody = Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            catch (ValhallaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.DeserializationError(this.logger, endpoint, ex.Message);
                throw new ValhallaException(
                    response.StatusCode,
                    $"Failed to read response body: {ex.Message}");
            }

            if (this.options.EnableSensitiveLogging)
            {
                var truncatedBody = responseBody.Length > MaxRawResponseSizeBytes
                    ? responseBody[..MaxRawResponseSizeBytes] + "... (truncated)"
                    : responseBody;
                Log.ResponseBody(this.logger, truncatedBody);
            }

            // Handle error responses
            if (!response.IsSuccessStatusCode)
            {
                var truncatedResponse = responseBody.Length > MaxRawResponseSizeBytes
                    ? responseBody[..MaxRawResponseSizeBytes]
                    : responseBody;

                // Try to parse Valhalla error response
                try
                {
                    using var errorDoc = JsonDocument.Parse(responseBody);
                    var root = errorDoc.RootElement;

                    var errorCode = root.TryGetProperty("error_code", out var errorCodeElement) &&
                        errorCodeElement.TryGetInt32(out var ec)
                        ? ec
                        : (int?)null;

                    var errorMessage = root.TryGetProperty("error", out var errorElement) &&
                        errorElement.ValueKind == JsonValueKind.String
                        ? errorElement.GetString()
                        : null;

                    var status = root.TryGetProperty("status", out var statusElement) &&
                        statusElement.ValueKind == JsonValueKind.String
                        ? statusElement.GetString()
                        : null;

                    var message = errorMessage ?? $"Request failed with status {(int)response.StatusCode}";

                    Log.RequestFailed(this.logger, endpoint, (int)response.StatusCode, message);

                    throw new ValhallaException(
                        response.StatusCode,
                        message,
                        truncatedResponse,
                        errorCode,
                        status);
                }
                catch (JsonException)
                {
                    // Not a valid JSON error response, use generic error
                    var message = $"Request failed with status {(int)response.StatusCode}";
                    Log.RequestFailed(this.logger, endpoint, (int)response.StatusCode, message);
                    throw new ValhallaException(
                        response.StatusCode,
                        message,
                        truncatedResponse);
                }
            }

            Log.RequestCompleted(this.logger, endpoint, stopwatch.ElapsedMilliseconds, (int)response.StatusCode);

            // Deserialize success response
            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                var rootElement = doc.RootElement;
                var rawElement = rootElement.Clone(); // Clone to detach from JsonDocument

                // For StatusResponse, manually construct with Raw property
                if (typeof(TResponse) == typeof(StatusResponse))
                {
                    var statusResponse = new StatusResponse
                    {
                        Raw = rawElement,
                        Version = rootElement.TryGetProperty("version", out var versionProp) && versionProp.ValueKind == JsonValueKind.String
                            ? versionProp.GetString()
                            : null,
                        TilesetLastModified = rootElement.TryGetProperty("tileset_last_modified", out var tilesetProp) && tilesetProp.ValueKind == JsonValueKind.Number
                            ? tilesetProp.GetInt64()
                            : null,
                        HasTiles = TryGetBooleanProperty(rootElement, "has_tiles"),
                        HasAdmins = TryGetBooleanProperty(rootElement, "has_admins"),
                        HasTimezones = TryGetBooleanProperty(rootElement, "has_timezones"),
                        HasLiveTraffic = TryGetBooleanProperty(rootElement, "has_live_traffic"),
                        Bbox = rootElement.TryGetProperty("bbox", out var bboxProp)
                            && bboxProp.ValueKind != JsonValueKind.Null
                            && bboxProp.ValueKind != JsonValueKind.Undefined
                            ? bboxProp.Clone()
                            : null,
                    };

                    return (TResponse)(object)statusResponse;
                }

                // For LocateResponse, manually construct with Raw property
                if (typeof(TResponse) == typeof(LocateResponse))
                {
                    List<LocateResult>? results = null;

                    if (rootElement.ValueKind == JsonValueKind.Array)
                    {
                        // Response is a bare array of locate results
                        results = JsonSerializer.Deserialize<List<LocateResult>>(responseBody, this.jsonOptions);
                    }
                    else if (rootElement.ValueKind == JsonValueKind.Object &&
                             rootElement.TryGetProperty("results", out var resultsElement) &&
                             resultsElement.ValueKind == JsonValueKind.Array)
                    {
                        // Response is an object containing a results array
                        results = JsonSerializer.Deserialize<List<LocateResult>>(resultsElement.GetRawText(), this.jsonOptions);
                    }

                    // Check for top-level warnings and id (they may be in the response when not an array)
                    List<string>? warnings = null;
                    string? id = null;

                    if (rootElement.ValueKind == JsonValueKind.Object)
                    {
                        // If response is an object with optional warnings and id
                        if (rootElement.TryGetProperty("warnings", out var warningsElement) && warningsElement.ValueKind == JsonValueKind.Array)
                        {
                            warnings = JsonSerializer.Deserialize<List<string>>(warningsElement.GetRawText(), this.jsonOptions);
                        }

                        if (rootElement.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String)
                        {
                            id = idElement.GetString();
                        }
                    }

                    var locateResponse = new LocateResponse
                    {
                        Raw = rawElement,
                        Results = results,
                        Warnings = warnings,
                        Id = id,
                    };

                    return (TResponse)(object)locateResponse;
                }

                // For TraceRouteResponse, manually construct with Raw property
                if (typeof(TResponse) == typeof(TraceRouteResponse))
                {
                    Trip? trip = null;
                    if (rootElement.TryGetProperty("trip", out var tripElement))
                    {
                        trip = JsonSerializer.Deserialize<Trip>(tripElement.GetRawText(), this.jsonOptions);
                    }

                    string? id = null;
                    if (rootElement.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String)
                    {
                        id = idElement.GetString();
                    }

                    var traceRouteResponse = new TraceRouteResponse
                    {
                        Raw = rawElement,
                        Trip = trip,
                        Id = id,
                    };

                    return (TResponse)(object)traceRouteResponse;
                }

                // For TraceAttributesResponse, manually construct with Raw property
                if (typeof(TResponse) == typeof(TraceAttributesResponse))
                {
                    string? id = null;
                    if (rootElement.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String)
                    {
                        id = idElement.GetString();
                    }

                    string? units = null;
                    if (rootElement.TryGetProperty("units", out var unitsElement) && unitsElement.ValueKind == JsonValueKind.String)
                    {
                        units = unitsElement.GetString();
                    }

                    string? shape = null;
                    if (rootElement.TryGetProperty("shape", out var shapeElement) && shapeElement.ValueKind == JsonValueKind.String)
                    {
                        shape = shapeElement.GetString();
                    }

                    IReadOnlyList<MatchedPoint>? matchedPoints = null;
                    if (rootElement.TryGetProperty("matched_points", out var matchedPointsElement) && matchedPointsElement.ValueKind == JsonValueKind.Array)
                    {
                        matchedPoints = JsonSerializer.Deserialize<List<MatchedPoint>>(matchedPointsElement.GetRawText(), this.jsonOptions);
                    }

                    IReadOnlyList<TraceEdge>? edges = null;
                    if (rootElement.TryGetProperty("edges", out var edgesElement) && edgesElement.ValueKind == JsonValueKind.Array)
                    {
                        edges = JsonSerializer.Deserialize<List<TraceEdge>>(edgesElement.GetRawText(), this.jsonOptions);
                    }

                    var traceAttributesResponseWithRaw = new TraceAttributesResponse
                    {
                        Raw = rawElement,
                        Id = id,
                        Units = units,
                        MatchedPoints = matchedPoints,
                        Edges = edges,
                        Shape = shape,
                    };

                    return (TResponse)(object)traceAttributesResponseWithRaw;
                }

                // For other response types, use standard deserialization
                var result = JsonSerializer.Deserialize<TResponse>(responseBody, this.jsonOptions);

                if (result == null)
                {
                    throw new ValhallaException(
                        HttpStatusCode.OK,
                        "Response deserialized to null");
                }

                return result;
            }
        catch (JsonException ex)
        {
            Log.DeserializationError(this.logger, endpoint, ex.Message);
            var errorResponse = responseBody.Length > MaxRawResponseSizeBytes
                ? responseBody[..MaxRawResponseSizeBytes]
                : responseBody;
            throw new ValhallaException(
                HttpStatusCode.OK,
                $"Failed to deserialize response: {ex.Message}",
                errorResponse);
        }
        }
    }
}
