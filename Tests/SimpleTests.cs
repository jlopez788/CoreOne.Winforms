namespace CoreOne.Winforms.Tests;

[TestFixture]
public class SimpleTests
{
    [Test]
    public void StringAlign_PropertiesExist()
    {
        Assert.Multiple(() => {
            Assert.That(StringAlign.Left, Is.Not.Null);
            Assert.That(StringAlign.Center, Is.Not.Null);
            Assert.That(StringAlign.Right, Is.Not.Null);
            Assert.That(StringAlign.TopLeft, Is.Not.Null);
            Assert.That(StringAlign.TopCenter, Is.Not.Null);
            Assert.That(StringAlign.TopRight, Is.Not.Null);
        });
    }
}
