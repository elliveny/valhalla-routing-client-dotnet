# Valhalla .NET Client – Implementation Specification

## Executive Summary

**What:** A production-ready, open-source .NET client library for the Valhalla routing engine HTTP API.

**Why:** Enable .NET developers to integrate Valhalla routing capabilities (directions, map matching, road network queries) without dealing with HTTP/JSON plumbing.

**Scope:** Supports 5 core endpoints: route, trace_route, trace_attributes, status, and locate. Multi-target .NET 6.0/8.0.

**Status:** ✅ **Production Ready** - All core endpoints have been implemented and tested. The API surface is stable. Currently in pre-release (v0.x) with stable API that may have minor refinements before v1.0.

**Key Features:**
- 🔄 **Thread-safe** - Designed for concurrent usage in ASP.NET Core applications
- 💉 **Dependency Injection** - First-class support for DI with `IServiceCollection` extensions
- 🔧 **Builder Pattern** - Non-DI scenarios supported via fluent builder API
- 📝 **Comprehensive Documentation** - Extensive XML documentation for IntelliSense
- 🛡️ **Robust Error Handling** - Detailed exception types for different failure scenarios
- 🔒 **Security-Conscious** - API key redaction in logs, response size limits, DoS protection
- ⚡ **Modern .NET** - Leverages latest C# features and async/await patterns
- 🧪 **Well-Tested** - High test coverage with both unit and integration tests (78+ tests)

**Key Principles:**
- **Forward compatible:** Use `JsonElement` for flexibility as Valhalla API evolves
- **Production ready:** Thread-safe, DI-integrated, comprehensive error handling, security-conscious (DoS protection, API key redaction)
- **Developer friendly:** Clean interfaces, builder pattern for non-DI scenarios, extensive XML documentation
- **Test-driven:** 78+ tests (unit + integration) with Docker-based Valhalla instance

**Critical Implementation Points:**
- Byte-accurate response size limits (10MB) for DoS protection
- `JsonElement.Clone()` required for all `Raw` properties (memory safety)
- API keys applied per-request, never logged
- Distinguish timeout from cancellation in exception handling

---

## Development Philosophy & Approach

### AI-Driven Development Experiment

This project represents an experiment in **specification-driven development using AI tools**. The entire codebase—from design to implementation—was created through a collaborative process between AI assistants and human review.

**Development Timeline:**
- **Day 1:** Initial specification.md created using ChatGPT, then refined through multiple rounds of review using GitHub Copilot (Claude Opus 4.5 model)
- **Days 2-3:** Phased implementation with continuous agent and human review at each stage
- **Post-development:** Agent review validated specification against code, resulting in refinements to both

### This Specification is the Source of Truth

**This document serves as the definitive, authoritative source for all design and implementation decisions in this project.**

#### Why This Matters

1. **Single Source of Truth** - The specification defines what the code should do and how it should be structured
2. **Maintainability** - Future developers (and AI assistants) can understand the complete system from the specification
3. **Consistency** - Design decisions are documented and can be referenced during implementation
4. **Regenerability** - The codebase can theoretically be regenerated from the specification using AI tools
5. **Specification leads development** - Changes to requirements should update the specification first
6. **Code follows specification** - Implementation should be derived from the specification document
7. **Continuous evolution** - Both specification and code evolve together, with the specification maintaining the authoritative design

### AI-Assisted Workflow (Recommended)

This project encourages contributors to leverage AI tools for implementation:

**Recommended Workflow for Contributors:**

1. **Update Specification First**
   - Propose changes to this specification document before writing code
   - Ensure the specification clearly describes the intended behavior
   - Get specification changes reviewed and approved

2. **AI-Assisted Implementation**
   - Use AI coding assistants (GitHub Copilot, Cursor, ChatGPT, Claude, etc.) to generate implementation from the updated specification
   - Let AI tools handle boilerplate, tests, and documentation generation
   - Review AI-generated code carefully for correctness and quality

3. **Human Review**
   - Always review AI-generated changes with human judgment
   - Verify security, performance, and correctness
   - Ensure changes align with project standards

4. **Iterative Refinement**
   - Use AI tools to address review feedback
   - Refine both code and specification as needed
   - Maintain alignment between specification and implementation

**Important Notes:**
- Human-written code is perfectly acceptable and encouraged
- You are not required to use AI tools to contribute
- AI output must never be merged without thorough human review
- Consider how changes fit into the specification
- Documentation-first thinking is highly valued

💡 **For Contributors:** When proposing changes, consider updating this specification first. See [CONTRIBUTING.md](../../CONTRIBUTING.md) for details on the recommended workflow.

---

## Table of Contents

