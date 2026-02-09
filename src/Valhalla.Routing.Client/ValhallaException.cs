using System.Net;
#if NET6_0
using System.Runtime.Serialization;
#endif

namespace Valhalla.Routing;

/// <summary>
/// Exception thrown when the Valhalla API returns an error response.
/// </summary>
#if NET6_0
[Serializable]
#endif
public class ValhallaException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValhallaException"/> class.
    /// </summary>
    public ValhallaException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValhallaException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ValhallaException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValhallaException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ValhallaException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValhallaException"/> class.
    /// </summary>
    /// <param name="httpStatusCode">The HTTP status code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="rawResponse">The raw HTTP response body.</param>
    /// <param name="errorCode">The Valhalla internal error code.</param>
    /// <param name="httpStatus">The HTTP status text.</param>
    public ValhallaException(
        HttpStatusCode httpStatusCode,
        string message,
        string? rawResponse = null,
        int? errorCode = null,
        string? httpStatus = null)
        : base(message)
    {
        this.HttpStatusCode = httpStatusCode;
        this.HttpStatus = httpStatus;
        this.RawResponse = rawResponse;
        this.ErrorCode = errorCode;
    }

#if NET6_0
    /// <summary>
    /// Initializes a new instance of the <see cref="ValhallaException"/> class.
    /// </summary>
    /// <param name="info">The serialization info.</param>
    /// <param name="context">The streaming context.</param>
    protected ValhallaException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        this.HttpStatusCode = (HttpStatusCode)info.GetInt32(nameof(this.HttpStatusCode));
        this.HttpStatus = info.GetString(nameof(this.HttpStatus));
        var errorCodeObj = info.GetValue(nameof(this.ErrorCode), typeof(object));
        if (errorCodeObj is int errorCodeValue)
        {
            this.ErrorCode = errorCodeValue;
        }
        else
        {
            this.ErrorCode = null;
        }

        this.RawResponse = info.GetString(nameof(this.RawResponse));
    }
#endif

    /// <summary>
    /// Gets the HTTP status code from the error response.
    /// </summary>
    public HttpStatusCode HttpStatusCode { get; }

    /// <summary>
    /// Gets the HTTP status text from the error response.
    /// </summary>
    public string? HttpStatus { get; }

    /// <summary>
    /// Gets the Valhalla internal error code.
    /// </summary>
    public int? ErrorCode { get; }

    /// <summary>
    /// Gets the raw HTTP response body (truncated to 8KB).
    /// </summary>
    public string? RawResponse { get; }

#if NET6_0
    /// <inheritdoc/>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(this.HttpStatusCode), (int)this.HttpStatusCode);
        info.AddValue(nameof(this.HttpStatus), this.HttpStatus);
        info.AddValue(nameof(this.ErrorCode), this.ErrorCode);
        info.AddValue(nameof(this.RawResponse), this.RawResponse);
    }
#endif
}
