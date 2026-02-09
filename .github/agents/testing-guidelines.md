# Testing Guidelines for AI Agents

## Purpose

This document provides structured, actionable testing guidelines specifically formatted for AI agents working on the Valhalla .NET Routing Client project. These guidelines are optimized for agent interpretation and execution.

---

## Quick Reference

### Project Context
- **Project Type:** .NET C# Client Library
- **Target Frameworks:** .NET 6.0, .NET 8.0
- **Test Framework:** xUnit
- **Minimum Coverage:** 80%
- **Test Location:** `/test/Valhalla.Routing.Client.Tests/`

### Key Commands
```bash
# Unit tests only (fast)
dotnet test --filter "Category!=Integration"

# Integration tests only (requires Docker)
dotnet test --filter "Category=Integration"

# All tests
dotnet test

# With coverage
dotnet test /p:CollectCoverage=true
```

---

## Testing Framework Stack

### Required Packages
```xml
<PackageReference Include="xUnit" Version="2.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="Moq" Version="4.*" />
<PackageReference Include="RichardSzalay.MockHttp" Version="7.*" />
```

### When to Use Each Tool
- **xUnit:** All test method declarations (`[Fact]`, `[Theory]`)
- **FluentAssertions:** All assertions (`.Should()` syntax)
- **Moq:** Mocking custom interfaces/classes (`Mock<T>`)
- **MockHttp:** Mocking HTTP calls (`MockHttpMessageHandler`)

---

## Unit Test Template

### Standard Unit Test Structure

```csharp
using Xunit;
using FluentAssertions;
using RichardSzalay.MockHttp;

namespace Valhalla.Routing.Client.Tests.Unit.{Category};

public class {ClassName}Tests
{
    [Fact]
    public async Task {MethodName}_{Scenario}_{ExpectedBehavior}()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("*/{endpoint}")
            .Respond("application/json", ReadFixture("{fixture_name}.json"));
        
        var client = new ValhallaRoutingClient(
            mockHttp.ToHttpClient(),
            new ValhallaOptions { BaseUrl = "https://test.example.com" }
        );
        
        var request = new {RequestType}
        {
            // Configure request
        };

        // Act
        var result = await client.{MethodName}(request);

        // Assert
        result.Should().NotBeNull();
        // Additional assertions
    }
    
    private static string ReadFixture(string fileName)
    {
        var path = Path.Combine("Fixtures", fileName);
        return File.ReadAllText(path);
    }
}
```

### Test Naming Convention
**Pattern:** `MethodName_Scenario_ExpectedBehavior`

**Examples:**
- `RouteAsync_WithValidRequest_ReturnsRouteResponse`
- `RouteAsync_WithNullRequest_ThrowsArgumentNullException`
- `Location_WithInvalidLatitude_ThrowsArgumentException`

---

## Integration Test Template

### Standard Integration Test Structure

```csharp
using Xunit;
using FluentAssertions;

namespace Valhalla.Routing.Client.Tests.Integration;

[Trait("Category", "Integration")]
public class {Feature}IntegrationTests : IAsyncLifetime
{
    private ValhallaRoutingClient _client;
    private const string ValhallaBaseUrl = "http://localhost:8002";

    public async Task InitializeAsync()
    {
        _client = new ValhallaRoutingClient(
            new HttpClient(),
            new ValhallaOptions { BaseUrl = ValhallaBaseUrl }
        );
        await WaitForValhallaAsync();
    }

    public Task DisposeAsync()
    {
        _client?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task {MethodName}_{Scenario}_{ExpectedBehavior}()
    {
        // Arrange
        var request = new {RequestType}
        {
            Locations = new[]
            {
                LuxembourgTestLocations.LuxembourgCity,
                LuxembourgTestLocations.Airport
            },
            Costing = CostingModel.Auto
        };

        // Act
        var response = await _client.{MethodName}(request);

        // Assert
        response.Should().NotBeNull();
        response.Trip.Should().NotBeNull();
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
            catch { /* Retry */ }
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
        throw new InvalidOperationException("Valhalla not ready");
    }
}
```

### Test Locations
**Always use these predefined Luxembourg coordinates:**

