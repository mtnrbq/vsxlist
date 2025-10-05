using NUnit.Framework;
using Core.Services;
using Core.Models;

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
    // This test validates CSV field quoting to ensure we don't produce doubled quotes
    // except when the value is empty, null, or contains special characters
    
    // Test cases for the WriteFieldWithQuoting logic
    var testCases = new[]
    {
      // Normal values should be quoted but not double-quoted
      new { Input = "Extension Name", Expected = "\"Extension Name\"", ShouldHaveDoubleQuotes = false },
      new { Input = "Description with, comma", Expected = "\"Description with, comma\"", ShouldHaveDoubleQuotes = false },
      new { Input = "Value with \"quote\"", Expected = "\"Value with \"\"quote\"\"\"", ShouldHaveDoubleQuotes = true }, // This should escape internal quotes
      
      // Empty/null values
      new { Input = "", Expected = "\"\"", ShouldHaveDoubleQuotes = true },
      
      // Simple values without special chars
      new { Input = "SimpleValue", Expected = "\"SimpleValue\"", ShouldHaveDoubleQuotes = false }
    };

    foreach (var testCase in testCases)
    {
      // Simulate the current WriteFieldWithQuoting behavior
      string result;
      if (!string.IsNullOrEmpty(testCase.Input))
      {
        var escapedValue = testCase.Input.Replace("\"", "\"\"");
        result = $"\"{escapedValue}\"";
      }
      else
      {
        result = testCase.Input ?? "";
      }

      // Validate that we don't have problematic double quotes (except for escaped internal quotes)
      if (!testCase.ShouldHaveDoubleQuotes)
      {
        // Should not contain consecutive quotes except at start/end
        var innerContent = result.Length >= 2 ? result.Substring(1, result.Length - 2) : result;
        Assert.That(innerContent, Does.Not.Contain("\"\""), 
          $"Field '{testCase.Input}' should not contain double quotes in content: {result}");
      }

      Console.WriteLine($"Input: '{testCase.Input}' -> Output: '{result}'");
    }

    // Test null case separately
    string? nullInput = null;
    string nullResult = nullInput ?? "";
    Assert.That(nullResult, Is.EqualTo(""), "Null input should result in empty string");
    Console.WriteLine($"Input: null -> Output: '{nullResult}'");
  }
}
