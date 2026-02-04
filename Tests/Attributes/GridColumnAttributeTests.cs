using CoreOne.Winforms.Attributes;
using CoreOne.Winforms.Models;

namespace CoreOne.Winforms.Tests.Attributes;

[TestFixture]
public class GridColumnAttributeTests
{
    [Test]
    public void Constructor_WithColumnSpan_SetsSpan()
    {
        var attribute = new GridColumnAttribute(GridColumnSpan.Half);
        
        Assert.That(attribute.Span, Is.EqualTo(GridColumnSpan.Half));
    }

    [Test]
    public void Constructor_DefaultSetsFullSpan()
    {
        var attribute = new GridColumnAttribute();
        
        Assert.That(attribute.Span, Is.EqualTo(GridColumnSpan.Default));
    }
}
