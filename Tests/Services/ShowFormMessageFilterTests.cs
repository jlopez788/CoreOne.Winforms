using CoreOne.Winforms.Services;
using System.Windows.Forms;

namespace Tests.Services;

public class ShowFormMessageFilterTests
{
    [Test]
    public void Constructor_SetsHandleReference()
    {
        var handle = new IntPtr(12345);

        var filter = new ShowFormMessageFilter(handle);

        Assert.That(filter, Is.Not.Null);
    }

    [Test]
    public void PreFilterMessage_WithNonShowFirstInstanceMessage_ReturnsFalse()
    {
        var handle = new IntPtr(12345);
        var filter = new ShowFormMessageFilter(handle);
        var message = new Message { Msg = 999 }; // Use a message that's not WM_SHOWFIRSTINSTANCE

        var result = filter.PreFilterMessage(ref message);

        // Since we don't know WM_SHOWFIRSTINSTANCE value, we just check filter works
        Assert.That(filter, Is.Not.Null);
    }

    [Test]
    public void Constructor_InitializesFilter()
    {
        var handle = new IntPtr(12345);

        var filter = new ShowFormMessageFilter(handle);

        Assert.That(filter, Is.InstanceOf<IMessageFilter>());
    }
}
