using NUnit.Framework;
using Core.Services;
using Core.Models;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Tests;

public class Tests
{
  [SetUp]
  public void Setup()
  {
  }

  [Test]
  public void Test1()
  {
    Assert.Pass();
  }

  [Test]
  public void CsvQuoting_ShouldNotProduceDoubleQuotes_ExceptForEmptyValues()
  {
    // This test validates CSV field quoting using actual VsCodeExtension models
    // to ensure we don't produce doubled quotes except when escaping internal quotes
    
    // Create test extensions with problematic field values for CSV quoting
    var testExtensions = new List<VsCodeExtension>
    {
      // Normal extension without special characters
      new VsCodeExtension
      {
        Id = "publisher.simple-extension",
        DisplayName = "Simple Extension",
        Version = "1.0.0",
        Publisher = "Publisher",
        Description = "A simple extension without special characters",
        IsBuiltIn = false,
        IsEnabled = true
      },
      
      // Extension with quotes in display name and description
      new VsCodeExtension
      {
        Id = "publisher.quoted-extension",
        DisplayName = "My \"Special\" Extension",
        Version = "2.1.0",
        Publisher = "QuotePublisher",
        Description = "An extension with \"quotes\" in description",
        IsBuiltIn = false,
        IsEnabled = true
      },
      
      // Extension with commas in description
      new VsCodeExtension
      {
        Id = "publisher.comma-extension",
        DisplayName = "Comma Extension",
        Version = "1.5.0",
        Publisher = "CommaPublisher",
        Description = "A great extension, really useful, with multiple commas",
        IsBuiltIn = false,
        IsEnabled = true
      },
      
      // Extension with empty/null optional fields
      new VsCodeExtension
      {
        Id = "publisher.minimal-extension",
        DisplayName = "Minimal Extension",
        Version = "0.1.0",
        Publisher = "MinimalPublisher",
        Description = "", // Empty description
        IsBuiltIn = true,
        IsEnabled = false
      },
      
      // Extension with null description
      new VsCodeExtension
      {
        Id = "publisher.null-desc-extension",
        DisplayName = "No Description Extension",
        Version = "3.0.0",
        Publisher = "NullPublisher",
        Description = null, // Null description
        IsBuiltIn = false,
        IsEnabled = true
      },
      
      // Extension with null ProfileName, Category, and Repository
      new VsCodeExtension
      {
        Id = "publisher.null-fields-extension",
        DisplayName = "Null Fields Extension",
        Version = "1.0.0",
        Publisher = "NullFieldsPublisher",
        Description = "Extension with null optional fields",
        ProfileName = null, // Null profile name
        Category = null, // Null category
        Repository = null, // Null repository
        IsBuiltIn = false,
        IsEnabled = true
      },
      
      // Extension with empty ProfileName, Category, and Repository
      new VsCodeExtension
      {
        Id = "publisher.empty-fields-extension",
        DisplayName = "Empty Fields Extension",
        Version = "2.0.0",
        Publisher = "EmptyFieldsPublisher",
        Description = "Extension with empty optional fields",
        ProfileName = "", // Empty profile name
        Category = "", // Empty category
        Repository = "", // Empty repository
        IsBuiltIn = false,
        IsEnabled = true
      },
      
      // Extension with special characters in ProfileName, Category, and Repository
      new VsCodeExtension
      {
        Id = "publisher.special-chars-extension",
        DisplayName = "Special Characters Extension",
        Version = "1.5.0",
        Publisher = "SpecialPublisher",
        Description = "Extension with special characters in optional fields",
        ProfileName = "Profile \"Dev\", Main", // Profile name with quotes and comma
        Category = "Languages, Tools & Extensions", // Category with commas
        Repository = "https://github.com/user/repo \"awesome\"", // Repository with quotes
        IsBuiltIn = false,
        IsEnabled = true
      }
    };

    // Create VsCodeProfile instances containing the test extensions
    var testProfiles = new List<VsCodeProfile>
    {
      // Main profile with most extensions
      new VsCodeProfile
      {
        Name = "Default",
        Path = "/home/user/.vscode/User",
        Extensions = new List<VsCodeExtension> 
        { 
          testExtensions[0], // Simple extension
          testExtensions[1], // Extension with quotes
          testExtensions[2], // Extension with commas
          testExtensions[5]  // Extension with null fields
        }.AsReadOnly(),
        IsDefault = true,
        LastModified = DateTime.Now.AddDays(-1)
      },
      
      // Development profile with problematic extensions
      new VsCodeProfile
      {
        Name = "Dev \"Environment\"", // Profile name with quotes
        Path = "/home/user/.vscode/profiles/dev-env",
        Extensions = new List<VsCodeExtension>
        {
          testExtensions[3], // Extension with empty description
          testExtensions[4], // Extension with null description  
          testExtensions[6], // Extension with empty fields
          testExtensions[7]  // Extension with special characters
        }.AsReadOnly(),
        IsDefault = false,
        LastModified = DateTime.Now.AddHours(-6)
      }
    };

    // Test the WriteFieldWithQuoting method directly using our test data
    using var writer = new StringWriter();
    using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));
    
    // Test cases for the WriteFieldWithQuoting logic
    var fieldTestCases = new[]
    {
      // Test DisplayName field values (should be force-quoted)
      new { FieldName = "DisplayName", Value = (string?)testExtensions[0].DisplayName, ForceQuote = true, ExpectedContent = (string?)"Simple Extension" },
      new { FieldName = "DisplayName", Value = (string?)testExtensions[1].DisplayName, ForceQuote = true, ExpectedContent = (string?)"My \"\"Special\"\" Extension" }, // Quotes should be escaped
      
      // Test Description field values (should be force-quoted)  
      new { FieldName = "Description", Value = (string?)testExtensions[2].Description, ForceQuote = true, ExpectedContent = (string?)"A great extension, really useful, with multiple commas" },
      new { FieldName = "Description", Value = (string?)testExtensions[3].Description, ForceQuote = true, ExpectedContent = (string?)"" }, // Empty string
      new { FieldName = "Description", Value = (string?)testExtensions[4].Description, ForceQuote = true, ExpectedContent = (string?)null }, // Null value
      
      // Test ProfileName field values
      new { FieldName = "ProfileName", Value = (string?)testExtensions[5].ProfileName, ForceQuote = false, ExpectedContent = (string?)null }, // Null value, no force quote
      new { FieldName = "ProfileName", Value = (string?)testExtensions[6].ProfileName, ForceQuote = false, ExpectedContent = (string?)"" }, // Empty string, no force quote
      new { FieldName = "ProfileName", Value = (string?)testExtensions[7].ProfileName, ForceQuote = false, ExpectedContent = (string?)"Profile \"Dev\", Main" }, // Special chars, no force quote
      
      // Additional edge cases for comprehensive testing
      new { FieldName = "EdgeCase1", Value = (string?)"\"", ForceQuote = true, ExpectedContent = (string?)"\"\"" }, // Single quote should be escaped
      new { FieldName = "EdgeCase2", Value = (string?)"\"\"", ForceQuote = true, ExpectedContent = (string?)"\"\"\"\"" }, // Double quote should be escaped
      new { FieldName = "EdgeCase3", Value = (string?)",", ForceQuote = true, ExpectedContent = (string?)"," }, // Single comma
      new { FieldName = "EdgeCase4", Value = (string?)"Field with\nnewline", ForceQuote = true, ExpectedContent = (string?)"Field with\nnewline" }, // Newline character
      new { FieldName = "EdgeCase5", Value = (string?)"   ", ForceQuote = true, ExpectedContent = (string?)"   " }, // Only whitespace
    };

    foreach (var testCase in fieldTestCases)
    {
      // Reset the writer for each test
      writer.GetStringBuilder().Clear();
      
      // Test the WriteFieldWithQuoting method directly
      CsvWriterService.WriteFieldWithQuoting(csv, testCase.Value, testCase.ForceQuote);
      
      var result = writer.ToString();
      
      Console.WriteLine($"{testCase.FieldName} '{testCase.Value}' (ForceQuote: {testCase.ForceQuote}) -> '{result}'");
      
      // Validate the quoting behavior with specific assertions
      if (testCase.ForceQuote && !string.IsNullOrEmpty(testCase.Value))
      {
        // Force-quoted non-empty fields should be wrapped in quotes
        Assert.That(result, Does.StartWith("\""), $"Force-quoted field {testCase.FieldName} should start with quote");
        Assert.That(result, Does.EndWith("\""), $"Force-quoted field {testCase.FieldName} should end with quote");
        Assert.That(result.Length, Is.GreaterThan(2), $"Force-quoted field {testCase.FieldName} should have content between quotes");
        
        // Extract content between quotes and verify proper escaping
        var innerContent = result.Substring(1, result.Length - 2);
        
        if (testCase.Value.Contains("\""))
        {
          // Original quotes should be escaped as double quotes
          Assert.That(innerContent, Contains.Substring("\"\""), $"Field {testCase.FieldName} with quotes should have escaped quotes (\"\"): got '{innerContent}'");
          
          // Verify that the original single quotes are NOT present (they should be escaped)
          var originalQuoteCount = testCase.Value.Count(c => c == '"');
          var escapedQuoteCount = innerContent.Split("\"\"").Length - 1;
          Assert.That(escapedQuoteCount, Is.EqualTo(originalQuoteCount), $"Field {testCase.FieldName} should have all quotes properly escaped");
        }
        else
        {
          // No quotes in original, so inner content should not contain any double quotes
          Assert.That(innerContent, Does.Not.Contain("\"\""), $"Field {testCase.FieldName} without quotes should not have doubled quotes in content");
          Assert.That(innerContent, Is.EqualTo(testCase.Value), $"Field {testCase.FieldName} content should match original value");
        }
      }
      else if (testCase.ForceQuote && string.IsNullOrEmpty(testCase.Value))
      {
        // Force-quoted empty/null fields should result in empty quotes or empty string
        if (testCase.Value == "")
        {
          Assert.That(result, Is.EqualTo("\"\""), $"Force-quoted empty field {testCase.FieldName} should result in empty quotes");
        }
        else // null case
        {
          Assert.That(result, Is.EqualTo("").Or.EqualTo("\"\""), $"Force-quoted null field {testCase.FieldName} should result in empty string or empty quotes");
        }
      }
      else if (!testCase.ForceQuote)
      {
        // Non-force-quoted fields should rely on CsvHelper's default behavior
        if (string.IsNullOrEmpty(testCase.Value))
        {
          Assert.That(result, Is.EqualTo("").Or.EqualTo("\"\""), $"Non-force-quoted null/empty field {testCase.FieldName} should result in empty string or quotes");
        }
        else if (testCase.Value.Contains(",") || testCase.Value.Contains("\"") || testCase.Value.Contains("\n"))
        {
          // CsvHelper should auto-quote fields with special characters
          Assert.That(result, Does.StartWith("\"").And.EndWith("\""), $"Non-force-quoted field {testCase.FieldName} with special chars should be auto-quoted by CsvHelper");
        }
        else
        {
          // Simple values might not be quoted by CsvHelper
          // Just verify the content is preserved
          var unquotedResult = result.Trim('"');
          Assert.That(unquotedResult, Is.EqualTo(testCase.Value).Or.EqualTo(testCase.Value?.Replace("\"", "\"\"")), 
            $"Non-force-quoted field {testCase.FieldName} should preserve content");
        }
      }
      
      // Additional validation: ensure we don't have problematic triple quotes or more
      Assert.That(result, Does.Not.Contain("\"\"\"\""), $"Field {testCase.FieldName} should not contain quadruple quotes or more: {result}");
      
      // Validate that the result is a single field (no commas outside quotes)
      if (result.StartsWith("\"") && result.EndsWith("\"") && result.Length > 1)
      {
        var innerContent = result.Substring(1, result.Length - 2);
        // Commas inside quotes are fine, but we shouldn't have unescaped quotes followed by commas
        Assert.That(result, Does.Not.Match(@"[^""]""[^""]"), $"Field {testCase.FieldName} should not have unescaped internal quotes: {result}");
      }
      
      // CSV Round-trip validation for critical test cases
      if (testCase.Value != null && (testCase.Value.Contains("\"") || testCase.Value.Contains(",") || testCase.Value.Contains("\n")))
      {
        // Test round-trip parsing to ensure no data loss
        var testCsv = $"TestField\n{result}";
        using var csvReader = new StringReader(testCsv);
        using var reader = new CsvReader(csvReader, new CsvConfiguration(CultureInfo.InvariantCulture));
        
        reader.Read();
        reader.ReadHeader();
        Assert.That(reader.Read(), Is.True, $"Should be able to read data row for {testCase.FieldName}");
        
        var parsedValue = reader.GetField("TestField");
        Assert.That(parsedValue, Is.EqualTo(testCase.Value), 
          $"Round-trip parsing failed for {testCase.FieldName}. Original: '{testCase.Value}', Parsed: '{parsedValue}'");
        
        Console.WriteLine($"  ✓ Round-trip validation passed for {testCase.FieldName}");
      }
    }
    
    Console.WriteLine($"\nCreated {testExtensions.Count} test extensions with various field values");
    Console.WriteLine($"Created {testProfiles.Count} test profiles containing the extensions");
    
    foreach (var profile in testProfiles)
    {
      Console.WriteLine($"Profile '{profile.Name}' ({profile.Extensions.Count} extensions):");
      foreach (var ext in profile.Extensions)
      {
        Console.WriteLine($"  {ext.Id}: DisplayName='{ext.DisplayName}', Description='{ext.Description}'");
        Console.WriteLine($"    ProfileName='{ext.ProfileName}', Category='{ext.Category}', Repository='{ext.Repository}'");
      }
    }
  }
}
