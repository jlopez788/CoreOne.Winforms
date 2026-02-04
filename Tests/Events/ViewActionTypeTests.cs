using CoreOne.Winforms.Events;

namespace CoreOne.Winforms.Tests.Events;

[TestFixture]
public class ViewActionTypeTests
{
    [Test]
    public void ViewActionType_HasCorrectValues()
    {
        Assert.Multiple(() => {
            Assert.That((int)ViewActionType.GoToNamedView, Is.EqualTo(0));
            Assert.That((int)ViewActionType.GoBackView, Is.EqualTo(1));
            Assert.That((int)ViewActionType.GoHomeView, Is.EqualTo(2));
        });
    }
}
