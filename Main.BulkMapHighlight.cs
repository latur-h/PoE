using PoE.dlls.Gamble.Bulk;
using PoE.dlls.Gamble.UI;
using PoE.dlls.Settings.Mods;

namespace PoE
{
    public partial class Main
    {
        private const int BulkMapHighlightRefreshDebounceMs = 120;

        private GambleGridOverlayForm? _bulkMapHighlightOverlay;
        private IReadOnlyList<BulkMapHighlightEntry>? _bulkMapHighlightEntries;
        private System.Windows.Forms.Timer? _bulkMapHighlightRefreshTimer;

        internal void ClearBulkMapHighlight()
        {
            _bulkMapHighlightEntries = null;
            CancelBulkMapHighlightRefresh();
            HideBulkMapHighlightOverlay();
        }

        internal void ApplyBulkMapHighlight(IReadOnlyList<BulkMapSlot> slots, GambleMapBulkSettings? grid)
        {
            if (grid is null || grid.BrokenMapDisposition != BulkMapBrokenDisposition.Highlight)
            {
                ClearBulkMapHighlight();
                return;
            }

            EnsureForegroundWindowMonitor();
            _bulkMapHighlightEntries = BulkMapHighlightBuilder.Build(slots, grid);
            RefreshBulkMapHighlight();
        }

        internal void RefreshBulkMapHighlight()
        {
            if (InvokeRequired)
            {
                BeginInvoke(RefreshBulkMapHighlight);
                return;
            }

            CancelBulkMapHighlightRefresh();
            ApplyBulkMapHighlightOverlay();
        }

        internal void RequestRefreshBulkMapHighlight()
        {
            if (InvokeRequired)
            {
                BeginInvoke(RequestRefreshBulkMapHighlight);
                return;
            }

            EnsureBulkMapHighlightRefreshTimer();
            _bulkMapHighlightRefreshTimer!.Stop();
            _bulkMapHighlightRefreshTimer.Start();
        }

        private void ApplyBulkMapHighlightOverlay()
        {
            if (_bulkMapHighlightEntries is null || _bulkMapHighlightEntries.Count == 0)
            {
                HideBulkMapHighlightOverlay();
                return;
            }

            if (!ShouldShowBulkMapHighlight())
            {
                HideBulkMapHighlightOverlay();
                return;
            }

            EnsureBulkMapHighlightOverlay();
            _bulkMapHighlightOverlay!.Apply(_bulkMapHighlightEntries);
        }

        private void EnsureBulkMapHighlightRefreshTimer()
        {
            if (_bulkMapHighlightRefreshTimer is not null)
                return;

            _bulkMapHighlightRefreshTimer = new System.Windows.Forms.Timer { Interval = BulkMapHighlightRefreshDebounceMs };
            _bulkMapHighlightRefreshTimer.Tick += (_, _) =>
            {
                _bulkMapHighlightRefreshTimer!.Stop();
                ApplyBulkMapHighlightOverlay();
            };
        }

        private void CancelBulkMapHighlightRefresh()
        {
            _bulkMapHighlightRefreshTimer?.Stop();
        }

        private bool ShouldShowBulkMapHighlight()
        {
            if (_bulkMapHighlightEntries is null || _bulkMapHighlightEntries.Count == 0)
                return false;

            if (_settings.Modifiers.MapBulk.BrokenMapDisposition != BulkMapBrokenDisposition.Highlight)
                return false;

            return _inputHost.Simulator.IsActiveWindow();
        }

        private void EnsureBulkMapHighlightOverlay()
        {
            if (_bulkMapHighlightOverlay is not null)
                return;

            _bulkMapHighlightOverlay = new GambleGridOverlayForm(this);
        }

        private void HideBulkMapHighlightOverlay()
        {
            if (_bulkMapHighlightOverlay is null)
                return;

            _bulkMapHighlightOverlay.Hide();
        }

        private void DisposeBulkMapHighlightOverlay()
        {
            _bulkMapHighlightEntries = null;
            _bulkMapHighlightRefreshTimer?.Stop();
            _bulkMapHighlightRefreshTimer?.Dispose();
            _bulkMapHighlightRefreshTimer = null;
            _bulkMapHighlightOverlay?.Close();
            _bulkMapHighlightOverlay?.Dispose();
            _bulkMapHighlightOverlay = null;
        }
    }
}
