
using System;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace DupploPulse.UsImaging.Standalone.UI;

public class ImageAreaHost : HwndHost
{
    private nint hwndHost;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName,
        int dwStyle, int x, int y, int nWidth, int nHeight, nint hWndParent, nint hMenu, nint hInstance,
        nint lpParam);

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        hwndHost = CreateWindowEx(0, "static", "", 0x40000000, 0, 0, (int)Width, (int)Height, hwndParent.Handle,
            nint.Zero, nint.Zero, nint.Zero);
        return new HandleRef(this, hwndHost);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        if (hwnd.Handle != nint.Zero)
        {
            // Destroy the window
        }
    }

    // Additional methods to handle rendering can be added here
}

