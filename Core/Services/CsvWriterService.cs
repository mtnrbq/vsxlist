using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Core.Models;

namespace Core.Services;

/// <summary>
/// Service for writing VS Code extensions to CSV files
/// </summary>
public class CsvWriterService
{
    /// <summary>
    /// Writes extensions from all profiles to a CSV file
    /// </summary>
    public async Task WriteExtensionsToCsvAsync(IReadOnlyList<VsCodeProfile> profiles, string outputPath)
    {
        var allExtensions = profiles
            .SelectMany(profile => profile.Extensions.Select(ext => ext with { ProfileName = profile.Name }))
            .OrderBy(ext => ext.ProfileName)
            .ThenBy(ext => ext.Publisher)
            .ThenBy(ext => ext.DisplayName)
            .ToList();

        await WriteExtensionsToCsvAsync(allExtensions, outputPath);
    }

    /// <summary>
    /// Writes a collection of extensions to a CSV file
    /// </summary>
    public async Task WriteExtensionsToCsvAsync(IReadOnlyList<VsCodeExtension> extensions, string outputPath)
    {
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        });

        // Write header
        csv.WriteField("Profile");
        csv.WriteField("ExtensionID");
        csv.WriteField("DisplayName");
        csv.WriteField("Publisher");
        csv.WriteField("Version");
        csv.WriteField("Description");
        csv.WriteField("Category");
        csv.WriteField("Enabled");
        csv.WriteField("BuiltIn");
        csv.WriteField("InstallDate");
        csv.WriteField("Repository");
        csv.WriteField("InstallCount");
        csv.WriteField("Rating");
        csv.NextRecord();

        // Write data
        foreach (var extension in extensions)
        {
            csv.WriteField(extension.ProfileName ?? "Unknown");
            csv.WriteField(extension.Id);
            WriteFieldWithQuoting(csv, extension.DisplayName, true); // Column 3 - quoted
            csv.WriteField(extension.Publisher);
            csv.WriteField(extension.Version);
            WriteFieldWithQuoting(csv, extension.Description, true); // Column 6 - quoted
            csv.WriteField(extension.Category);
            csv.WriteField(extension.IsEnabled ? "Yes" : "No");
            csv.WriteField(extension.IsBuiltIn ? "Yes" : "No");
            WriteFieldWithQuoting(csv, extension.InstallDate?.ToString("yyyy-MM-dd HH:mm:ss"), true); // Column 10 - quoted
            csv.WriteField(extension.Repository);
            csv.WriteField(extension.InstallCount?.ToString());
            csv.WriteField(extension.Rating?.ToString("F1"));
            csv.NextRecord();
        }

        await File.WriteAllTextAsync(outputPath, writer.ToString());
        Console.WriteLine($"Extensions exported to: {outputPath}");
        Console.WriteLine($"Total extensions: {extensions.Count}");
    }

    /// <summary>
    /// Writes extensions grouped by profile to separate CSV files
    /// </summary>
    public async Task WriteExtensionsByProfileAsync(IReadOnlyList<VsCodeProfile> profiles, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        foreach (var profile in profiles)
        {
            var fileName = SanitizeFileName($"extensions-{profile.Name}.csv");
            var outputPath = Path.Combine(outputDirectory, fileName);
            
            var extensionsWithProfile = profile.Extensions
                .Select(ext => ext with { ProfileName = profile.Name })
                .OrderBy(ext => ext.Publisher)
                .ThenBy(ext => ext.DisplayName)
                .ToList();

            await WriteExtensionsToCsvAsync(extensionsWithProfile, outputPath);
        }
    }

    /// <summary>
    /// Creates a summary CSV with extension counts per profile
    /// </summary>
    public async Task WriteProfileSummaryAsync(IReadOnlyList<VsCodeProfile> profiles, string outputPath)
    {
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        });

        // Write header
        csv.WriteField("ProfileName");
        csv.WriteField("ExtensionCount");
        csv.WriteField("BuiltInExtensions");
        csv.WriteField("ThirdPartyExtensions");
        csv.WriteField("ProfilePath");
        csv.WriteField("LastModified");
        csv.NextRecord();

        // Write data
        foreach (var profile in profiles.OrderBy(p => p.Name))
        {
            var builtInCount = profile.Extensions.Count(e => e.IsBuiltIn);
            var thirdPartyCount = profile.Extensions.Count(e => !e.IsBuiltIn);

            csv.WriteField(profile.Name);
            csv.WriteField(profile.Extensions.Count.ToString());
            csv.WriteField(builtInCount.ToString());
            csv.WriteField(thirdPartyCount.ToString());
            csv.WriteField(profile.Path);
            csv.WriteField(profile.LastModified?.ToString("yyyy-MM-dd HH:mm:ss"));
            csv.NextRecord();
        }

        await File.WriteAllTextAsync(outputPath, writer.ToString());
        Console.WriteLine($"Profile summary exported to: {outputPath}");
    }



    /// <summary>
    /// Writes a field with optional quoting
    /// </summary>
    internal static void WriteFieldWithQuoting(CsvWriter csv, string? value, bool forceQuote)
    {
        if (forceQuote && !string.IsNullOrEmpty(value))
        {
            // Escape any existing quotes and wrap in quotes
            var escapedValue = value.Replace("\"", "\"\"");
            csv.WriteField($"\"{escapedValue}\"", false); // false means don't auto-quote
        }
        else
        {
            csv.WriteField(value);
        }
    }

    /// <summary>
    /// Sanitizes a filename by removing invalid characters
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }
}