using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.InteropServices
{
    internal class InteropHelper
    {
        public static ResolutionType GetScreenResolution()
        {
            int width = Interop.GetSystemMetrics(Interop.SM_CXSCREEN);
            int height = Interop.GetSystemMetrics(Interop.SM_CYSCREEN);

            switch (width, height)
            {
                case (3840, 2160):
                    return ResolutionType.UHD; // 3840x2160
                case (2560, 1440):
                    return ResolutionType.QHD; // 2560x1440
                case (1920, 1080):
                    return ResolutionType.FullHD; // 1920x1080
                case (1280, 720):
                    return ResolutionType.HD; // 1280x720
                default:
                    throw new NotSupportedException("Unsupported screen resolution.");
            }
        }
        public static Color GetColorAt(int x, int y)
        {
            IntPtr hdc = Interop.GetDC(IntPtr.Zero);
            uint pixel = Interop.GetPixel(hdc, x, y);
            Interop.ReleaseDC(IntPtr.Zero, hdc);

            int red = (int)(pixel & 0x000000FF);
            int green = (int)(pixel & 0x0000FF00) >> 8;
            int blue = (int)(pixel & 0x00FF0000) >> 16;

            return Color.FromArgb(red, green, blue);
        }
        public static void ShowConsole()
        {
            if (Interop.GetConsoleWindow() == IntPtr.Zero)
            {
                Interop.AllocConsole();

                StreamWriter standardOutput = new StreamWriter(Console.OpenStandardOutput())
                {
                    AutoFlush = true
                };
                Console.SetOut(standardOutput);

                StreamWriter standardError = new StreamWriter(Console.OpenStandardError())
                {
                    AutoFlush = true
                };
                Console.SetError(standardError);

                StreamReader standardInput = new StreamReader(Console.OpenStandardInput());
                Console.SetIn(standardInput);
            }
        }
        public static void HideConsole()
        {
            if (Interop.GetConsoleWindow() != IntPtr.Zero)
                Interop.FreeConsole();
        }
        public static void ScrollToBottom(IntPtr hWnd)
        {
            Interop.SendMessage(hWnd, Interop.WM_VSCROLL, Interop.SB_BOTTOM, 0);
        }
        public static Coordinates GetMousePos()
        {
            Interop.GetCursorPos(out Interop.POINT point);

            Coordinates coordinates = new Coordinates(point.X, point.Y);

            return coordinates;
        }
    }
}
