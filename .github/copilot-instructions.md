# VSXList - AI Coding Agent Instructions

## Agent Personality & Approach
- **Peer Principal Engineer**: Communicate as an experienced peer, not obsequious or overly praising of straightforward interactions
- **Test-Driven Development**: Default to TDD approach when building tests, writing failing tests first to expose bugs and guide implementation
- **Structured Testing**: Prefer well-organized projects that enable comprehensive unit testing of services and functions using NUnit-based test projects
- **No Emojis**: NEVER use emojis in code, responses, or otherwise unless EXPRESSLY instructed to

## Project Overview
VSXList is a .NET 9.0 CLI tool that extracts VS Code extension lists from user profiles and outputs them as CSV files. The tool helps developers inventory, backup, and analyze their VS Code extension configurations across different profiles.

## Architecture & Structure
- **Console Application**: Built on .NET 9.0 with modern C# features
- **VS Code Profile Reader**: Discovers and parses VS Code user data directories
- **Extension Parser**: Extracts extension metadata from profile configurations
- **CSV Writer**: Outputs formatted extension data for analysis/backup
- **Cross-platform**: Supports Windows, macOS, and Linux VS Code installations

## Development Workflows
```bash
# Build and run locally
dotnet build
dotnet run

# Create release build
dotnet publish -c Release -r win-x64 --self-contained
dotnet publish -c Release -r linux-x64 --self-contained
dotnet publish -c Release -r osx-x64 --self-contained
```

## Git Conventions
- Prefer minimal but sufficient commit messages

## Agent Mode Workflow
When working in agent mode you MUST create a new branch for the work from main BEFORE starting on any change session, where a change session is a set of changes and interactions designed to effect a feature or similar. When it is indicated that we're done, e.g. 'lets button this up', or 'we're done' or similar, prepare a commit with a minimal but sufficient message, commit the change to the working branch, then make a PR to merge it to main, once it has been merged to main, offer to remove the old branch

## Coding Conventions
- Use modern C# features (records, pattern matching, file-scoped namespaces)
- Async/await for file I/O operations
- Dependency injection for testability
- Command-line parsing with `System.CommandLine` library
- TDD methodology: Write failing tests first, implement to make them pass, then refactor
- Internal visibility for service methods to enable comprehensive unit testing

## Key Files & Directories
- `VSXList.csproj`: Main project file targeting .NET 9.0
- `Program.cs`: Entry point with command-line argument parsing
- `Services/`: Core business logic (ProfileReader, ExtensionParser, CsvWriter)
- `Models/`: Data models for VS Code profiles and extensions
- `Constants/`: Platform-specific paths and configuration constants

---

## Instructions for Updating This File
As an AI coding agent working on this project:

1. **Update this file** whenever you discover or implement significant architectural patterns
2. **Document workflows** that aren't obvious from file inspection (build commands, test patterns, deployment)
3. **Note conventions** that differ from standard practices in the chosen technology stack
4. **Reference specific files** that exemplify important patterns
5. **Keep it concise** - focus on what makes THIS project unique

## Template Sections to Fill In
- [ ] Technology stack and framework choices
- [ ] Project structure and module organization  
- [ ] Build and development commands
- [ ] Testing approaches and patterns
- [ ] API design patterns
- [ ] Database schema and data flow
- [ ] Deployment and configuration management
- [ ] Code style and naming conventions
- [ ] Error handling patterns
- [ ] Integration points and external dependencies