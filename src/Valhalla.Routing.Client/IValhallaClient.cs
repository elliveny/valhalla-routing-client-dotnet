namespace Valhalla.Routing;

/// <summary>
/// Interface for the Valhalla routing client.
/// </summary>
public interface IValhallaClient
{
    /// <summary>
    /// Gets the status of the Valhalla server, including version and tileset information.
    /// </summary>
    /// <param name="request">Optional request parameters. If null, default parameters are used.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the status response.</returns>
    public Task<StatusResponse> StatusAsync(StatusRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the nearest roads to the specified locations.
    /// </summary>
    /// <param name="request">The locate request with locations and options.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the locate response with road candidates.</returns>
    public Task<LocateResponse> LocateAsync(LocateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates a route between two or more locations.
    /// </summary>
    /// <param name="request">The route request with locations and routing options. Must contain at least 2 locations.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the route response with directions and trip details.</returns>
    public Task<RouteResponse> RouteAsync(RouteRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Matches a GPS trace to the road network and returns a route.
    /// </summary>
    /// <param name="request">The trace route request with GPS points and matching options.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the matched route.</returns>
    public Task<TraceRouteResponse> TraceRouteAsync(TraceRouteRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Matches a GPS trace and returns detailed edge attributes.
    /// </summary>
    /// <param name="request">The trace attributes request with GPS points and filtering options.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains matched points and edge details.</returns>
    public Task<TraceAttributesResponse> TraceAttributesAsync(TraceAttributesRequest request, CancellationToken cancellationToken = default);
}
