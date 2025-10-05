using System.Text.Json;
using VSXList.Constants;
using VSXList.Models;

namespace VSXList.Services;

/// <summary>
/// Service for discovering and reading VS Code user profiles
/// </summary>
public class ProfileReaderService
{
    /// <summary>
    /// Discovers all VS Code profiles for the current user
    /// </summary>
    public async Task<IReadOnlyList<VsCodeProfile>> DiscoverProfilesAsync()
    {
        var profiles = new List<VsCodeProfile>();

        // Add default profile
        var defaultProfile = await ReadDefaultProfileAsync();
        if (defaultProfile != null)
        {
            profiles.Add(defaultProfile);
        }

        // Add custom profiles
        var customProfiles = await ReadCustomProfilesAsync();
        profiles.AddRange(customProfiles);

        return profiles.AsReadOnly();
    }

    /// <summary>
    /// Reads the default VS Code profile
    /// </summary>
    private async Task<VsCodeProfile?> ReadDefaultProfileAsync()
    {
        try
        {
            var userDataDir = VsCodePaths.GetUserDataDirectory();
            if (!Directory.Exists(userDataDir))
                return null;

            var extensions = await ReadExtensionsFromDirectoryAsync(VsCodePaths.GetExtensionsDirectory());
            var lastModified = Directory.GetLastWriteTime(userDataDir);

            return new VsCodeProfile
            {
                Name = "Default",
                Path = userDataDir,
                Extensions = extensions,
                IsDefault = true,
                LastModified = lastModified
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not read default profile: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Reads all custom VS Code profiles
    /// </summary>
    private async Task<IReadOnlyList<VsCodeProfile>> ReadCustomProfilesAsync()
    {
        var profiles = new List<VsCodeProfile>();
        
        try
        {
            var profilesDir = VsCodePaths.GetProfilesDirectory();
            if (!Directory.Exists(profilesDir))
                return profiles.AsReadOnly();

            var profileDirectories = Directory.GetDirectories(profilesDir);
            
            foreach (var profileDir in profileDirectories)
            {
                var profile = await ReadProfileFromDirectoryAsync(profileDir);
                if (profile != null)
                {
                    profiles.Add(profile);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not read custom profiles: {ex.Message}");
        }

        return profiles.AsReadOnly();
    }

    /// <summary>
    /// Reads a profile from a specific directory
    /// </summary>
    private async Task<VsCodeProfile?> ReadProfileFromDirectoryAsync(string profileDir)
    {
        try
        {
            var profileName = Path.GetFileName(profileDir);
            var extensionsConfigPath = VsCodePaths.GetExtensionsConfigPath(profileDir);
            
            var extensions = File.Exists(extensionsConfigPath)
                ? await ReadExtensionsFromConfigAsync(extensionsConfigPath)
                : await ReadExtensionsFromDirectoryAsync(VsCodePaths.GetExtensionsDirectory());

            var lastModified = Directory.GetLastWriteTime(profileDir);

            return new VsCodeProfile
            {
                Name = profileName,
                Path = profileDir,
                Extensions = extensions,
                IsDefault = false,
                LastModified = lastModified
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not read profile from {profileDir}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Reads extensions from a directory structure
    /// </summary>
    private async Task<IReadOnlyList<VsCodeExtension>> ReadExtensionsFromDirectoryAsync(string extensionsDir)
    {
        var extensions = new List<VsCodeExtension>();

        if (!Directory.Exists(extensionsDir))
            return extensions.AsReadOnly();

        try
        {
            var extensionDirectories = Directory.GetDirectories(extensionsDir);
            
            foreach (var extDir in extensionDirectories)
            {
                var extension = await ReadExtensionFromDirectoryAsync(extDir);
                if (extension != null)
                {
                    extensions.Add(extension);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not read extensions from {extensionsDir}: {ex.Message}");
        }

        return extensions.AsReadOnly();
    }

    /// <summary>
    /// Reads extensions from a profile's extensions.json configuration file
    /// </summary>
    private async Task<IReadOnlyList<VsCodeExtension>> ReadExtensionsFromConfigAsync(string configPath)
    {
        try
        {
            var json = await File.ReadAllTextAsync(configPath);
            var config = JsonSerializer.Deserialize<JsonElement>(json);
            
            // TODO: Parse the extensions.json format and convert to VsCodeExtension objects
            // For now, fall back to reading from directory
            return await ReadExtensionsFromDirectoryAsync(VsCodePaths.GetExtensionsDirectory());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not read extensions config from {configPath}: {ex.Message}");
            return await ReadExtensionsFromDirectoryAsync(VsCodePaths.GetExtensionsDirectory());
        }
    }

    /// <summary>
    /// Reads extension metadata from a single extension directory
    /// </summary>
    private async Task<VsCodeExtension?> ReadExtensionFromDirectoryAsync(string extensionDir)
    {
        try
        {
            var packageJsonPath = Path.Combine(extensionDir, "package.json");
            if (!File.Exists(packageJsonPath))
                return null;

            var json = await File.ReadAllTextAsync(packageJsonPath);
            var package = JsonSerializer.Deserialize<JsonElement>(json);

            var name = package.TryGetProperty("name", out var nameProperty) ? nameProperty.GetString() : null;
            var displayName = package.TryGetProperty("displayName", out var displayNameProperty) ? displayNameProperty.GetString() : name;
            var version = package.TryGetProperty("version", out var versionProperty) ? versionProperty.GetString() : "unknown";
            var publisher = package.TryGetProperty("publisher", out var publisherProperty) ? publisherProperty.GetString() : "unknown";
            var description = package.TryGetProperty("description", out var descProperty) ? descProperty.GetString() : null;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(publisher))
                return null;

            var extensionId = $"{publisher}.{name}";
            var installDate = Directory.GetCreationTime(extensionDir);

            return new VsCodeExtension
            {
                Id = extensionId,
                DisplayName = displayName ?? name,
                Version = version ?? "unknown",
                Publisher = publisher,
                Description = description,
                IsBuiltIn = false,
                IsEnabled = true, // Assume enabled if present
                InstallDate = installDate
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not read extension from {extensionDir}: {ex.Message}");
            return null;
        }
    }
}