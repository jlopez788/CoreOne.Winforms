using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Tests.Attributes;

[TestFixture]
public class IgnoreAttributeTests
{
    [Test]
    public void IgnoreAttribute_CanBeAppliedToProperty()
    {
        var property = typeof(TestModel).GetProperty(nameof(TestModel.IgnoredProperty));
        var attribute = property?.GetCustomAttributes(typeof(CoreOne.Winforms.Attributes.IgnoreAttribute), false).FirstOrDefault();
        
        Assert.That(attribute, Is.Not.Null);
        Assert.That(attribute, Is.InstanceOf<CoreOne.Winforms.Attributes.IgnoreAttribute>());
    }

    private class TestModel
    {
        [CoreOne.Winforms.Attributes.Ignore]
        public string IgnoredProperty { get; set; } = "";
    }
}
