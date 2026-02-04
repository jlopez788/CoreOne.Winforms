using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Tests.Attributes;

[TestFixture]
public class WatchPropertiesAttributeTests
{
    [Test]
    public void Constructor_WithPropertyNames_SetsPropertyNames()
    {
        var attribute = new WatchPropertiesAttribute("Property1", "Property2", "Property3");
        
        Assert.That(attribute.PropertyNames, Is.Not.Null);
        Assert.That(attribute.PropertyNames.Count, Is.EqualTo(3));
        Assert.That(attribute.PropertyNames, Does.Contain("Property1"));
    }

    [Test]
    public void PropertyNames_IsCaseInsensitive()
    {
        var attribute = new WatchPropertiesAttribute("Property");
        Assert.Multiple(() => {
            Assert.That(attribute.PropertyNames.Contains("property"), Is.True);
            Assert.That(attribute.PropertyNames.Contains("PROPERTY"), Is.True);
        });
    }
}
