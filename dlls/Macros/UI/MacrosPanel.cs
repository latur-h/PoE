using PoE.dlls.InteropServices;
using PoE.dlls.Macros;
using PoE.dlls.Settings.Macros;
using PoE.dlls.Style;

namespace PoE.dlls.Macros.UI
{
    public sealed class MacrosPanel : UserControl
    {
        private const int CompactRowHeight = 72;
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
        private const int LayoutDebounceMs = 100;

        private static readonly Font UiFont = new("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);

        private readonly Panel _scrollPanel;
        private readonly Panel _rowsHost;
        private readonly Button _addButton;
        private readonly Dictionary<MacroProfile, ProfileView> _views = [];
        private readonly Label _headerActive;
        private readonly Label _headerTrigger;
        private readonly Label _headerFire;
        private readonly Label _headerBehavior;
        private readonly Label _headerKey;
        private readonly Label _headerCycle;
        private readonly Label _headerLock;
        private readonly Label _headerToggle;

        private MacroProfile? _visibleProfile;
        private ProfileView? _activeView;
        private MacroSettings? _macroSettings;
        private MacroRowControl? _captureRow;
        private bool _suppressEvents;
        private bool _layoutPending;
        private int _lastLayoutWidth = -1;
        private int _lastLayoutRowCount = -1;
        private bool _uiReady;
        private readonly System.Windows.Forms.Timer _layoutTimer;

        public MacrosPanel()
        {
            SuspendLayout();
            BackColor = StaticColors.BackGround;

            _layoutTimer = new System.Windows.Forms.Timer { Interval = LayoutDebounceMs };
            _layoutTimer.Tick += (_, _) =>
            {
                _layoutTimer.Stop();
                PerformRowLayout();
            };

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
            _scrollPanel.Resize += (_, _) => RequestLayout();

            Controls.Add(_scrollPanel);
            Controls.Add(_addButton);
            Controls.Add(header);

            Font = UiFont;
            _uiReady = true;
            ResumeLayout(false);
        }

        public void RequestLayout()
        {
            if (!_uiReady)
                return;

            _layoutPending = true;
            _layoutTimer.Stop();
            _layoutTimer.Start();
        }

        public void EnsureLayout()
        {
            _layoutPending = true;
            PerformRowLayout();
        }

        public void PreloadProfilesWithContent(MacroSettings settings)
        {
            MacroSettingsHelper.EnsureInitialized(settings);
            _macroSettings ??= settings;

            if (MacroProfileContentHelper.HasContent(settings.GlobalProfile))
                EnsureProfileView(settings.GlobalProfile);

            foreach (MacroProfile profile in settings.BuildProfiles)
            {
                if (MacroProfileContentHelper.HasContent(profile))
                    EnsureProfileView(profile);
            }
        }

        public void DropProfileView(MacroProfile profile)
        {
            if (!_views.Remove(profile, out ProfileView? view))
                return;

            if (ReferenceEquals(_visibleProfile, profile))
            {
                _visibleProfile = null;
                _activeView = null;
                _captureRow = null;
            }

            _rowsHost.Controls.Remove(view.Host);
            foreach (MacroRowControl row in view.Rows)
                row.Dispose();
            view.Host.Dispose();
        }

        public void PurgeProfileViewsExcept(IEnumerable<MacroProfile> keepProfiles)
        {
            var keep = keepProfiles.ToHashSet();
            foreach (MacroProfile profile in _views.Keys.Where(p => !keep.Contains(p)).ToList())
                DropProfileView(profile);
        }

        public event EventHandler? Changed;
        public event EventHandler? CaptureArmed;

        public void Bind(MacroProfile profile, MacroSettings macroSettings)
        {
            if (_visibleProfile is not null && !ReferenceEquals(_visibleProfile, profile))
                Commit();

            _macroSettings = macroSettings;
            ShowProfile(profile);
        }

        public void Commit()
        {
            if (_visibleProfile is null || _activeView is null)
                return;

            _visibleProfile.Triggers = _activeView.Rows.Select(r => r.ToPersistedTrigger()).ToList();
        }

        public IReadOnlyList<MacroTrigger> GetRuntimeTriggers() =>
            ActiveRows.Select(r => r.ToRuntimeTrigger()).ToList();

        public void RefreshActiveStates() =>
            SyncActiveFromEngine(null);

        public void SyncActiveFromEngine(MacroEngine? engine)
        {
            foreach (MacroRowControl row in ActiveRows)
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

        private IReadOnlyList<MacroRowControl> ActiveRows => _activeView?.Rows ?? [];

        private void ShowProfile(MacroProfile profile)
        {
            foreach (ProfileView view in _views.Values)
                view.Host.Visible = false;

            _activeView = EnsureProfileView(profile);
            _visibleProfile = profile;
            SyncActiveViewFromProfile();
            _activeView.Host.Visible = true;
            _activeView.Host.BringToFront();
            _lastLayoutWidth = -1;
            _lastLayoutRowCount = -1;
            RequestLayout();
        }

        private void SyncActiveViewFromProfile()
        {
            if (_visibleProfile is null || _activeView is null)
                return;

            if (_activeView.Rows.Count == _visibleProfile.Triggers.Count)
                return;

            var host = _activeView.Host;
            foreach (MacroRowControl row in _activeView.Rows)
                row.Dispose();

            _activeView.Rows.Clear();
            host.Controls.Clear();

            _suppressEvents = true;
            try
            {
                foreach (MacroTrigger trigger in _visibleProfile.Triggers)
                    CreateRowControl(host, trigger, _activeView.Rows);
            }
            finally
            {
                _suppressEvents = false;
            }
        }

        private ProfileView EnsureProfileView(MacroProfile profile)
        {
            if (_views.TryGetValue(profile, out ProfileView? existing))
                return existing;

            if (_macroSettings is null)
                throw new InvalidOperationException("Macro settings must be bound before creating profile views.");

            var host = new Panel
            {
                Location = new Point(0, 0),
                Width = Math.Max(1, _scrollPanel.ClientSize.Width),
                Visible = false,
                BackColor = StaticColors.BackGround,
            };

            var rows = new List<MacroRowControl>();
            _suppressEvents = true;
            try
            {
                foreach (MacroTrigger trigger in profile.Triggers)
                    CreateRowControl(host, trigger, rows);
            }
            finally
            {
                _suppressEvents = false;
            }

            _rowsHost.Controls.Add(host);

            var view = new ProfileView(host, rows);
            _views[profile] = view;
            return view;
        }

        private void AddRow(MacroTrigger trigger)
        {
            if (_visibleProfile is null || _activeView is null || _macroSettings is null)
                return;

            CreateRowControl(_activeView.Host, trigger, _activeView.Rows);
            NotifyChanged();
            RequestLayout();

            if (_rowsHost.Height > 0)
                _scrollPanel.AutoScrollPosition = new Point(0, _rowsHost.Height);
        }

        private void CreateRowControl(Panel host, MacroTrigger trigger, List<MacroRowControl> rows)
        {
            if (_macroSettings is null)
                return;

            var row = new MacroRowControl(trigger, _macroSettings);
            row.RemoveRequested += (_, _) => RemoveRow(row);
            row.Changed += (_, _) => NotifyChanged();
            row.RowHeightChanged += (_, _) => RequestLayout();
            row.CaptureArmed += (_, _) =>
            {
                foreach (MacroRowControl other in rows)
                {
                    if (!ReferenceEquals(other, row))
                        other.ClearCaptureUi();
                }

                _captureRow = row;
                CaptureArmed?.Invoke(this, EventArgs.Empty);
            };

            rows.Add(row);
            host.Controls.Add(row);
        }

        private void RemoveRow(MacroRowControl row)
        {
            if (_activeView is null)
                return;

            if (_captureRow == row)
                _captureRow = null;

            int index = _activeView.Rows.IndexOf(row);
            if (index < 0)
                return;

            _activeView.Rows.RemoveAt(index);
            _activeView.Host.Controls.Remove(row);
            row.Dispose();
            NotifyChanged();
            RequestLayout();
        }

        private void PerformRowLayout()
        {
            if (_activeView is null)
                return;

            int width = ResolveLayoutWidth();
            int rowCount = _activeView.Rows.Count;
            if (!_layoutPending && width == _lastLayoutWidth && rowCount == _lastLayoutRowCount)
                return;

            _layoutPending = false;
            _lastLayoutWidth = width;
            _lastLayoutRowCount = rowCount;
            LayoutProfileView(_activeView, width);
        }

        private int ResolveLayoutWidth()
        {
            int width = GetLayoutWidth(_scrollPanel, this);
            return width > 0 ? width : Math.Max(ClientSize.Width, 1);
        }

        private void LayoutProfileView(ProfileView view, int width)
        {
            _rowsHost.SuspendLayout();
            view.Host.SuspendLayout();
            try
            {
                UpdateHeaderColumnVisibility(view.Rows);
                LayoutHeader(width);

                int y = 0;
                foreach (MacroRowControl row in view.Rows)
                {
                    row.Location = new Point(0, y);
                    row.SetWidth(width);
                    y += row.Height + 4;
                }

                view.Host.Location = new Point(0, 0);
                view.Host.Width = width;
                view.Host.Height = Math.Max(CompactRowHeight, y);

                if (ReferenceEquals(view, _activeView))
                {
                    _rowsHost.Location = new Point(0, 0);
                    _rowsHost.Width = width;
                    _rowsHost.Height = view.Host.Height;
                }
            }
            finally
            {
                view.Host.ResumeLayout(false);
                _rowsHost.ResumeLayout(false);
            }
        }

        private void UpdateHeaderColumnVisibility(IReadOnlyList<MacroRowControl> rows)
        {
            if (rows.Count == 0)
            {
                _headerTrigger.Visible = true;
                _headerCycle.Visible = true;
                _headerLock.Visible = true;
                return;
            }

            _headerTrigger.Visible = rows.Any(r => r.UsesTriggerKey);
            _headerCycle.Visible = rows.Any(r => r.UsesCycleDelay);
            _headerLock.Visible = rows.Any(r => r.IsPixelMode);
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
            if (_suppressEvents || _visibleProfile is null || _activeView is null)
                return;

            _visibleProfile.Triggers = _activeView.Rows.Select(r => r.ToPersistedTrigger()).ToList();
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

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RequestLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _layoutTimer.Dispose();

            base.Dispose(disposing);
        }

        private sealed class ProfileView(Panel host, List<MacroRowControl> rows)
        {
            public Panel Host { get; } = host;
            public List<MacroRowControl> Rows { get; } = rows;
        }
    }
}
