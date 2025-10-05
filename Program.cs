using VSXList.Services;

namespace VSXList;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            // Parse command line arguments
            var outputPath = GetArgValue(args, "--output", "-o") ?? Path.Combine(Environment.CurrentDirectory, "vscode-extensions.csv");
            var profileName = GetArgValue(args, "--profile", "-p");
            var separateFiles = HasArg(args, "--separate", "-s");
            var createSummary = HasArg(args, "--summary");
            var showHelp = HasArg(args, "--help", "-h");

            if (showHelp)
            {
                ShowHelp();
                return 0;
            }

            Console.WriteLine("VSXList - VS Code Extension Exporter");
            Console.WriteLine("=====================================");
            Console.WriteLine();

            var profileReader = new ProfileReaderService();
            var csvWriter = new CsvWriterService();

            Console.WriteLine("Discovering VS Code profiles...");
            var profiles = await profileReader.DiscoverProfilesAsync();

            if (profiles.Count == 0)
            {
                Console.WriteLine("No VS Code profiles found. Make sure VS Code is installed and has been run at least once.");
                return 1;
            }

            Console.WriteLine($"Found {profiles.Count} profile(s):");
            foreach (var p in profiles)
            {
                Console.WriteLine($"  - {p.Name}: {p.Extensions.Count} extensions");
            }
            Console.WriteLine();

            // Filter to specific profile if requested
            var profilesToExport = string.IsNullOrEmpty(profileName)
                ? profiles
                : profiles.Where(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase)).ToList();

            if (profilesToExport.Count == 0)
            {
                Console.WriteLine($"Profile '{profileName}' not found.");
                Console.WriteLine("Available profiles:");
                foreach (var p in profiles)
                {
                    Console.WriteLine($"  - {p.Name}");
                }
                return 1;
            }

            // Export based on options
            if (separateFiles)
            {
                var outputDir = Path.GetDirectoryName(outputPath) ?? Environment.CurrentDirectory;
                Console.WriteLine($"Exporting each profile to separate files in: {outputDir}");
                await csvWriter.WriteExtensionsByProfileAsync(profilesToExport, outputDir);
            }
            else
            {
                Console.WriteLine($"Exporting extensions to: {outputPath}");
                await csvWriter.WriteExtensionsToCsvAsync(profilesToExport, outputPath);
            }

            if (createSummary)
            {
                var summaryPath = Path.ChangeExtension(outputPath, null) + "-summary.csv";
                Console.WriteLine($"Creating profile summary: {summaryPath}");
                await csvWriter.WriteProfileSummaryAsync(profiles, summaryPath);
            }

            Console.WriteLine();
            Console.WriteLine("Export completed successfully!");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (HasArg(args, "--verbose", "-v"))
            {
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            return 1;
        }
    }

    static string? GetArgValue(string[] args, params string[] names)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (names.Contains(args[i], StringComparer.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }
        return null;
    }

    static bool HasArg(string[] args, params string[] names)
    {
        return args.Any(arg => names.Contains(arg, StringComparer.OrdinalIgnoreCase));
    }

    static void ShowHelp()
    {
        Console.WriteLine("VSXList - Extract VS Code extension lists to CSV");
        Console.WriteLine();
        Console.WriteLine("USAGE:");
        Console.WriteLine("  vsxlist [OPTIONS]");
        Console.WriteLine();
        Console.WriteLine("OPTIONS:");
        Console.WriteLine("  -o, --output <path>     Output CSV file path (default: vscode-extensions.csv)");
        Console.WriteLine("  -p, --profile <name>    Export specific profile only");
        Console.WriteLine("  -s, --separate          Create separate CSV files for each profile");
        Console.WriteLine("      --summary           Create a profile summary CSV file");
        Console.WriteLine("  -v, --verbose           Show detailed error information");
        Console.WriteLine("  -h, --help              Show this help message");
        Console.WriteLine();
        Console.WriteLine("EXAMPLES:");
        Console.WriteLine("  vsxlist                                    # Export all profiles to vscode-extensions.csv");
        Console.WriteLine("  vsxlist -o my-extensions.csv              # Export to custom file");
        Console.WriteLine("  vsxlist -p \"Work Profile\"                 # Export specific profile only");
        Console.WriteLine("  vsxlist -s --summary                      # Create separate files plus summary");
    }
}
