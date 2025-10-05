using NUnit.Framework;

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
  public void DummyTest_BasicAssertion_ShouldPass()
  {
    // Arrange
    var expected = "Hello, VSXList!";
    var actual = "Hello, VSXList!";

    // Act & Assert
    Assert.That(actual, Is.EqualTo(expected));
    Assert.That(actual, Is.Not.Null);
    Assert.That(actual.Length, Is.GreaterThan(0));
  }

  [Test]
  public void DummyTest_NumberComparison_ShouldPass()
  {
    // Arrange
    var number1 = 42;
    var number2 = 21;

    // Act
    var result = number1 + number2;

    // Assert
    Assert.That(result, Is.EqualTo(63));
    Assert.That(result, Is.GreaterThan(number1));
    Assert.That(result, Is.GreaterThan(number2));
  }
}