```csharp
public static class LuxembourgTestLocations
{
    public static readonly Location LuxembourgCity = new() { Lat = 49.6116, Lon = 6.1319 };
    public static readonly Location Kirchberg = new() { Lat = 49.6283, Lon = 6.1617 };
    public static readonly Location Esch = new() { Lat = 49.4958, Lon = 5.9806 };
    public static readonly Location Airport = new() { Lat = 49.6233, Lon = 6.2044 };
}
```

---

## Test Patterns by Scenario

### Pattern 1: Test Successful API Call

```csharp
[Fact]
public async Task {Method}_WithValidRequest_ReturnsExpectedResponse()
{
    // Arrange
    var mockHttp = new MockHttpMessageHandler();
    mockHttp.When("*/{endpoint}")
        .Respond("application/json", ReadFixture("{endpoint}_success.json"));
    var client = CreateClient(mockHttp);

    // Act
    var response = await client.{Method}(validRequest);

    // Assert
    response.Should().NotBeNull();
    response.{Property}.Should().{Condition};
}
```

### Pattern 2: Test Null/Invalid Input

```csharp
[Fact]
public async Task {Method}_WithNullRequest_ThrowsArgumentNullException()
{
    // Arrange
    var client = CreateClient();

    // Act & Assert
    await client.Invoking(c => c.{Method}(null))
        .Should().ThrowAsync<ArgumentNullException>()
        .WithParameterName("request");
}
```

### Pattern 3: Test Validation

```csharp
[Theory]
[InlineData(-91.0, 0.0)]   // Invalid latitude
[InlineData(91.0, 0.0)]    // Invalid latitude
[InlineData(0.0, -181.0)]  // Invalid longitude
[InlineData(0.0, 181.0)]   // Invalid longitude
public void {Method}_WithInvalidCoordinates_ThrowsArgumentException(double lat, double lon)
{
    // Act & Assert
    var action = () => new Location { Lat = lat, Lon = lon }.Validate();
    action.Should().Throw<ArgumentException>();
}
```

### Pattern 4: Test API Error Response

```csharp
[Fact]
public async Task {Method}_WhenApiReturnsError_ThrowsValhallaException()
{
    // Arrange
    var mockHttp = new MockHttpMessageHandler();
    mockHttp.When("*/{endpoint}")
        .Respond(HttpStatusCode.BadRequest, "application/json",
            ReadFixture("{endpoint}_error.json"));
    var client = CreateClient(mockHttp);

    // Act & Assert
    await client.Invoking(c => c.{Method}(request))
        .Should().ThrowAsync<ValhallaException>()
        .Where(e => e.ErrorCode == {expectedErrorCode});
}
```

### Pattern 5: Test Serialization (snake_case)

```csharp
[Fact]
public void {RequestClass}_Serialization_UsesSnakeCase()
{
    // Arrange
    var request = new {RequestClass}
    {
        {PropertyInPascalCase} = value
    };

    // Act
    var json = JsonSerializer.Serialize(request, ValhallaJsonOptions.Default);

    // Assert
    json.Should().Contain("\"{property_in_snake_case}\"");
    json.Should().NotContain("\"{PropertyInPascalCase}\"");
}
```

### Pattern 6: Test Deserialization

```csharp
[Fact]
public void {ResponseClass}_Deserialization_ParsesJsonCorrectly()
{
    // Arrange
    var json = ReadFixture("{endpoint}_success.json");

    // Act
    var response = JsonSerializer.Deserialize<{ResponseClass}>(json, ValhallaJsonOptions.Default);

    // Assert
    response.Should().NotBeNull();
    response.{Property}.Should().{Condition};
}
```

### Pattern 7: Test Timeout vs Cancellation

```csharp
[Fact]
public async Task {Method}_WithCancellationToken_ThrowsOperationCanceledException()
{
    // Arrange
    var cts = new CancellationTokenSource();
    cts.Cancel();
    var client = CreateClient();

    // Act & Assert
    await client.Invoking(c => c.{Method}(request, cts.Token))
        .Should().ThrowAsync<OperationCanceledException>();
}

[Fact]
public async Task {Method}_WithShortTimeout_ThrowsTimeoutException()
{
    // Arrange
    var options = new ValhallaOptions 
    { 
        BaseUrl = "https://test.example.com",
        Timeout = TimeSpan.FromMilliseconds(1)
    };
    var client = new ValhallaRoutingClient(new HttpClient(), options);

    // Act & Assert
    await client.Invoking(c => c.{Method}(request))
        .Should().ThrowAsync<TimeoutException>();
}
```

