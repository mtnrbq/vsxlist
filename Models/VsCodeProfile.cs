namespace VSXList.Models;

/// <summary>
/// Represents a VS Code user profile with its location and extensions
/// </summary>
public record VsCodeProfile
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required IReadOnlyList<VsCodeExtension> Extensions { get; init; }
    public bool IsDefault { get; init; }
    public DateTime? LastModified { get; init; }
}