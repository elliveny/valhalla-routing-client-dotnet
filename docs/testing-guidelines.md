# Testing Guidelines for Valhalla Routing Client

## Table of Contents

1. [Overview](#overview)
2. [Testing Philosophy](#testing-philosophy)
3. [Testing Framework Setup](#testing-framework-setup)
4. [Unit Testing Guidelines](#unit-testing-guidelines)
5. [Integration Testing Guidelines](#integration-testing-guidelines)
6. [Test Organization](#test-organization)
7. [Test Naming Conventions](#test-naming-conventions)
8. [Test Fixtures and Data](#test-fixtures-and-data)
9. [Mocking and Test Doubles](#mocking-and-test-doubles)
10. [Code Coverage Requirements](#code-coverage-requirements)
11. [Running Tests](#running-tests)
12. [CI/CD Integration](#cicd-integration)
13. [Common Patterns and Examples](#common-patterns-and-examples)
14. [Anti-Patterns to Avoid](#anti-patterns-to-avoid)
15. [Troubleshooting](#troubleshooting)

---

## Overview

This document provides comprehensive testing guidelines for the Valhalla .NET Routing Client project. These guidelines ensure consistent, maintainable, and effective tests that validate both individual components and system integration.

**Target Frameworks:** .NET 6.0 and .NET 8.0

**Minimum Coverage Target:** 80%

---

## Testing Philosophy

### Core Principles

1. **Tests are first-class citizens** - Tests are as important as production code and should be maintained with the same rigor
2. **Test behavior, not implementation** - Focus on what the code does, not how it does it
3. **Fast feedback loops** - Unit tests should run quickly; integration tests can be slower but still reasonable
4. **Isolation** - Unit tests should not depend on external systems; integration tests should be reproducible
5. **Clear intent** - Test names and structure should make the purpose obvious
6. **Maintainability** - Tests should be easy to understand and modify as requirements change

### Testing Pyramid

```
        /\
       /  \        Few, slower, expensive
      /Intg.\      Integration Tests
     /------\      
    /        \     Many, fast, cheap
   /   Unit   \    Unit Tests
  /____________\   
```

**Unit Tests (70-80%):** Test individual classes and methods in isolation
**Integration Tests (20-30%):** Test interactions with the Valhalla API

---

## Testing Framework Setup

### Required NuGet Packages

All test projects must include these packages:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
  <PackageReference Include="xUnit" Version="2.*" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
  <PackageReference Include="FluentAssertions" Version="6.*" />
  <PackageReference Include="Moq" Version="4.*" />
  <PackageReference Include="RichardSzalay.MockHttp" Version="7.*" />
  <PackageReference Include="coverlet.collector" Version="6.*" />
</ItemGroup>
```

### Framework Choices

| Framework | Purpose | Why We Use It |
|-----------|---------|---------------|
| **xUnit** | Test framework | Modern, extensible, parallel test execution |
| **FluentAssertions** | Assertions | Readable, expressive test assertions with excellent failure messages |
| **Moq** | Mocking | Industry standard for creating test doubles |
| **MockHttp** | HTTP mocking | Specifically designed for testing HttpClient interactions |

---

## Unit Testing Guidelines

### What to Unit Test

✅ **DO** unit test:
- Request model serialization (verify snake_case conversion)
- Response model deserialization from JSON
- Validation logic (coordinate bounds, required fields)
- Error response parsing
- Exception handling and mapping
- Business logic and calculations
- Extension methods and utilities

❌ **DON'T** unit test:
- Simple properties (getters/setters)
- Framework code
- Configuration binding (unless custom logic)

### Unit Test Structure

Follow the **Arrange-Act-Assert (AAA)** pattern:

```csharp
[Fact]
public async Task RouteAsync_WithValidRequest_ReturnsRouteResponse()
{
    // Arrange - Set up test data and dependencies
    var mockHttp = new MockHttpMessageHandler();
    mockHttp.When("https://valhalla.example.com/route")
        .Respond("application/json", ReadFixture("route_success.json"));
    
    var client = new ValhallaRoutingClient(mockHttp.ToHttpClient(), new ValhallaOptions
    {
        BaseUri = new Uri("https://valhalla.example.com")
    });
    
    var request = new RouteRequest
    {
        Locations = new[] 
        { 
            new Location { Lat = 49.6116, Lon = 6.1319 },
            new Location { Lat = 49.6233, Lon = 6.2044 }
        },
        Costing = CostingModel.Auto
    };

    // Act - Execute the method under test
    var response = await client.RouteAsync(request);

    // Assert - Verify the outcome
    response.Should().NotBeNull();
    response.Trip.Should().NotBeNull();
    response.Trip.Legs.Should().HaveCountGreaterThan(0);
}
```

### Mocking HTTP Calls

**Always mock HTTP calls in unit tests:**

```csharp
// Set up mock HTTP response
var mockHttp = new MockHttpMessageHandler();
mockHttp.When(HttpMethod.Post, "*/route")
    .Respond("application/json", File.ReadAllText("Fixtures/route_success.json"));

// Create client with mocked handler
var httpClient = mockHttp.ToHttpClient();
httpClient.BaseAddress = new Uri("https://valhalla.example.com");

var client = new ValhallaRoutingClient(httpClient, options);
```

### Testing Serialization

**Verify snake_case output:**

```csharp
[Fact]
public void RouteRequest_Serialization_UsesSnakeCase()
{
    // Arrange
    var request = new RouteRequest
    {
        Locations = new[] { new Location { Lat = 49.6116, Lon = 6.1319 } },
        Costing = CostingModel.Auto,
        DirectionsType = DirectionsType.None
    };

    // Act
    var json = JsonSerializer.Serialize(request, ValhallaJsonOptions.Default);

    // Assert
    json.Should().Contain("\"directions_type\"");  // snake_case, not DirectionsType
    json.Should().NotContain("\"DirectionsType\""); // PascalCase should not appear
}
```

### Testing Validation

**Test boundary conditions and error cases:**

```csharp
[Theory]
[InlineData(-91.0, 0.0)]  // Latitude too low
[InlineData(91.0, 0.0)]   // Latitude too high
[InlineData(0.0, -181.0)] // Longitude too low
[InlineData(0.0, 181.0)]  // Longitude too high
public void Location_WithInvalidCoordinates_ThrowsArgumentException(double lat, double lon)
{
    // Act & Assert
    var action = () => new Location { Lat = lat, Lon = lon }.Validate();
    action.Should().Throw<ArgumentException>();
}
```

### Testing Error Responses

**Parse and map API errors correctly:**

```csharp
[Fact]
public async Task RouteAsync_WhenNoRouteFound_ThrowsValhallaException()
{
    // Arrange
    var mockHttp = new MockHttpMessageHandler();
    mockHttp.When("*/route")
        .Respond(HttpStatusCode.BadRequest, "application/json", 
            ReadFixture("route_error_no_route.json"));

    var client = CreateClient(mockHttp);
    var request = CreateValidRouteRequest();

    // Act & Assert
    await client.Invoking(c => c.RouteAsync(request))
        .Should().ThrowAsync<ValhallaException>()
        .WithMessage("*No route found*")
        .Where(e => e.ErrorCode == 442);
}
```

---

## Integration Testing Guidelines

### What to Integration Test

✅ **DO** integration test:
- Actual API endpoints against a real Valhalla instance
- End-to-end request/response flows
- Network error handling (timeouts, cancellation)
- Large or complex responses
- API version compatibility

❌ **DON'T** integration test:
- Serialization/deserialization (covered by unit tests)
- Validation logic (covered by unit tests)
- Every possible parameter combination (use unit tests)

### Docker Setup

Integration tests require a running Valhalla instance. Use Docker Compose:

**docker-compose.integration.yml:**
```yaml
version: '3.8'

services:
  valhalla:
    image: ghcr.io/valhalla/valhalla:run-latest
    ports:
      - "8002:8002"
    volumes:
      - valhalla_tiles:/data/valhalla
    environment:
      - tile_urls=https://download.geofabrik.de/europe/luxembourg-latest.osm.pbf
      - build_elevation=false
      - build_admins=false
      - build_time_zones=false
      - serve_tiles=true
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8002/status"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  valhalla_tiles:
```

### Integration Test Structure

**Mark with [Trait] attribute:**

```csharp
[Trait("Category", "Integration")]
public class RouteIntegrationTests : IAsyncLifetime
{
    private ValhallaRoutingClient _client;
    private const string ValhallaBaseUrl = "http://localhost:8002";

    public async Task InitializeAsync()
    {
        // Set up client before each test
        _client = new ValhallaRoutingClient(new HttpClient(), new ValhallaOptions
        {
            BaseUri = new Uri(ValhallaBaseUrl)
        });

        // Wait for Valhalla to be ready
        await WaitForValhallaAsync();
    }

    public Task DisposeAsync()
    {
        _client?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task RouteAsync_BetweenLuxembourgLocations_ReturnsValidRoute()
    {
        // Arrange - Use real Luxembourg coordinates
        var request = new RouteRequest
        {
            Locations = new[]
            {
                LuxembourgTestLocations.LuxembourgCity,  // (49.6116, 6.1319)
                LuxembourgTestLocations.Airport          // (49.6233, 6.2044)
            },
            Costing = CostingModel.Auto
        };

        // Act
        var response = await _client.RouteAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Trip.Should().NotBeNull();
        response.Trip.Legs.Should().HaveCountGreaterThan(0);
        response.Trip.Summary.Length.Should().BeGreaterThan(0);
    }

    private async Task WaitForValhallaAsync()
    {
        var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        for (int i = 0; i < 30; i++)
        {
            try
            {
                var response = await httpClient.GetAsync($"{ValhallaBaseUrl}/status");
                if (response.IsSuccessStatusCode) return;
            }
            catch { /* Ignore and retry */ }
            
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
        throw new InvalidOperationException("Valhalla server did not become ready in time");
    }
}
```

### Test Locations

**Use consistent test coordinates (Luxembourg):**

```csharp
public static class LuxembourgTestLocations
{
    // Luxembourg City center
    public static readonly Location LuxembourgCity = new() { Lat = 49.6116, Lon = 6.1319 };
    
    // Kirchberg (European Quarter)
    public static readonly Location Kirchberg = new() { Lat = 49.6283, Lon = 6.1617 };
    
    // Esch-sur-Alzette
    public static readonly Location Esch = new() { Lat = 49.4958, Lon = 5.9806 };
    
    // Findel Airport
    public static readonly Location Airport = new() { Lat = 49.6233, Lon = 6.2044 };
}
```

### Timeout and Cancellation Testing

**Distinguish between timeout and cancellation:**

```csharp
[Fact]
public async Task RouteAsync_WithCancellationToken_ThrowsOperationCanceledException()
{
    // Arrange
    var cts = new CancellationTokenSource();
    cts.Cancel(); // Cancel immediately
    var request = CreateValidRouteRequest();

    // Act & Assert
    await _client.Invoking(c => c.RouteAsync(request, cts.Token))
        .Should().ThrowAsync<OperationCanceledException>();
}

[Fact]
public async Task RouteAsync_WithShortTimeout_ThrowsTimeoutException()
{
    // Arrange
    var options = new ValhallaOptions 
    { 
        BaseUri = new Uri(ValhallaBaseUrl),
        Timeout = TimeSpan.FromMilliseconds(1) // Intentionally too short
    };
    var client = new ValhallaRoutingClient(new HttpClient(), options);
    var request = CreateValidRouteRequest();

    // Act & Assert
    await client.Invoking(c => c.RouteAsync(request))
        .Should().ThrowAsync<TimeoutException>()
        .WithMessage("*timeout*");
}
```

---

## Test Organization

### Directory Structure

```
test/
└── Valhalla.Routing.Client.Tests/
    ├── Unit/
    │   ├── Models/
    │   │   ├── RouteRequestTests.cs
    │   │   └── LocationTests.cs
    │   ├── Serialization/
    │   │   ├── SnakeCaseSerializationTests.cs
    │   │   └── ResponseDeserializationTests.cs
    │   └── Validation/
    │       └── CoordinateValidationTests.cs
    ├── Integration/
    │   ├── RouteIntegrationTests.cs
    │   ├── MapMatchingIntegrationTests.cs
    │   └── StatusIntegrationTests.cs
    ├── Fixtures/
    │   ├── route_success.json
    │   ├── route_error_no_route.json
    │   ├── trace_route_success.json
    │   └── status_verbose.json
    └── TestHelpers/
        ├── LuxembourgTestLocations.cs
        └── MockHttpHelper.cs
```

### File Organization

- **One test class per file**
- **Group related tests in the same class**
- **Namespace should mirror production code structure**

```csharp
// File: test/Valhalla.Routing.Client.Tests/Unit/Models/RouteRequestTests.cs
namespace Valhalla.Routing.Client.Tests.Unit.Models;

public class RouteRequestTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesInstance() { }
    
    [Fact]
    public void Validate_WithMissingLocations_ThrowsException() { }
}
```

---

## Test Naming Conventions

### Method Naming Pattern

Use the format: **`MethodName_Scenario_ExpectedBehavior`**

```csharp
// ✅ Good examples
[Fact]
public async Task RouteAsync_WithValidRequest_ReturnsRouteResponse()

[Fact]
public async Task RouteAsync_WithNullRequest_ThrowsArgumentNullException()

[Fact]
public void Location_WithLatitudeOutOfRange_ThrowsArgumentException()

// ❌ Bad examples
[Fact]
public async Task TestRoute()  // Too vague

[Fact]
public async Task Test1()  // Meaningless name

[Fact]
public async Task RouteWorks()  // Not descriptive enough
```

### Class Naming Pattern

Test class names should clearly indicate what they test:

```csharp
// ✅ Good examples
public class RouteRequestTests { }
public class ValhallaRoutingClientTests { }
public class RouteIntegrationTests { }

// ❌ Bad examples
public class Tests { }  // Too generic
public class UnitTests { }  // Doesn't say what is tested
```

---

## Test Fixtures and Data

### Fixture File Naming

Use the pattern: **`{endpoint}_{scenario}.json`**

```
Fixtures/
├── route_success.json
├── route_with_alternates.json
├── route_error_no_route.json
├── trace_route_success.json
├── trace_attributes_success.json
├── status_basic.json
├── status_verbose.json
└── locate_multiple_results.json
```

### Loading Fixtures

**Create a helper method:**

```csharp
protected static string ReadFixture(string fileName)
{
    var path = Path.Combine("Fixtures", fileName);
    if (!File.Exists(path))
    {
        throw new FileNotFoundException($"Fixture not found: {fileName}", path);
    }
    return File.ReadAllText(path);
}
```

### Fixture Content

**Store real API responses:**

```json
// Fixtures/route_success.json
{
  "trip": {
    "locations": [
      {"type": "break", "lat": 49.611599, "lon": 6.131899},
      {"type": "break", "lat": 49.623299, "lon": 6.204399}
    ],
    "legs": [
      {
        "summary": {
          "time": 720.5,
          "length": 10.234
        },
        "maneuvers": [...]
      }
    ]
  }
}
```

---

## Mocking and Test Doubles

### When to Mock

✅ **DO** mock:
- HTTP calls to external APIs
- Time-dependent operations (use `ITimeProvider` or similar)
- File system operations
- Database calls (if any)
- Expensive computations

❌ **DON'T** mock:
- Simple DTOs or value objects
- Data structures with no behavior
- The system under test itself

### HTTP Mocking Examples

**Basic mock:**

```csharp
var mockHttp = new MockHttpMessageHandler();
mockHttp.When("*/route")
    .Respond("application/json", ReadFixture("route_success.json"));
```

**With request verification:**

```csharp
mockHttp.When(HttpMethod.Post, "*/route")
    .WithContent("*auto*")  // Verify request contains "auto"
    .Respond("application/json", ReadFixture("route_success.json"));
```

**Simulating errors:**

```csharp
mockHttp.When("*/route")
    .Respond(HttpStatusCode.BadRequest, "application/json", 
        ReadFixture("route_error_no_route.json"));
```

**Simulating network issues:**

```csharp
mockHttp.When("*/route")
    .Throw(new HttpRequestException("Network error"));
```

### Using Moq

**For custom dependencies:**

```csharp
var mockLogger = new Mock<ILogger<ValhallaRoutingClient>>();
var client = new ValhallaRoutingClient(httpClient, options, mockLogger.Object);

// Verify logging occurred
mockLogger.Verify(
    x => x.Log(
        LogLevel.Error,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("error")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
    Times.Once);
```

---

## Code Coverage Requirements

### Coverage Targets

- **Minimum coverage:** 80%
- **Aspirational coverage:** 90%+
- **Critical paths:** 100% (error handling, validation, security)

### Measuring Coverage

**Using coverlet:**

```bash
# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=cobertura

# Generate HTML report (requires ReportGenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage-report
```

### What NOT to Cover

Excluding these from coverage requirements:
- Auto-generated code
- Simple DTOs with only properties
- Explicit interface implementations that just delegate
- Code marked with `[ExcludeFromCodeCoverage]`

---

## Running Tests

### Command Line

```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test --filter "Category!=Integration"

# Run only integration tests
dotnet test --filter "Category=Integration"

# Run tests with detailed output
dotnet test --verbosity detailed

# Run specific test class
dotnet test --filter "FullyQualifiedName~RouteRequestTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~RouteAsync_WithValidRequest_ReturnsRouteResponse"
```

### Visual Studio

1. Open **Test Explorer** (Test → Test Explorer)
2. Click **Run All** or right-click specific tests
3. View test output in the **Test Explorer** window
4. Double-click failed tests to jump to code

### Visual Studio Code

1. Install **C# Dev Kit** extension
2. Tests appear in **Testing** sidebar
3. Click play button to run tests
4. View results inline

### Integration Test Setup

**Start Valhalla before integration tests:**

```bash
# Start Valhalla container
docker-compose -f docker-compose.integration.yml up -d

# Wait for it to be ready (first run: ~3 minutes for tile build)
docker-compose -f docker-compose.integration.yml logs -f

# Run integration tests
dotnet test --filter "Category=Integration"

# Clean up
docker-compose -f docker-compose.integration.yml down
```

---

## CI/CD Integration

### GitHub Actions Workflow

Integration tests can and should run in GitHub Actions. The workflow uses a two-stage pipeline with tile caching to optimize performance.

#### Complete Workflow Example

**File:** `.github/workflows/build.yml`

```yaml
name: Build and Test

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

env:
  DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true
  DOTNET_CLI_TELEMETRY_OPTOUT: true

jobs:
  build-and-unit-test:
    name: Build & Unit Tests (.NET ${{ matrix.dotnet-version }})
    runs-on: ubuntu-latest
    strategy:
      matrix:
        dotnet-version: ['6.0.x', '8.0.x']
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET ${{ matrix.dotnet-version }}
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ matrix.dotnet-version }}
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build (warnings as errors)
        run: dotnet build --no-restore --configuration Release /p:TreatWarningsAsErrors=true
      
      - name: Run Unit Tests
        run: dotnet test --no-build --configuration Release --filter "Category!=Integration" --verbosity normal

  integration-test:
    name: Integration Tests
    runs-on: ubuntu-latest
    needs: build-and-unit-test
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET 8.0
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      # Cache Valhalla tiles to avoid rebuilding on every run
      - name: Cache Valhalla tiles
        uses: actions/cache@v4
        with:
          path: ~/.valhalla-tiles
          key: valhalla-tiles-luxembourg-v1
          restore-keys: |
            valhalla-tiles-luxembourg-
      
      # Restore cached tiles into Docker volume
      - name: Prepare tile cache volume
        run: |
          mkdir -p ~/.valhalla-tiles
          if [ -d ~/.valhalla-tiles/valhalla_tiles ]; then
            echo "Restoring tiles from cache..."
            docker volume create valhalla-routing-client-dotnet_valhalla_tiles
            docker run --rm -v ~/.valhalla-tiles:/source -v valhalla-routing-client-dotnet_valhalla_tiles:/dest alpine cp -r /source/. /dest/
          fi
      
      - name: Start Valhalla container
        run: docker-compose -f docker-compose.integration.yml up -d
      
      - name: Wait for Valhalla to be ready
        run: |
          echo "Waiting for Valhalla to build tiles and start..."
          timeout 300 bash -c 'until curl -sf http://localhost:8002/status; do sleep 5; done'
          echo "Valhalla is ready!"
      
      # Save built tiles for future runs
      - name: Save tiles to cache
        run: |
          docker run --rm -v valhalla-routing-client-dotnet_valhalla_tiles:/source -v ~/.valhalla-tiles:/dest alpine cp -r /source/. /dest/
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore --configuration Release
      
      - name: Run Integration Tests
        run: dotnet test --no-build --configuration Release --filter "Category=Integration" --verbosity normal
      
      - name: Stop Valhalla container
        if: always()
        run: docker-compose -f docker-compose.integration.yml down
```

### Key CI/CD Features

#### 1. Two-Stage Pipeline

- **Stage 1: Build & Unit Tests** - Fast feedback (~2-3 minutes)
  - Runs on matrix of .NET versions (6.0 and 8.0)
  - Fails fast if code doesn't compile or unit tests fail
  - No Docker required

- **Stage 2: Integration Tests** - Only if unit tests pass
  - Runs on .NET 8.0 only (to avoid duplicate integration test runs)
  - Uses Docker to run Valhalla
  - Takes ~3-5 minutes (first run with tile building) or ~1-2 minutes (with cached tiles)

#### 2. Tile Caching Strategy

**Problem:** Valhalla tile building takes 2-3 minutes on first run

**Solution:** Use GitHub Actions cache to persist tiles between workflow runs

**Benefits:**
- First run: ~3-5 minutes (includes tile building)
- Subsequent runs: ~1-2 minutes (tiles cached)
- Cache is shared across all branches
- Cache key: `valhalla-tiles-luxembourg-v1` (increment version to rebuild)

**How it works:**
1. Cache action restores tiles from previous run to `~/.valhalla-tiles`
2. Tiles are copied into Docker volume before starting container
3. After tests, tiles are saved back to cache directory
4. Next run uses cached tiles

#### 3. Matrix Testing

Unit tests run on multiple .NET versions to ensure compatibility:

```yaml
strategy:
  matrix:
    dotnet-version: ['6.0.x', '8.0.x']
```

Integration tests run once on .NET 8.0 (Docker behavior is framework-agnostic).

#### 4. Timeout Protection

Valhalla startup has a 5-minute timeout to prevent hanging workflows:

```yaml
timeout 300 bash -c 'until curl -sf http://localhost:8002/status; do sleep 5; done'
```

### Coverage in CI (Optional)

```yaml
- name: Run Tests with Coverage
  run: dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=opencover

- name: Upload Coverage
  uses: codecov/codecov-action@v3
  with:
    files: ./coverage.opencover.xml
```

### CI Best Practices

✅ **DO:**
- Cache Valhalla tiles to speed up integration tests
- Run integration tests only after unit tests pass
- Use `if: always()` for cleanup steps
- Set reasonable timeouts (5 minutes for Valhalla startup)
- Use matrix testing for unit tests across .NET versions
- Run integration tests on single .NET version to save CI time

❌ **DON'T:**
- Run integration tests in matrix (unnecessary, doubles test time)
- Skip tile caching (makes every run slow)
- Use excessively short timeouts (tile building needs time on first run)
- Run integration tests on every commit (consider running on PR only)

---

## Common Patterns and Examples

### Testing Async Methods

```csharp
[Fact]
public async Task RouteAsync_WithValidRequest_ReturnsRouteResponse()
{
    // Arrange
    var client = CreateClient();
    var request = CreateValidRouteRequest();

    // Act
    var response = await client.RouteAsync(request);

    // Assert
    response.Should().NotBeNull();
}
```

### Testing Exceptions

**Using FluentAssertions:**

```csharp
[Fact]
public async Task RouteAsync_WithNullRequest_ThrowsArgumentNullException()
{
    // Arrange
    var client = CreateClient();

    // Act & Assert
    await client.Invoking(c => c.RouteAsync(null))
        .Should().ThrowAsync<ArgumentNullException>()
        .WithParameterName("request");
}
```

### Parameterized Tests

**Using [Theory] and [InlineData]:**

```csharp
[Theory]
[InlineData(CostingModel.Auto)]
[InlineData(CostingModel.Bicycle)]
[InlineData(CostingModel.Pedestrian)]
public async Task RouteAsync_WithDifferentCostingModels_ReturnsRoute(CostingModel costing)
{
    // Arrange
    var request = CreateRouteRequest(costing);
    var client = CreateClient();

    // Act
    var response = await client.RouteAsync(request);

    // Assert
    response.Trip.Costing.Should().Be(costing.ToString().ToLowerInvariant());
}
```

**Using [MemberData] for complex data:**

```csharp
public static IEnumerable<object[]> InvalidLocationData =>
    new List<object[]>
    {
        new object[] { -91.0, 0.0, "Latitude" },
        new object[] { 91.0, 0.0, "Latitude" },
        new object[] { 0.0, -181.0, "Longitude" },
        new object[] { 0.0, 181.0, "Longitude" }
    };

[Theory]
[MemberData(nameof(InvalidLocationData))]
public void Location_WithInvalidCoordinates_ThrowsArgumentException(
    double lat, double lon, string paramName)
{
    // Act & Assert
    var action = () => new Location { Lat = lat, Lon = lon }.Validate();
    action.Should().Throw<ArgumentException>()
        .WithParameterName(paramName);
}
```

### Testing Collections

```csharp
[Fact]
public async Task RouteAsync_WithMultipleWaypoints_ReturnsLegsForEachSegment()
{
    // Arrange
    var request = new RouteRequest
    {
        Locations = new[] 
        { 
            LuxembourgTestLocations.LuxembourgCity,
            LuxembourgTestLocations.Kirchberg,
            LuxembourgTestLocations.Airport
        },
        Costing = CostingModel.Auto
    };
    var client = CreateClient();

    // Act
    var response = await client.RouteAsync(request);

    // Assert
    response.Trip.Legs.Should().HaveCount(2); // 3 locations = 2 legs
    response.Trip.Legs.Should().OnlyContain(leg => leg.Summary.Length > 0);
}
```

### Setup and Teardown

**Using IAsyncLifetime:**

```csharp
public class IntegrationTestBase : IAsyncLifetime
{
    protected ValhallaRoutingClient Client;
    
    public async Task InitializeAsync()
    {
        // Setup before each test
        Client = CreateClient();
        await WaitForValhallaAsync();
    }
    
    public Task DisposeAsync()
    {
        // Cleanup after each test
        Client?.Dispose();
        return Task.CompletedTask;
    }
}
```

---

## Anti-Patterns to Avoid

### ❌ Testing Implementation Details

```csharp
// BAD - Testing private implementation
[Fact]
public void ParseJson_ShouldCallJsonSerializer()
{
    // Don't test that a specific method is called
}

// GOOD - Testing behavior
[Fact]
public async Task RouteAsync_WithValidJson_ReturnsDeserializedResponse()
{
    // Test that the right data comes back
}
```

### ❌ One Giant Test

```csharp
// BAD - Testing everything in one test
[Fact]
public async Task TestEverything()
{
    // Test serialization
    // Test deserialization
    // Test validation
    // Test error handling
    // etc.
}

// GOOD - Focused tests
[Fact]
public void Request_Serialization_UsesSnakeCase() { }

[Fact]
public async Task RouteAsync_WithValidRequest_ReturnsResponse() { }
```

### ❌ Fragile Assertions

```csharp
// BAD - Too specific
response.Trip.Summary.Time.Should().Be(720.5);

// GOOD - Test what matters
response.Trip.Summary.Time.Should().BeGreaterThan(0);
```

### ❌ Hidden Test Dependencies

```csharp
// BAD - Tests depend on execution order
private static RouteResponse _lastResponse;

[Fact]
public async Task Test1_CreateRoute()
{
    _lastResponse = await client.RouteAsync(request);
}

[Fact]
public void Test2_VerifyRoute()
{
    _lastResponse.Should().NotBeNull(); // Depends on Test1!
}

// GOOD - Each test is independent
[Fact]
public async Task RouteAsync_WithValidRequest_ReturnsRouteResponse()
{
    var response = await client.RouteAsync(request);
    response.Should().NotBeNull();
}
```

### ❌ Slow Unit Tests

```csharp
// BAD - Real HTTP call in unit test
[Fact]
public async Task TestRoute()
{
    var client = new HttpClient();
    var response = await client.GetAsync("http://valhalla.example.com/route");
    // ...
}

// GOOD - Mock HTTP calls
[Fact]
public async Task RouteAsync_WithValidRequest_ReturnsRouteResponse()
{
    var mockHttp = new MockHttpMessageHandler();
    mockHttp.When("*/route").Respond("application/json", fixture);
    // ...
}
```

### ❌ Meaningless Test Names

```csharp
// BAD
[Fact]
public void Test1() { }

// GOOD
[Fact]
public void Location_WithInvalidLatitude_ThrowsArgumentException() { }
```

---

## Troubleshooting

### Common Issues

#### Integration Tests Fail with Connection Refused

**Problem:** Valhalla container is not ready

**Solution:**
```bash
# Check container status
docker-compose -f docker-compose.integration.yml ps

# View logs
docker-compose -f docker-compose.integration.yml logs valhalla

# Wait for "Tile building complete" or "HTTP server ready"
```

#### Integration Tests Timeout on First Run

**Problem:** Valhalla is building tiles (takes 2-3 minutes)

**Solution:** Wait for tile building to complete. Subsequent runs are fast due to volume caching.

#### MockHttp Not Matching Requests

**Problem:** Request doesn't match mock setup

**Solution:**
```csharp
// Use wildcards
mockHttp.When("*/route")  // Matches any host ending in /route

// Debug by checking actual request
mockHttp.When(HttpMethod.Post, "*")
    .Respond(req => {
        var content = req.Content.ReadAsStringAsync().Result;
        Console.WriteLine($"Received request: {content}");
        return new HttpResponseMessage(HttpStatusCode.OK);
    });
```

#### Tests Pass Locally but Fail in CI

**Problem:** Environment differences

**Solution:**
- Check .NET version matches CI (6.0 or 8.0)
- Verify Docker version is compatible
- Check for hardcoded paths or URLs
- Review CI logs for specific error messages

#### Coverage Lower Than Expected

**Problem:** Not all code paths exercised

**Solution:**
```bash
# Generate detailed coverage report
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=cobertura
reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage-report

# Open coverage-report/index.html to see uncovered lines
```

---

## Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [.NET Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
- [Project Specification](specification/specification.md)
- [Contributing Guide](../CONTRIBUTING.md)

---

**Last Updated:** 2026-02-08
**Version:** 1.0
