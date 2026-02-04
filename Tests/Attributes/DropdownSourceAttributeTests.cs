using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Tests.Attributes;

[TestFixture]
public class DropdownSourceAttributeTests
{
    [Test]
    public void Constructor_WithType_SetsProperty()
    {
        var attribute = new DropdownSourceAttribute(typeof(TestProvider));
        
        Assert.That(attribute.SourceType, Is.EqualTo(typeof(TestProvider)));
    }

    [Test]
    public void DropdownSourceAttribute_CanBeAppliedToProperty()
    {
        var property = typeof(TestModel).GetProperty(nameof(TestModel.Country));
        var attribute = property?.GetCustomAttributes(typeof(DropdownSourceAttribute), false).FirstOrDefault() as DropdownSourceAttribute;
        
        Assert.That(attribute, Is.Not.Null);
        Assert.That(attribute!.SourceType, Is.EqualTo(typeof(TestProvider)));
    }

    private class TestModel
    {
        [DropdownSource(typeof(TestProvider))]
        public string Country { get; set; } = "";
    }

    private class TestProvider : IDropdownSourceProvider
    {
        public void Initialize(IWatchHandler handler) { }
        public IEnumerable<CoreOne.Winforms.Models.DropdownItem> GetItems() => Array.Empty<CoreOne.Winforms.Models.DropdownItem>();
        public void Dispose() { }
    }
}
