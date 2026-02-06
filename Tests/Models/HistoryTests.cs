using CoreOne.Winforms.Models;

namespace CoreOne.Winforms.Tests.Models;

[TestFixture]
public class HistoryTests
{
    [Test]
    public void Constructor_SetsProperties()
    {
        var args = new object[] { "arg1", "arg2" };
        var history = new History("TestView", args);
        Assert.Multiple(() => {
            Assert.That(history.ViewName, Is.EqualTo("TestView"));
            Assert.That(history.Args, Is.EqualTo(args));
        });
    }

    [Test]
    public void Equals_SameInstance_ReturnsTrue()
    {
        var history1 = new History("TestView", []);
        Assert.Multiple(() => {
            Assert.That(history1, Is.EqualTo(history1)); 
        });
    }

    [Test]
    public void Equals_DifferentInstances_ReturnsFalse()
    {
        var history1 = new History("TestView", Array.Empty<object>());
        var history2 = new History("TestView", Array.Empty<object>());
        Assert.Multiple(() => {
            Assert.That(history1.Equals(history2), Is.False);
            Assert.That(history1 != history2, Is.True);
        });
    }

    [Test]
    public void GetHashCode_IsDifferentForDifferentInstances()
    {
        var history1 = new History("TestView", Array.Empty<object>());
        var history2 = new History("TestView", Array.Empty<object>());
        
        Assert.That(history1.GetHashCode(), Is.Not.EqualTo(history2.GetHashCode()));
    }
}
