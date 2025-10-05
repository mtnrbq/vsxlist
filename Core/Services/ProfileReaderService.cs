using System.Text.Json;
using Core.Constants;
using Core.Models;

namespace Core.Services;

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

            // Read profile name mappings from sync configuration
            var profileNameMap = await ReadProfileNameMappingsAsync();

            var profileDirectories = Directory.GetDirectories(profilesDir);
            
            foreach (var profileDir in profileDirectories)
            {
                var profile = await ReadProfileFromDirectoryAsync(profileDir, profileNameMap);
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
    /// Reads a profile from a specific directory (overload for testing)
    /// </summary>
    internal async Task<VsCodeProfile?> ReadProfileFromDirectoryAsync(string profileDir)
    {
        return await ReadProfileFromDirectoryAsync(profileDir, null);
    }

    /// <summary>
    /// Reads a profile from a specific directory
    /// </summary>
    internal async Task<VsCodeProfile?> ReadProfileFromDirectoryAsync(string profileDir, Dictionary<string, string>? profileNameMap = null)
    {
        try
        {
            var profileId = Path.GetFileName(profileDir);
            var profileName = profileNameMap?.GetValueOrDefault(profileId) ?? profileId;
            var extensionsConfigPath = VsCodePaths.GetExtensionsConfigPath(profileDir);
            
            var extensions = File.Exists(extensionsConfigPath)
                ? await ReadExtensionsFromConfigAsync(extensionsConfigPath)
                : new List<VsCodeExtension>();

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
    /// Reads profile name mappings from VS Code sync configuration
    /// </summary>
    private async Task<Dictionary<string, string>> ReadProfileNameMappingsAsync()
    {
        var mappings = new Dictionary<string, string>();
        
        try
        {
            var userDataDir = VsCodePaths.GetUserDataDirectory();
            var syncProfilesPath = Path.Combine(userDataDir, "sync", "profiles", "lastSyncprofiles.json");
            
            if (!File.Exists(syncProfilesPath))
                return mappings;

            var jsonContent = await File.ReadAllTextAsync(syncProfilesPath);
            var syncDoc = JsonDocument.Parse(jsonContent);
            
            if (syncDoc.RootElement.TryGetProperty("syncData", out var syncData) &&
                syncData.TryGetProperty("content", out var content))
            {
                var profilesJson = content.GetString();
                if (!string.IsNullOrEmpty(profilesJson))
                {
                    var profilesDoc = JsonDocument.Parse(profilesJson);
                    if (profilesDoc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var profile in profilesDoc.RootElement.EnumerateArray())
                        {
                            if (profile.TryGetProperty("id", out var id) &&
                                profile.TryGetProperty("name", out var name))
                            {
                                var profileId = id.GetString();
                                var profileName = name.GetString();
                                if (!string.IsNullOrEmpty(profileId) && !string.IsNullOrEmpty(profileName))
                                {
                                    mappings[profileId] = profileName;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not read profile name mappings: {ex.Message}");
        }

        return mappings;
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
            var extensionsArray = JsonSerializer.Deserialize<JsonElement[]>(json);
            
            if (extensionsArray == null || extensionsArray.Length == 0)
                return new List<VsCodeExtension>();

            var extensions = new List<VsCodeExtension>();
            
            foreach (var extensionElement in extensionsArray)
            {
                try
                {
                    var extension = ParseExtensionFromConfig(extensionElement);
                    if (extension != null)
                        extensions.Add(extension);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not parse extension from config: {ex.Message}");
                }
            }
            
            return extensions;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not read extensions config from {configPath}: {ex.Message}");
            return new List<VsCodeExtension>();
        }
    }

    /// <summary>
    /// Parses a VS Code extension from the extensions.json configuration format
    /// </summary>
    private VsCodeExtension? ParseExtensionFromConfig(JsonElement extensionElement)
    {
        try
        {
            // VS Code extensions.json format has nested structure:
            // {
            //   "identifier": { "id": "publisher.extensionName" },
            //   "metadata": { ... extension details ... }
            // }
            
            if (!extensionElement.TryGetProperty("identifier", out var identifierElement) ||
                !identifierElement.TryGetProperty("id", out var idElement))
            {
                return null;
            }

            var id = idElement.GetString();
            if (string.IsNullOrEmpty(id))
                return null;

            var displayName = id;
            var version = "unknown";
            var description = "";
            var publisher = "";
            var category = "";
            var repository = "";

            // Extract metadata if available
            if (extensionElement.TryGetProperty("metadata", out var metadataElement))
            {
                if (metadataElement.TryGetProperty("displayName", out var displayNameProp))
                    displayName = displayNameProp.GetString() ?? id;
                
                if (metadataElement.TryGetProperty("version", out var versionProp))
                    version = versionProp.GetString() ?? "unknown";
                
                if (metadataElement.TryGetProperty("description", out var descProp))
                    description = descProp.GetString() ?? "";
                
                if (metadataElement.TryGetProperty("publisherDisplayName", out var publisherProp))
                    publisher = publisherProp.GetString() ?? "";
            }

            return new VsCodeExtension
            {
                Id = id,
                DisplayName = displayName,
                Version = version,
                Description = description,
                Publisher = publisher,
                Category = category,
                Repository = repository
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not parse extension element: {ex.Message}");
            return null;
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