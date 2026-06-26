namespace PoE.dlls.InteropServices
{
    /// <summary>
    /// Reads multiple screen pixels with a single <see cref="Interop.GetDC"/> / <see cref="Interop.ReleaseDC"/> pair.
    /// </summary>
    internal sealed class ScreenPixelCapture : IDisposable
    {
        private readonly IntPtr _hdc;
        private bool _disposed;

        public ScreenPixelCapture()
        {
            _hdc = Interop.GetDC(IntPtr.Zero);
            if (_hdc == IntPtr.Zero)
                throw new InvalidOperationException("GetDC failed.");
        }

        public Color GetColorAt(int x, int y)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            uint pixel = Interop.GetPixel(_hdc, x, y);
            return ScreenPixelColor.FromGdiPixel(pixel);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Interop.ReleaseDC(IntPtr.Zero, _hdc);
            _disposed = true;
        }
    }

    internal static class ScreenPixelColor
    {
        public static Color FromGdiPixel(uint pixel)
        {
            int red = (int)(pixel & 0x000000FF);
            int green = (int)(pixel & 0x0000FF00) >> 8;
            int blue = (int)(pixel & 0x00FF0000) >> 16;
            return Color.FromArgb(red, green, blue);
        }
    }
}