---

## Test Fixture Management

### Fixture File Convention
**Naming:** `{endpoint}_{scenario}.json`

**Examples:**
- `route_success.json`
- `route_error_no_route.json`
- `route_with_alternates.json`
- `trace_route_success.json`
- `status_verbose.json`

### Fixture Directory Structure
```
test/Valhalla.Routing.Client.Tests/
└── Fixtures/
    ├── route_success.json
    ├── route_error_no_route.json
    ├── trace_route_success.json
    └── status_verbose.json
```

### Loading Fixtures
```csharp
private static string ReadFixture(string fileName)
{
    var path = Path.Combine("Fixtures", fileName);
    if (!File.Exists(path))
    {
        throw new FileNotFoundException($"Fixture not found: {fileName}", path);
    }
    return File.ReadAllText(path);
}
```

---

## Assertion Patterns

### FluentAssertions Syntax

#### Basic Assertions
```csharp
// Null checks
result.Should().NotBeNull();
result.Should().BeNull();

// Equality
result.Should().Be(expectedValue);
result.Should().NotBe(unexpectedValue);

// Numeric comparisons
length.Should().BeGreaterThan(0);
time.Should().BeLessThanOrEqualTo(maxTime);
value.Should().BeInRange(min, max);

// String assertions
message.Should().Contain("expected text");
message.Should().StartWith("prefix");
message.Should().Match("*pattern*");

// Collection assertions
list.Should().HaveCount(expectedCount);
list.Should().HaveCountGreaterThan(0);
list.Should().Contain(item);
list.Should().OnlyContain(x => x.IsValid);
list.Should().BeInAscendingOrder();

// Exception assertions
action.Should().Throw<ArgumentException>();
action.Should().ThrowAsync<InvalidOperationException>();
action.Should().Throw<ArgumentException>()
    .WithMessage("*invalid*")
    .WithParameterName("param");
```

#### Complex Assertions
```csharp
// Object comparison
actual.Should().BeEquivalentTo(expected);

// Property assertions
result.Should().NotBeNull()
    .And.Subject.Id.Should().BeGreaterThan(0);

// Conditional assertions
if (condition)
{
    result.Should().Match<Type>(x => x.Property == value);
}
```

---

## Mock HTTP Setup

### Basic Mock
```csharp
var mockHttp = new MockHttpMessageHandler();
mockHttp.When("*/{endpoint}")
    .Respond("application/json", jsonContent);

var httpClient = mockHttp.ToHttpClient();
httpClient.BaseAddress = new Uri("https://test.example.com");
```

### Mock with HTTP Method
```csharp
mockHttp.When(HttpMethod.Post, "*/{endpoint}")
    .Respond("application/json", jsonContent);
```

### Mock with Content Verification
```csharp
mockHttp.When(HttpMethod.Post, "*/{endpoint}")
    .WithContent("*auto*")  // Verify request contains "auto"
    .Respond("application/json", jsonContent);
```

### Mock Error Response
```csharp
mockHttp.When("*/{endpoint}")
    .Respond(HttpStatusCode.BadRequest, "application/json", errorJson);
```

### Mock Network Exception
```csharp
mockHttp.When("*/{endpoint}")
    .Throw(new HttpRequestException("Network error"));
```

---

## Test Organization Rules

### Directory Structure
```
test/Valhalla.Routing.Client.Tests/
├── Unit/
│   ├── Models/
│   ├── Serialization/
│   ├── Validation/
│   └── Client/
├── Integration/
├── Fixtures/
└── TestHelpers/
```

### File Organization Rules
1. **One test class per file**
2. **File name matches class name:** `RouteRequestTests.cs`
3. **Namespace mirrors directory:** `Valhalla.Routing.Client.Tests.Unit.Models`
4. **Group related tests in same class**

