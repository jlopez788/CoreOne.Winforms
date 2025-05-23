namespace CoreOne.Winforms.Native;

public static partial class WindowsApi
{
    public const int SW_SHOWNA = 8;
    public const int SW_SHOWNORMAL = 1;
    public const int SWP_FRAMECHANGED = 0x0020;
    public const int SWP_NOACTIVATE = 0x0010;
    public const int SWP_NOMOVE = 0x0002;
    public const int SWP_NOREDRAW = 0x0008;
    public const int SWP_NOSIZE = 0x0001;
    public const int SWP_NOZORDER = 0x0004;

    public const int HWND_BROADCAST = 0xffff;
    public const int WA_ACTIVE = 1;
    public const int WA_CLICKACTIVE = 2;
    public const int WA_INACTIVE = 0;
    public const int WM_PAINT = 0x000F;
}