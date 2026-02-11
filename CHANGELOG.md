# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- N/A

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

### [0.1.5] - 2026-02-11

Enhanced TraceEdge with additional edge properties:
- Added `SpeedLimit` property (posted speed limit, distinct from routing speed)
- Added `WayId` property (OpenStreetMap way identifier for OSM correlation)
- Added `Id` property (Valhalla's internal edge identifier)
- Added `Use` property (road use classification: road, ramp, ferry, cycleway, etc.)
- Added `Surface` property (surface type: paved, gravel, etc.)
- Added `Toll` property (whether the edge has a toll)
- Added `Tunnel` property (whether the edge is a tunnel)
- Added `Bridge` property (whether the edge is a bridge)
- Updated specification documentation to reflect new properties

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

[Unreleased]: https://github.com/elliveny/valhalla-routing-client-dotnet/compare/v0.1.5...HEAD
[0.1.5]: https://github.com/elliveny/valhalla-routing-client-dotnet/compare/v0.1.4...v0.1.5
[0.1.4]: https://github.com/elliveny/valhalla-routing-client-dotnet/compare/v0.1.3...v0.1.4
[0.1.3]: https://github.com/elliveny/valhalla-routing-client-dotnet/compare/v0.1.2...v0.1.3
[0.1.2]: https://github.com/elliveny/valhalla-routing-client-dotnet/compare/v0.1.1...v0.1.2
[0.1.1]: https://github.com/elliveny/valhalla-routing-client-dotnet/releases/tag/v0.1.1
[0.1.0]: https://github.com/elliveny/valhalla-routing-client-dotnet/releases/tag/v0.1.0
