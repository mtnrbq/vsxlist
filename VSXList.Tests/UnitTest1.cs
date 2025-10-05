using System.Text.RegularExpressions;
using Xunit;
using VSXList.Models;
using VSXList.Services;

namespace VSXList.Tests;

public class CsvWriterServiceTests
{
    [Fact]
    public void WriteExtensionsToCsv_ShouldNotContainDoubledDoubleQuotes_ExceptForEmptyFields()
    {
        // Arrange
        var csvWriter = new CsvWriterService();
        var testExtensions = new List<VsCodeExtension>
        {
            new VsCodeExtension
            {
                Id = "test.extension",
                DisplayName = "Test Extension with \"quotes\" inside",
                Publisher = "TestPublisher",
                Version = "1.0.0",
                Description = "A test extension with \"embedded quotes\" and special characters",
                Category = null, // This should result in empty field
                IsEnabled = true,
                IsBuiltIn = false,
                ProfileName = "TestProfile",
                InstallDate = new DateTime(2025, 10, 5, 12, 30, 45)
            },
            new VsCodeExtension
            {
                Id = "another.test",
                DisplayName = "Another Test",
                Publisher = "AnotherPublisher", 
                Version = "2.0.0",
                Description = null, // This should result in empty field
                IsEnabled = false,
                IsBuiltIn = true,
                ProfileName = "TestProfile",
                InstallDate = null // This should result in empty field
            }
        };

        var outputPath = Path.GetTempFileName();

        try
        {
            // Act
            csvWriter.WriteExtensionsToCsvAsync(testExtensions, outputPath).Wait();
            var csvContent = File.ReadAllText(outputPath);

            // Assert
            // Check that we don't have any doubled double quotes except for empty fields
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines.Skip(1)) // Skip header
            {
                // Check that we don't have """ (triple quotes) anywhere
                Assert.DoesNotContain("\"\"\"", line);
                
                // Check for doubled quotes that are not empty fields
                // Pattern: "" that are not part of ,"", pattern
                var invalidPattern = @"(?<!^|,)""""(?!,|$)";
                var invalidMatches = Regex.Matches(line, invalidPattern);
                Assert.Empty(invalidMatches);
            }

            // Verify that quoted fields (columns 3, 6, 10) are properly quoted
            var dataLines = lines.Skip(1);
            foreach (var line in dataLines)
            {
                var fields = ParseCsvLine(line);
                
                // Column 3 (DisplayName) should be quoted if not empty
                if (!string.IsNullOrEmpty(fields[2]) && fields[2] != "\"\"")
                {
                    Assert.StartsWith("\"", fields[2]);
                    Assert.EndsWith("\"", fields[2]);
                }
                
                // Column 6 (Description) should be quoted if not empty  
                if (fields.Length > 5 && !string.IsNullOrEmpty(fields[5]) && fields[5] != "\"\"")
                {
                    Assert.StartsWith("\"", fields[5]);
                    Assert.EndsWith("\"", fields[5]);
                }
                
                // Column 10 (InstallDate) should be quoted if not empty
                if (fields.Length > 9 && !string.IsNullOrEmpty(fields[9]) && fields[9] != "\"\"")
                {
                    Assert.StartsWith("\"", fields[9]);
                    Assert.EndsWith("\"", fields[9]);
                }
            }

            // Verify content contains expected test data
            Assert.Contains("Test Extension with", csvContent);
            Assert.Contains("embedded quotes", csvContent);
        }
        finally
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }

    [Fact]
    public void WriteExtensionsToCsv_ShouldHandleEmptyFieldsCorrectly()
    {
        // Arrange
        var csvWriter = new CsvWriterService();
        var testExtensions = new List<VsCodeExtension>
        {
            new VsCodeExtension
            {
                Id = "test.empty",
                DisplayName = "", // Empty string
                Publisher = "TestPub",
                Version = "1.0.0",
                Description = null, // Null value
                IsEnabled = true,
                IsBuiltIn = false,
                ProfileName = "TestProfile"
            }
        };

        var outputPath = Path.GetTempFileName();

        try
        {
            // Act
            csvWriter.WriteExtensionsToCsvAsync(testExtensions, outputPath).Wait();
            var csvContent = File.ReadAllText(outputPath);

            // Assert
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var dataLine = lines[1]; // Skip header
            
            // Should contain empty quoted fields for empty DisplayName and null Description
            Assert.Contains("\"\",", dataLine); // Empty DisplayName (column 3)
            Assert.Contains(",\"\",", dataLine); // Empty Description (column 6)
        }
        finally
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }

    private static string[] ParseCsvLine(string line)
    {
        // Simple CSV parser for testing - handles quoted fields
        var fields = new List<string>();
        var currentField = "";
        var inQuotes = false;
        var i = 0;

        while (i < line.Length)
        {
            var c = line[i];
            
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    // Escaped quote
                    currentField += '"';
                    i += 2;
                }
                else
                {
                    // Start or end of quoted field
                    inQuotes = !inQuotes;
                    currentField += c;
                    i++;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField);
                currentField = "";
                i++;
            }
            else
            {
                currentField += c;
                i++;
            }
        }
        
        fields.Add(currentField);
        return fields.ToArray();
    }
}
