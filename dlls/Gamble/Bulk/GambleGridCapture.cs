using PoE.dlls.InteropServices;
using PoE.dlls.Logger;
using PoE.dlls.Settings.Mods;

namespace PoE.dlls.Gamble.Bulk
{
    internal enum GambleGridCapturePhase
    {
        Idle,
        WaitingDragStart,
        Dragging,
    }

    internal sealed class GambleGridCapture
    {
        private GambleGridCapturePhase _phase = GambleGridCapturePhase.Idle;
        private Coordinates _dragStart;
        private bool _wasLeftDown;

        public GambleGridCapturePhase Phase => _phase;

        public void ArmRectangleCapture()
        {
            _phase = GambleGridCapturePhase.WaitingDragStart;
            _wasLeftDown = IsLeftButtonDown();
            GamblerLog.Info("Drag LMB over the map grid area (top-left to bottom-right)");
        }

        public void Cancel()
        {
            if (_phase == GambleGridCapturePhase.Idle)
                return;

            _phase = GambleGridCapturePhase.Idle;
            GamblerLog.Info("Grid capture cancelled");
        }

        public bool Poll(GambleMapBulkSettings grid)
        {
            if (_phase == GambleGridCapturePhase.Idle)
                return false;

            bool leftDown = IsLeftButtonDown();

            if (_phase == GambleGridCapturePhase.WaitingDragStart)
            {
                if (leftDown && !_wasLeftDown)
                    _dragStart = InteropHelper.GetMousePos();

                if (!leftDown && _wasLeftDown && _phase == GambleGridCapturePhase.WaitingDragStart)
                {
                    var end = InteropHelper.GetMousePos();
                    grid.GridStart = _dragStart;
                    grid.GridEnd = end;
                    _phase = GambleGridCapturePhase.Idle;
                    GamblerLog.Info($"Grid area set: {_dragStart.X},{_dragStart.Y} → {end.X},{end.Y}");
                    return true;
                }

                if (leftDown && _wasLeftDown)
                    _phase = GambleGridCapturePhase.Dragging;
            }
            else if (_phase == GambleGridCapturePhase.Dragging)
            {
                if (!leftDown && _wasLeftDown)
                {
                    var end = InteropHelper.GetMousePos();
                    grid.GridStart = _dragStart;
                    grid.GridEnd = end;
                    _phase = GambleGridCapturePhase.Idle;
                    GamblerLog.Info($"Grid area set: {_dragStart.X},{_dragStart.Y} → {end.X},{end.Y}");
                    return true;
                }
            }

            _wasLeftDown = leftDown;
            return false;
        }

        private static bool IsLeftButtonDown() =>
            (Interop.GetAsyncKeyState(0x01) & 0x8000) != 0;
    }
}
