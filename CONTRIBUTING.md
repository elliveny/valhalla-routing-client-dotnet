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

## Development Philosophy

### Specification-First Development

This project follows a **specification-driven development approach** where the [specification.md](docs/specification/specification.md) document serves as the authoritative source for all design and implementation decisions.

**Why This Matters:**

1. **Single Source of Truth** - The specification defines what the code should do and how it should be structured
2. **Maintainability** - Future developers (and AI assistants) can understand the complete system from the specification
3. **Consistency** - Design decisions are documented and can be referenced during implementation
4. **Regenerability** - The codebase can theoretically be regenerated from the specification using AI tools

### AI-Assisted Workflow

This project was built entirely using AI tools (ChatGPT, GitHub Copilot with Claude Opus 4.5), and we encourage contributors to continue this approach:

**Recommended Workflow:**

1. **Update Specification First**
   - Propose changes to [specification.md](docs/specification/specification.md) before writing code
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

**This Does Not Mean:**
- Human-written code is discouraged (it's perfectly acceptable!)
- You must use AI tools to contribute (but it's encouraged)
- AI output should be merged without review (always apply human judgment)

**This Does Mean:**
- Consider how changes fit into the specification
- Documentation-first thinking is valued
- Leverage AI to handle repetitive or boilerplate tasks
- Maintain the specification as changes evolve

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

- **[Project Specification](docs/specification/specification.md)** - **START HERE!** This is the authoritative source that defines all requirements, design decisions, and implementation details
- [.NET Best Practices](docs/dotnet-best-practices.md) - Coding standards and guidelines
- [Testing Guidelines](docs/testing-guidelines.md) - Unit and integration test best practices
- [Interface Design Template](docs/interface-design-template.md) - Interface design patterns
- [Quick Reference](docs/quick-reference.md) - Common patterns cheat sheet

**Important:** The specification.md file is not just documentation—it's the **source of truth** for this project. When in doubt about design decisions, refer to the specification first.

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

1. **Consider specification updates** - For significant changes, update [specification.md](docs/specification/specification.md) first
2. **Write code** following the [coding standards](#coding-standards)
3. **Write tests** for your changes
4. **Update documentation** if needed
5. **Commit your changes** with clear, descriptive messages

```bash
git add .
git commit -m "feat: add support for new endpoint"
```

### Types of Contributions

#### 1. Bug Fixes
For simple bug fixes:
- Write tests that reproduce the bug
- Fix the code
- Verify tests pass
- Update specification.md if the bug reveals a specification gap

#### 2. New Features
For new features or significant changes:
- **Start with specification.md** - Propose the design in the specification first
- Get feedback on the specification changes
- Implement the feature (consider using AI tools)
- Write comprehensive tests
- Update other documentation (README, etc.)

#### 3. Documentation Updates
- Keep specification.md aligned with code
- Update inline XML documentation
- Improve README and guides as needed

#### 4. Refactoring
- Ensure specification.md still accurately describes behavior
- Maintain test coverage
- Document any architectural changes in specification.md

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
3. ✅ Update [specification.md](docs/specification/specification.md) if applicable (for new features or significant changes)
4. ✅ Update other documentation (README, inline docs, etc.)
5. ✅ Add/update tests for your changes
6. ✅ Rebase on latest upstream/main
7. ✅ Write clear commit messages

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
- [ ] Specification update

## Specification Changes
- [ ] specification.md updated (if applicable)
- [ ] No specification changes needed
- [ ] Specification already covers this change

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
