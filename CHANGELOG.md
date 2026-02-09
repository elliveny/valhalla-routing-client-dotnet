# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial project structure and documentation
- Comprehensive specification document
- .NET best practices guidelines
- Testing guidelines document
- Interface design templates
- Quick reference guide
- EditorConfig, Directory.Build.props, and StyleCop configuration
- MIT LICENSE file
- Comprehensive documentation review
- Complete implementation of all Valhalla API endpoints:
  - Status endpoint for server health checks
  - Locate endpoint for finding nearest roads
  - Route endpoint for calculating routes between locations
  - TraceRoute endpoint for GPS trace matching
  - TraceAttributes endpoint for extracting edge attributes
- Polyline encoding/decoding utilities (precision 6)
- Dependency injection support via `AddValhallaClient()`
- Builder pattern for non-DI scenarios via `ValhallaClientBuilder`
- Comprehensive sample applications demonstrating all features:
  - ServerHealthCheckSample - Server status and version check
  - BasicRoutingSample - Simple routing between two points
  - MultiStopRouteSample - Multi-waypoint routing
  - NearestRoadSample - Locate nearest road to coordinates
  - GpsTraceMatchingSample - GPS trace matching to road network
  - TraceAttributesSample - Extract detailed edge attributes
- Sample project with dependency injection setup
- Samples README with usage instructions

### Changed
- N/A

### Deprecated
- N/A

### Removed
- N/A

### Fixed
- N/A

### Security
- N/A

---

## Release History

### [0.1.4] - 2026-02-09

Documentation and branding:
- Updated package icon
- Updated README with NuGet badge and pre-release notice

### [0.1.3] - 2026-02-09

Packaging fix:
- Fixed package metadata not being included in NuGet package (README, icon, description)

### [0.1.2] - 2026-02-09

Packaging fix:
- Fixed package icon size (under 1 MB limit)

### [0.1.1] - 2026-02-09

Packaging improvements:
- Added README to NuGet package
- Added package icon

### [0.1.0] - 2026-02-09

Initial release with support for:
- Route endpoint (`/route`)
- Map matching endpoints (`/trace_route`, `/trace_attributes`)
- Status endpoint (`/status`)
- Locate endpoint (`/locate`)
- Polyline encoding/decoding utilities
- Dependency injection support
- Builder pattern for non-DI scenarios
- Comprehensive XML documentation
- .NET 6.0 and .NET 8.0 support

[Unreleased]: https://github.com/elliveny/valhalla-routing-client-dotnet/compare/v0.1.4...HEAD
[0.1.4]: https://github.com/elliveny/valhalla-routing-client-dotnet/compare/v0.1.3...v0.1.4
[0.1.3]: https://github.com/elliveny/valhalla-routing-client-dotnet/compare/v0.1.2...v0.1.3
[0.1.2]: https://github.com/elliveny/valhalla-routing-client-dotnet/compare/v0.1.1...v0.1.2
[0.1.1]: https://github.com/elliveny/valhalla-routing-client-dotnet/releases/tag/v0.1.1
[0.1.0]: https://github.com/elliveny/valhalla-routing-client-dotnet/releases/tag/v0.1.0
