using CoreOne.Attributes;
using CoreOne.Models;
using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Tests.Attributes;

[TestFixture]
public class EnabledWhenAttributeTests
{
    [Test]
    public void Constructor_WithPropertyAndValue_SetsProperties()
    {
        var attribute = new EnabledWhenAttribute("IsActive", true);
        Assert.Multiple(() => {
            Assert.That(attribute.PropertyName, Is.EqualTo("IsActive"));
            Assert.That(attribute.ExpectedValue, Is.EqualTo(true));
            Assert.That(attribute.ComparisonType, Is.EqualTo(ComparisonType.EqualTo));
        });
    }

    [Test]
    public void Constructor_WithComparisonType_SetsCorrectly()
    {
        var attribute = new EnabledWhenAttribute("Age", 18, ComparisonType.GreaterThanOrEqualTo);
        
        Assert.That(attribute.ComparisonType, Is.EqualTo(ComparisonType.GreaterThanOrEqualTo));
    }
}
