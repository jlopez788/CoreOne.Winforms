using CoreOne.Winforms.Native;

namespace CoreOne.Winforms.Services;

public class ShowFormMessageFilter(IntPtr handle) : IMessageFilter
{
    protected IntPtr Handle { get; } = handle;

    public bool PreFilterMessage(ref Message m)
    {
        bool handled = false;
        if (m.Msg == Startup.WM_SHOWFIRSTINSTANCE)
        {
            handled = true;
            WindowsApi.ShowToFront(Handle);
        }
        return handled;
    }
}