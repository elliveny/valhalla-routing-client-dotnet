# Integration Test Setup

## Phase 0 Infrastructure

This document describes the integration test infrastructure setup for the Valhalla .NET Routing Client.

## Docker Compose Configuration

The `docker-compose.integration.yml` file configures a Valhalla server instance for running integration tests.

### Container Image

We use the community-maintained image: `ghcr.io/gis-ops/docker-valhalla/valhalla:latest`

**Note**: The specification originally referenced `ghcr.io/valhalla/valhalla:run-latest`, but this image is not currently available on GitHub Container Registry. The GIS-OPS image provides the same functionality and is actively maintained.

### Configuration

- **Port**: 8002 (standard Valhalla HTTP port)
- **Volume**: Named volume `valhalla-tiles` for persistent tile storage
- **Map Data**: Luxembourg OSM extract (small, fast to build)

### Starting the Container

```bash
docker compose -f docker-compose.integration.yml up -d
```

### Stopping the Container

```bash
docker compose -f docker-compose.integration.yml down
```

### Tile Building

On first run, the container will:
1. Download the Luxembourg OSM extract (~35MB)
2. Build routing tiles (~2-3 minutes)
3. Start the Valhalla server

Subsequent runs will reuse the cached tiles from the named volume.

### Running Integration Tests

```bash
# Start the container first
docker compose -f docker-compose.integration.yml up -d

# Wait for health check to pass (check with: docker ps)
# Then run integration tests
dotnet test --filter Category=Integration

# Clean up
docker compose -f docker-compose.integration.yml down
```

## Test Locations

All integration tests use coordinates within Luxembourg for consistency. See `TestLocations.cs` for predefined coordinates.

## Network Requirements

The Valhalla container requires internet access on first run to:
- Download OSM extract from Geofabrik
- Download administrative boundary data
- Download timezone data

If running in an environment without internet access, you can:
1. Pre-build tiles on a machine with internet
2. Export the `valhalla-tiles` volume
3. Import the volume in the restricted environment

## Health Check

The container includes a health check that polls the `/status` endpoint every 10 seconds. The container is considered healthy when this endpoint responds successfully.