### Class Organization
```csharp
namespace Valhalla.Routing.Client.Tests.Unit.{Category};

public class {ClassUnderTest}Tests
{
    // Helper methods/setup at top
    private static ValhallaRoutingClient CreateClient() { }
    private static string ReadFixture(string name) { }
    
    // Happy path tests first
    [Fact]
    public async Task Method_WithValidInput_ReturnsExpectedResult() { }
    
    // Error/edge case tests next
    [Fact]
    public async Task Method_WithNullInput_ThrowsException() { }
    
    // Parameterized tests last
    [Theory]
    [InlineData(...)]
    public void Method_WithVariousInputs_BehavesCorrectly() { }
}
```

---

## Integration Test Setup

### Docker Compose
**File:** `docker-compose.integration.yml` (in repository root)

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
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8002/status"]
      interval: 10s
volumes:
  valhalla_tiles:
```

### Running Integration Tests
```bash
# Start Valhalla
docker-compose -f docker-compose.integration.yml up -d

# Wait for ready (first run: 2-3 minutes)
docker-compose -f docker-compose.integration.yml logs -f

# Run tests
dotnet test --filter "Category=Integration"

# Cleanup
docker-compose -f docker-compose.integration.yml down
```

---

## Coverage Requirements

### Targets
- **Minimum:** 80%
- **Critical paths:** 100% (error handling, validation, security)

### Measuring Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=cobertura
```

### What to Cover
✅ **Must cover:**
- All public methods
- Error handling paths
- Validation logic
- Serialization/deserialization
- Exception cases

❌ **Can skip:**
- Simple property getters/setters
- Auto-generated code
- Explicit interface implementations that delegate

---

## Common Mistakes to Avoid

### ❌ Don't Test Implementation Details
```csharp
// BAD
[Fact]
public void ShouldCallHttpClientGetAsync()
{
    // Testing internal implementation
}

// GOOD
[Fact]
public async Task RouteAsync_WithValidRequest_ReturnsRouteResponse()
{
    // Testing behavior
}
```

### ❌ Don't Make Tests Depend on Each Other
```csharp
// BAD
private static int _sharedState;

[Fact]
public void Test1() { _sharedState = 1; }

[Fact]
public void Test2() { Assert.Equal(1, _sharedState); } // Depends on Test1!

// GOOD - Each test is independent
[Fact]
public void Test1() { /* Independent */ }

[Fact]
public void Test2() { /* Independent */ }
```

### ❌ Don't Use Real HTTP in Unit Tests
```csharp
// BAD
[Fact]
public async Task TestRoute()
{
    var client = new HttpClient();
    var response = await client.GetAsync("http://actual-api.com/route");
}

// GOOD
[Fact]
public async Task RouteAsync_WithValidRequest_ReturnsRouteResponse()
{
    var mockHttp = new MockHttpMessageHandler();
    mockHttp.When("*/route").Respond("application/json", fixture);
    // ...
}
```

### ❌ Don't Use Vague Test Names
```csharp
// BAD
[Fact]
public void Test1() { }

[Fact]
public void TestRoute() { }

// GOOD
[Fact]
public async Task RouteAsync_WithNullRequest_ThrowsArgumentNullException() { }
```

---

## Decision Trees

### When Writing a New Test

```
START: What are you testing?
│
├─ Public API method?
│  ├─ Unit test with mocked HTTP
│  └─ Integration test against real API
│
├─ Validation logic?
│  └─ Unit test with [Theory] for multiple cases
│
├─ Serialization/Deserialization?
│  └─ Unit test with JSON fixtures
│
├─ Error handling?
│  └─ Unit test with mocked error responses
│
└─ Private method?
   └─ Don't test directly - test through public API
```

### Choosing Test Type

```
Do you need real HTTP calls?
│
├─ NO → Unit Test
│  - Mark with nothing (no [Trait])
│  - Use MockHttpMessageHandler
│  - Place in /Unit directory
│  - Should run in < 100ms
│
└─ YES → Integration Test
   - Mark with [Trait("Category", "Integration")]
   - Use real HttpClient
   - Place in /Integration directory
   - Requires Docker Compose
   - Can take several seconds
```

---

## Checklist for New Tests

### Before Writing Tests
- [ ] Understand what behavior needs testing
- [ ] Identify if unit or integration test is needed
- [ ] Check if similar tests exist to use as template
- [ ] Prepare test fixtures if needed (JSON responses)

