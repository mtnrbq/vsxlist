# VSXList

A .NET 9.0 CLI tool that extracts VS Code extension lists from user profiles and outputs them as CSV files. Perfect for backing up, analyzing, or inventorying your VS Code extension configurations across different profiles.

## Features

- **Cross-platform**: Works on Windows, macOS, and Linux
- **Profile Discovery**: Automatically finds all VS Code profiles
- **Multiple Export Options**: Single file, separate files per profile, or summary reports
- **Rich Metadata**: Extracts extension ID, name, version, publisher, description, and install dates
- **CSV Format**: Easy to import into spreadsheets or other tools for analysis

## Installation

### Prerequisites
- .NET 9.0 SDK installed

### Building from Source
```bash
git clone https://github.com/mtnrbq/vsxlist.git
cd vsxlist
dotnet build -c Release
```

### Publishing Self-Contained Executables
```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained

# Linux
dotnet publish -c Release -r linux-x64 --self-contained

# macOS
dotnet publish -c Release -r osx-x64 --self-contained
```

## Usage

### Basic Usage
```bash
# Export all profiles to default file
dotnet run

# Export to custom file
dotnet run -- -o my-extensions.csv

# Export specific profile only
dotnet run -- -p "Work Profile"

# Create separate files for each profile
dotnet run -- -s

# Create profile summary
dotnet run -- --summary

# Combine options
dotnet run -- -s --summary -o /path/to/output.csv
```

### Command Line Options

| Option | Description |
|--------|-------------|
| `-o, --output <path>` | Output CSV file path (default: vscode-extensions.csv) |
| `-p, --profile <name>` | Export specific profile only |
| `-s, --separate` | Create separate CSV files for each profile |
| `--summary` | Create a profile summary CSV file |
| `-v, --verbose` | Show detailed error information |
| `-h, --help` | Show help message |

## Output Format

### Extensions CSV
The main output contains these columns:
- **Profile**: Profile name
- **ExtensionID**: Unique extension identifier (publisher.name)
- **DisplayName**: Human-readable extension name
- **Publisher**: Extension publisher
- **Version**: Installed version
- **Description**: Extension description
- **Category**: Extension category (if available)
- **Enabled**: Whether extension is enabled
- **BuiltIn**: Whether it's a built-in VS Code extension
- **InstallDate**: When the extension was installed
- **Repository**: Extension repository URL (if available)
- **InstallCount**: Download count (if available)
- **Rating**: Extension rating (if available)

### Profile Summary CSV
When using `--summary`, also creates a summary with:
- **ProfileName**: Name of the profile
- **ExtensionCount**: Total number of extensions
- **BuiltInExtensions**: Count of built-in extensions
- **ThirdPartyExtensions**: Count of third-party extensions
- **ProfilePath**: Filesystem path to profile
- **LastModified**: When profile was last modified

## VS Code Profile Locations

VSXList automatically discovers profiles from standard locations:

- **Windows**: `%APPDATA%\Code\User`
- **macOS**: `~/Library/Application Support/Code/User`
- **Linux**: `~/.config/Code/User`

## Use Cases

- **Backup**: Create backups of your extension configurations
- **Migration**: Transfer extensions between machines or profiles
- **Analysis**: Analyze extension usage patterns across profiles
- **Cleanup**: Identify duplicate or unused extensions
- **Documentation**: Document your development environment
- **Team Setup**: Share extension lists with team members

## Architecture

The tool is structured with clean separation of concerns:

- **Models**: Data structures for extensions and profiles (`VsCodeExtension`, `VsCodeProfile`)
- **Services**: Core business logic
  - `ProfileReaderService`: Discovers and reads VS Code profiles
  - `CsvWriterService`: Handles CSV output in various formats
- **Constants**: Platform-specific path handling (`VsCodePaths`)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Related Projects

- **[VSXViz](https://github.com/mtnrbq/vsxviz)** - Flutter desktop application for visualizing VSXList CSV output

## Troubleshooting

### No profiles found
- Ensure VS Code is installed and has been run at least once
- Check that VS Code user data directory exists in the expected location
- Try running with `-v` for verbose error information

### Permission errors
- Ensure the tool has read access to VS Code's user data directory
- On Linux/macOS, check file permissions for `~/.config/Code` or `~/Library/Application Support/Code`

### Extensions not showing
- Some extensions may not have complete metadata in their package.json
- Built-in extensions are detected but may have limited metadata
- Custom or development extensions may not be detected properly