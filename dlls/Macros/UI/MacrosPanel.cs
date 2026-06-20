using PoE.dlls.InteropServices;
using PoE.dlls.Macros;
using PoE.dlls.Settings.Macros;
using PoE.dlls.Style;

namespace PoE.dlls.Macros.UI
{
    public sealed class MacrosPanel : UserControl
    {
        private const int CompactRowHeight = 72;
        private const int ExpandedRowHeight = 110;
        private const int HeaderHeight = 28;
        private const int AddBarHeight = 36;
        private const int ColumnGap = 6;
        private const int RemoveReserve = 22;
        private const int MinFireWidth = 72;

        private const int ActiveX = 0;
        private const int ActiveWidth = 24;
        private const int TriggerX = ActiveX + ActiveWidth + ColumnGap;
        private const int TriggerWidth = 68;
        private const int FireX = TriggerX + TriggerWidth + ColumnGap;
        private const int BehaviorWidth = 82;
        private const int KeyDelayWidth = 48;
        private const int CycleDelayWidth = 48;
        private const int LockWidth = 48;
        private const int ToggleWidth = 62;

        private static readonly Font UiFont = new("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);

        private readonly Panel _scrollPanel;
        private readonly Panel _rowsHost;
        private readonly Button _addButton;
        private readonly List<MacroRowControl> _rows = [];
        private readonly Label _headerActive;
        private readonly Label _headerTrigger;
        private readonly Label _headerFire;
        private readonly Label _headerBehavior;
        private readonly Label _headerKey;
        private readonly Label _headerCycle;
        private readonly Label _headerLock;
        private readonly Label _headerToggle;

        private MacroProfile? _profile;
        private MacroSettings? _macroSettings;
        private MacroRowControl? _captureRow;
        private bool _suppressEvents;
        private int _lastLayoutWidth = -1;
        private bool _uiReady;
        private readonly System.Windows.Forms.Timer _widthLayoutTimer;

        public MacrosPanel()
        {
            SuspendLayout();
            BackColor = StaticColors.BackGround;

            _widthLayoutTimer = new System.Windows.Forms.Timer { Interval = 64 };
            _widthLayoutTimer.Tick += (_, _) =>
            {
                _widthLayoutTimer.Stop();
                TryLayoutForWidthChange(force: true);
            };
            Resize += (_, _) => ScheduleWidthLayout();

            var header = new Panel { Dock = DockStyle.Top, Height = HeaderHeight, BackColor = StaticColors.BackGround };
            _headerActive = CreateHeaderLabel("On");
            _headerTrigger = CreateHeaderLabel("Trigger");
            _headerFire = CreateHeaderLabel("Fire sequence");
            _headerBehavior = CreateHeaderLabel("Mode");
            _headerKey = CreateHeaderLabel("Key ms");
            _headerCycle = CreateHeaderLabel("Cycle ms");
            _headerLock = CreateHeaderLabel("Lock ms");
            _headerToggle = CreateHeaderLabel("Toggle");
            header.Controls.AddRange([
                _headerActive,
                _headerTrigger,
                _headerFire,
                _headerBehavior,
                _headerKey,
                _headerCycle,
                _headerLock,
                _headerToggle,
            ]);

            _addButton = new Button
            {
                Dock = DockStyle.Bottom,
                Height = AddBarHeight,
                Text = "+ Add macro",
                FlatStyle = FlatStyle.Flat,
                BackColor = StaticColors.BackGround,
                ForeColor = StaticColors.ForeGround,
                Cursor = Cursors.Hand,
                TabStop = false,
            };
            _addButton.FlatAppearance.BorderColor = StaticColors.ForeGround;
            _addButton.Click += (_, _) => AddRow(new MacroTrigger());

            _rowsHost = new Panel
            {
                AutoSize = false,
                Location = new Point(0, 0),
                Width = 0,
                Height = 0,
                BackColor = StaticColors.BackGround,
            };

            _scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = StaticColors.BackGround,
            };
            _scrollPanel.HorizontalScroll.Enabled = false;
            _scrollPanel.HorizontalScroll.Visible = false;
            _scrollPanel.Controls.Add(_rowsHost);

            Controls.Add(_scrollPanel);
            Controls.Add(_addButton);
            Controls.Add(header);

            Font = UiFont;
            _uiReady = true;
            ResumeLayout(false);
        }

