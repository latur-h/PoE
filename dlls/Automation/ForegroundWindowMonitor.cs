using System.Runtime.InteropServices;

namespace PoE.dlls.Automation
{
    /// <summary>
    /// Raises <see cref="ForegroundChanged"/> when the foreground window changes
    /// (<c>EVENT_SYSTEM_FOREGROUND</c>).
    /// </summary>
    internal sealed class ForegroundWindowMonitor : IDisposable
    {
        private const uint EventSystemForeground = 0x0003;
        private const uint WineventOutOfContext = 0x0000;

        private delegate void WinEventProc(
            IntPtr hWinEventHook,
            uint eventType,
            IntPtr hwnd,
            int idObject,
            int idChild,
            uint dwEventThread,
            uint dwmsEventTime);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(
            uint eventMin,
            uint eventMax,
            IntPtr hmodWinEventProc,
            WinEventProc lpfnWinEventProc,
            uint idProcess,
            uint idThread,
            uint dwFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        private WinEventProc? _callback;
        private IntPtr _hook;
        private bool _disposed;

        public event Action? ForegroundChanged;

        public void Start()
        {
            if (_disposed || _hook != IntPtr.Zero)
                return;

            _callback = OnWinEvent;
            _hook = SetWinEventHook(
                EventSystemForeground,
                EventSystemForeground,
                IntPtr.Zero,
                _callback,
                0,
                0,
                WineventOutOfContext);

            if (_hook == IntPtr.Zero)
                throw new InvalidOperationException("SetWinEventHook failed.");
        }

        public void Stop()
        {
            if (_hook == IntPtr.Zero)
                return;

            UnhookWinEvent(_hook);
            _hook = IntPtr.Zero;
            _callback = null;
        }

        private void OnWinEvent(
            IntPtr hWinEventHook,
            uint eventType,
            IntPtr hwnd,
            int idObject,
            int idChild,
            uint dwEventThread,
            uint dwmsEventTime)
        {
            try
            {
                ForegroundChanged?.Invoke();
            }
            catch
            {
                // WinEvent callbacks must not throw.
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Stop();
            _disposed = true;
            ForegroundChanged = null;
        }
    }
}
