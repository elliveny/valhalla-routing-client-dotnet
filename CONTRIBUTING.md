# Contributing to Valhalla .NET Routing Client

Thank you for your interest in contributing! This document provides guidelines and instructions for contributing to the project.

## Table of Contents

1. [Code of Conduct](#code-of-conduct)
2. [Getting Started](#getting-started)
3. [Development Workflow](#development-workflow)
4. [Coding Standards](#coding-standards)
5. [Testing Requirements](#testing-requirements)
6. [Documentation](#documentation)
7. [Pull Request Process](#pull-request-process)
8. [Code Review](#code-review)

## Code of Conduct

This project adheres to a code of conduct adapted from the Contributor Covenant. By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

### Our Standards

- Be respectful and inclusive
- Welcome newcomers and help them get started
- Accept constructive criticism gracefully
- Focus on what is best for the community
- Show empathy towards other community members

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Git
- A code editor (Visual Studio, VS Code, or Rider recommended)
- Docker (for running integration tests)

### Setting Up Your Development Environment

1. **Fork the repository** on GitHub

2. **Clone your fork:**
   ```bash
   git clone https://github.com/YOUR-USERNAME/valhalla-routing-client-dotnet.git
   cd valhalla-routing-client-dotnet
   ```

3. **Add upstream remote:**
   ```bash
   git remote add upstream https://github.com/elliveny/valhalla-routing-client-dotnet.git
   ```

4. **Install dependencies:**
   ```bash
   dotnet restore
   ```

5. **Build the project:**
   ```bash
   dotnet build
   ```

6. **Run tests:**
   ```bash
   dotnet test
   ```

### Understanding the Project Structure

Before contributing, familiarize yourself with:

- [Project Specification](docs/specification/specification.md) - Complete implementation requirements
- [.NET Best Practices](docs/dotnet-best-practices.md) - Coding standards and guidelines
- [Interface Design Template](docs/interface-design-template.md) - Interface design patterns
- [Quick Reference](docs/quick-reference.md) - Common patterns cheat sheet

## Development Workflow

### Creating a Feature Branch

```bash
# Update your local main branch
git checkout main
git pull upstream main

# Create a feature branch
git checkout -b feature/your-feature-name
```

### Making Changes

1. **Write code** following the [coding standards](#coding-standards)
2. **Write tests** for your changes
3. **Update documentation** if needed
4. **Commit your changes** with clear, descriptive messages

```bash
git add .
git commit -m "feat: add support for new endpoint"
```

### Commit Message Convention

We follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

**Types:**
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation changes
- `style:` - Code style changes (formatting, etc.)
- `refactor:` - Code refactoring
- `test:` - Adding or updating tests
- `chore:` - Maintenance tasks

**Examples:**
```
feat(client): add support for optimized_route endpoint
fix(serialization): handle null values in location type
docs(readme): update installation instructions
test(integration): add tests for trace_attributes endpoint
```

### Keeping Your Branch Updated

```bash
# Fetch latest changes from upstream
git fetch upstream

# Rebase your branch on upstream/main
git rebase upstream/main

# If there are conflicts, resolve them and continue
git rebase --continue
```

## Coding Standards

### Overview

All code must adhere to the project's [.NET Best Practices](docs/dotnet-best-practices.md). Key requirements:

### Code Style

- Use **file-scoped namespaces**
- Follow **naming conventions** (PascalCase for public members, _camelCase for private fields)
- Use **explicit access modifiers** (public, private, internal)
- Order members logically (constants, fields, constructors, properties, methods)

### Documentation

- **All public types and members** must have XML documentation
- Use **`<inheritdoc/>`** for interface implementations
- Document **all parameters**, **return values**, and **exceptions**
- Include **examples** for complex scenarios

### Error Handling

- Validate all inputs
- Throw appropriate exception types
- Log errors before re-throwing or wrapping
- Distinguish timeout from cancellation

### Async/Await

- All I/O operations must be async
- Use `ConfigureAwait(false)` in library code
- Pass `CancellationToken` to all async methods
- Suffix async methods with `Async`

### Example: Well-Formatted Code

```csharp
namespace Valhalla.Routing.Client.Services;

/// <summary>
/// Provides route calculation services.
/// </summary>
public sealed class RouteService : IRouteService
{
    private readonly IValhallaClient _client;
    private readonly ILogger<RouteService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RouteService"/> class.
    /// </summary>
    /// <param name="client">The Valhalla client.</param>
    /// <param name="logger">The logger instance.</param>
    public RouteService(IValhallaClient client, ILogger<RouteService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<RouteResponse> CalculateRouteAsync(
        RouteRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogDebug("Calculating route with {LocationCount} locations", 
            request.Locations.Count);

        return await _client.RouteAsync(request, cancellationToken)
            .ConfigureAwait(false);
    }
}
```

## Testing Requirements

For comprehensive testing guidelines, see [Testing Guidelines](docs/testing-guidelines.md).

### Unit Tests

- Write unit tests for all new functionality
- Aim for ≥80% code coverage
- Test both success and failure paths
- Use descriptive test names

### Integration Tests

- Add integration tests for API interactions
- Use Docker to run Valhalla server for tests
- Clean up resources after tests

### Test Naming Convention

```csharp
[Fact]
public async Task MethodName_WithCondition_ExpectedBehavior()
{
    // Arrange
    // ...

    // Act
    // ...

    // Assert
    // ...
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test --filter Category!=Integration

# Run integration tests
dotnet test --filter Category=Integration

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## Documentation

### XML Documentation Comments

All public APIs must have comprehensive XML documentation:

```csharp
/// <summary>
/// Calculates a route between two or more locations.
/// </summary>
/// <param name="request">The route request. Must contain at least 2 locations.</param>
/// <param name="cancellationToken">A token to cancel the operation.</param>
/// <returns>
/// A task that represents the asynchronous operation. The task result contains
/// the route response with directions and distance estimates.
/// </returns>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="request"/> is <c>null</c>.
/// </exception>
Task<RouteResponse> RouteAsync(
    RouteRequest request,
    CancellationToken cancellationToken = default);
```

### README and Documentation Files

- Update README.md if adding new features
- Update relevant documentation in `/docs` directory
- Add examples to demonstrate usage

## Pull Request Process

### Before Submitting

1. ✅ Ensure all tests pass
2. ✅ Verify no compiler warnings
3. ✅ Update documentation
4. ✅ Add/update tests for your changes
5. ✅ Rebase on latest upstream/main
6. ✅ Write clear commit messages

### Creating a Pull Request

1. **Push your branch** to your fork:
   ```bash
   git push origin feature/your-feature-name
   ```

2. **Create a pull request** on GitHub

3. **Fill out the PR template** with:
   - Clear description of changes
   - Related issue numbers (if applicable)
   - Testing performed
   - Screenshots (if UI changes)

### PR Title Format

Follow the same convention as commit messages:

```
feat(client): add support for isochrone endpoint
```

### PR Description Template

```markdown
## Description
Brief description of the changes

## Related Issues
Fixes #123

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing performed

## Checklist
- [ ] Code follows project style guidelines
- [ ] Self-review completed
- [ ] Comments added for complex code
- [ ] Documentation updated
- [ ] No new warnings introduced
- [ ] Tests pass locally
```

## Code Review

### What Reviewers Look For

1. **Correctness** - Does the code work as intended?
2. **Security** - Are there any security vulnerabilities?
3. **Performance** - Are there obvious inefficiencies?
4. **Maintainability** - Is the code clear and well-documented?
5. **Testing** - Are tests adequate and meaningful?
6. **Consistency** - Does it follow project conventions?

### Responding to Reviews

- Be open to feedback
- Ask questions if feedback is unclear
- Make requested changes promptly
- Push updates to the same branch
- Request re-review when ready

### After Approval

Once your PR is approved:
1. A maintainer will merge your changes
2. Your branch will be deleted automatically
3. You'll be credited in the release notes

## Questions?

- 💬 **Discussions** - Ask questions in [GitHub Discussions](https://github.com/elliveny/valhalla-routing-client-dotnet/discussions)
- 🐛 **Issues** - Report bugs or request features via [GitHub Issues](https://github.com/elliveny/valhalla-routing-client-dotnet/issues)

## Thank You!

Your contributions make this project better for everyone. We appreciate your time and effort! 🎉
