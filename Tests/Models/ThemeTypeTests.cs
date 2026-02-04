using CoreOne.Winforms.Models;

namespace CoreOne.Winforms.Tests.Models;

[TestFixture]
public class ThemeTypeTests
{
    [Test]
    public void ThemeType_HasExpectedValues()
    {
        Assert.Multiple(() => {
            Assert.That((int)ThemeType.Default, Is.EqualTo(0));
            Assert.That((int)ThemeType.Primary, Is.EqualTo(1));
            Assert.That((int)ThemeType.Success, Is.EqualTo(2));
            Assert.That((int)ThemeType.Warning, Is.EqualTo(3));
            Assert.That((int)ThemeType.Danger, Is.EqualTo(4));
            Assert.That((int)ThemeType.DefaultPrimary, Is.EqualTo(5));
        });
    }
}
