namespace Core.Models;

/// <summary>
/// Represents a VS Code extension with metadata for CSV export
/// </summary>
public record VsCodeExtension
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required string Version { get; init; }
    public required string Publisher { get; init; }
    public string? Description { get; init; }
    public bool IsBuiltIn { get; init; }
    public bool IsEnabled { get; init; }
    public string? ProfileName { get; init; }
    public DateTime? InstallDate { get; init; }
    public string? Category { get; init; }
    public string? Repository { get; init; }
    public long? InstallCount { get; init; }
    public double? Rating { get; init; }
}