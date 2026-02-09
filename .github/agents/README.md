# GitHub Copilot Agent Instructions

This directory contains specialized agent instruction files that guide AI-powered development assistance for the Valhalla .NET Routing Client project.

## What are Agent Instructions?

Agent instructions are specialized prompts that help AI coding assistants (like GitHub Copilot) understand project-specific requirements, coding standards, and best practices. They act as "expert advisors" for different aspects of development.

## Available Agents

### 1. dotnet-developer.md

**Purpose:** Enforces .NET coding standards and best practices

**Key Responsibilities:**
- Code quality and consistency
- SOLID principles application
- Proper error handling
- Async/await best practices
- XML documentation compliance
- Testing guidelines

**When to reference:** When writing or reviewing any .NET code in this project.

### 2. documentation-reviewer.md

**Purpose:** Ensures comprehensive XML documentation quality

**Key Responsibilities:**
- Verifying all public APIs are documented
- Ensuring documentation completeness (summary, params, returns, exceptions)
- Enforcing `<inheritdoc/>` usage for implementations
- Maintaining documentation consistency
- Providing clear examples for complex scenarios

**When to reference:** When adding or reviewing XML documentation comments.

## How to Use These Instructions

### For Human Developers

1. **Read before coding** - Familiarize yourself with the standards before starting new work
2. **Reference during code review** - Use these as checklists when reviewing PRs
3. **Training resource** - Help new team members understand project expectations

### For AI Assistants

These files are automatically loaded by compatible AI coding assistants to provide context-aware suggestions that align with project standards.

### For CI/CD

The standards defined here complement automated enforcement through:
- `.editorconfig` - Formatting rules
- `Directory.Build.props` - Compiler settings and warnings
- `stylecop.json` - StyleCop analyzer configuration

## Related Documentation

- `/docs/dotnet-best-practices.md` - Comprehensive best practices guide
- `/docs/interface-design-template.md` - Interface design templates and examples
- `/docs/specification/specification.md` - Complete project specification

## Contributing to Agent Instructions

When updating agent instructions:

1. **Be specific** - Provide concrete examples and patterns
2. **Stay current** - Update when project practices evolve
3. **Include rationale** - Explain *why* not just *what*
4. **Add examples** - Show good and bad patterns
5. **Test applicability** - Ensure instructions work in practice

## Maintenance

These instructions should be reviewed and updated:
- When adopting new .NET features or language versions
- After identifying recurring code review issues
- When team consensus changes on a practice
- After major dependency updates

---

**Note:** These instructions complement, but do not replace, thorough code reviews by experienced developers. They help maintain consistency and catch common issues early in the development process.
