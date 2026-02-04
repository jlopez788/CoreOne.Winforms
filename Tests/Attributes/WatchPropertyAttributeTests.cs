using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Tests.Attributes;

[TestFixture]
public class WatchPropertyAttributeTests
{
    [Test]
    public void Constructor_WithPropertyName_SetsProperty()
    {
        var attribute = new WatchPropertyAttribute("Status");
        
        Assert.That(attribute.PropertyName, Is.EqualTo("Status"));
    }

    [Test]
    public void WatchPropertyAttribute_CanBeAppliedToProperty()
    {
        var property = typeof(TestModel).GetProperty(nameof(TestModel.DisplayName));
        var attribute = property?.GetCustomAttributes(typeof(WatchPropertyAttribute), false).FirstOrDefault() as WatchPropertyAttribute;
        
        Assert.That(attribute, Is.Not.Null);
        Assert.That(attribute!.PropertyName, Is.EqualTo("Name"));
    }

    private class TestModel
    {
        [WatchProperty("Name")]
        public string DisplayName { get; set; } = "";
    }
}
