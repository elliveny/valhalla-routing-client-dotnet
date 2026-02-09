using Microsoft.Extensions.Logging;

namespace Valhalla.Routing;

/// <summary>
/// High-performance logging using LoggerMessage.Define to avoid allocations.
/// </summary>
internal static partial class Log
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Debug,
        Message = "Sending {Method} request to {Endpoint}")]
    public static partial void RequestStarting(
        ILogger logger, HttpMethod method, string endpoint);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Request to {Endpoint} completed in {ElapsedMs}ms with status {StatusCode}")]
    public static partial void RequestCompleted(
        ILogger logger, string endpoint, long elapsedMs, int statusCode);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "Request to {Endpoint} failed with status {StatusCode}: {ErrorMessage}")]
    public static partial void RequestFailed(
        ILogger logger, string endpoint, int statusCode, string errorMessage);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Warning,
        Message = "API key is configured but BaseUri uses HTTP instead of HTTPS. Credentials may be transmitted insecurely.")]
    public static partial void InsecureTransportWarning(ILogger logger);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Warning,
        Message = "Custom HttpClient provided. Caller is responsible for HttpClient lifecycle, connection pooling, and DNS change handling.")]
    public static partial void CustomHttpClientWarning(ILogger logger);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Debug,
        Message = "Request body: {RequestBody}")]
    public static partial void RequestBody(ILogger logger, string requestBody);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Debug,
        Message = "Response body: {ResponseBody}")]
    public static partial void ResponseBody(ILogger logger, string responseBody);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Warning,
        Message = "Request to {Endpoint} timed out after {TimeoutMs}ms")]
    public static partial void RequestTimedOut(ILogger logger, string endpoint, long timeoutMs);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Error,
        Message = "Failed to deserialize response from {Endpoint}: {ErrorMessage}")]
    public static partial void DeserializationError(ILogger logger, string endpoint, string errorMessage);
}
