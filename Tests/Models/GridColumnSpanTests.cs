using CoreOne.Winforms.Models;

namespace CoreOne.Winforms.Tests.Models;

[TestFixture]
public class GridColumnSpanTests
{
    [Test]
    public void GridColumnSpan_ValuesAreCorrect()
    {
        Assert.Multiple(() => {
            Assert.That((int)GridColumnSpan.One, Is.EqualTo(1));
            Assert.That((int)GridColumnSpan.Two, Is.EqualTo(2));
            Assert.That((int)GridColumnSpan.Three, Is.EqualTo(3));
            Assert.That((int)GridColumnSpan.Four, Is.EqualTo(4));
            Assert.That((int)GridColumnSpan.Five, Is.EqualTo(5));
            Assert.That((int)GridColumnSpan.Six, Is.EqualTo(6));
        });
    }

    [Test]
    public void GridColumnSpan_Half_EqualsThree()
    {
        Assert.That(GridColumnSpan.Half, Is.EqualTo(GridColumnSpan.Three));
    }

    [Test]
    public void GridColumnSpan_Full_EqualsSix()
    {
        Assert.That(GridColumnSpan.Full, Is.EqualTo(GridColumnSpan.Six));
    }
}
