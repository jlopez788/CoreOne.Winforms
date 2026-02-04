using CoreOne.Winforms.Attributes;

namespace CoreOne.Winforms.Tests.Attributes;

[TestFixture]
public class ComputeAttributeTests
{
    [Test]
    public void Constructor_WithMethodName_SetsProperty()
    {
        var attribute = new ComputeAttribute("CalculateTotal");
        
        Assert.That(attribute.MethodName, Is.EqualTo("CalculateTotal"));
    }
}
