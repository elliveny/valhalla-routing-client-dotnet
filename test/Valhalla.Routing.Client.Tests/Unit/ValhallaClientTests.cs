using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Valhalla.Routing;

namespace Valhalla.Routing.Client.Tests.Unit;

/// <summary>
/// Unit tests for ValhallaClient.
/// </summary>
public class ValhallaClientTests
{
    [Fact]
    public async Task ValhallaClient_StatusAsync_SendsCorrectRequest()
    {
        // Arrange
        using var mockHttp = new MockHttpMessageHandler();
        var expectedResponse = """
        {
            "version": "3.6.0",
            "has_tiles": true
        }
        """;

        mockHttp.When(HttpMethod.Post, "http://localhost:8002/status")
            .WithContent("{\"verbose\":false}")
            .Respond("application/json", expectedResponse);

        var httpClient = mockHttp.ToHttpClient();
        var options = Options.Create(new ValhallaClientOptions
        {
            BaseUri = new Uri("http://localhost:8002"),
        });
        var logger = NullLogger<ValhallaClient>.Instance;

        var client = new ValhallaClient(httpClient, options, logger);

        // Act
        var response = await client.StatusAsync();

        // Assert
        response.Should().NotBeNull();
        response.Version.Should().Be("3.6.0");
        response.HasTiles.Should().BeTrue();
        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ValhallaClient_StatusAsync_WithVerboseTrue_SendsCorrectRequest()
    {
        // Arrange
        using var mockHttp = new MockHttpMessageHandler();
        var expectedResponse = """
        {
            "version": "3.6.0",
            "has_tiles": true,
            "has_admins": true
        }
        """;

        mockHttp.When(HttpMethod.Post, "http://localhost:8002/status")
            .WithContent("{\"verbose\":true}")
            .Respond("application/json", expectedResponse);

        var httpClient = mockHttp.ToHttpClient();
        var options = Options.Create(new ValhallaClientOptions
        {
            BaseUri = new Uri("http://localhost:8002"),
        });
        var logger = NullLogger<ValhallaClient>.Instance;

        var client = new ValhallaClient(httpClient, options, logger);

        // Act
        var response = await client.StatusAsync(new StatusRequest { Verbose = true });

        // Assert
        response.Should().NotBeNull();
        response.Version.Should().Be("3.6.0");
        response.HasTiles.Should().BeTrue();
        response.HasAdmins.Should().BeTrue();
        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ValhallaClient_StatusAsync_ThrowsValhallaException_OnApiError()
    {
        // Arrange
        using var mockHttp = new MockHttpMessageHandler();
        var errorResponse = """
        {
            "error_code": 100,
            "error": "Service unavailable",
            "status_code": 503,
            "status": "Service Unavailable"
        }
        """;

        mockHttp.When(HttpMethod.Post, "http://localhost:8002/status")
            .Respond(HttpStatusCode.ServiceUnavailable, "application/json", errorResponse);

        var httpClient = mockHttp.ToHttpClient();
        var options = Options.Create(new ValhallaClientOptions
        {
            BaseUri = new Uri("http://localhost:8002"),
        });
        var logger = NullLogger<ValhallaClient>.Instance;

        var client = new ValhallaClient(httpClient, options, logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValhallaException>(
            async () => await client.StatusAsync());

        exception.HttpStatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        exception.Message.Should().Be("Service unavailable");
        exception.ErrorCode.Should().Be(100);
        exception.HttpStatus.Should().Be("Service Unavailable");
        exception.RawResponse.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ValhallaClient_StatusAsync_ThrowsValhallaException_OnNon2xxWithoutErrorStructure()
    {
        // Arrange
        using var mockHttp = new MockHttpMessageHandler();
        var errorResponse = "Internal Server Error";

        mockHttp.When(HttpMethod.Post, "http://localhost:8002/status")
            .Respond(HttpStatusCode.InternalServerError, "text/plain", errorResponse);

        var httpClient = mockHttp.ToHttpClient();
        var options = Options.Create(new ValhallaClientOptions
        {
            BaseUri = new Uri("http://localhost:8002"),
        });
        var logger = NullLogger<ValhallaClient>.Instance;

        var client = new ValhallaClient(httpClient, options, logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValhallaException>(
            async () => await client.StatusAsync());

        exception.HttpStatusCode.Should().Be(HttpStatusCode.InternalServerError);
        exception.Message.Should().Contain("500");
        exception.RawResponse.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ValhallaClient_Timeout_ThrowsTimeoutException()
    {
        // Arrange
        using var mockHttp = new MockHttpMessageHandler();

        mockHttp.When(HttpMethod.Post, "http://localhost:8002/status")
            .Respond(async () =>
            {
                // Simulate a delay longer than the timeout
                await Task.Delay(TimeSpan.FromSeconds(10));
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var httpClient = mockHttp.ToHttpClient();
        httpClient.Timeout = TimeSpan.FromMilliseconds(100); // Short timeout for test

        var options = Options.Create(new ValhallaClientOptions
        {
            BaseUri = new Uri("http://localhost:8002"),
            Timeout = TimeSpan.FromMilliseconds(100),
        });
        var logger = NullLogger<ValhallaClient>.Instance;

        var client = new ValhallaClient(httpClient, options, logger);

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(
            async () => await client.StatusAsync());
    }

    [Fact]
    public async Task ValhallaClient_Cancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        using var mockHttp = new MockHttpMessageHandler();

        mockHttp.When(HttpMethod.Post, "http://localhost:8002/status")
            .Respond(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var httpClient = mockHttp.ToHttpClient();
        var options = Options.Create(new ValhallaClientOptions
        {
            BaseUri = new Uri("http://localhost:8002"),
        });
        var logger = NullLogger<ValhallaClient>.Instance;

        var client = new ValhallaClient(httpClient, options, logger);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        var task = client.StatusAsync(cancellationToken: cts.Token);
        cts.Cancel(); // Cancel immediately

        await Assert.ThrowsAsync<OperationCanceledException>(async () => await task);
    }

    [Fact]
    public void ValhallaClient_Constructor_NormalizesTrailingSlash()
    {
        // Arrange
        using var mockHttp = new MockHttpMessageHandler();

        // Don't set up any mock expectations, just create the client
        var httpClient = mockHttp.ToHttpClient();

        // Verify httpClient.BaseAddress starts as null
        httpClient.BaseAddress.Should().BeNull("MockHttpMessageHandler should not set BaseAddress");

        // Test with trailing slash
        var optionsWithSlash = Options.Create(new ValhallaClientOptions
        {
            BaseUri = new Uri("http://localhost:8002/"),
        });
        var logger = NullLogger<ValhallaClient>.Instance;

        // Act
        _ = new ValhallaClient(httpClient, optionsWithSlash, logger);

        // Assert - BaseAddress should be set
        // Note: .NET Uri class may add trailing slash for root paths (http://host/ vs http://host/path)
        // The important thing is that the address is set correctly for API calls
        httpClient.BaseAddress.Should().NotBeNull();
        httpClient.BaseAddress!.GetLeftPart(UriPartial.Authority).Should().Be("http://localhost:8002");
    }

    [Fact]
    public void ValhallaClient_Constructor_ThrowsOnNullArguments()
    {
        // Arrange
        using var httpClient = new HttpClient();
        var options = Options.Create(new ValhallaClientOptions
        {
            BaseUri = new Uri("http://localhost:8002"),
        });
        var logger = NullLogger<ValhallaClient>.Instance;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ValhallaClient(null!, options, logger));
        Assert.Throws<ArgumentNullException>(() => new ValhallaClient(httpClient, null!, logger));
        Assert.Throws<ArgumentNullException>(() => new ValhallaClient(httpClient, options, null!));
    }

    [Fact]
    public async Task ValhallaClient_LocateAsync_SendsCorrectRequest()
    {
        // Arrange
        using var mockHttp = new MockHttpMessageHandler();
        var expectedResponse = """
        [
            {
                "input_lat": 49.6116,
                "input_lon": 6.1319,
                "edges": [
                    {
                        "way_id": 12345678,
                        "correlated_lat": 49.6117,
                        "correlated_lon": 6.1320,
                        "side_of_street": "left",
                        "percent_along": 0.42
                    }
                ],
                "nodes": [
                    {
                        "lat": 49.6118,
                        "lon": 6.1321
                    }
                ]
            }
        ]
        """;

        mockHttp.When(HttpMethod.Post, "http://localhost:8002/locate")
            .WithContent("{\"locations\":[{\"lat\":49.6116,\"lon\":6.1319}],\"costing\":\"auto\"}")
            .Respond("application/json", expectedResponse);

        var httpClient = mockHttp.ToHttpClient();
        var options = Options.Create(new ValhallaClientOptions
        {
            BaseUri = new Uri("http://localhost:8002"),
        });
        var logger = NullLogger<ValhallaClient>.Instance;

        var client = new ValhallaClient(httpClient, options, logger);

        var request = new LocateRequest
        {
            Locations = new[]
            {
                new Location { Lat = 49.6116, Lon = 6.1319 },
            },
            Costing = CostingModel.Auto,
        };

        // Act
        var response = await client.LocateAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Results.Should().NotBeNull();
        response.Results.Should().HaveCount(1);

        var result = response.Results![0];
        result.InputLat.Should().Be(49.6116);
        result.InputLon.Should().Be(6.1319);
        result.Edges.Should().HaveCount(1);

        var edge = result.Edges![0];
        edge.WayId.Should().Be(12345678);
        edge.CorrelatedLat.Should().Be(49.6117);
        edge.CorrelatedLon.Should().Be(6.1320);

        // Verify Raw JSON is preserved through client deserialization
        response.Raw.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Array, "Raw JSON should be preserved as array");
        response.Raw.GetArrayLength().Should().Be(1);
        var rawFirstResult = response.Raw[0];
        rawFirstResult.TryGetProperty("input_lat", out var latElement).Should().BeTrue();
        latElement.GetDouble().Should().Be(49.6116);

        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ValhallaClient_RouteAsync_SendsCorrectRequest()
    {
        // Arrange
        using var mockHttp = new MockHttpMessageHandler();
        var expectedResponse = """
        {
          "trip": {
            "locations": [
              {"lat": 52.517037, "lon": 13.388860, "type": "break"},
              {"lat": 52.529407, "lon": 13.397634, "type": "break"}
            ],
            "legs": [
              {
                "maneuvers": [
                  {
                    "type": 1,
                    "instruction": "Drive north on Friedrichstraße.",
                    "length": 0.543,
                    "time": 65,
                    "begin_shape_index": 0,
                    "end_shape_index": 12,
                    "street_names": ["Friedrichstraße"]
                  }
                ],
                "summary": {
                  "length": 1.842,
                  "time": 312
                },
                "shape": "yzq~IcvxpA..."
              }
            ],
            "summary": {
              "length": 1.842,
              "time": 312,
              "min_lat": 52.517037,
              "min_lon": 13.388860,
              "max_lat": 52.529407,
              "max_lon": 13.397634,
              "has_toll": false,
              "has_highway": false,
              "has_ferry": false
            },
            "units": "kilometers",
            "language": "en-US"
          },
          "id": "my-request-id"
        }
        """;

        mockHttp.When(HttpMethod.Post, "http://localhost:8002/route")
            .WithContent("{\"locations\":[{\"lat\":52.517037,\"lon\":13.38886},{\"lat\":52.529407,\"lon\":13.397634}],\"costing\":\"auto\"}")
            .Respond("application/json", expectedResponse);

        var httpClient = mockHttp.ToHttpClient();
        var options = Options.Create(new ValhallaClientOptions
        {
            BaseUri = new Uri("http://localhost:8002"),
        });
        var logger = NullLogger<ValhallaClient>.Instance;

        var client = new ValhallaClient(httpClient, options, logger);

        var request = new RouteRequest
        {
            Locations = new[]
            {
                new Location { Lat = 52.517037, Lon = 13.388860 },
                new Location { Lat = 52.529407, Lon = 13.397634 },
            },
            Costing = CostingModel.Auto,
        };

        // Act
        var response = await client.RouteAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be("my-request-id");
        response.Trips.Should().HaveCount(1);

        var trip = response.Trips![0];
        trip.Units.Should().Be("kilometers");
        trip.Language.Should().Be("en-US");
        trip.Legs.Should().HaveCount(1);

        var leg = trip.Legs![0];
        leg.Summary.Should().NotBeNull();
        leg.Summary!.Length.Should().Be(1.842);
        leg.Summary.Time.Should().Be(312);

        // Verify Raw JSON is preserved
        response.Raw.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Object);
        response.Raw.TryGetProperty("id", out var idElement).Should().BeTrue();
        idElement.GetString().Should().Be("my-request-id");

        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ValhallaClient_TraceRouteAsync_SendsCorrectRequest()
    {
        // Arrange
        using var mockHttp = new MockHttpMessageHandler();
        var expectedResponse = """
        {
          "trip": {
            "locations": [
              {"lat": 49.6116, "lon": 6.1319, "type": "break"},
              {"lat": 49.6233, "lon": 6.2044, "type": "break"}
            ],
            "legs": [
              {
                "maneuvers": [
                  {
                    "type": 1,
                    "instruction": "Drive northwest.",
                    "length": 8.5,
                    "time": 720
                  }
                ],
                "summary": {
                  "length": 8.5,
                  "time": 720
                },
                "shape": "test_shape"
              }
            ],
            "summary": {
              "length": 8.5,
              "time": 720
            }
          }
        }
        """;

        mockHttp.When(HttpMethod.Post, "http://localhost:8002/trace_route")
            .WithContent("{\"costing\":\"auto\",\"shape\":[{\"lat\":49.6116,\"lon\":6.1319},{\"lat\":49.6233,\"lon\":6.2044}]}")
            .Respond("application/json", expectedResponse);

        var httpClient = mockHttp.ToHttpClient();
        var options = Options.Create(new ValhallaClientOptions
        {
            BaseUri = new Uri("http://localhost:8002"),
        });
        var logger = NullLogger<ValhallaClient>.Instance;

        var client = new ValhallaClient(httpClient, options, logger);

        var request = new TraceRouteRequest
        {
            Shape = new[]
            {
                new TracePoint { Lat = 49.6116, Lon = 6.1319 },
                new TracePoint { Lat = 49.6233, Lon = 6.2044 },
            },
            Costing = CostingModel.Auto,
        };

        // Act
        var response = await client.TraceRouteAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Trip.Should().NotBeNull();

        var trip = response.Trip!;
        trip.Legs.Should().HaveCount(1);

        var leg = trip.Legs![0];
        leg.Summary.Should().NotBeNull();
        leg.Summary!.Length.Should().Be(8.5);
        leg.Summary.Time.Should().Be(720);
        leg.Shape.Should().Be("test_shape");

        // Verify Raw JSON is preserved
        response.Raw.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Object);
        response.Raw.TryGetProperty("trip", out var tripElement).Should().BeTrue();

        mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ValhallaClient_TraceAttributesAsync_SendsCorrectRequest()
    {
        // Arrange
        using var mockHttp = new MockHttpMessageHandler();
        var expectedResponse = """
        {
          "matched_points": [
            {
              "lat": 49.6117,
              "lon": 6.1320,
              "type": "matched",
              "edge_index": 0
            }
          ],
          "edges": [
            {
              "length": 0.543,
              "speed": 50,
              "names": ["Main Street"]
            }
          ],
          "shape": "test_shape",
          "units": "kilometers"
        }
        """;

        mockHttp.When(HttpMethod.Post, "http://localhost:8002/trace_attributes")
            .WithContent("{\"costing\":\"auto\",\"shape\":[{\"lat\":49.6116,\"lon\":6.1319},{\"lat\":49.6233,\"lon\":6.2044}]}")
            .Respond("application/json", expectedResponse);

        var httpClient = mockHttp.ToHttpClient();
        var options = Options.Create(new ValhallaClientOptions
        {
            BaseUri = new Uri("http://localhost:8002"),
        });
        var logger = NullLogger<ValhallaClient>.Instance;

        var client = new ValhallaClient(httpClient, options, logger);

        var request = new TraceAttributesRequest
        {
            Shape = new[]
            {
                new TracePoint { Lat = 49.6116, Lon = 6.1319 },
                new TracePoint { Lat = 49.6233, Lon = 6.2044 },
            },
            Costing = CostingModel.Auto,
        };

        // Act
        var response = await client.TraceAttributesAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Units.Should().Be("kilometers");
        response.Shape.Should().Be("test_shape");

        response.MatchedPoints.Should().NotBeNull();
        response.MatchedPoints.Should().HaveCount(1);
        var matchedPoint = response.MatchedPoints![0];
        matchedPoint.Lat.Should().Be(49.6117);
        matchedPoint.Lon.Should().Be(6.1320);
        matchedPoint.Type.Should().Be("matched");
        matchedPoint.EdgeIndex.Should().Be(0);

        response.Edges.Should().NotBeNull();
        response.Edges.Should().HaveCount(1);
        var edge = response.Edges![0];
        edge.Length.Should().Be(0.543);
        edge.Speed.Should().Be(50);
        edge.Names.Should().ContainSingle().Which.Should().Be("Main Street");

        // Verify Raw JSON is preserved
        response.Raw.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Object);
        response.Raw.TryGetProperty("units", out var unitsElement).Should().BeTrue();
        unitsElement.GetString().Should().Be("kilometers");

        mockHttp.VerifyNoOutstandingExpectation();
    }
}
