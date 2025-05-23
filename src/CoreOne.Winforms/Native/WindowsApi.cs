using System.Runtime.InteropServices;

namespace CoreOne.Winforms.Native;

public static partial class WindowsApi
{
    [DllImport("gdi32.dll")]
    public static extern bool BitBlt(
       IntPtr hdcDest, // handle to destination DC
       int nXDest,     // x-coord of destination upper-left corner
       int nYDest,     // y-coord of destination upper-left corner
       int nWidth,     // width of destination rectangle
       int nHeight,    // height of destination rectangle
       IntPtr hdcSrc,  // handle to source DC
       int nXSrc,      // x-coordinate of source upper-left corner
       int nYSrc,      // y-coordinate of source upper-left corner
       int dwRop     // raster operation code
   );

    [DllImport("user32.dll")]
    public static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32")]
    public static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam);

    [DllImport("user32.dll")]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    public static void ShowToFront(IntPtr window)
    {
        ShowWindow(window, SW_SHOWNORMAL);
        SetForegroundWindow(window);
    }

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}