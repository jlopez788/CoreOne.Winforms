using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Tests.Attributes;

[TestFixture]
public class VisibleAttributeTests
{
    [Test]
    public void Constructor_WithTrueValue_SetsIsVisible()
    {
        var attribute = new VisibleAttribute(true);
        
        Assert.That(attribute.IsVisible, Is.True);
    }

    [Test]
    public void Constructor_WithFalseValue_SetsIsVisible()
    {
        var attribute = new VisibleAttribute(false);
        
        Assert.That(attribute.IsVisible, Is.False);
    }
}
