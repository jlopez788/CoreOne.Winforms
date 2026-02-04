using CoreOne.Winforms.Models;

namespace CoreOne.Winforms.Tests.Models;

[TestFixture]
public class DropdownItemTests
{
    [Test]
    public void Constructor_WithDisplayAndValue_SetsProperties()
    {
        var item = new DropdownItem("United States", "US");
        Assert.Multiple(() => {
            Assert.That(item.Display, Is.EqualTo("United States"));
            Assert.That(item.Value, Is.EqualTo("US"));
        });
    }

    [Test]
    public void Constructor_WithValueOnly_UsesValueForBoth()
    {
        var item = new DropdownItem("US");
        Assert.Multiple(() => {
            Assert.That(item.Display, Is.EqualTo("US"));
            Assert.That(item.Value, Is.EqualTo("US"));
        });
    }

    [Test]
    public void Constructor_Default_SetsEmptyValues()
    {
        var item = new DropdownItem();
        Assert.Multiple(() => {
            Assert.That(item.Display, Is.EqualTo(string.Empty));
            Assert.That(item.Value, Is.Null);
        });
    }

    [Test]
    public void Value_CanBeModified()
    {
        var item = new DropdownItem("United States", "US");
        item.Value = "USA";
        
        Assert.That(item.Value, Is.EqualTo("USA"));
    }
}
