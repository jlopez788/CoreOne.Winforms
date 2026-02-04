using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Tests.Attributes;

[TestFixture]
public class SourceTypeAttributeTests
{
    [Test]
    public void Constructor_WithType_SetsProperty()
    {
        var attribute = new SourceTypeAttribute(typeof(string));
        
        Assert.That(attribute.SourceType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void SourceTypeAttribute_CanBeAppliedToProperty()
    {
        var property = typeof(TestModel).GetProperty(nameof(TestModel.Items));
        var attribute = property?.GetCustomAttributes(typeof(SourceTypeAttribute), false).FirstOrDefault() as SourceTypeAttribute;
        
        Assert.That(attribute, Is.Not.Null);
        Assert.That(attribute!.SourceType, Is.EqualTo(typeof(int)));
    }

    private class TestModel
    {
        [SourceType(typeof(int))]
        public object? Items { get; set; }
    }
}
