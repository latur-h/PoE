using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.InteropServices
{
    internal static class Interop
    {
        internal const int WM_VSCROLL = 0x0115;
        internal const int SB_BOTTOM = 7;
        internal const int SM_CXSCREEN = 0;
        internal const int SM_CYSCREEN = 1;

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        internal static extern int GetSystemMetrics(int nIndex);
        [DllImport("user32.dll")]
        internal static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        internal static extern uint GetPixel(IntPtr hdc, int x, int y);

        [DllImport("user32.dll")]
        internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("kernel32.dll")]
        internal static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        internal static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        internal static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        internal static extern bool GetCursorPos(out POINT lpPoint);
    }
}