        public void EnsureLayout() => LayoutRows();

        private void ScheduleWidthLayout()
        {
            if (!_uiReady)
                return;

            _widthLayoutTimer.Stop();
            _widthLayoutTimer.Start();
        }

        public event EventHandler? Changed;
        public event EventHandler? CaptureArmed;

        public void Bind(MacroProfile profile, MacroSettings macroSettings)
        {
            DisarmCapture();
            Commit();
            ClearRows();

            _profile = profile;
            _macroSettings = macroSettings;
            _suppressEvents = true;
            try
            {
                foreach (var trigger in profile.Triggers)
                    CreateRowControl(trigger);
            }
            finally
            {
                _suppressEvents = false;
            }

            LayoutRows();
            if (IsHandleCreated)
                BeginInvoke(LayoutRows);
        }

        public void Commit()
        {
            if (_profile is null)
                return;

            _profile.Triggers = _rows.Select(r => r.ToPersistedTrigger()).ToList();
        }

        public IReadOnlyList<MacroTrigger> GetRuntimeTriggers() =>
            _rows.Select(r => r.ToRuntimeTrigger()).ToList();

        public void RefreshActiveStates() =>
            SyncActiveFromEngine(null);

        public void SyncActiveFromEngine(MacroEngine? engine)
        {
            foreach (var row in _rows)
            {
                if (engine is null)
                {
                    row.RefreshActiveCheckbox();
                    continue;
                }

                var resolved = engine.FindTrigger(row.TriggerId);
                if (resolved is not null)
                    row.SetActive(resolved.Active);
            }
        }

        public bool TryApplyCapture(Coordinates coordinates)
        {
            if (_captureRow is null || _macroSettings is null)
                return false;

            if (!_captureRow.TryApplyCapture(coordinates, _macroSettings))
                return false;

            _captureRow = null;
            NotifyChanged();
            return true;
        }

        public void DisarmCapture()
        {
            _captureRow?.ClearCaptureUi();
            _captureRow = null;
        }

        public bool HasCaptureArmed => _captureRow is not null;

        private void ClearRows()
        {
            foreach (var row in _rows)
            {
                _rowsHost.Controls.Remove(row);
                row.Dispose();
            }

            _rows.Clear();
            _rowsHost.Height = 0;
        }

        private void AddRow(MacroTrigger trigger)
        {
            if (_profile is null || _macroSettings is null)
                return;

            CreateRowControl(trigger);
            LayoutRows();
            NotifyChanged();
            _scrollPanel.AutoScrollPosition = new Point(0, _rowsHost.Height);
        }

        private void CreateRowControl(MacroTrigger trigger)
        {
            var row = new MacroRowControl(trigger, _macroSettings);
            row.RemoveRequested += (_, _) => RemoveRow(row);
            row.Changed += (_, _) => NotifyChanged();
            row.RowHeightChanged += (_, _) => LayoutRows();
            row.CaptureArmed += (_, _) =>
            {
                foreach (var other in _rows)
                {
                    if (!ReferenceEquals(other, row))
                        other.ClearCaptureUi();
                }

                _captureRow = row;
                CaptureArmed?.Invoke(this, EventArgs.Empty);
            };

            _rows.Add(row);
            _rowsHost.Controls.Add(row);
        }

        private void RemoveRow(MacroRowControl row)
        {
            if (_captureRow == row)
                _captureRow = null;

            int index = _rows.IndexOf(row);
            if (index < 0)
                return;

            _rows.RemoveAt(index);
            _rowsHost.Controls.Remove(row);
            row.Dispose();
            LayoutRows();
            NotifyChanged();
        }