1. [Purpose](#1-purpose)
2. [Scope](#2-scope)
3. [Target Framework & Dependencies](#3-target-framework--dependencies)
4. [Public Open Source / NuGet Considerations](#4-public-open-source--nuget-considerations-important)
5. [Project Deliverables](#5-project-deliverables)
6. [High-Level Design](#6-high-level-design)
7. [Configuration](#7-configuration)
8. [HTTP Requirements](#8-http-requirements)
9. [Serialization / DTO Strategy](#9-serialization--dto-strategy)
10. [Error Handling](#10-error-handling)
11. [Logging](#11-logging)
12. [Endpoint Specifications](#12-endpoint-specifications)
    - [12.1 Route API](#121-route-api-route)
    - [12.2 Map Matching API](#122-map-matching-api-trace_route-trace_attributes)
    - [12.3 Status API](#123-status-api-status)
    - [12.4 Locate API](#124-locate-api-locate)
    - [12.5 Costing Models Reference](#125-costing-models-reference)
    - [12.6 Polyline Utilities](#126-polyline-utilities)
13. [Implementation Requirements](#13-implementation-requirements)
14. [Testing Requirements](#14-testing-requirements)
15. [CI Requirements](#15-ci-requirements-github-actions)
16. [Documentation Requirements](#16-documentation-requirements-readmemd)
17. [Definition of Done](#17-definition-of-done-acceptance-criteria)
18. [Recommended Repository Structure](#18-recommended-repository-structure)
19. [Notes for Future Phases](#19-notes-for-future-phases-non-mvp)
20. [Developer Notes / Priorities](#20-developer-notes--priorities)
21. [Contributor Workflow & Specification-First Development](#21-contributor-workflow--specification-first-development)
22. [Glossary](#22-glossary)
23. [Development Phases (TDD Approach)](#23-development-phases-tdd-approach)

---

## 1. Purpose

Build a reusable **.NET (C#) client library** that wraps the Valhalla routing engine HTTP API and provides a clean developer-friendly interface for the following endpoints:

- **Route** (`/route`)
- **Map Matching** (`/trace_route`, `/trace_attributes`)
- **Status** (`/status`)
- **Locate** (`/locate`)

The library MUST be suitable for:

- internal usage in production applications
- public open-source release (GitHub + NuGet)

**Target Valhalla Version:** This specification is designed for Valhalla API version **3.5 and later** (tested against 3.6.x). Earlier versions may work but are not officially supported.

Valhalla API documentation:  
https://valhalla.github.io/valhalla/api/

---

## 2. Scope

### In Scope
Implement support for:

- `route`
- `trace_route`
- `trace_attributes`
- `status`
- `locate`

### Out of Scope (for now)
The following APIs are explicitly excluded from the initial implementation:

- matrix
- isochrone
- optimized_route
- elevation/height
- centroid
- expansion/debug endpoints
- OSRM output format support
- GPX output format support
- protobuf output format support

---

## 3. Target Framework & Dependencies

### Target Framework
The library MUST target:
- `.NET 8.0` (primary)
- `.NET 6.0` (for LTS compatibility)

Use multi-targeting in the `.csproj` file to support both frameworks.

### Compiler Settings
The project MUST enforce:
- `<Nullable>enable</Nullable>` — Nullable reference types enabled
- `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` — All warnings treated as errors
- `<WarningLevel>latest</WarningLevel>` — Latest warning level for the target framework

### Required Dependencies
- `System.Text.Json` for serialization/deserialization (part of .NET runtime, no package needed)
- `HttpClient` + `IHttpClientFactory` (part of .NET runtime)
- `Microsoft.Extensions.Logging.Abstractions` for `ILogger<T>` interface
- `Microsoft.Extensions.Options` for `IOptions<T>` pattern

Note: Only the abstractions packages are required. Consumers provide their own logging and DI implementations (e.g., Microsoft.Extensions.Logging.Console in applications).

### Optional Dependencies (Deferred to Future Phase)
The following are explicitly out of scope for the initial implementation but planned for future versions:
- Polly for retry/backoff policies (see Section 8.5 Resilience)

---

## 4. Public Open Source / NuGet Considerations (Important)

This library is intended to be publishable as a **public GitHub repository** and **public NuGet package**, so the implementation should be structured accordingly.

### 4.1 Naming & Discoverability

The package/repository name must clearly communicate:
- this is for **routing**
- this is for **Valhalla**
- this is for **.NET / C#**

The name `Valhalla.Client` is functional but is **not recommended** for public discovery, because "Valhalla" is a highly overloaded term (games, blockchain projects, etc.) and does not strongly imply routing/navigation.

#### Recommended naming strategy
**GitHub repository name (recommended):**
- `valhalla-routing-client-dotnet`

**NuGet package name (recommended):**
- `Valhalla.Routing.Client`

**Assembly name (recommended):**
- `Valhalla.Routing.Client`

**Root namespace (recommended):**
- `Valhalla.Routing`

### 4.2 Explicit “Unofficial” Positioning
The README must clearly state this is an **unofficial** client library, not part of the core Valhalla project.

Suggested README wording:

> "Unofficial .NET client library for the Valhalla routing engine."

### 4.3 License
A license must be included on day one.

Recommended license:
- **MIT License** (matches Valhalla’s MIT licensing ecosystem and is standard for client libraries)

### 4.4 Versioning Strategy
Use **Semantic Versioning**:

- `0.x` during early development (DTOs may change)
- `1.0.0` once public API surface is stable

Breaking changes must increment major version.

### 4.5 API Drift & Backward Compatibility
Valhalla responses may evolve over time (new fields added). The library must be resilient to this.

Requirements:
- do not fail deserialization if extra fields appear
- avoid over-typing deeply nested response objects too early
- keep a raw JSON escape hatch for future compatibility

### 4.6 CI & Release Readiness
The repository MUST include:

- GitHub Actions workflow for build + test
- Docker-based integration tests using Valhalla container
- NuGet packaging metadata in `.csproj`

Publishing to NuGet may be added later, but the project should be structured as if it will happen.

### 4.7 NuGet Metadata for Search Ranking
The package should include metadata for discoverability:

- `PackageTags`: `valhalla`, `routing`, `navigation`, `maps`, `gis`, `directions`
- `RepositoryUrl`
- `PackageLicenseExpression`
- `PackageReadmeFile`

### 4.8 Installation & Package Distribution

#### Installation Command
The README and documentation should prominently feature the installation command:

```bash
dotnet add package Valhalla.Routing.Client
```

Or via Package Manager Console:
```
Install-Package Valhalla.Routing.Client
```

#### NuGet Package Badge
Include a NuGet version badge in the README for visibility:

```markdown
[![NuGet](https://img.shields.io/nuget/v/Valhalla.Routing.Client.svg)](https://www.nuget.org/packages/Valhalla.Routing.Client)
```

#### Pre-release Status Communication
The package is currently in **pre-release (v0.x)** phase:

- Version numbers follow `0.x.y` pattern during pre-release
- The API surface is stable but may have minor refinements before v1.0
- Breaking changes are possible but will be clearly communicated in release notes
- Users should be informed: "Pre-release: This library is in active development (0.x). The API is stable but may have minor changes before v1.0. Feedback welcome!"

Once the API is fully stable and field-tested:
- Version will be bumped to `1.0.0`
- Semantic versioning will be strictly followed for all future releases

#### Target Frameworks in Package
The NuGet package must support:
- **.NET 6.0** (netcoreapp6.0) - Long-term support
- **.NET 8.0** (netcoreapp8.0) - Current recommended version

This should be configured in the `.csproj` file:
```xml
<TargetFrameworks>net6.0;net8.0</TargetFrameworks>
```

#### Package Dependencies
The NuGet package will have minimal dependencies:
- **System.Text.Json** - Built into .NET runtime (no explicit package reference needed)
- **Microsoft.Extensions.Logging.Abstractions** - For ILogger<T> interface
- **Microsoft.Extensions.Options** - For IOptions<T> pattern
- **Microsoft.Extensions.DependencyInjection.Abstractions** - For service collection extensions

Note: These are *abstractions only*. Consumers provide their own implementations (e.g., Microsoft.Extensions.Logging.Console).

#### Package Icon and Metadata
Include in the `.csproj`:
```xml
<PackageIcon>icon.png</PackageIcon>
<PackageReadmeFile>README.md</PackageReadmeFile>
<PackageProjectUrl>https://github.com/elliveny/valhalla-routing-client-dotnet</PackageProjectUrl>
<RepositoryUrl>https://github.com/elliveny/valhalla-routing-client-dotnet</RepositoryUrl>
<RepositoryType>git</RepositoryType>
<PackageLicenseExpression>MIT</PackageLicenseExpression>
<PackageTags>valhalla;routing;navigation;maps;gis;directions</PackageTags>
<Description>A production-ready .NET client library for the Valhalla routing engine HTTP API. Supports route calculation, map matching, status checks, and location queries.</Description>
```

#### NuGet Publishing Process
The package should be published via CI/CD automation:

1. **Automated Publishing:**
   - Trigger on Git tags (e.g., `v0.1.0`, `v1.0.0`)
   - GitHub Actions workflow builds, tests, and packages the library
   - Workflow publishes to NuGet.org using API key stored in GitHub Secrets

2. **Manual Review Before Release:**
   - All tests must pass (unit + integration)
   - Code quality checks must pass (StyleCop, analyzers)
   - Release notes must be prepared in CHANGELOG.md
   - Version number must be updated in `.csproj`

3. **Release Checklist:**
   - [ ] Update version in `.csproj`
   - [ ] Update CHANGELOG.md with release notes
   - [ ] Ensure all tests pass
   - [ ] Ensure no compiler warnings
   - [ ] Create and push Git tag
   - [ ] Verify GitHub Actions workflow completes successfully
   - [ ] Verify package appears on NuGet.org
   - [ ] Test installation in a clean project

#### Quick Start Documentation
The README must include a clear "Quick Start" section showing:
1. Installation command
2. Basic usage with Dependency Injection (ASP.NET Core)
3. Basic usage without Dependency Injection (using builder pattern)

This ensures users can get started immediately after installing the package.

---

## 5. Project Deliverables

### 5.1 Projects
The solution must include:

1. `Valhalla.Routing.Client` (class library)
2. `Valhalla.Routing.Client.Tests` (unit tests + integration tests)
3. `Valhalla.Routing.Client.Samples` (optional but strongly recommended)

### 5.2 Documentation
Required documentation files:
- `README.md` - Project overview, installation, quick start, and usage examples
- `CHANGELOG.md` (optional but recommended) - Version history and release notes
- `LICENSE` - MIT License
- This specification document (optional to include publicly)

### 5.3 NuGet Package Deliverable
The project must produce a NuGet package suitable for public distribution:

**Package Output:**
- `Valhalla.Routing.Client.{version}.nupkg`
- Includes compiled assemblies for .NET 6.0 and .NET 8.0
- Includes XML documentation files for IntelliSense
- Includes package icon (icon.png)
- Includes README.md as package readme

**Package Metadata Requirements:**
All metadata must be configured in `.csproj` (see Section 4.8 for complete details):
- Package version
- Package description
- Package tags for discoverability
- License expression (MIT)
- Project and repository URLs
- Package icon and readme file references

**Distribution:**
- Published to NuGet.org
- Available via `dotnet add package Valhalla.Routing.Client`
- Searchable by tags: valhalla, routing, navigation, maps, gis, directions

---

## 6. High-Level Design

### 6.1 Primary Client Class
A single entry point client:

```csharp
public interface IValhallaClient
{
    Task<RouteResponse> RouteAsync(RouteRequest request, CancellationToken cancellationToken = default);
    Task<TraceRouteResponse> TraceRouteAsync(TraceRouteRequest request, CancellationToken cancellationToken = default);
    Task<TraceAttributesResponse> TraceAttributesAsync(TraceAttributesRequest request, CancellationToken cancellationToken = default);
    Task<StatusResponse> StatusAsync(StatusRequest? request = null, CancellationToken cancellationToken = default);
    Task<LocateResponse> LocateAsync(LocateRequest request, CancellationToken cancellationToken = default);
}
```

Implementation:
- `ValhallaClient : IValhallaClient`

### 6.2 Lifecycle Management
**`ValhallaClient` MUST NOT implement `IDisposable`** when used with `IHttpClientFactory`.

Rationale:
- `IHttpClientFactory` manages `HttpClient` lifetime and connection pooling.
- Disposing the client would interfere with connection reuse.
- The factory handles DNS changes and socket exhaustion automatically.

**Warning in documentation:** If consumers create `ValhallaClient` manually without `IHttpClientFactory`, they are responsible for `HttpClient` lifecycle. This pattern is NOT recommended.

### 6.3 Thread Safety
`ValhallaClient` instances MUST be thread-safe for concurrent use.

Requirements:
- All public methods MUST be safe to call from multiple threads simultaneously.
- No mutable shared state within the client instance.
- Thread safety is guaranteed when using DI with `IHttpClientFactory` (singleton-safe).

### 6.4 Constructor Signature

```csharp
public class ValhallaClient : IValhallaClient
{
    private readonly HttpClient _httpClient;
    private readonly ValhallaClientOptions _options;
    private readonly ILogger<ValhallaClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ValhallaClient(
        HttpClient httpClient,
        IOptions<ValhallaClientOptions> options,
        ILogger<ValhallaClient> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options?.Value);
        ArgumentNullException.ThrowIfNull(logger);

        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        
        _httpClient.BaseAddress ??= _options.BaseUri;
        _httpClient.Timeout = _options.Timeout;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            Converters = { new EpochSecondsConverter() }
        };
    }
}
```

### 6.5 Non-DI Usage: ValhallaClientBuilder

For console applications, scripts, or scenarios where dependency injection is not available, use `ValhallaClientBuilder`:

```csharp
using Valhalla.Routing.Builder;

var client = new ValhallaClientBuilder()
    .WithBaseUri("https://valhalla.example.com")
    .WithTimeout(TimeSpan.FromSeconds(30))
    .Build();

var response = await client.StatusAsync();
```

#### Builder API

```csharp
namespace Valhalla.Routing.Builder;

public class ValhallaClientBuilder
{
    /// <summary>
    /// Sets the Valhalla server base URI. Required.
    /// </summary>
    public ValhallaClientBuilder WithBaseUri(Uri baseUri);
    
    /// <summary>
    /// Sets the Valhalla server base URI from a string. Required.
    /// </summary>
    public ValhallaClientBuilder WithBaseUri(string baseUri);
    
    /// <summary>
    /// Sets the request timeout. Default: 15 seconds.
    /// </summary>
    public ValhallaClientBuilder WithTimeout(TimeSpan timeout);
    
    /// <summary>
    /// Configures API key authentication header.
    /// </summary>
    public ValhallaClientBuilder WithApiKey(string headerName, string headerValue);
    
    /// <summary>
    /// Provides a custom logger. Default: NullLogger.
    /// </summary>
    public ValhallaClientBuilder WithLogger(ILogger<ValhallaClient> logger);
    
    /// <summary>
    /// Provides a custom HttpClient instance.
    /// WARNING: Caller is responsible for HttpClient lifecycle management.
    /// A warning will be logged when Build() is called.
    /// </summary>
    public ValhallaClientBuilder WithHttpClient(HttpClient httpClient);
    
    /// <summary>
    /// Builds the ValhallaClient instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if BaseUri is not set.</exception>
    public IValhallaClient Build();
}
```

#### Implementation Requirements

- `Build()` MUST throw `InvalidOperationException` if `WithBaseUri()` was not called.
- `Build()` MUST log a warning at `LogLevel.Warning` if `WithHttpClient()` was used:
  > "Custom HttpClient provided. Caller is responsible for HttpClient lifecycle, connection pooling, and DNS change handling."
- If no logger is provided, use `NullLogger<ValhallaClient>.Instance`.
- If no `HttpClient` is provided, the builder creates one internally.
- `BaseUri` MUST be normalized (see Section 7.1 for normalization rules).

#### When to Use DI vs Builder

| Scenario | Recommendation |
|----------|----------------|
| ASP.NET Core / Generic Host | Use `AddValhallaClient()` (Section 7.2) |
| Console app, one-off script | Use `ValhallaClientBuilder` |
| Azure Functions (isolated) | Use `AddValhallaClient()` |
| Unit tests | Mock `IValhallaClient` directly |

---

## 7. Configuration

### 7.1 Options Object
The client must be configurable via an options object.

```csharp
public class ValhallaClientOptions
{
    public required Uri BaseUri { get; init; }

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(15);

    public bool PreferPostRequests { get; init; } = true;

    // Optional auth header support
    public string? ApiKeyHeaderName { get; init; }
    public string? ApiKeyHeaderValue { get; init; }

    // If true, request/response bodies may be logged (default false)
    public bool EnableSensitiveLogging { get; init; } = false;
}
```

#### BaseUri Normalization

`BaseUri` MUST be normalized to ensure consistent path joining regardless of whether the provided URI ends with a trailing slash.

**Normalization rules:**
1. If `BaseUri` ends with `/`, strip the trailing slash before storing.
2. All endpoint paths (e.g., `/route`, `/status`) MUST start with `/`.
3. Path joining uses simple concatenation: `BaseUri + endpointPath`.

**Example:**
```csharp
// Both of these should work identically:
options.BaseUri = new Uri("https://valhalla.example.com");
options.BaseUri = new Uri("https://valhalla.example.com/");

// Internal normalization ensures:
// "https://valhalla.example.com" + "/route" = "https://valhalla.example.com/route"
```

**Implementation:**
```csharp
private static Uri NormalizeBaseUri(Uri uri)
{
    var uriString = uri.AbsoluteUri;
    return uriString.EndsWith('/') 
        ? new Uri(uriString.TrimEnd('/')) 
        : uri;
}
```

This normalization MUST be applied in both `ValhallaClient` constructor and `ValhallaClientBuilder.Build()`.

**API Key Header Application:** When `ApiKeyHeaderName` and `ApiKeyHeaderValue` are configured, the header MUST be applied per-request on each `HttpRequestMessage`, NOT on `HttpClient.DefaultRequestHeaders`. This ensures proper behavior with `IHttpClientFactory` where `HttpClient` instances may be shared.

```csharp
// Continuing from above
```

### 7.2 HttpClientFactory Support
The library MUST support DI registration via an extension method:

```csharp
public static class ValhallaClientServiceCollectionExtensions
{
    public static IHttpClientBuilder AddValhallaClient(
        this IServiceCollection services,
        Action<ValhallaClientOptions> configure)
    {
        services.Configure(configure);
        return services.AddHttpClient<IValhallaClient, ValhallaClient>();
    }
}
```

**Usage:**
```csharp
services.AddValhallaClient(options =>
{
    options.BaseUri = new Uri("https://valhalla.example.com");
});
```

---

## 8. HTTP Requirements

### 8.0 Transport Security
All production connections MUST use HTTPS (TLS 1.2 or higher). The library MUST log a warning if `BaseUri` uses HTTP when an API key is configured.

**Warning timing:** This warning MUST be logged once during `ValhallaClient` construction (not on every request). This ensures early visibility of misconfiguration and avoids log spam.

### 8.1 Default HTTP Behavior
- MUST default to `POST` with JSON body for all requests.
- `GET` with `?json=` query parameter is NOT supported in the initial implementation (POST handles all scenarios).

### 8.2 Headers
All requests MUST include:
- `Accept: application/json`

POST requests MUST include:
- `Content-Type: application/json; charset=utf-8`

Optional API key header MUST be included if configured.

**Security:** API keys MUST NEVER be logged, even when `EnableSensitiveLogging = true`. Use `[REDACTED]` placeholder in logs.

**Request payload size limit:** While Valhalla typically handles reasonably-sized routing requests, request payloads exceeding 5MB (5,242,880 bytes) SHOULD be considered exceptional. The client does not enforce a hard limit on request size, but requests with extremely large shape arrays or costing_options may fail or timeout. If you encounter issues with large requests, consider splitting into multiple smaller requests or contacting the Valhalla server administrator about limits.

### 8.3 Response Handling
HTTP responses MUST be handled according to this matrix:

| Status Code | Behavior |
|-------------|----------|
| `200-299` | Deserialize JSON response, return DTO |
| `400-499` | Throw `ValhallaException` with error details (client error) |
| `500-599` | Throw `ValhallaException` with error details (server error) |
| `429` | Throw `ValhallaException` with message indicating rate limiting |

**Response size limit:** Responses exceeding 10MB (10,485,760 bytes) MUST throw `ValhallaException` to prevent memory exhaustion (DoS protection).

**Size limit enforcement:**
1. Check `Content-Length` header first (if present). If it exceeds 10MB, throw `ValhallaException` immediately without reading the body.
2. For responses without `Content-Length` (chunked encoding), use `HttpCompletionOption.ResponseHeadersRead` and read the response stream with a **byte counter** (not character counter). Count actual bytes read from the stream using `Stream.ReadAsync()` and track the cumulative byte count. If the cumulative byte count exceeds 10MB, abort and throw `ValhallaException`.

**Important:** The limit is enforced on **raw HTTP response bytes** before any character decoding. This ensures accurate measurement regardless of UTF-8 multibyte characters. Do not use `StreamReader.ReadToEndAsync()` with character-based estimation.

### 8.4 Timeout and Cancellation Behavior
- `Timeout` from `ValhallaClientOptions` applies to each individual HTTP request.
- The `CancellationToken` passed to methods allows the caller to cancel the request.
- If the timeout is exceeded, throw `TimeoutException`.
- If the `CancellationToken` is cancelled, throw `OperationCanceledException`.
- Partial responses MUST be discarded on cancellation or timeout.

#### Timeout vs CancellationToken Precedence

When both `HttpClient.Timeout` and the caller's `CancellationToken` can cancel a request, **whichever triggers first wins**.

**Exception handling:**
- `HttpClient` throws `TaskCanceledException` for both timeout and cancellation.
- Distinguish using `CancellationToken.IsCancellationRequested`:

```csharp
try
{
    response = await _httpClient.SendAsync(request, cancellationToken);
}
catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
{
    // Caller requested cancellation
    throw new OperationCanceledException("Request was cancelled.", ex, cancellationToken);
}
catch (TaskCanceledException ex)
{
    // Timeout occurred (not caller cancellation)
    throw new TimeoutException($"Request to {request.RequestUri} timed out after {_options.Timeout}.", ex);
}
```

**Behavior summary:**
| Trigger | Exception Thrown | `IsCancellationRequested` |
|---------|------------------|---------------------------|
| `HttpClient.Timeout` | `TimeoutException` | `false` |
| Caller's `CancellationToken` | `OperationCanceledException` | `true` |

### 8.5 Resilience (Deferred)
Retry logic is explicitly NOT included in the initial implementation. Consumers requiring retry/backoff MUST wrap the client with Polly or similar library.

Future phase will add:
- Configurable retry policies
- Circuit breaker support
- Rate limiting awareness

---

## 9. Serialization / DTO Strategy

### 9.1 JSON Serializer
Use `System.Text.Json`.

Serializer settings must:
- ignore unknown properties
- support case-insensitive matching
- **use `snake_case` for all property names** (Valhalla API requirement)

**Required configuration:**

```csharp
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true
};
```

All C# properties use PascalCase but serialize to snake_case (e.g., `HeadingTolerance` → `heading_tolerance`).

### 9.2 Avoid Over-Modeling
Valhalla contains many optional and variant structures.

Rules:
- strongly type stable request fields (locations, costing, format, etc.)
- keep `costing_options` flexible using `JsonElement?`

`JsonElement` is required (not `Dictionary<string, object>`) to avoid type ambiguity and preserve JSON fidelity when round-tripping through the API.

### 9.3 Raw JSON Support
All response DTOs MUST include a raw payload:

```csharp
public JsonElement Raw { get; init; }
```

This ensures forward compatibility if response schemas evolve.

**Size limit:** Maximum response size is 10MB (responses exceeding this throw `ValhallaException`). The `Raw` property contains the complete response JSON. Since the response is already buffered for deserialization, no separate cap on `Raw` is needed.

#### JsonElement Lifetime and Clone Requirement

`JsonElement` is only valid while its parent `JsonDocument` is not disposed. Since response DTOs outlive the HTTP processing scope, the `Raw` property MUST store a **cloned** `JsonElement`.

**Implementation requirement:**
```csharp
// INCORRECT — JsonElement becomes invalid after JsonDocument disposal
using var doc = JsonDocument.Parse(responseBody);
return new RouteResponse { Raw = doc.RootElement }; // ❌ Invalid after 'using' scope

// CORRECT — Clone detaches from parent document
using var doc = JsonDocument.Parse(responseBody);
return new RouteResponse { Raw = doc.RootElement.Clone() }; // ✅ Safe to use
```

**Memory consideration:** `Clone()` creates an independent copy of the JSON data. For large responses (up to 10MB), this is acceptable given the response size limit. The cloned `JsonElement` does not require disposal.

### 9.4 Record vs Class Usage

DTOs MUST follow these conventions:

| DTO Type | Use | Rationale |
|----------|-----|-----------|
| Simple value objects | `record` | Immutability, value equality, concise syntax |
| Response containers with `Raw` | `class` | May need reference semantics, larger objects |
| Request objects | `class` | Mutable during construction, no equality comparison needed |

**Use `record` for:**
- `Location`
- `TracePoint`
- `BoundingBox`
- `SearchFilter`
- `TraceOptions`
- `DateTimeOptions`
- `FilterAttributes`

**Use `class` for:**
- `RouteRequest`, `TraceRouteRequest`, etc. (request DTOs)
- `RouteResponse`, `TraceRouteResponse`, etc. (response containers)
- `ValhallaClientOptions`

**Example record syntax:**
```csharp
public record Location
{
    public required double Lat { get; init; }
    public required double Lon { get; init; }
    public string? Type { get; init; }
    // ... other properties
}
```

### 9.5 Custom JSON Converters

#### EpochSecondsConverter
Used for `TracePoint.Time` to convert between `DateTimeOffset` and Unix epoch seconds:

```csharp
/// <summary>
/// Converts DateTimeOffset to/from Unix epoch seconds for Valhalla timestamp fields.
/// </summary>
public class EpochSecondsConverter : JsonConverter<DateTimeOffset?>
{
    public override DateTimeOffset? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out var epochSeconds))
            return DateTimeOffset.FromUnixTimeSeconds(epochSeconds);

        throw new JsonException($"Expected number for epoch seconds, got {reader.TokenType}");
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateTimeOffset? value,
        JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteNumberValue(value.Value.ToUnixTimeSeconds());
    }
}
```

---

## 10. Error Handling

### 10.1 Custom Exception Type
Create a custom exception:

```csharp
public class ValhallaException : Exception
{
    public HttpStatusCode HttpStatusCode { get; }
    public string? HttpStatus { get; }
    public int? ErrorCode { get; }
    public string? RawResponse { get; }

    public ValhallaException(
        HttpStatusCode httpStatusCode,
        string message,
        string? rawResponse = null,
        int? errorCode = null,
        string? httpStatus = null) : base(message)
    {
        HttpStatusCode = httpStatusCode;
        HttpStatus = httpStatus;
        RawResponse = rawResponse;
        ErrorCode = errorCode;
    }
}
```

### 10.2 Requirements
- capture raw body for debugging (truncate `RawResponse` to 8KB maximum)
- include Valhalla error payload if present
- NEVER return `null` silently
- NEVER swallow exceptions
- throw `ArgumentException` for invalid input (e.g., fewer than 2 locations, null required fields)
- throw `ValhallaException` for all API errors
- throw `TimeoutException` when request timeout is exceeded
- throw `OperationCanceledException` when cancellation is requested

### 10.3 Error Response Format
Valhalla returns errors in the following JSON structure:

```json
{
  "error_code": 171,
  "error": "No suitable edges near location",
  "status_code": 400,
  "status": "Bad Request"
}
```

| Field | Type | Description |
|-------|------|-------------|
| `error_code` | int | Valhalla internal error code |
| `error` | string | Human-readable error message |
| `status_code` | int | HTTP status code |
| `status` | string | HTTP status text |

The library must parse this structure and populate `ValhallaException` accordingly.

### 10.4 Validation Timing

Input validation MUST occur **synchronously at method entry**, before any async operations begin. This ensures:
- Fail-fast behavior with clear stack traces
- No wasted network calls for invalid requests
- Consistent with .NET BCL patterns

**Implementation pattern:**
```csharp
public Task<RouteResponse> RouteAsync(RouteRequest request, CancellationToken cancellationToken = default)
{
    // Synchronous validation — throws immediately, no Task returned yet
    ArgumentNullException.ThrowIfNull(request);
    ValidateRouteRequest(request);  // Throws ArgumentException if invalid
    
    return RouteInternalAsync(request, cancellationToken);
}

private async Task<RouteResponse> RouteInternalAsync(RouteRequest request, CancellationToken cancellationToken)
{
    // Actual async HTTP work here
}

private static void ValidateRouteRequest(RouteRequest request)
{
    if (request.Locations is null || request.Locations.Count < 2)
        throw new ArgumentException("At least 2 locations are required.", nameof(request));
    
    if (string.IsNullOrWhiteSpace(request.Costing))
        throw new ArgumentException("Costing is required.", nameof(request));
    
    foreach (var location in request.Locations)
    {
        if (location.Lat < -90.0 || location.Lat > 90.0)
            throw new ArgumentOutOfRangeException(nameof(request), 
                $"Latitude must be between -90 and 90 (inclusive). Got: {location.Lat}");
        
        if (location.Lon < -180.0 || location.Lon > 180.0)
            throw new ArgumentOutOfRangeException(nameof(request), 
                $"Longitude must be between -180 and 180 (inclusive). Got: {location.Lon}");
    }
}
```

**Exception types:**
| Condition | Exception |
|-----------|-----------|
| Null required parameter | `ArgumentNullException` |
| Invalid value (out of range) | `ArgumentOutOfRangeException` |
| Invalid state (missing required fields, wrong combination) | `ArgumentException` |

### 10.5 Partial Success Responses

Some endpoints (notably `/locate`) may return HTTP 200 with valid results for some inputs and empty results for others. For example, when locating multiple points, one point might have no nearby edges while others do.

**Behavior:** Empty results within a successful HTTP 200 response are NOT errors. The client MUST:
- Return the response as-is without throwing
- Allow consumers to inspect individual results for empty data
- NOT treat empty `Edges` or `Nodes` arrays as exceptions

**Consumer responsibility:** Consumers MUST check individual result items (e.g., `result.Edges?.Count > 0`) and handle empty results according to their application logic.

---

## 11. Logging

### 11.1 Logging Integration
Support injection of:

```csharp
ILogger<ValhallaClient>
```

### 11.2 Logging Policy
- log request duration + endpoint
- do not log request/response bodies unless `EnableSensitiveLogging = true`
- warn on non-2xx responses

### 11.3 Log Levels

| Event | Level | Condition |
|-------|-------|--------|
| Request starting | `Debug` | Always |
| Request/response body | `Debug` | Only if `EnableSensitiveLogging = true` |
| Request completed successfully | `Information` | Always |
| Request duration (performance) | `Debug` | Always |
| Non-2xx response | `Warning` | Always |
| Timeout occurred | `Warning` | Always |
| Deserialization error | `Error` | Always |
| Transport security warning (HTTP with API key) | `Warning` | Once at construction |
| Custom HttpClient provided (via builder) | `Warning` | Once at Build() |

### 11.4 High-Performance Logging

For production scenarios, use `LoggerMessage.Define` to avoid allocations and improve performance:

```csharp
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
}
```

### 11.5 API Key Redaction

API keys MUST NEVER be logged, even when `EnableSensitiveLogging = true`.

**Implementation:**
```csharp
private string RedactApiKey(string headerValue)
{
    if (string.IsNullOrEmpty(headerValue) || headerValue.Length <= 4)
        return "[REDACTED]";
    
    // Show first 2 and last 2 characters only
    return $"{headerValue[..2]}...{headerValue[^2..]}";
}

// When logging headers (if EnableSensitiveLogging is true):
_logger.LogDebug("Request headers: Accept={Accept}, {ApiKeyHeader}={ApiKeyValue}",
    "application/json",
    _options.ApiKeyHeaderName ?? "(none)",
    _options.ApiKeyHeaderValue is not null ? "[REDACTED]" : "(none)");
```

**Sensitive logging scope:** When `EnableSensitiveLogging = true`, the following MAY be logged at `Debug` level:
- Request body JSON (excluding API keys)
- Response body JSON (truncated to 8KB)
- Request headers (API key values replaced with `[REDACTED]`)

---

## 12. Endpoint Specifications

---

### 12.1 Route API (`/route`)

## Common DTOs

### DateTimeType Enum
```csharp
public enum DateTimeType
{
    Current = 0,
    DepartAt = 1,
    ArriveBy = 2,
    Invariant = 3
}
```

### DateTimeOptions
```csharp
public record DateTimeOptions
{
    public required DateTimeType Type { get; init; }
    
    /// <summary>
    /// Date and time in ISO 8601 format: YYYY-MM-DDTHH:mm (e.g., "2026-02-07T14:30").
    /// Seconds are optional and typically ignored by Valhalla (minute precision).
    /// The time is interpreted in the local timezone of the departure or arrival location.
    /// Required when Type is DepartAt or ArriveBy.
    /// </summary>
    public string? Value { get; init; }
}
```

### SearchFilter
```csharp
public record SearchFilter
{
    public bool? ExcludeTunnel { get; init; }
    public bool? ExcludeBridge { get; init; }
    public bool? ExcludeRamp { get; init; }
    public bool? ExcludeClosures { get; init; }
    public bool? ExcludeToll { get; init; }
    public bool? ExcludeFerry { get; init; }
    public bool? ExcludeCashOnlyTolls { get; init; }
    public string? MinRoadClass { get; init; }
    public string? MaxRoadClass { get; init; }
}
```

**Valid road class values (ordered from highest to lowest):**
- `motorway`
- `trunk`
- `primary`
- `secondary`
- `tertiary`
- `unclassified`
- `residential`
- `service_other`

### BoundingBox
```csharp
public record BoundingBox
{
    public required double MinLon { get; init; }
    public required double MinLat { get; init; }
    public required double MaxLon { get; init; }
    public required double MaxLat { get; init; }
}
```

## Request DTOs

### RouteRequest
```csharp
public class RouteRequest
{
    public required IReadOnlyList<Location> Locations { get; init; }
    public required string Costing { get; init; }

    // Directions options (top-level, not nested)
    public string? Units { get; init; }           // "miles", "mi", "kilometers", "km"
    public string? Language { get; init; }        // IETF BCP 47 language tag
    public string? DirectionsType { get; init; }  // "none", "maneuvers", "instructions"
    public string? Format { get; init; }          // "json", "gpx", "osrm", "pbf"

    // Costing options
    public JsonElement? CostingOptions { get; init; }

    // Date/time for departure or arrival
    public DateTimeOptions? DateTime { get; init; }

    // Exclusions
    public IReadOnlyList<Location>? ExcludeLocations { get; init; }
    public JsonElement? ExcludePolygons { get; init; } // GeoJSON polygon(s)

    // Additional options
    public string? Id { get; init; }              // Request ID, echoed in response
    public int? Alternates { get; init; }         // Number of alternate routes
    public int? ElevationInterval { get; init; }  // Meters between elevation samples
    public bool? RoundaboutExits { get; init; }   // Include exit instructions (default true)
    public bool? LinearReferences { get; init; }  // Include OpenLR references
}
```

### Location
```csharp
/// <summary>
/// Represents a geographic location for routing or map matching.
/// 
/// Validation requirements vary by endpoint:
/// - Route: Heading (0-360°), HeadingTolerance (0-180°), Radius (≥ 0) are validated
/// - Locate: Only Lat/Lon are validated; other fields are optional without validation
/// - Common: Lat must be -90 to 90, Lon must be -180 to 180 (inclusive)
/// </summary>
public record Location
{
    public required double Lat { get; init; }
    public required double Lon { get; init; }

    // Location type: "break", "via", "through", "break_through"
    public string? Type { get; init; }
    public string? Name { get; init; }

    // Heading (validated for Route: 0-360°)
    public double? Heading { get; init; }          // 0-360 degrees from north
    public double? HeadingTolerance { get; init; } // Default 60 degrees (validated for Route: 0-180°)

    // Address fields (pass-through for narration)
    public string? Street { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }

    // Side of street positioning
    public double? DisplayLat { get; init; }
    public double? DisplayLon { get; init; }
    public string? PreferredSide { get; init; }   // "same", "opposite", "either"

    // Search options
    public double? Radius { get; init; }           // Search radius in meters (default 0)
    public int? MinimumReachability { get; init; } // Min reachable nodes (default 50)
    public bool? RankCandidates { get; init; }     // Rank edge candidates
    public double? SearchCutoff { get; init; }     // Max search distance (default 35000m)
    public double? NodeSnapTolerance { get; init; }    // Snap to intersection (default 5m)
    public double? StreetSideTolerance { get; init; }  // Side of street tolerance (default 5m)
    public SearchFilter? SearchFilter { get; init; }
    public int? PreferredLayer { get; init; }      // For multi-level routing
    public int? Waiting { get; init; }             // Waiting time at location in seconds
}
```

**Location types:**
| Type | U-turns | Generates legs/maneuvers |
|------|---------|-------------------------|
| `break` | Allowed | Yes |
| `through` | Not allowed | No |
| `via` | Allowed | No |
| `break_through` | Not allowed | Yes |

## Response DTOs

### RouteResponse
```csharp
public class RouteResponse
{
    public JsonElement Raw { get; init; }
    
    // Typed access to common fields
    public string? Id { get; init; }
    
    /// <summary>
    /// The list of trips. Contains one trip for a standard route request.
    /// When Alternates > 0 is specified, may contain multiple alternate routes.
    /// </summary>
    public IReadOnlyList<Trip>? Trips { get; init; }
}

public record Trip
{
    public IReadOnlyList<Leg>? Legs { get; init; }
    public TripSummary? Summary { get; init; }
    public string? Units { get; init; }
    public string? Language { get; init; }
    public IReadOnlyList<Location>? Locations { get; init; }
}

public record TripSummary
{
    public double? Length { get; init; }       // Distance in specified units
    public double? Time { get; init; }         // Duration in seconds
    public double? MinLat { get; init; }
    public double? MinLon { get; init; }
    public double? MaxLat { get; init; }
    public double? MaxLon { get; init; }
    public bool? HasTimeRestrictions { get; init; }
    public bool? HasToll { get; init; }
    public bool? HasHighway { get; init; }
    public bool? HasFerry { get; init; }
}

public record Leg
{
    public IReadOnlyList<Maneuver>? Maneuvers { get; init; }
    public LegSummary? Summary { get; init; }
    public string? Shape { get; init; }        // Encoded polyline
}

public record LegSummary
{
    public double? Length { get; init; }
    public double? Time { get; init; }
    public double? MinLat { get; init; }
    public double? MinLon { get; init; }
    public double? MaxLat { get; init; }
    public double? MaxLon { get; init; }
}

public record Maneuver
{
    public int? Type { get; init; }
    public string? Instruction { get; init; }
    public double? Length { get; init; }
    public double? Time { get; init; }
    public int? BeginShapeIndex { get; init; }
    public int? EndShapeIndex { get; init; }
    public IReadOnlyList<string>? StreetNames { get; init; }
}
```

### Sample Valhalla Route Response (for reference)

**Note:** Valhalla returns a singular `trip` object. The client MUST transform this into `IReadOnlyList<Trip>` for the `Trips` property. When `alternates` is requested and Valhalla returns multiple trips, all are included in the list. For standard requests, the list contains a single trip.

```json
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
```

### Sample Valhalla Route Response with Alternates

When `alternates` is requested (e.g., `"alternates": 2`), Valhalla returns the primary route in the root `trip` object and additional routes in an `alternates` array:

```json
{
  "trip": {
    "locations": [...],
    "legs": [...],
    "summary": { "length": 1.842, "time": 312, ... },
    "units": "kilometers",
    "language": "en-US"
  },
  "alternates": [
    {
      "trip": {
        "locations": [...],
        "legs": [...],
        "summary": { "length": 2.156, "time": 378, ... },
        "units": "kilometers",
        "language": "en-US"
      }
    },
    {
      "trip": {
        "locations": [...],
        "legs": [...],
        "summary": { "length": 2.521, "time": 425, ... },
        "units": "kilometers",
        "language": "en-US"
      }
    }
  ],
  "id": "my-request-id"
}
```

### Response Deserialization: `trip` to `Trips` Transformation

**Implementation Note:** The generic `SendAsync<T>` helper cannot be used directly for `/route` due to the required JSON transformation. The `RouteAsync` method MUST implement custom deserialization:

```csharp
private RouteResponse DeserializeRouteResponse(JsonElement root)
{
    var trips = new List<Trip>();
    
    // Primary trip (always present)
    if (root.TryGetProperty("trip", out var tripElement))
    {
        var primaryTrip = tripElement.Deserialize<Trip>(_jsonOptions);
        if (primaryTrip != null) trips.Add(primaryTrip);
    }
    
    // Alternate trips (present when alternates > 0 was requested)
    if (root.TryGetProperty("alternates", out var alternatesElement))
    {
        foreach (var alt in alternatesElement.EnumerateArray())
        {
            if (alt.TryGetProperty("trip", out var altTripElement))
            {
                var altTrip = altTripElement.Deserialize<Trip>(_jsonOptions);
                if (altTrip != null) trips.Add(altTrip);
            }
        }
    }
    
    return new RouteResponse
    {
        Raw = root.Clone(),  // MUST clone to detach from parent JsonDocument
        Id = root.TryGetProperty("id", out var id) ? id.GetString() : null,
        Trips = trips
    };
}
```

**Alternates limitations:**
- Alternates are only supported for 2-location routes (origin + destination)
- Alternates are not supported for time-dependent routes
- Valhalla may return fewer alternates than requested if suitable alternatives don't exist

**Validation of alternates constraint:**
The client does NOT enforce the "2-location only" constraint. If a user requests alternates with 3+ locations, the request is sent to Valhalla, which will ignore the alternates parameter and return only the primary route. This is intentional:
- Valhalla's behavior may evolve to support multi-waypoint alternates in the future
- The constraint is a Valhalla limitation, not a protocol violation
- Validation would add complexity without significant benefit

Developers should be aware of this limitation through documentation, but the client remains forward-compatible.

## Client Method

```csharp
Task<RouteResponse> RouteAsync(RouteRequest request, CancellationToken cancellationToken = default);
```

## Validation Rules
- At least 2 locations required.
- Costing required (see Section 12.5 for valid values).
- `Lat`/`Lon` must be valid range: -90 ≤ lat ≤ 90, -180 ≤ lon ≤ 180 (inclusive).
- `Heading` must be 0-360 degrees if provided.
- `HeadingTolerance` must be 0-180 degrees if provided.
- `Radius` must be ≥ 0 if provided.
- `DateTimeOptions.Type` must be 0-3 if provided.
- `DateTimeOptions.Value` required when Type is `DepartAt` or `ArriveBy`.
- `Alternates` must be ≥ 0 if provided.

---

### 12.2 Map Matching API (`/trace_route`, `/trace_attributes`)

## TraceOptions DTO

```csharp
public record TraceOptions
{
    /// <summary>
    /// Search radius for map matching in meters.
    /// Must be between 0 and 100 meters. Default: 40 meters.
    /// Per-point Radius on TracePoint overrides this global value.
    /// </summary>
    public double? SearchRadius { get; init; }       // Search radius in meters (0-100, default 40)
    
    public double? GpsAccuracy { get; init; }        // GPS accuracy in meters (default 5)
    public double? BreakageDistance { get; init; }   // Distance to break trace (default 2000m)
    public double? InterpolationDistance { get; init; } // Merge threshold in meters
}
```

> **Note:** `turn_penalty_factor` is a costing option, not a trace option. Pass it via `costing_options` if needed.

## TracePoint DTO

```csharp
public record TracePoint
{
    public required double Lat { get; init; }
    public required double Lon { get; init; }

    /// <summary>
    /// Optional type for trace points. Common values:
    /// - "break" - Force route break at this location (new leg)
    /// - "via" - Pass through this location without breaking
    /// - "through" - Pass through without stopping
    /// - "break_through" - Break but continue through
    /// If not specified, defaults to "via" behavior.
    /// Note: Type semantics for trace points may differ from route locations.
    /// </summary>
    public string? Type { get; init; }  // break, via, through, break_through
    
    /// <summary>
    /// Timestamp for temporal matching. Serializes to/from epoch seconds in JSON.
    /// </summary>
    [JsonConverter(typeof(EpochSecondsConverter))]
    public DateTimeOffset? Time { get; init; }
    
    public double? Radius { get; init; } // Per-point search radius in meters
}
```

> **Note:** Per-point `Radius` overrides the global `TraceOptions.SearchRadius` for that specific point.

---

## TraceRouteRequest and TraceAttributesRequest DTOs

**Design Note:** `TraceRouteRequest` and `TraceAttributesRequest` share approximately 60% of their properties. This duplication is **intentional** for the following reasons:
- Request DTOs remain flat and simple to construct
- `System.Text.Json` serialization works without polymorphic type configuration
- Each request type is self-contained and easy to understand
- Aligns directly with Valhalla's API structure

Shared validation logic (e.g., `Shape` vs `EncodedPolyline` mutual exclusivity) SHOULD be extracted into internal helper methods to avoid code duplication while keeping the DTOs flat.

## TraceRouteRequest DTO

```csharp
public class TraceRouteRequest
{
    public required string Costing { get; init; }

    // Shape input (provide one of Shape or EncodedPolyline)
    public IReadOnlyList<TracePoint>? Shape { get; init; }
    public string? EncodedPolyline { get; init; }  // Alternative to Shape array

    // Timestamps for encoded polyline
    public long? BeginTime { get; init; }           // Start timestamp (epoch seconds)
    public IReadOnlyList<int>? Durations { get; init; } // Delta times between points
    public bool? UseTimestamps { get; init; }       // Use input timestamps for elapsed time

    // Matching options
    public string? ShapeMatch { get; init; }        // "edge_walk", "map_snap", "walk_or_snap"
    public TraceOptions? TraceOptions { get; init; }
    public JsonElement? CostingOptions { get; init; }

    // Output options
    public string? Format { get; init; }
    public string? Units { get; init; }
    public string? Language { get; init; }
    public string? DirectionsType { get; init; }
    public bool? LinearReferences { get; init; }  // Include OpenLR references
    public string? Id { get; init; }
}
```

## TraceAttributesRequest DTO

```csharp
public class TraceAttributesRequest
{
    public required string Costing { get; init; }

    // Shape input (provide one of Shape or EncodedPolyline)
    public IReadOnlyList<TracePoint>? Shape { get; init; }
    public string? EncodedPolyline { get; init; }  // Alternative to Shape array

    // Timestamps for encoded polyline
    public long? BeginTime { get; init; }           // Start timestamp (epoch seconds)
    public IReadOnlyList<int>? Durations { get; init; } // Delta times between points
    public bool? UseTimestamps { get; init; }       // Use input timestamps for elapsed time

    // Matching options
    public string? ShapeMatch { get; init; }        // "edge_walk", "map_snap", "walk_or_snap"
    public TraceOptions? TraceOptions { get; init; }
    public JsonElement? CostingOptions { get; init; }

    // Attribute filtering
    public FilterAttributes? Filters { get; init; }
    public string? Id { get; init; }
}
```

### FilterAttributes
```csharp
public record FilterAttributes
{
    public IReadOnlyList<string>? Attributes { get; init; }
    public string? Action { get; init; }  // "include" or "exclude"
}
```

## Response DTOs

### TraceRouteResponse
```csharp
public class TraceRouteResponse
{
    public JsonElement Raw { get; init; }
    
    // Typed access to common fields
    public string? Id { get; init; }
    
    /// <summary>
    /// The matched trip. Unlike RouteResponse.Trips, trace_route always returns
    /// a single matched route (no alternates support for map matching).
    /// </summary>
    public Trip? Trip { get; init; }
}
```

### TraceAttributesResponse
```csharp
public class TraceAttributesResponse
{
    public JsonElement Raw { get; init; }
    
    // Typed access to common fields
    public string? Id { get; init; }
    public string? Units { get; init; }
    public IReadOnlyList<MatchedPoint>? MatchedPoints { get; init; }
    public IReadOnlyList<TraceEdge>? Edges { get; init; }
    public string? Shape { get; init; }  // Encoded polyline
}

public record MatchedPoint
{
    public double? Lat { get; init; }
    public double? Lon { get; init; }
    public string? Type { get; init; }           // "matched", "interpolated", "unmatched"
    public int? EdgeIndex { get; init; }         // Index into edges array
    public double? DistanceAlongEdge { get; init; }
    public double? DistanceFromTracePoint { get; init; }
}

public record TraceEdge
{
    public IReadOnlyList<string>? Names { get; init; }
    public double? Length { get; init; }         // In units specified
    public double? Speed { get; init; }          // km/h
    public string? RoadClass { get; init; }
    public int? BeginShapeIndex { get; init; }
    public int? EndShapeIndex { get; init; }
    public int? TrafficSegments { get; init; }
}
```

### Sample Valhalla trace_attributes Response (for reference)
```json
{
  "id": "my-trace-request",
  "matched_points": [
    {
      "lat": 52.5200,
      "lon": 13.4050,
      "type": "matched",
      "edge_index": 0,
      "distance_along_edge": 0.123,
      "distance_from_trace_point": 2.5
    }
  ],
  "edges": [
    {
      "names": ["Friedrichstraße"],
      "length": 0.543,
      "speed": 50,
      "road_class": "secondary",
      "begin_shape_index": 0,
      "end_shape_index": 12
    }
  ],
  "shape": "yzq~IcvxpA...",
  "units": "kilometers"
}
```

## Client Methods

```csharp
Task<TraceRouteResponse> TraceRouteAsync(TraceRouteRequest request, CancellationToken cancellationToken = default);

Task<TraceAttributesResponse> TraceAttributesAsync(TraceAttributesRequest request, CancellationToken cancellationToken = default);
```

## Validation Rules
- `Shape` or `EncodedPolyline` must be provided (exactly one, not both).
  - If both are provided, throw `ArgumentException`.
  - If neither is provided, throw `ArgumentException`.
- `Shape` must contain at least 2 points if provided.
- `TracePoint.Lat`/`Lon` must be valid range: -90 ≤ lat ≤ 90, -180 ≤ lon ≤ 180 (inclusive).
- `Costing` is required (see Section 12.5 for valid values).
- `FilterAttributes.Action` must be `"include"` or `"exclude"` if provided.
- `TraceOptions.SearchRadius` must be ≥ 0 and ≤ 100 meters.

## Notes
Map matching payloads can be large; POST must be supported as the default.

---

### 12.3 Status API (`/status`)

## StatusRequest DTO

```csharp
/// <summary>
/// Request options for the /status endpoint.
/// Uses record (not class) since it's a simple, immutable value type with a single property.
/// </summary>
public record StatusRequest
{
    public bool Verbose { get; init; } = false;
}
```

## StatusResponse DTO
Prefer typed fields where stable, plus raw.

```csharp
public class StatusResponse
{
    public JsonElement Raw { get; init; }

    public string? Version { get; init; }
    public long? TilesetLastModified { get; init; }  // UNIX timestamp
    public bool? HasTiles { get; init; }             // Valid tileset loaded
    public bool? HasAdmins { get; init; }            // Built with admin database
    public bool? HasTimezones { get; init; }         // Built with timezone database
    public bool? HasLiveTraffic { get; init; }       // Live traffic available
    public JsonElement? Bbox { get; init; }          // GeoJSON tileset extent
}
```

## Client Method
```csharp
Task<StatusResponse> StatusAsync(StatusRequest? request = null, CancellationToken cancellationToken = default);
```

## Implementation Guidance
When implementing `StatusAsync`, handle the nullable `StatusRequest` parameter as follows:

```csharp
public async Task<StatusResponse> StatusAsync(StatusRequest? request = null, CancellationToken cancellationToken = default)
{
    // When request is null, use a default instance to avoid serialization issues
    var requestBody = request ?? new StatusRequest { Verbose = false };
    
    // No validation required for StatusRequest (all fields are optional)
    
    return await SendAsync<StatusResponse>("/status", requestBody, cancellationToken);
}
```

**Rationale:** `JsonSerializer.Serialize(null, ...)` produces `"null"` (JSON null literal), not `{}`. By using a default instance, we ensure a valid JSON object is sent. Since Valhalla treats missing `verbose` field as `false`, this is semantically equivalent to omitting the request entirely.

## Validation Rules
None. `StatusRequest` is optional and has no required fields. The `Verbose` property defaults to `false` if not specified.

**Handling null/omitted StatusRequest:**
- When `request` is `null`, the client creates a default `StatusRequest` with `Verbose = false`.
- When `request` is provided with explicit values, serialize normally.
- The serializer's `DefaultIgnoreCondition.WhenWritingNull` will omit null fields, but `false` is a value and will be included as `{"verbose": false}`.
- Valhalla treats missing `verbose` field as `false` by default, so both behaviors are equivalent.

---

### 12.4 Locate API (`/locate`)

## LocateRequest DTO

**Note:** The implementation uses `record` instead of `class` for this type, which provides value-based equality semantics.

```csharp
public record LocateRequest
{
    public required IReadOnlyList<Location> Locations { get; init; }
    public required string Costing { get; init; }

    public JsonElement? CostingOptions { get; init; }
    public bool? Verbose { get; init; }   // Include detailed edge info (default false)
    public string? Units { get; init; }   // "miles", "mi", "kilometers", "km"
    public string? Id { get; init; }      // Request ID, echoed in response
}
```

## LocateResponse DTO
```csharp
public class LocateResponse
{
    public IReadOnlyList<LocateResult>? Results { get; init; }
    public JsonElement Raw { get; init; }
    public IReadOnlyList<string>? Warnings { get; init; }  // API warnings (forward compatibility)
    public string? Id { get; init; }
}

public class LocateResult
{
    public double? InputLat { get; init; }   // Original input latitude (echoed from request)
    public double? InputLon { get; init; }   // Original input longitude (echoed from request)
    public IReadOnlyList<EdgeCandidate>? Edges { get; init; }
    public IReadOnlyList<NodeCandidate>? Nodes { get; init; }
    public IReadOnlyList<string>? Warnings { get; init; }  // Per-location warnings (forward compatibility)
}

public class EdgeCandidate
{
    public long? WayId { get; init; }                // OpenStreetMap way ID
    public double? CorrelatedLat { get; init; }
    public double? CorrelatedLon { get; init; }
    public string? SideOfStreet { get; init; }       // "left", "right", "neither" (matches Valhalla API field name)
    public double? PercentAlong { get; init; }       // 0.0 to 1.0 along edge
    public double? Distance { get; init; }           // Distance from input point in meters (verbose mode)
    public EdgeInfo? EdgeInfo { get; init; }         // Detailed edge info (verbose mode)
}

public class EdgeInfo
{
    public IReadOnlyList<string>? Names { get; init; }
    public string? RoadClass { get; init; }
    public int? Speed { get; init; }                 // Speed limit km/h
    public string? Use { get; init; }                // Road use type
    public double? Length { get; init; }             // Edge length in kilometers
    public bool? Bridge { get; init; }               // Edge is a bridge
    public bool? Tunnel { get; init; }               // Edge is a tunnel
    public bool? Toll { get; init; }                 // Edge is a toll road
}

public class NodeCandidate
{
    public double? Lat { get; init; }
    public double? Lon { get; init; }
    public double? Distance { get; init; }           // Distance from input point
}
```

### Sample Valhalla locate Response (for reference)
**Note:** Actual API returns `side_of_street` (not `side`), and includes `way_id`, plus optional `bridge`, `tunnel`, `toll` fields.
```json
[
  {
    "input_lat": 52.5200,
    "input_lon": 13.4050,
    "edges": [
      {
        "way_id": 12345678,
        "correlated_lat": 52.5201,
        "correlated_lon": 13.4052,
        "side_of_street": "right",
        "percent_along": 0.35,
        "distance": 2.5,
        "edge_info": {
          "names": ["Friedrichstraße"],
          "road_class": "secondary",
          "speed": 50,
          "use": "road",
          "length": 0.543,
          "bridge": false,
          "tunnel": false,
          "toll": false
        }
      }
    ],
    "nodes": []
  }
]
```
```

## Client Method
```csharp
Task<LocateResponse> LocateAsync(LocateRequest request, CancellationToken cancellationToken = default);
```

**Implementation Note:** Valhalla's `/locate` endpoint returns a JSON array at the root level (not a wrapped object). The client MUST deserialize this array and wrap it into `LocateResponse { Results = deserializedArray }` to maintain consistency with other response types and preserve the `Raw` property.

## Validation Rules
- At least 1 location required (unlike Route which requires 2+ locations).
- Costing required (see Section 12.5 for valid values).
- `Lat`/`Lon` must be valid range: -90 ≤ lat ≤ 90, -180 ≤ lon ≤ 180 (inclusive).

---

### 12.5 Costing Models Reference

The following costing models are available in Valhalla:

| Model | Description | Notes |
|-------|-------------|-------|
| `auto` | Standard automobile routing | Default for cars |
| `bicycle` | Bicycle routing | Surface and hill preferences |
| `bus` | Bus routing | Inherits from auto, uses bus lanes |
| `truck` | Truck routing | Weight/height/width restrictions |
| `taxi` | Taxi routing | Access to taxi lanes |
| `motor_scooter` | Scooter/moped routing | Speed and road restrictions |
| `motorcycle` | Motorcycle routing | **BETA** |
| `pedestrian` | Walking routes | Sidewalk preferences |
| `multimodal` | Pedestrian + transit | Public transit integration |
| `bikeshare` | Pedestrian + shared bikes | **BETA** |

**Deprecated:** `hov` — Use `auto` with HOV costing options instead.

Each costing model accepts specific options via `costing_options`. See Valhalla documentation for model-specific parameters.

**Validation:** Costing values are NOT validated client-side. Invalid costing values will be passed through to the Valhalla API, which will return an appropriate error. This avoids version coupling—newer Valhalla versions may support additional costing models.

### Costing Constants (Developer Experience)
To avoid magic strings, provide a constants class:

```csharp
public static class CostingModel
{
    public const string Auto = "auto";
    public const string Bicycle = "bicycle";
    public const string Bus = "bus";
    public const string Truck = "truck";
    public const string Taxi = "taxi";
    public const string MotorScooter = "motor_scooter";
    public const string Motorcycle = "motorcycle";
    public const string Pedestrian = "pedestrian";
    public const string Multimodal = "multimodal";
    public const string Bikeshare = "bikeshare";
}
```

**Usage:**
```csharp
var request = new RouteRequest
{
    Locations = locations,
    Costing = CostingModel.Auto
};
```

#### Design Decision: String Constants vs Enum

**Decision:** Use `const string` values in a static class rather than an `enum`.

**Rationale:**
1. **Forward compatibility** — Valhalla adds new costing models without major version bumps (e.g., `bikeshare` is marked BETA). String constants allow consumers to use new models immediately without waiting for a library update.
2. **Custom costing support** — Organizations running modified Valhalla builds with custom costing models can pass their model names directly (e.g., `"my_custom_costing"`).
3. **API alignment** — The Valhalla API expects string values; using strings avoids enum-to-string conversion overhead.
4. **IntelliSense discoverability** — The `CostingModel` class provides autocomplete suggestions while keeping the API open for extension.

**Trade-off acknowledged:** Enums provide compile-time safety and prevent typos. However, forward compatibility is prioritized for a client library that wraps an actively-developed API.

**Alternative pattern for strict validation (consumer choice):**
```csharp
// Consumers who want compile-time safety can define their own enum:
public enum MyCostingModel { Auto, Bicycle, Pedestrian }

// And use a mapping method:
public static string ToValhallaCosting(this MyCostingModel model) => model switch
{
    MyCostingModel.Auto => CostingModel.Auto,
    MyCostingModel.Bicycle => CostingModel.Bicycle,
    MyCostingModel.Pedestrian => CostingModel.Pedestrian,
    _ => throw new ArgumentOutOfRangeException(nameof(model))
};
```

---

### 12.6 Polyline Utilities

The library MUST include utilities for encoding and decoding polylines, as this is essential for working with route shapes and map matching.

## Polyline Algorithm
Valhalla uses Google's Polyline Algorithm with **precision 6** (6 decimal places for coordinates).

## PolylineEncoder Static Class

**Note:** Implementation uses lowercase parameter names `(double latitude, double longitude)` following C# naming conventions, not `(double Lat, double Lon)`.

```csharp
public static class PolylineEncoder
{
    /// <summary>
    /// Encodes a list of coordinates into a polyline string.
    /// </summary>
    /// <param name="coordinates">List of (latitude, longitude) tuples.</param>
    /// <param name="precision">Decimal precision (default 6 for Valhalla).</param>
    /// <returns>Encoded polyline string.</returns>
    public static string Encode(IEnumerable<(double latitude, double longitude)> coordinates, int precision = 6);

    /// <summary>
    /// Decodes a polyline string into a list of coordinates.
    /// </summary>
    /// <param name="encodedPolyline">The encoded polyline string.</param>
    /// <param name="precision">Decimal precision (default 6 for Valhalla).</param>
    /// <returns>List of (latitude, longitude) tuples.</returns>
    public static IReadOnlyList<(double latitude, double longitude)> Decode(string encodedPolyline, int precision = 6);
}
```

## Usage Examples

**Decoding a route shape from response:**
```csharp
var response = await client.RouteAsync(request);
var shape = response.Raw.GetProperty("trip").GetProperty("legs")[0].GetProperty("shape").GetString();
var coordinates = PolylineEncoder.Decode(shape);
```

**Encoding GPS points for map matching:**
```csharp
var gpsPoints = new List<(double latitude, double longitude)>
{
    (52.5200, 13.4050),
    (52.5205, 13.4060),
    (52.5210, 13.4070)
};

var request = new TraceRouteRequest
{
    Costing = CostingModel.Auto,
    EncodedPolyline = PolylineEncoder.Encode(gpsPoints)
};
```

## Implementation Notes
- The algorithm multiplies coordinates by 10^precision, rounds to integers, then encodes deltas as ASCII.
- Decoding reverses this process.
- Reference implementation: https://developers.google.com/maps/documentation/utilities/polylinealgorithm

---

## 13. Implementation Requirements

### 13.1 Internal Request Helpers

The `ValhallaClient` implementation MUST include two internal helper methods for making HTTP requests:

**1. SendAsync<T>** - For standard object responses:

```csharp
/// <summary>
/// Sends a request and deserializes the response as type T.
/// Used for endpoints that return JSON objects at root level.
/// </summary>
private async Task<T> SendAsync<T>(
    string path,
    object requestBody,
    CancellationToken cancellationToken);
```

**2. SendRawAsync** - For non-standard responses (arrays, custom deserialization):

```csharp
/// <summary>
/// Sends a request and returns the raw JSON response string.
/// Used for endpoints with non-standard response structures (e.g., array at root).
/// Shares HTTP/error handling logic while allowing custom deserialization.
/// </summary>
private async Task<string> SendRawAsync(
    string path,
    object requestBody,
    CancellationToken cancellationToken);
```

**Relationship:** Both methods share the same core HTTP handling logic (headers, API keys, size limits, error handling). `SendAsync<T>` includes generic deserialization, while `SendRawAsync` returns raw JSON for endpoints that need custom deserialization (like `/route` with alternates transformation, or `/locate` with array root).

**Shared responsibilities (both methods MUST implement):**
1. Serialize requestBody to JSON
2. Create HTTP POST request with proper headers (`Accept: application/json`, `Content-Type: application/json; charset=utf-8`)
3. Apply API key header per-request (if configured)
4. Check `Content-Length` header and reject if > 10MB
5. Read response body with byte counting (for chunked responses without Content-Length)
6. Throw `ValhallaException` for non-2xx status codes with error details
7. Throw `TimeoutException` or `OperationCanceledException` appropriately
8. Log request timing and errors

**Difference:** `SendAsync<T>` additionally deserializes the response JSON to type T and attaches the cloned raw JSON. `SendRawAsync` returns the raw JSON string for custom deserialization logic.

#### 13.1.1 Reference Implementation for SendAsync<T>

```csharp
/// <summary>
/// Maximum allowed response size in bytes (10 MB).
/// This limit prevents memory exhaustion from unexpectedly large responses.
/// </summary>
private const int MaxResponseSizeBytes = 10 * 1024 * 1024; // 10MB

private async Task<T> SendAsync<T>(string path, object requestBody, CancellationToken cancellationToken)
{
    var json = JsonSerializer.Serialize(requestBody, _jsonOptions);
    
    using var request = new HttpRequestMessage(HttpMethod.Post, path)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };
    
    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    
    // Apply API key per-request (NOT on DefaultRequestHeaders)
    if (!string.IsNullOrEmpty(_options.ApiKeyHeaderName) && 
        !string.IsNullOrEmpty(_options.ApiKeyHeaderValue))
    {
        request.Headers.TryAddWithoutValidation(
            _options.ApiKeyHeaderName, 
            _options.ApiKeyHeaderValue);
    }
    
    using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    
    // Enforce response size limit
    if (response.Content.Headers.ContentLength > MaxResponseSizeBytes)
    {
        throw new ValhallaException(
            response.StatusCode,
            $"Response size {response.Content.Headers.ContentLength} bytes exceeds maximum allowed {MaxResponseSizeBytes} bytes.");
    }
    
    var responseBody = await ReadResponseWithLimitAsync(response, cancellationToken);
    
    // Handle errors, deserialize, attach Raw...
}

private async Task<string> ReadResponseWithLimitAsync(
    HttpResponseMessage response,
    CancellationToken cancellationToken)
{
    // For chunked responses without Content-Length, read with BYTE counting
    await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
    
    // Read bytes directly to accurately enforce size limit
    using var memoryStream = new MemoryStream();
    var buffer = new byte[8192];
    int bytesRead;
    long totalBytesRead = 0;
    
    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
    {
        totalBytesRead += bytesRead;
        
        if (totalBytesRead > MaxResponseSizeBytes)
        {
            throw new ValhallaException(
                response.StatusCode,
                $"Response size exceeds maximum allowed {MaxResponseSizeBytes} bytes.");
        }
        
        await memoryStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
    }
    
    // Convert to string (UTF-8 decoding happens after size check)
    memoryStream.Position = 0;
    using var reader = new StreamReader(memoryStream, Encoding.UTF8);
    return await reader.ReadToEndAsync();
}
```

#### 13.1.2 Special Handling for Array Responses

The `/locate` endpoint returns a JSON array at the root level rather than an object. Since `SendAsync<T>` assumes object responses, `LocateAsync` MUST implement custom deserialization.

**Implementation approach:** Use the internal `SendRawAsync` helper (defined below) to get the raw JSON string, then deserialize it as an array:

```csharp
public async Task<LocateResponse> LocateAsync(LocateRequest request, CancellationToken cancellationToken = default)
{
    ArgumentNullException.ThrowIfNull(request);
    ValidateLocateRequest(request);
    
    var responseJson = await SendRawAsync("/locate", request, cancellationToken);
    
    // Parse as JsonDocument to clone the root element for Raw property
    using var jsonDocument = JsonDocument.Parse(responseJson);
    
    // Deserialize the array into typed results
    var results = JsonSerializer.Deserialize<List<LocateResult>>(responseJson, _jsonOptions);
    
    return new LocateResponse
    {
        Raw = jsonDocument.RootElement.Clone(),  // Clone before JsonDocument is disposed
        Results = results
    };
}
```

**Critical:** The `using` statement ensures `jsonDocument` is disposed after cloning. The cloned `JsonElement` is independent and safe to return in the response DTO.

**Required internal helper:** Add this private method to `ValhallaClient`:

```csharp
/// <summary>
/// Sends a request and returns the raw JSON response string.
/// Used for endpoints with non-standard response structures (e.g., array at root).
/// Shares HTTP/error handling logic while allowing custom deserialization.
/// </summary>
private async Task<string> SendRawAsync(string path, object requestBody, CancellationToken cancellationToken)
{
    // Implementation similar to SendAsync<T> but returns raw JSON string instead of deserializing
    // (Apply same validation, size limits, error handling, logging)
}
```

This allows endpoints with non-standard response structures to reuse HTTP/error handling while performing custom deserialization.

### 13.2 JSON Payload Format
Valhalla expects a JSON structure at the root, e.g.:

```json
{
  "locations": [ ... ],
  "costing": "auto"
}
```

The library must ensure correct casing and structure.

---

## 14. Testing Requirements

### 14.1 Unit Tests

#### Test Framework
The project MUST use:
- **xUnit** as the test framework
- **FluentAssertions** for readable assertions
- **Moq** or **NSubstitute** for mocking (developer preference)

```xml
<!-- Test project dependencies -->
<PackageReference Include="xunit" Version="2.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
<PackageReference Include="FluentAssertions" Version="6.*" />
<PackageReference Include="Moq" Version="4.*" />
<PackageReference Include="RichardSzalay.MockHttp" Version="7.*" />
```

#### Required Unit Tests
Required unit tests:
- request serialization tests (verify snake_case output)
- response deserialization tests using stored JSON fixtures
- error response parsing tests
- validation tests (invalid coordinates, missing required fields)
- exception behavior tests

**Mock strategy:** Unit tests MUST use mocked `HttpMessageHandler` to isolate from network. Recommended approach:

```csharp
// Use a custom mock handler or library like RichardSzalay.MockHttp
var mockHandler = new MockHttpMessageHandler();
mockHandler.When("/route").Respond("application/json", fixureJson);

var httpClient = new HttpClient(mockHandler) { BaseAddress = baseUri };
var client = new ValhallaClient(httpClient, options);
```

**Fixtures location:** JSON fixtures MUST be stored in `/test/Valhalla.Routing.Client.Tests/Fixtures/` with naming convention: `{endpoint}_{scenario}.json`.

**Naming conventions:**
- Use lowercase with underscores for separators (snake_case)
- Format: `{endpoint}_{scenario}.json`
- Examples:
  - `route_success.json` — Successful route response
  - `route_error_no_route.json` — Error when no route found
  - `route_with_alternates.json` — Route with multiple alternates
  - `trace_route_success.json` — Successful map matching
  - `status_verbose.json` — Status response with verbose=true
  - `locate_multiple_results.json` — Locate with multiple results

This convention ensures consistency and makes test fixtures easy to discover and understand.

### 14.2 Integration Tests (Required)
Integration tests MUST run against a real Valhalla instance.

#### Docker Compose Configuration

Create `docker-compose.integration.yml` in the repository root:

```yaml
version: '3.8'
services:
  valhalla:
    image: ghcr.io/valhalla/valhalla:run-latest
    ports:
      - "8002:8002"
    volumes:
      - valhalla-tiles:/custom_files  # Named volume for tile persistence
    environment:
      - tile_urls=https://download.geofabrik.de/europe/luxembourg-latest.osm.pbf
      - serve_tiles=True
      - build_admins=True
      - build_time_zones=True
      - force_rebuild=False  # Skip rebuild if tiles already exist
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8002/status"]
      interval: 10s
      timeout: 5s
      retries: 30
      start_period: 180s  # Allow time for first-run tile building

volumes:
  valhalla-tiles:  # Persists between docker-compose up/down
```

#### Tile Caching Behavior

The container uses lazy initialization with persistent volume caching:

1. **First run:** Downloads Luxembourg OSM extract (~35MB), builds tiles (~2-3 minutes), stores in volume.
2. **Subsequent runs:** Detects existing tiles, starts serving immediately (~10 seconds).
3. **Force rebuild:** Set `force_rebuild=True` to regenerate tiles (e.g., after OSM data update).

**Local development tip:** The named volume persists across `docker-compose down`. Use `docker volume rm valhalla-routing-client-dotnet_valhalla-tiles` to force a fresh tile build.

#### Test Tileset
Tests use the **Luxembourg OSM extract** because:
- Small download size (~35MB)
- Fast tile building (~2-3 minutes)
- Contains diverse road types (motorways, urban streets, bicycle paths)

#### Test Coordinates (Luxembourg)
All integration tests MUST use coordinates within Luxembourg:

```csharp
public static class TestLocations
{
    // Luxembourg City center
    public static readonly Location LuxembourgCity = new() { Lat = 49.6116, Lon = 6.1319 };
    
    // Kirchberg (EU institutions)
    public static readonly Location Kirchberg = new() { Lat = 49.6283, Lon = 6.1617 };
    
    // Esch-sur-Alzette
    public static readonly Location Esch = new() { Lat = 49.4958, Lon = 5.9806 };
    
    // Findel Airport
    public static readonly Location Airport = new() { Lat = 49.6233, Lon = 6.2044 };
}
```

#### Running Integration Tests

**Locally:**
```bash
docker-compose -f docker-compose.integration.yml up -d
dotnet test --filter "Category=Integration"
docker-compose -f docker-compose.integration.yml down
```

**CI pipeline:** See Section 15 for GitHub Actions configuration.

#### Integration Test Categories
Tests MUST be categorized using xUnit traits:

```csharp
[Trait("Category", "Integration")]
public class RouteIntegrationTests
{
    // ...
}
```

#### Required Test Coverage
Integration tests MUST cover:
- `/route` — Successful route between two points
- `/route` — Route with waypoints (3+ locations)
- `/route` — Alternate routes request
- `/route` — Coordinates outside tileset (error scenario)
- `/trace_route` — Match GPS trace to roads
- `/trace_attributes` — Get edge attributes for trace
- `/status` — Verify server is running (verbose and non-verbose)
- `/locate` — Find nearest edges to location
- Error scenarios (coordinates outside tileset)

### 14.3 Negative Test Cases
Tests MUST verify exception behavior for:
- Invalid coordinates (out of range `Lat`/`Lon`)
- Missing required fields (null locations, empty costing)
- API errors (no route found, location not found)
- Network errors (timeout, connection refused)
- Malformed JSON responses

### 14.4 Consumer Testing Guidance

The `IValhallaClient` interface is designed to be easily mockable for consumers writing their own unit tests.

**Recommended approach:**
```csharp
// Using Moq
var mockClient = new Mock<IValhallaClient>();
mockClient
    .Setup(c => c.RouteAsync(It.IsAny<RouteRequest>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new RouteResponse { /* ... */ });

var serviceUnderTest = new MyNavigationService(mockClient.Object);
```

**Recommended mocking libraries:**
- Moq
- NSubstitute
- FakeItEasy

The library does NOT provide a fake implementation. Consumers should mock the interface directly.

---

## 15. CI Requirements (GitHub Actions)

A GitHub Actions workflow MUST be included with the following requirements:

- Build and test against both **.NET 6.0** and **.NET 8.0**
- **Unit tests run first** for fast feedback on failures
- **Integration tests run after unit tests pass** (via docker-compose)
- All unit tests MUST pass on every pull request
- All integration tests MUST pass on every pull request
- Build MUST succeed with zero warnings (treat warnings as errors)

Optional later enhancement:
- Publish NuGet package on tag/release

### 15.1 Workflow File: `.github/workflows/build.yml`

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
      
      - name: Cache Valhalla tiles
        uses: actions/cache@v4
        with:
          path: ~/.valhalla-tiles
          key: valhalla-tiles-luxembourg-v1
          restore-keys: |
            valhalla-tiles-luxembourg-
      
      - name: Prepare tile cache volume
        run: |
          mkdir -p ~/.valhalla-tiles
          # Create docker volume from cache if tiles exist
          if [ -d ~/.valhalla-tiles/valhalla_tiles ]; then
            echo "Restoring tiles from cache..."
            docker volume create valhalla-routing-client-dotnet_valhalla-tiles
            docker run --rm -v ~/.valhalla-tiles:/source -v valhalla-routing-client-dotnet_valhalla-tiles:/dest alpine cp -r /source/. /dest/
          fi
      
      - name: Start Valhalla container
        run: docker-compose -f docker-compose.integration.yml up -d
      
      - name: Wait for Valhalla to be ready
        run: |
          echo "Waiting for Valhalla to build tiles and start..."
          timeout 300 bash -c 'until curl -sf http://localhost:8002/status; do sleep 5; done'
          echo "Valhalla is ready!"
      
      - name: Save tiles to cache
        run: |
          # Copy tiles from docker volume to cache directory
          docker run --rm -v valhalla-routing-client-dotnet_valhalla-tiles:/source -v ~/.valhalla-tiles:/dest alpine cp -r /source/. /dest/
      
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

---

## 16. Documentation Requirements (README.md)

The README must include:

### Basic Information
- Short description of the library
- **Explicit statement that it is unofficial** - "This is an unofficial client library and is not affiliated with or endorsed by the Valhalla project"
- Current project status: "✅ Production Ready - All core endpoints have been implemented and tested. The API surface is stable."
- Pre-release version notice: "Pre-release: This library is in active development (0.x). The API is stable but may have minor changes before v1.0. Feedback welcome!"

### Installation & Quick Start
- **NuGet installation instructions** - Prominent display of `dotnet add package Valhalla.Routing.Client` command
- NuGet badge with version and link: `[![NuGet](https://img.shields.io/nuget/v/Valhalla.Routing.Client.svg)](https://www.nuget.org/packages/Valhalla.Routing.Client)`
- Target frameworks (.NET 6.0, .NET 8.0)
- Quick start code examples:
  - With Dependency Injection (ASP.NET Core)
  - Without Dependency Injection (using builder pattern)

### Supported Endpoints
List with checkmarks:
- ✅ **Route** (`/route`) - Calculate optimal routes between locations
- ✅ **Map Matching** (`/trace_route`, `/trace_attributes`) - Match GPS traces to road networks
- ✅ **Status** (`/status`) - Check service health and version
- ✅ **Locate** (`/locate`) - Find nearest roads to a location

### Features Highlight
Emphasize key features:
- 🔄 Thread-safe
- 💉 Dependency Injection support
- 🔧 Builder Pattern for non-DI scenarios
- 📝 Comprehensive Documentation
- 🛡️ Robust Error Handling
- 🔒 Security-Conscious (API key redaction, DoS protection)
- ⚡ Modern .NET (async/await)
- 🧪 Well-Tested (unit + integration tests)

### Usage Examples
Include code examples for:
- Route calculation
- TraceRoute (GPS trace matching)
- TraceAttributes (edge attributes extraction)
- Status check
- Locate (nearest road lookup)
- Configuration options

### About This Project: AI-Driven Development

**Critical:** The README must clearly communicate the project's unique development approach:

#### Section: "AI-Driven Development Experiment"
Include a prominent section explaining:

1. **What makes this project unique:**
   - Entire codebase created through AI-assisted development (ChatGPT, GitHub Copilot with Claude Opus 4.5)
   - Specification-driven approach where specification.md is the definitive source of truth
   - Experimental approach to demonstrate AI-assisted coding at scale

2. **Development Philosophy:**
   - Specification leads development - changes should update specification.md first
   - Code follows specification - implementation derived from specification
   - Continuous evolution - both spec and code evolve together
   - AI-assisted workflow encouraged - leverage AI tools to translate spec to code

3. **For Contributors:**
   - Direct contributors to CONTRIBUTING.md for detailed workflow
   - Emphasize: "When proposing changes, consider updating specification.md first"
   - Note that AI tool usage is encouraged but human review is essential
   - Link to specification.md as the authoritative design document

**Example wording from current README:**
```markdown
### AI-Driven Development Experiment

This project represents an experiment in **specification-driven development using AI tools**. 
The entire codebase—from design to implementation—was created through a collaborative process 
between AI assistants and human review.

**Key Philosophy:**

The [specification.md](docs/specification/specification.md) file serves as the **definitive 
source of truth** for this project. Changes to requirements should update the specification 
first, and implementation should be derived from the specification document.

💡 **For Contributors:** When proposing changes, consider updating specification.md first. 
See our [contribution guidelines](CONTRIBUTING.md) for details on the recommended workflow.
```

### Documentation Links
- Link to [specification.md](docs/specification/specification.md) - Emphasize this is the definitive source of truth
- Link to [CONTRIBUTING.md](CONTRIBUTING.md) - Contribution guidelines with AI-assisted workflow
- Link to other documentation in /docs directory
- Link to Valhalla API documentation: https://valhalla.github.io/valhalla/api/

### Development & Contributing
- Development setup instructions
- Build and test commands
- Link to CONTRIBUTING.md with emphasis on specification-first workflow
- Code quality standards
- License information (MIT)

---

## 17. Definition of Done (Acceptance Criteria)

The project is considered complete when ALL of the following criteria are met:

### Functional Requirements
- A consumer can install the NuGet package and call all supported methods successfully.
- The client supports: route, trace_route, trace_attributes, status, and locate.
- All methods accept and respect `CancellationToken`.

### Error Handling
- All API errors throw `ValhallaException` with:
  - HTTP status code
  - Valhalla error code (if available)
  - Human-readable error message
  - Raw response body (truncated to 8KB)
- Invalid input throws `ArgumentException` with descriptive message.
- Timeout throws `TimeoutException`.
- Cancellation throws `OperationCanceledException`.

### Quality Metrics
- ≥80% code coverage for unit tests.
- All public types and methods have XML documentation comments.
- Zero compiler warnings (treat warnings as errors).
- All unit tests pass.
- All integration tests pass against a dockerized Valhalla instance.

### Deliverables
- Repository includes: `LICENSE` (MIT), `README.md`, `CHANGELOG.md`.
- CI pipeline builds and tests on every push.
- NuGet package metadata is complete and ready for publishing.

---

## 18. Recommended Repository Structure

```
/src
  /Valhalla.Routing.Client
    ValhallaClient.cs
    IValhallaClient.cs
    ValhallaClientOptions.cs
    ValhallaClientServiceCollectionExtensions.cs
    Builder/
      ValhallaClientBuilder.cs
    Exceptions/
      ValhallaException.cs
    Logging/
      Log.cs                   # LoggerMessage.Define definitions
    Models/
      Common/
        Location.cs
        BoundingBox.cs
        CostingModel.cs          # Constants class
      Route/
        RouteRequest.cs
        RouteResponse.cs
        Trip.cs
        Leg.cs
        Maneuver.cs
      MapMatching/
        TraceRouteRequest.cs
        TraceRouteResponse.cs
        TraceAttributesRequest.cs
        TraceAttributesResponse.cs
        TracePoint.cs
        TraceOptions.cs
      Status/
        StatusRequest.cs
        StatusResponse.cs
      Locate/
        LocateRequest.cs
        LocateResponse.cs
    Utilities/
      PolylineEncoder.cs
/test
  /Valhalla.Routing.Client.Tests
    Unit/
    Integration/
    Fixtures/                    # JSON test fixtures
      route_success.json
      route_error_no_route.json
      trace_attributes_success.json
      ...
/samples
  /Valhalla.Routing.Client.Samples
    Program.cs                   # Main entry point with menu
    Samples/
      BasicRoutingSample.cs      # Route between two locations
      MultiStopRouteSample.cs    # Route with multiple waypoints
      GpsTraceMatchingSample.cs  # Match GPS trace via TraceRouteAsync
      TraceAttributesSample.cs   # Get road attributes for a trace
      ServerHealthCheckSample.cs # Verify connectivity via StatusAsync
      NearestRoadSample.cs       # Find nearest edges via LocateAsync
Valhalla.Routing.Client.sln
README.md
LICENSE
CHANGELOG.md
docker-compose.integration.yml
.github/workflows/build.yml
```

---

## 19. Notes for Future Phases

Potential next-phase additions:

- Matrix API
- Isochrone API
- Strongly typed response models (beyond current implementation coverage)
- OSRM-compatible response formatting
- Rate limiting / retry policies
- Advanced costing option builders

---

## 20. Developer Notes / Priorities

This section provides guidance on what matters most and where to focus attention during implementation.

### Critical Success Factors

**1. JSON Payload Correctness**
- Valhalla is strict about payload structure. Test all DTOs against real Valhalla instances early.
- Pay special attention to snake_case serialization - it's non-negotiable.
- Use the integration tests to validate payloads before moving to the next endpoint.

**2. Robust Error Handling**
- Valhalla error responses vary in structure. Test with deliberately invalid requests (bad coordinates, unsupported costing, no route found).
- Ensure `ValhallaException` captures enough context for debugging without exposing sensitive data.
- Response size limits are a security feature - implement them correctly with actual byte counting.

**3. Thread Safety & DI Integration**
- The client will be used in production ASP.NET Core apps. Thread safety is non-negotiable.
- Test the DI registration against concurrent requests to ensure no state corruption.
- Avoid static mutable state - it's a common source of threading bugs.

### Areas Requiring Special Attention

**Response Size Enforcement (Security)**
- This is a DoS protection mechanism. The implementation MUST count actual bytes, not estimate from characters.
- Test with responses that approach and exceed the 10MB limit.
- See Section 8.3 for the corrected byte counting implementation.

**JsonElement Cloning (Memory Correctness)**
- `JsonElement` lifetime is tied to its parent `JsonDocument`. Failing to clone causes use-after-free bugs.
- Every response DTO's `Raw` property MUST use `.Clone()` - no exceptions.
- This is easy to get wrong and hard to debug in production.

**CancellationToken vs Timeout Handling**
- `TaskCanceledException` can come from either the timeout or caller cancellation.
- The implementation MUST distinguish these using `CancellationToken.IsCancellationRequested`.
- See Section 8.4 for the correct pattern.

**API Key Security**
- Never log API keys, even in debug/verbose modes. Use `[REDACTED]`.
- Apply API keys per-request on `HttpRequestMessage`, not on `HttpClient.DefaultRequestHeaders`.
- Test with EnableSensitiveLogging=true to ensure redaction works.

### Avoid Early Complexity

**Don't Over-Model Responses**
- Valhalla's response structures are deep and have many optional fields.
- Model the top-level stable fields (location, distance, time) but keep nested structures as `JsonElement`.
- Use the `Raw` property for forward compatibility - Valhalla adds fields over time.

**Don't Over-Model Costing Options**
- Each costing model (auto, bicycle, pedestrian) has different options.
- Use `JsonElement?` for `CostingOptions` - don't create 10 different option classes.
- Document common options in XML docs, but let consumers use raw JSON for full flexibility.

**Don't Add Features Not in Scope**
- No retry logic (consumers use Polly)
- No caching (consumers add their own)
- No rate limiting (Valhalla handles this)
- Keep the client focused on HTTP communication and serialization.

### Testing Strategy

**Integration Tests Are Critical**
- Unit tests for validation and serialization are good, but integration tests prove correctness.
- Use the Docker-based Valhalla instance with real tile data (Luxembourg sample).
- Test error scenarios (invalid requests) against the real server to see actual error responses.

**Test Coverage Priorities**
1. All validation rules (required fields, range checks)
2. Successful responses for each endpoint
3. Error responses (4xx, 5xx, no route found)
4. Size limits (response exceeds 10MB)
5. Cancellation and timeout behavior
6. Concurrent requests (thread safety)

### Common Pitfalls to Avoid

❌ **Don't use character counting for response size limits** - UTF-8 makes this unreliable  
✅ **Do use byte counting on the raw stream**

❌ **Don't forget to clone JsonElement** - causes memory corruption  
✅ **Do use .Clone() for all Raw properties**

❌ **Don't apply API keys to DefaultRequestHeaders** - breaks with IHttpClientFactory  
✅ **Do apply per-request on HttpRequestMessage**

❌ **Don't throw generic exceptions** - hard to distinguish errors  
✅ **Do use specific exception types (ValhallaException, ArgumentException, TimeoutException)**

❌ **Don't log API keys** - security violation  
✅ **Do redact them with [REDACTED]**

### Implementation Order

Follow the TDD phases in Section 22 - they're ordered by complexity and dependency:
1. Status (simplest, validates connectivity)
2. Locate (introduces validation)
3. Route (most complex, handles alternates)
4. Map Matching (similar to Route)
5. Utilities (polyline encoding)
6. DI/Builder (integration layer)

This order allows you to build confidence incrementally and reuse patterns learned from earlier endpoints.

---

## 21. Contributor Workflow & Specification-First Development

### Overview

This project follows a **specification-first development workflow** where this document serves as the authoritative source for all design and implementation decisions.

### When to Update This Specification

Contributors should update this specification document in the following scenarios:

**Always Update Specification:**
- Adding new endpoints or features
- Changing public API surface (interfaces, method signatures)
- Modifying request/response models
- Changing configuration options or behavior
- Adding new dependencies or requirements
- Updating error handling patterns
- Modifying security or performance requirements

**May Not Need Specification Update:**
- Fixing bugs that don't change documented behavior
- Adding tests for existing functionality
- Refactoring internal implementation without changing public API
- Updating documentation that doesn't affect design decisions
- Adding samples or examples

**If Unsure:** Update the specification. It's better to document design decisions even if they seem minor.

### Recommended Contribution Workflow

#### Step 1: Review Current Specification
- Read this specification document completely before proposing changes
- Understand the current design and implementation details
- Identify which sections your change affects

#### Step 2: Propose Specification Update
- Fork the repository and create a feature branch
- Update the relevant sections of this specification document
- Be specific about what changes and why
- Include examples if adding new features
- Ensure changes align with existing design principles

#### Step 3: Get Specification Review
- Submit a pull request with specification changes
- Request review from maintainers
- Discuss design decisions before implementation
- Iterate on specification until approved

#### Step 4: Implement Changes (AI-Assisted)
- Once specification is approved, begin implementation
- **AI tools encouraged:** Use GitHub Copilot, Cursor, ChatGPT, Claude, or other AI assistants to:
  - Generate code from specification
  - Create unit and integration tests
  - Generate XML documentation
  - Handle boilerplate and repetitive tasks
- Follow the patterns established in existing code
- Reference this specification throughout implementation

#### Step 5: Human Review & Refinement
- **Critical:** Always review AI-generated code with human judgment
- Verify security considerations (API key handling, DoS protection, input validation)
- Verify performance implications
- Verify correctness against specification
- Ensure code follows project standards (see docs/dotnet-best-practices.md)
- Run all tests (unit + integration)
- Ensure zero compiler warnings

#### Step 6: Submit Implementation PR
- Submit pull request with implementation
- Reference the specification sections that guided implementation
- Include test results and coverage information
- Link to the earlier specification PR if applicable

#### Step 7: Address Review Feedback
- Use AI tools to help address feedback efficiently
- Update both code and specification if design needs refinement
- Maintain alignment between specification and implementation
- Re-run tests after changes

### Benefits of This Workflow

**For Contributors:**
- Clear guidance on what to build
- Reduced back-and-forth during code review
- Leverage AI tools effectively with clear specifications
- Learn project design patterns before coding

**For Maintainers:**
- Design discussions happen before implementation work begins
- Easier to review code when specification exists
- Maintain consistency across contributions
- Ensure documentation stays current

**For the Project:**
- Specification remains the single source of truth
- Design decisions are documented and traceable
- Codebase can be regenerated or refactored from specification
- New contributors can understand design quickly

### AI-Assisted Development Best Practices

When using AI tools for implementation:

**Do:**
- ✅ Start with clear specifications
- ✅ Use AI to generate boilerplate and tests
- ✅ Review all AI-generated code carefully
- ✅ Verify security and correctness
- ✅ Use AI to help address code review feedback
- ✅ Maintain human oversight at all times

**Don't:**
- ❌ Accept AI output without review
- ❌ Skip testing AI-generated code
- ❌ Let AI make design decisions that contradict the specification
- ❌ Merge AI code that you don't fully understand
- ❌ Use AI to rush through security-critical code

### Questions and Support

For questions about:
- **Design decisions:** Open a GitHub Discussion before submitting specification changes
- **Implementation details:** Refer to this specification first, then ask in Discussions
- **Bugs:** Report via GitHub Issues
- **Contributing process:** See CONTRIBUTING.md for complete guidelines

### Reference Documents

Contributors should be familiar with:
- **This Specification** (specification.md) - Authoritative design document
- **CONTRIBUTING.md** - Contribution guidelines and code of conduct
- **.NET Best Practices** (docs/dotnet-best-practices.md) - Coding standards
- **Testing Guidelines** (docs/testing-guidelines.md) - Unit and integration test patterns
- **Interface Design Template** (docs/interface-design-template.md) - Interface patterns
- **Quick Reference** (docs/quick-reference.md) - Common patterns cheat sheet

---

## 22. Glossary

| Term | Definition |
|------|------------|
| **Costing** | The routing profile/mode (e.g., auto, bicycle, pedestrian) that determines how routes are calculated |
| **Edge** | A segment of road in the Valhalla graph; the basic unit of the road network |
| **Leg** | A portion of a route between two break locations |
| **Location** | A point specified by latitude/longitude, used as origin, destination, or waypoint |
| **Maneuver** | A navigation instruction (e.g., "turn left", "continue straight") |
| **Shape** | An array of coordinates representing a path (for map matching input) |
| **Trace** | GPS points to be matched to the road network |
| **Trip** | The complete route response containing legs, maneuvers, and summary |
| **Encoded Polyline** | A compressed string representation of a shape using the polyline algorithm |
| **Map Matching** | The process of aligning GPS traces to the road network |

---

## 23. Development Phases (TDD Approach)

This section defines the recommended development sequence using Test-Driven Development. Each phase specifies required tests that serve as acceptance criteria. **Tests should be written before implementation code.**

### Guiding Principles
- **Tests define the contract** — Write tests that describe expected behavior before implementation
- **Start with the simplest endpoint** — `/status` has minimal validation, simple request/response
- **Build infrastructure on-demand** — Only create helpers when a test requires them
- **Integration tests validate real behavior** — Write a failing integration test first, then unit tests for isolated behavior
- **Test pass/fail indicates progress** — All tests passing = phase complete

---

### Phase 0: Project Scaffold & Test Infrastructure

**Goal:** Establish project structure and verify test infrastructure works.

**Tasks (no production code yet):**
1. Create solution with `Valhalla.Routing.Client` and `Valhalla.Routing.Client.Tests` projects
2. Configure test project with xUnit, FluentAssertions, Moq, RichardSzalay.MockHttp
3. Set up `docker-compose.integration.yml` with Valhalla container
4. Create `TestLocations` constants (Luxembourg coordinates)
5. Verify Valhalla container starts and responds

**Required Integration Tests:**
- `Valhalla_Container_RespondsToStatusRequest` — Verifies Docker setup works

**Acceptance:** Test infrastructure runs, container starts, basic HTTP connectivity confirmed.

---

### Phase 1: Status Endpoint

**Goal:** Implement simplest endpoint to prove HTTP pipeline, serialization, and error handling.

**Required Integration Tests:**
- `StatusAsync_ReturnsVersionAndTileInfo`
- `StatusAsync_Verbose_ReturnsExtendedInfo`

**Required Unit Tests:**
- `StatusRequest_Verbose_SerializesToSnakeCase`
- `StatusResponse_Deserializes_AllTypedFields`
- `StatusResponse_Deserializes_RawJsonPreserved`
- `ValhallaClient_StatusAsync_SendsCorrectRequest`
- `ValhallaClient_StatusAsync_ThrowsValhallaException_OnApiError`
- `ValhallaClient_Timeout_ThrowsTimeoutException`
- `ValhallaClient_Cancellation_ThrowsOperationCanceledException`

**Components Implemented:**
- `IValhallaClient` interface (define contract from the start for testability)
- `ValhallaClient` (constructor, `StatusAsync`, internal `SendAsync`)
- `ValhallaClientOptions`
- `ValhallaException`
- `StatusRequest`, `StatusResponse`
- JSON serialization configuration
- `Log` class with `LoggerMessage.Define` definitions

**Acceptance:** All tests pass. Client can call `/status` and handle success/error responses.

---

### Phase 2: Locate Endpoint

**Goal:** Implement location-based endpoint with validation and typed response.

**Required Integration Tests:**
- `LocateAsync_ReturnsEdgeCandidates`
- `LocateAsync_Verbose_ReturnsDetailedEdgeInfo`
- `LocateAsync_InvalidLocation_ThrowsValhallaException`

**Required Unit Tests:**
- `LocateRequest_SerializesToSnakeCase`
- `LocateRequest_Validation_ThrowsIfNoLocations`
- `LocateRequest_Validation_ThrowsIfInvalidLatitude`
- `LocateRequest_Validation_ThrowsIfInvalidLongitude`
- `LocateRequest_Validation_ThrowsIfMissingCosting`
- `Location_SerializesToSnakeCase_AllFields`
- `LocateResponse_Deserializes_EdgeCandidates`
- `LocateResponse_Deserializes_RawJsonPreserved`
- `ValhallaClient_LocateAsync_SendsCorrectRequest`

**Components Implemented:**
- `Location` record
- `LocateRequest`, `LocateResponse`, `LocateResult`, `EdgeCandidate`, `EdgeInfo`, `NodeCandidate`
- `CostingModel` constants
- Validation logic for coordinates

**Acceptance:** All tests pass. Client can call `/locate` with validation.

---

### Phase 3: Route Endpoint

**Goal:** Implement most complex endpoint with full validation and rich response model.

**Required Integration Tests:**
- `RouteAsync_TwoLocations_ReturnsTripWithOneLeg`
- `RouteAsync_ThreeLocations_ReturnsTripWithTwoLegs`
- `RouteAsync_WithAlternates_ReturnsMultipleTrips`
- `RouteAsync_NoRoutePossible_ThrowsValhallaException`
- `RouteAsync_DifferentCostingModels_ReturnsValidRoutes` (auto, bicycle, pedestrian)

**Required Unit Tests:**
- `RouteRequest_SerializesToSnakeCase_AllFields`
- `RouteRequest_Validation_ThrowsIfFewerThanTwoLocations`
- `RouteRequest_Validation_ThrowsIfMissingCosting`
- `RouteRequest_Validation_ThrowsIfInvalidHeading`
- `RouteRequest_Validation_ThrowsIfInvalidHeadingTolerance`
- `RouteRequest_Validation_ThrowsIfInvalidRadius`
- `RouteRequest_Validation_ThrowsIfInvalidDateTimeType`
- `RouteRequest_Validation_ThrowsIfDateTimeValueMissingForDepartAt`
- `RouteRequest_Validation_ThrowsIfInvalidAlternates`
- `DateTimeOptions_SerializesToSnakeCase`
- `SearchFilter_SerializesToSnakeCase`
- `RouteResponse_Deserializes_TripWithLegsAndManeuvers`
- `RouteResponse_Deserializes_TripSummary`
- `RouteResponse_Deserializes_RawJsonPreserved`
- `ValhallaClient_RouteAsync_SendsCorrectRequest`

**Components Implemented:**
- `RouteRequest`, `RouteResponse`
- `Trip`, `TripSummary`, `Leg`, `LegSummary`, `Maneuver`
- `DateTimeOptions`, `DateTimeType`, `SearchFilter`, `BoundingBox`
- Extended validation logic

**Acceptance:** All tests pass. Client can call `/route` with full validation.

---

### Phase 4: Map Matching Endpoints

**Goal:** Implement `/trace_route` and `/trace_attributes` with shared DTOs.

**Required Integration Tests:**
- `TraceRouteAsync_WithShape_ReturnsMatchedRoute`
- `TraceRouteAsync_WithEncodedPolyline_ReturnsMatchedRoute`
- `TraceAttributesAsync_ReturnsMatchedPointsAndEdges`
- `TraceRouteAsync_NoMatch_ThrowsValhallaException`

**Required Unit Tests:**
- `TraceRouteRequest_SerializesToSnakeCase`
- `TraceRouteRequest_Validation_ThrowsIfNoShapeOrPolyline`
- `TraceRouteRequest_Validation_ThrowsIfBothShapeAndPolyline`
- `TraceRouteRequest_Validation_ThrowsIfShapeFewerThanTwoPoints`
- `TraceRouteRequest_Validation_ThrowsIfMissingCosting`
- `TraceRouteRequest_Validation_ThrowsIfSearchRadiusExceeds100`
- `TraceAttributesRequest_SerializesToSnakeCase`
- `TraceAttributesRequest_Validation_FilterActionMustBeIncludeOrExclude`
- `TracePoint_SerializesToSnakeCase`
- `TraceOptions_SerializesToSnakeCase`
- `TraceRouteResponse_Deserializes_TripData`
- `TraceAttributesResponse_Deserializes_MatchedPointsAndEdges`
- `ValhallaClient_TraceRouteAsync_SendsCorrectRequest`
- `ValhallaClient_TraceAttributesAsync_SendsCorrectRequest`

**Components Implemented:**
- `TraceRouteRequest`, `TraceRouteResponse`
- `TraceAttributesRequest`, `TraceAttributesResponse`
- `TracePoint`, `TraceOptions`, `FilterAttributes`
- `MatchedPoint`, `TraceEdge`
- `EpochSecondsConverter` (JSON converter for DateTimeOffset to/from epoch seconds)

**Acceptance:** All tests pass. Client can call both map matching endpoints.

---

### Phase 5: Polyline Utilities

**Goal:** Implement encode/decode utilities for working with route shapes.

**Required Unit Tests:**
- `PolylineEncoder_Decode_KnownTestVector_ReturnsCorrectCoordinates`
- `PolylineEncoder_Encode_KnownTestVector_ReturnsCorrectString`
- `PolylineEncoder_RoundTrip_PreservesCoordinates`
- `PolylineEncoder_Decode_EmptyString_ReturnsEmptyList`
- `PolylineEncoder_Encode_EmptyList_ReturnsEmptyString`
- `PolylineEncoder_Precision6_MatchesValhallaOutput`

**Required Integration Tests:**
- `RouteAsync_Shape_CanBeDecodedByPolylineEncoder`

**Components Implemented:**
- `PolylineEncoder` static class with `Encode` and `Decode` methods

**Acceptance:** All tests pass. Polyline utilities work with real Valhalla responses.

---

### Phase 6: DI Integration, Builder & Service Collection Extensions

**Goal:** Implement dependency injection support and non-DI builder for console applications.

**Required Unit Tests:**
- `AddValhallaClient_RegistersIValhallaClient`
- `AddValhallaClient_ConfiguresHttpClientBaseAddress`
- `AddValhallaClient_ConfiguresTimeout`
- `AddValhallaClient_ReturnsIHttpClientBuilder_ForFurtherConfiguration`
- `ValhallaClientBuilder_Build_ThrowsIfBaseUriNotSet`
- `ValhallaClientBuilder_Build_NormalizesTrailingSlash`
- `ValhallaClientBuilder_WithHttpClient_LogsWarning`
- `ValhallaClientBuilder_WithTimeout_SetsTimeout`
- `ValhallaClientBuilder_WithApiKey_ConfiguresHeader`

**Required Integration Tests:**
- `ServiceProvider_ResolvesValhallaClient_AndCallsStatus`
- `ValhallaClientBuilder_Build_CanCallStatus`

**Components Implemented:**
- `ValhallaClientServiceCollectionExtensions`
- `ValhallaClientBuilder` (in `Valhalla.Routing.Builder` namespace)
- BaseUri normalization logic

**Note:** `IValhallaClient` interface was created in Phase 1.

**Acceptance:** All tests pass. Consumers can register client via DI or build via `ValhallaClientBuilder`.

---

### Phase 7: Samples, Documentation & Final Polish

**Goal:** Complete samples project, documentation, verify coverage, prepare for release.

**Tasks:**
1. Create `Valhalla.Routing.Client.Samples` console project
2. Implement sample files:
   - `BasicRoutingSample.cs` — Route between two locations
   - `MultiStopRouteSample.cs` — Route with multiple waypoints
   - `GpsTraceMatchingSample.cs` — Match GPS trace via `TraceRouteAsync`
   - `TraceAttributesSample.cs` — Get road attributes for a trace
   - `ServerHealthCheckSample.cs` — Verify connectivity via `StatusAsync`
   - `NearestRoadSample.cs` — Find nearest edges via `LocateAsync`
3. Add XML documentation comments to all public types and members
4. Verify code coverage ≥80%
5. Complete README.md with all usage examples (copy from samples)
6. Create CHANGELOG.md
7. Verify NuGet package metadata
8. Run full test suite (unit + integration) on both .NET 6.0 and .NET 8.0

**Acceptance Criteria:**
- All tests pass on .NET 6.0 and .NET 8.0
- Samples project builds and runs successfully
- Code coverage ≥80%
- Zero compiler warnings
- All public types have XML documentation
- README contains working examples for all endpoints

---

### Progress Tracking

| Phase | Status | Unit Tests | Integration Tests |
|-------|--------|------------|-------------------|
| 0: Scaffold | ⬜ Not Started | — | 1 |
| 1: Status | ⬜ Not Started | 7 | 2 |
| 2: Locate | ⬜ Not Started | 9 | 3 |
| 3: Route | ⬜ Not Started | 15 | 5 |
| 4: Map Matching | ⬜ Not Started | 14 | 4 |
| 5: Polyline | ⬜ Not Started | 6 | 1 |
| 6: DI & Builder | ⬜ Not Started | 9 | 2 |
| 7: Samples & Polish | ⬜ Not Started | — | — |
| **Total** | | **60** | **18** |

---

