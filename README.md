# Valhalla Routing Client for .NET

A production-ready .NET client library for the [Valhalla](https://github.com/valhalla/valhalla) routing engine HTTP API.

> **Note:** This is an unofficial client library and is not affiliated with or endorsed by the Valhalla project.

## Status

✅ **Production Ready** - All core endpoints have been implemented and tested. The API surface is stable.

## About This Project

### AI-Driven Development Experiment

This project represents an experiment in **specification-driven development using AI tools**. The entire codebase—from design to implementation—was created through a collaborative process between AI assistants and human review.

**Development Timeline:**
- **Day 1:** Initial [specification.md](docs/specification/specification.md) created using ChatGPT, then refined through multiple rounds of review using GitHub Copilot (Claude Opus 4.5 model)
- **Days 2-3:** Phased implementation with continuous agent and human review at each stage
- **Post-development:** Agent review validated specification against code, resulting in refinements to both

**Key Philosophy:**

The [specification.md](docs/specification/specification.md) file serves as the **definitive source of truth** for this project. It is intended that:

1. **Specification leads development** - Changes to requirements should update the specification first
2. **Code follows specification** - Implementation should be derived from the specification document
3. **Continuous evolution** - Both specification and code evolve together, with the specification maintaining the authoritative design
4. **AI-assisted workflow** - While human input is valued, the project encourages using AI tools to translate specification updates into code changes

This approach aims to create a codebase that can be reliably regenerated, understood, and extended from its written specification with AI assistance.

💡 **For Contributors:** When proposing changes, consider updating [specification.md](docs/specification/specification.md) first. See our [contribution guidelines](CONTRIBUTING.md) for details on the recommended workflow.

## Supported Endpoints

This client library provides support for the following Valhalla API endpoints:

- ✅ **Route** (`/route`) - Calculate optimal routes between locations
- ✅ **Map Matching** (`/trace_route`, `/trace_attributes`) - Match GPS traces to road networks
- ✅ **Status** (`/status`) - Check service health and version
- ✅ **Locate** (`/locate`) - Find nearest roads to a location

## Target Frameworks

- .NET 6.0
- .NET 8.0

## Features

- 🔄 **Thread-safe** - Designed for concurrent usage in ASP.NET Core applications
- 💉 **Dependency Injection** - First-class support for DI with `IServiceCollection` extensions
- 🔧 **Builder Pattern** - Non-DI scenarios supported via fluent builder API
- 📝 **Comprehensive Documentation** - Extensive XML documentation for IntelliSense
- 🛡️ **Robust Error Handling** - Detailed exception types for different failure scenarios
- 🔒 **Security-Conscious** - API key redaction in logs, response size limits, DoS protection
- ⚡ **Modern .NET** - Leverages latest C# features and async/await patterns
- 🧪 **Well-Tested** - High test coverage with both unit and integration tests

## Installation

[![NuGet](https://img.shields.io/nuget/v/Valhalla.Routing.Client.svg)](https://www.nuget.org/packages/Valhalla.Routing.Client)

```bash
dotnet add package Valhalla.Routing.Client
```

> **Pre-release:** This library is in active development (0.x). The API is stable but may have minor changes before v1.0. Feedback welcome!

## Quick Start

### With Dependency Injection (ASP.NET Core)

```csharp
// In Startup.cs or Program.cs
services.AddValhallaClient(options =>
{
    options.BaseUri = new Uri("http://localhost:8002");
    options.Timeout = TimeSpan.FromSeconds(30);
});

// In your service
public class RouteService
{
    private readonly IValhallaClient _client;

    public RouteService(IValhallaClient client)
    {
        _client = client;
    }

    public async Task<RouteResponse> GetDirectionsAsync()
    {
        var request = new RouteRequest
        {
            Locations = new List<Location>
            {
                new() { Lat = 40.7128, Lon = -74.0060 }, // New York
                new() { Lat = 34.0522, Lon = -118.2437 } // Los Angeles
            },
            Costing = CostingModel.Auto
        };

        return await _client.RouteAsync(request);
    }
}
```

### Without Dependency Injection

```csharp
var client = ValhallaClientBuilder
    .Create()
    .WithBaseUrl("http://localhost:8002")
    .WithTimeout(TimeSpan.FromSeconds(30))
    .Build();

var request = new RouteRequest
{
    Locations = new List<Location>
    {
        new() { Lat = 40.7128, Lon = -74.0060 },
        new() { Lat = 34.0522, Lon = -118.2437 }
    }
};

var response = await client.RouteAsync(request);
Console.WriteLine($"Distance: {response.Trip.Summary.Length} km");
Console.WriteLine($"Duration: {response.Trip.Summary.Time} seconds");
```

## Samples

Practical examples demonstrating all features:

```bash
cd samples/Valhalla.Routing.Client.Samples
dotnet run
```

Available samples:
- **ServerHealthCheckSample** - Check server status and version
- **BasicRoutingSample** - Simple route between two points
- **MultiStopRouteSample** - Route with multiple waypoints
- **NearestRoadSample** - Find nearest road using Locate endpoint
- **GpsTraceMatchingSample** - Match GPS trace to road network
- **TraceAttributesSample** - Extract edge attributes from GPS trace

See [samples/README.md](samples/README.md) for detailed documentation.

## Documentation

### For Developers Using This Library

- 📖 **API Documentation** - Available through IntelliSense with comprehensive XML documentation on all public APIs. The library generates full XML documentation files for IDE integration.
- 💡 **Examples** - See [samples/](samples/) directory for practical usage examples covering all endpoints
- 🔗 **Valhalla API Docs** - [valhalla.github.io/valhalla/api](https://valhalla.github.io/valhalla/api/) - For understanding the underlying Valhalla routing engine API

### For Contributors

- 📋 [Project Specification](docs/specification/specification.md) - Complete implementation requirements
- ✨ [.NET Best Practices](docs/dotnet-best-practices.md) - Coding standards and guidelines
- 🧪 [Testing Guidelines](docs/testing-guidelines.md) - Unit and integration test best practices
- 📐 [Interface Design Template](docs/interface-design-template.md) - Interface design patterns
- 🚀 [Quick Reference](docs/quick-reference.md) - Quick cheat sheet for common patterns
- 🤖 [Agent Instructions](.github/agents/README.md) - AI assistant guidance
- 📝 [Documentation Review](docs/DOCUMENTATION_REVIEW.md) - Comprehensive pre-coding review
- ✅ [Development Checklist](docs/DEVELOPMENT_CHECKLIST.md) - Phase-by-phase implementation guide

## Development Setup

### Prerequisites

- .NET 8.0 SDK or later
- Docker (for running Valhalla server for integration tests)
- Git

### Building the Project

```bash
# Clone the repository
git clone https://github.com/elliveny/valhalla-routing-client-dotnet.git
cd valhalla-routing-client-dotnet

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

### Running Integration Tests

Integration tests require a running Valhalla instance:

```bash
# Start Valhalla server with Docker
docker-compose -f docker-compose.integration.yml up -d

# Run integration tests
dotnet test --filter Category=Integration

# Stop Valhalla server
docker-compose -f docker-compose.integration.yml down
```

## Project Structure

```
/src                           # Source code
  /Valhalla.Routing.Client    # Main client library
/test                         # Tests
  /Valhalla.Routing.Client.Tests
    /Unit                     # Unit tests
    /Integration              # Integration tests
/samples                      # Sample applications
/docs                         # Documentation
  /specification              # Project specification
  dotnet-best-practices.md    # .NET best practices guide
  interface-design-template.md # Interface templates
  quick-reference.md          # Quick reference guide
/.github
  /agents                     # AI agent instruction files
.editorconfig                 # Code style configuration
Directory.Build.props         # Project-wide MSBuild settings
stylecop.json                 # StyleCop analyzer configuration
```

## Code Quality Standards

This project maintains high code quality through:

- ✅ **Zero compiler warnings** - Warnings treated as errors
- ✅ **Code analyzers** - StyleCop, Microsoft.CodeAnalysis.NetAnalyzers
- ✅ **XML documentation** - All public APIs documented (CS1591 enforced)
- ✅ **Consistent formatting** - Automated via `.editorconfig`
- ✅ **Test coverage** - ≥80% coverage target
- ✅ **CI/CD pipeline** - Automated builds and tests

## Contributing

Contributions are welcome! Please read our [contribution guidelines](CONTRIBUTING.md) before submitting pull requests.

### Development Guidelines

1. **Read the [specification.md](docs/specification/specification.md) first** - Understand the authoritative design
2. **Update specification for significant changes** - Propose specification updates before implementing new features
3. Follow the [.NET Best Practices](docs/dotnet-best-practices.md)
4. Follow the [Testing Guidelines](docs/testing-guidelines.md) when writing tests
5. Use the [Interface Design Template](docs/interface-design-template.md) for new interfaces
6. Write comprehensive XML documentation
7. Include unit tests for all new functionality
8. Ensure integration tests pass
9. Keep changes focused and minimal
10. **Consider using AI tools** - Leverage AI assistants for implementation, but always apply human review

### Code Review Focus

- Correctness and functionality
- Security considerations
- Performance implications
- Documentation quality
- Test coverage
- Consistency with project standards

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- The [Valhalla](https://github.com/valhalla/valhalla) routing engine team
- Contributors and community members

## Support

- 🐛 **Issues** - [GitHub Issues](https://github.com/elliveny/valhalla-routing-client-dotnet/issues)
- 💬 **Discussions** - [GitHub Discussions](https://github.com/elliveny/valhalla-routing-client-dotnet/discussions)

## Roadmap

### Current (v0.1.0)
- ✅ Project structure and guidelines
- ✅ Core client implementation
- ✅ All basic endpoints (route, trace, status, locate)
- ✅ Comprehensive testing
- ✅ Sample applications
- ✅ Complete documentation

### Future Enhancements
- Additional endpoint support (matrix, isochrone)
- Advanced costing option builders
- Rate limiting and retry policies
- Enhanced error recovery

---

**Made with ❤️ by Elliveny**