        private void LayoutRows()
        {
            int width = GetLayoutWidth();
            if (width <= 0)
                return;

            _lastLayoutWidth = width;
            LayoutRowsCore(width);
        }

        private void TryLayoutForWidthChange(bool force = false)
        {
            int width = GetLayoutWidth();
            if (width <= 0)
                return;

            if (!force && width == _lastLayoutWidth)
                return;

            _lastLayoutWidth = width;
            LayoutRowsCore(width);
        }

        private void LayoutRowsCore(int width)
        {
            SuspendLayout();
            _rowsHost.SuspendLayout();
            try
            {
                UpdateHeaderColumnVisibility();
                LayoutHeader(width);

                int y = 0;
                foreach (var row in _rows)
                {
                    if (row.Location.Y != y)
                        row.Location = new Point(0, y);

                    row.SetWidth(width);
                    y += row.Height + 4;
                }

                _rowsHost.Location = new Point(0, 0);
                if (_rowsHost.Width != width)
                    _rowsHost.Width = width;

                int hostHeight = Math.Max(CompactRowHeight, y);
                if (_rowsHost.Height != hostHeight)
                    _rowsHost.Height = hostHeight;
            }
            finally
            {
                _rowsHost.ResumeLayout(false);
                ResumeLayout(false);
            }
        }

        private void UpdateHeaderColumnVisibility()
        {
            if (_rows.Count == 0)
            {
                _headerTrigger.Visible = true;
                _headerCycle.Visible = true;
                _headerLock.Visible = true;
                return;
            }

            _headerTrigger.Visible = _rows.Any(r => r.UsesTriggerKey);
            _headerCycle.Visible = _rows.Any(r => r.UsesCycleDelay);
            _headerLock.Visible = _rows.Any(r => r.IsPixelMode);
        }

        private void LayoutHeader(int width)
        {
            int x = width - RemoveReserve - ColumnGap;
            x = PlaceHeader(_headerToggle, x, ToggleWidth);

            if (_headerLock.Visible)
                x = PlaceHeader(_headerLock, x, LockWidth);

            if (_headerCycle.Visible)
                x = PlaceHeader(_headerCycle, x, CycleDelayWidth);

            x = PlaceHeader(_headerKey, x, KeyDelayWidth);
            x = PlaceHeader(_headerBehavior, x, BehaviorWidth);

            int modeLeft = _headerBehavior.Left;
            int fireLeft = _headerTrigger.Visible ? FireX : TriggerX;
            int fireRight = modeLeft - ColumnGap;
            int fireWidth = Math.Max(MinFireWidth, fireRight - fireLeft);

            _headerFire.Location = new Point(fireLeft, 4);
            _headerFire.Width = fireWidth;

            if (_headerTrigger.Visible)
            {
                _headerTrigger.Location = new Point(TriggerX, 4);
                _headerTrigger.Width = TriggerWidth;
            }

            _headerActive.Location = new Point(ActiveX, 4);
            _headerActive.Width = ActiveWidth;
        }

        private static int PlaceHeader(Label label, int rightEdge, int fieldWidth)
        {
            rightEdge -= fieldWidth;
            label.Location = new Point(rightEdge, 4);
            label.Width = fieldWidth;
            return rightEdge - ColumnGap;
        }

        private void NotifyChanged()
        {
            if (_suppressEvents || _profile is null)
                return;

            _profile.Triggers = _rows.Select(r => r.ToPersistedTrigger()).ToList();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private static Label CreateHeaderLabel(string text) =>
            new()
            {
                Text = text,
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Font = UiFont,
            };

        private static int GetLayoutWidth(Panel? scrollPanel, Control host)
        {
            if (scrollPanel is not null && scrollPanel.ClientSize.Width > 0)
                return scrollPanel.ClientSize.Width;

            return host.ClientSize.Width;
        }

        private int GetLayoutWidth() => GetLayoutWidth(_scrollPanel, this);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _widthLayoutTimer.Dispose();

            base.Dispose(disposing);
        }
    }
}
