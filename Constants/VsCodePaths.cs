using System.Runtime.InteropServices;

namespace VSXList.Constants;

/// <summary>
/// Platform-specific paths for VS Code user data directories
/// </summary>
public static class VsCodePaths
{
    /// <summary>
    /// Gets the VS Code user data directory for the current platform
    /// </summary>
    public static string GetUserDataDirectory()
    {
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Code", "User")
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? Path.Combine(homeDir, "Library", "Application Support", "Code", "User")
                : Path.Combine(homeDir, ".config", "Code", "User"); // Linux
    }

    /// <summary>
    /// Gets the VS Code extensions directory for the current platform
    /// </summary>
    public static string GetExtensionsDirectory()
    {
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".vscode", "extensions")
            : Path.Combine(homeDir, ".vscode", "extensions");
    }

    /// <summary>
    /// Gets the profiles directory within the user data directory
    /// </summary>
    public static string GetProfilesDirectory()
    {
        return Path.Combine(GetUserDataDirectory(), "profiles");
    }

    /// <summary>
    /// Gets the default profile settings file path
    /// </summary>
    public static string GetDefaultSettingsPath()
    {
        return Path.Combine(GetUserDataDirectory(), "settings.json");
    }

    /// <summary>
    /// Gets the extensions.json file path for a specific profile
    /// </summary>
    public static string GetExtensionsConfigPath(string profilePath)
    {
        return Path.Combine(profilePath, "extensions.json");
    }
}