### While Writing Tests
- [ ] Use correct test attribute (`[Fact]` or `[Theory]`)
- [ ] Follow naming convention: `Method_Scenario_ExpectedBehavior`
- [ ] Structure as Arrange-Act-Assert
- [ ] Use FluentAssertions for all assertions
- [ ] Mock external dependencies (HTTP, time, etc.)
- [ ] Add `[Trait("Category", "Integration")]` if integration test

### After Writing Tests
- [ ] Test passes when run individually
- [ ] Test passes when run with full test suite
- [ ] Test fails when it should (verify it's actually testing something)
- [ ] Code coverage increased appropriately
- [ ] Test name clearly describes what is tested
- [ ] No hardcoded values that should be constants

---

## Quick Command Reference

### Running Tests
```bash
# All tests
dotnet test

# Unit tests only
dotnet test --filter "Category!=Integration"

# Integration tests only
dotnet test --filter "Category=Integration"

# Specific test class
dotnet test --filter "FullyQualifiedName~RouteRequestTests"

# With coverage
dotnet test /p:CollectCoverage=true

# Verbose output
dotnet test --verbosity detailed
```

### Docker Commands
```bash
# Start Valhalla for integration tests
docker-compose -f docker-compose.integration.yml up -d

# Check status
docker-compose -f docker-compose.integration.yml ps

# View logs
docker-compose -f docker-compose.integration.yml logs -f valhalla

# Stop and clean up
docker-compose -f docker-compose.integration.yml down

# Clean up including volumes
docker-compose -f docker-compose.integration.yml down -v
```

### Build Commands
```bash
# Build project
dotnet build

# Build with warnings as errors
dotnet build /p:TreatWarningsAsErrors=true

# Clean build artifacts
dotnet clean
```

---

## Expected Test Structure Summary

### For Unit Tests
1. Create class in `/test/Valhalla.Routing.Client.Tests/Unit/{Category}/`
2. Name class `{ClassUnderTest}Tests`
3. Use `MockHttpMessageHandler` for HTTP mocking
4. Store JSON fixtures in `/test/Valhalla.Routing.Client.Tests/Fixtures/`
5. Use FluentAssertions for all assertions
6. No `[Trait]` attribute needed

### For Integration Tests
1. Create class in `/test/Valhalla.Routing.Client.Tests/Integration/`
2. Name class `{Feature}IntegrationTests`
3. Add `[Trait("Category", "Integration")]` to class
4. Implement `IAsyncLifetime` for setup/teardown
5. Use real HttpClient against `http://localhost:8002`
6. Use `LuxembourgTestLocations` for coordinates

---

## Test Coverage by Component

### Required Coverage Areas

| Component | Unit Tests | Integration Tests |
|-----------|------------|-------------------|
| **Route API** | Serialization, validation, error parsing | Real routing between Luxembourg locations |
| **Map Matching** | Request/response models | trace_route and trace_attributes calls |
| **Status API** | Response parsing (verbose/non-verbose) | Health check integration |
| **Locate API** | Multiple results handling | Finding locations |
| **Validation** | Coordinate bounds, required fields | N/A |
| **Error Handling** | Exception mapping, error codes | Timeout, cancellation |
| **Serialization** | snake_case conversion | N/A |

---

## Agent Workflow

### When Asked to Add Tests

1. **Identify scope:**
   - What functionality needs testing?
   - Unit or integration test?

2. **Check existing patterns:**
   - Look for similar tests in the same area
   - Reuse helper methods and patterns

3. **Create test file:**
   - Follow directory structure
   - Use appropriate namespace
   - Copy template from this document

4. **Write test:**
   - Use Arrange-Act-Assert pattern
   - Follow naming conventions
   - Use FluentAssertions

5. **Add fixtures if needed:**
   - Create JSON file in Fixtures directory
   - Use naming convention: `{endpoint}_{scenario}.json`

6. **Run and verify:**
   ```bash
   dotnet test --filter "FullyQualifiedName~{TestClassName}"
   ```

7. **Check coverage:**
   ```bash
   dotnet test /p:CollectCoverage=true
   ```

### When Asked to Fix Failing Tests

1. **Run test to see failure:**
   ```bash
   dotnet test --filter "FullyQualifiedName~{TestName}" --verbosity detailed
   ```

2. **Analyze error message:**
   - What assertion failed?
   - What was expected vs actual?

3. **Identify root cause:**
   - Production code bug?
   - Test assumption wrong?
   - Environment issue?

4. **Fix appropriately:**
   - Fix production code if it's a bug
   - Update test if assumption changed
   - Update environment if misconfigured

5. **Verify fix:**
   ```bash
   dotnet test --filter "FullyQualifiedName~{TestName}"
   ```

6. **Run full suite to avoid regressions:**
   ```bash
   dotnet test
   ```

---

## CI/CD Integration (GitHub Actions)

### Running Integration Tests in CI

**Yes, integration tests can and should run in GitHub Actions.**

### Key Points

1. **Tile Caching:** Use GitHub Actions cache to persist Valhalla tiles
   - First run: 3-5 minutes (builds tiles)
   - Subsequent runs: 1-2 minutes (tiles cached)
   - Cache key: `valhalla-tiles-luxembourg-v1`

2. **Two-Stage Pipeline:**
   - Stage 1: Unit tests (fast, runs on .NET 6.0 and 8.0 matrix)
   - Stage 2: Integration tests (only if unit tests pass, runs on .NET 8.0 only)

3. **Docker Volume Caching Strategy:**
   ```bash
   # Before tests: Restore cache to Docker volume
   docker run --rm -v ~/.valhalla-tiles:/source -v volume_name:/dest alpine cp -r /source/. /dest/
   
   # After tests: Save Docker volume to cache
   docker run --rm -v volume_name:/source -v ~/.valhalla-tiles:/dest alpine cp -r /source/. /dest/
   ```

### Complete Workflow Template

```yaml
name: Build and Test

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

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
      - run: dotnet restore
      - run: dotnet build --no-restore --configuration Release /p:TreatWarningsAsErrors=true
      - run: dotnet test --no-build --configuration Release --filter "Category!=Integration"

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
      
      # Cache tiles to avoid rebuilding on every run
      - name: Cache Valhalla tiles
        uses: actions/cache@v4
        with:
          path: ~/.valhalla-tiles
          key: valhalla-tiles-luxembourg-v1
          restore-keys: valhalla-tiles-luxembourg-
      
      # Restore cached tiles to Docker volume
      - name: Prepare tile cache volume
        run: |
          mkdir -p ~/.valhalla-tiles
          if [ -d ~/.valhalla-tiles/valhalla_tiles ]; then
            docker volume create valhalla-routing-client-dotnet_valhalla_tiles
            docker run --rm -v ~/.valhalla-tiles:/source -v valhalla-routing-client-dotnet_valhalla_tiles:/dest alpine cp -r /source/. /dest/
          fi
      
      - name: Start Valhalla container
        run: docker-compose -f docker-compose.integration.yml up -d
      
      - name: Wait for Valhalla to be ready
        run: timeout 300 bash -c 'until curl -sf http://localhost:8002/status; do sleep 5; done'
      
      # Save tiles for next run
      - name: Save tiles to cache
        run: docker run --rm -v valhalla-routing-client-dotnet_valhalla_tiles:/source -v ~/.valhalla-tiles:/dest alpine cp -r /source/. /dest/
      
      - run: dotnet restore
      - run: dotnet build --no-restore --configuration Release
      - run: dotnet test --no-build --configuration Release --filter "Category=Integration"
      
      - name: Stop Valhalla container
        if: always()
        run: docker-compose -f docker-compose.integration.yml down
```

### CI Best Practices

✅ **DO:**
- Cache Valhalla tiles using GitHub Actions cache
- Run integration tests only after unit tests pass
- Use timeout for Valhalla startup (300 seconds)
- Run integration tests on single .NET version (8.0)
- Use matrix for unit tests across .NET versions

❌ **DON'T:**
- Skip tile caching (every run will be slow)
- Run integration tests in matrix (unnecessary, wastes CI time)
- Use short timeouts (first run needs time for tile building)

---

## Related Documentation

- [Full Testing Guidelines (Human-Readable)](testing-guidelines.md)
- [Project Specification](specification/specification.md)
- [Contributing Guide](../CONTRIBUTING.md)
- [.NET Best Practices](dotnet-best-practices.md)

---

**Document Version:** 1.0  
**Last Updated:** 2026-02-08  
**Target Audience:** AI Agents (GitHub Copilot, Custom Agents)
