using PoE.dlls.KeyBindings;
using PoE.dlls.Macros;
using PoE.dlls.Settings.Macros;
using PoE.dlls.Style;

namespace PoE.dlls.Macros.UI
{
    public sealed class MacrosPanel : UserControl
    {
        private const int RowHeight = 72;
        private const int HeaderHeight = 28;
        private const int AddBarHeight = 36;
        private const int ColumnGap = 6;

        private const int ActiveX = 0;
        private const int ActiveWidth = 24;
        private const int TriggerX = ActiveX + ActiveWidth + ColumnGap;
        private const int TriggerWidth = 68;
        private const int FireX = TriggerX + TriggerWidth + ColumnGap;
        private const int FireWidth = 220;
        private const int BehaviorX = FireX + FireWidth + ColumnGap;
        private const int BehaviorWidth = 88;
        private const int KeyDelayX = BehaviorX + BehaviorWidth + ColumnGap;
        private const int KeyDelayWidth = 52;
        private const int CycleDelayX = KeyDelayX + KeyDelayWidth + ColumnGap;
        private const int CycleDelayWidth = 52;
        private const int ToggleX = CycleDelayX + CycleDelayWidth + ColumnGap;
        private const int ToggleWidth = 68;

        private static readonly Font UiFont = new("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
        private static readonly string[] BehaviorNames = Enum.GetNames<MacroBehavior>();

        private readonly Panel _scrollPanel;
        private readonly Panel _rowsHost;
        private readonly Button _addButton;
        private readonly List<MacroRowControl> _rows = [];

        private MacroProfile? _profile;
        private bool _suppressEvents;

        public MacrosPanel()
        {
            BackColor = StaticColors.BackGround;
            Font = UiFont;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = HeaderHeight,
                BackColor = StaticColors.BackGround,
            };

            header.Controls.Add(CreateHeaderLabel("On", ActiveX, ActiveWidth));
            header.Controls.Add(CreateHeaderLabel("Trigger", TriggerX, TriggerWidth));
            header.Controls.Add(CreateHeaderLabel("Fire sequence", FireX, FireWidth));
            header.Controls.Add(CreateHeaderLabel("Mode", BehaviorX, BehaviorWidth));
            header.Controls.Add(CreateHeaderLabel("Key ms", KeyDelayX, KeyDelayWidth));
            header.Controls.Add(CreateHeaderLabel("Cycle ms", CycleDelayX, CycleDelayWidth));
            header.Controls.Add(CreateHeaderLabel("Toggle", ToggleX, ToggleWidth));

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
            _scrollPanel.Controls.Add(_rowsHost);
            _scrollPanel.Resize += (_, _) => LayoutRows();

            Controls.Add(_scrollPanel);
            Controls.Add(_addButton);
            Controls.Add(header);
        }

        public event EventHandler? Changed;

        public void Bind(MacroProfile profile)
        {
            Commit();
            ClearRows();

            _profile = profile;
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
        }

        public void Commit()
        {
            if (_profile is null)
                return;

            _profile.Triggers = _rows.Select(r => r.ToPersistedTrigger()).ToList();
        }

        public IReadOnlyList<MacroTrigger> GetRuntimeTriggers() =>
            _rows.Select(r => r.ToRuntimeTrigger()).ToList();

        public void RefreshActiveStates()
        {
            foreach (var row in _rows)
                row.RefreshActiveCheckbox();
        }

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
            if (_profile is null)
                return;

            CreateRowControl(trigger);
            LayoutRows();
            NotifyChanged();
            _scrollPanel.AutoScrollPosition = new Point(0, _rowsHost.Height);
        }

        private void CreateRowControl(MacroTrigger trigger)
        {
            var row = new MacroRowControl(trigger);
            row.RemoveRequested += (_, _) => RemoveRow(row);
            row.Changed += (_, _) => NotifyChanged();

            _rows.Add(row);
            _rowsHost.Controls.Add(row);
        }

        private void RemoveRow(MacroRowControl row)
        {
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
            int width = Math.Max(640, _scrollPanel.ClientSize.Width);
            int y = 0;

            foreach (var row in _rows)
            {
                row.Location = new Point(0, y);
                row.SetWidth(width);
                y += RowHeight;
            }

            _rowsHost.Width = width;
            _rowsHost.Height = Math.Max(RowHeight, y);
        }

        private void NotifyChanged()
        {
            if (_suppressEvents || _profile is null)
                return;

            _profile.Triggers = _rows.Select(r => r.ToPersistedTrigger()).ToList();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private static Label CreateHeaderLabel(string text, int x, int width) =>
            new()
            {
                Location = new Point(x, 4),
                Size = new Size(width, 20),
                Text = text,
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Font = UiFont,
            };

        private sealed class MacroRowControl : Panel
        {
            private MacroTrigger _trigger;
            private readonly CheckBox _active;
            private readonly FlatTextBox _triggerKey;
            private readonly TextBox _fireSequence;
            private readonly FlatComboBox _behavior;
            private readonly FlatTextBox _keyDelay;
            private readonly FlatTextBox _cycleDelay;
            private readonly FlatTextBox _toggleKey;
            private readonly Label _remove;
            private readonly MacroKeyFieldBinder _triggerKeyBinder;
            private readonly MacroKeyFieldBinder _toggleKeyBinder;
            private readonly System.Windows.Forms.Timer _fireValidateDebounce;

            public event EventHandler? RemoveRequested;
            public event EventHandler? Changed;

            public MacroRowControl(MacroTrigger trigger)
            {
                _trigger = trigger;
                BackColor = StaticColors.BackGround;
                Height = RowHeight;

                _active = new CheckBox
                {
                    Location = new Point(ActiveX + 4, 28),
                    AutoSize = true,
                    Checked = trigger.Active,
                    ForeColor = StaticColors.ForeGround,
                    BackColor = StaticColors.BackGround,
                };
                _active.CheckedChanged += (_, _) =>
                {
                    _trigger.Active = _active.Checked;
                    Changed?.Invoke(this, EventArgs.Empty);
                };

                _triggerKey = CreateKeyBox(new Point(TriggerX, 20), new Size(TriggerWidth, 30));
                _triggerKeyBinder = new MacroKeyFieldBinder(
                    _triggerKey,
                    value => _trigger.TriggerKey = value,
                    (_, _) => Changed?.Invoke(this, EventArgs.Empty));
                _triggerKeyBinder.LoadFromStored(trigger.TriggerKey);

                _fireSequence = new TextBox
                {
                    Location = new Point(FireX, 8),
                    Size = new Size(FireWidth, RowHeight - 12),
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = StaticColors.BackGround,
                    ForeColor = StaticColors.ForeGround,
                    Font = UiFont,
                    Text = trigger.FireSequence,
                    AcceptsReturn = true,
                };

                _fireValidateDebounce = new System.Windows.Forms.Timer { Interval = 300 };
                _fireValidateDebounce.Tick += (_, _) =>
                {
                    _fireValidateDebounce.Stop();
                    ValidateFireSequence();
                };

                _fireSequence.TextChanged += (_, _) =>
                {
                    _trigger.FireSequence = _fireSequence.Text;
                    _fireValidateDebounce.Stop();
                    _fireValidateDebounce.Start();
                    Changed?.Invoke(this, EventArgs.Empty);
                };
                ValidateFireSequence();

                _behavior = new FlatComboBox
                {
                    Location = new Point(BehaviorX, 20),
                    Size = new Size(BehaviorWidth, 30),
                    Font = UiFont,
                };
                _behavior.Items.AddRange(BehaviorNames);
                _behavior.SelectedItem = trigger.Behavior.ToString();
                _behavior.SelectedIndexChanged += (_, _) =>
                {
                    if (_behavior.SelectedItem is string name)
                        _trigger.Behavior = Enum.Parse<MacroBehavior>(name);
                    ApplyBehaviorVisibility();
                    Changed?.Invoke(this, EventArgs.Empty);
                };

                _keyDelay = CreateNumericBox(new Point(KeyDelayX, 20), new Size(KeyDelayWidth, 30), trigger.KeyDelayMs, value =>
                {
                    _trigger.KeyDelayMs = value;
                    Changed?.Invoke(this, EventArgs.Empty);
                });

                _cycleDelay = CreateNumericBox(new Point(CycleDelayX, 20), new Size(CycleDelayWidth, 30), trigger.CycleDelayMs, value =>
                {
                    _trigger.CycleDelayMs = value;
                    Changed?.Invoke(this, EventArgs.Empty);
                });

                _toggleKey = CreateKeyBox(new Point(ToggleX, 20), new Size(ToggleWidth, 30));
                _toggleKeyBinder = new MacroKeyFieldBinder(
                    _toggleKey,
                    value => _trigger.ToggleKey = value,
                    (_, _) => Changed?.Invoke(this, EventArgs.Empty));
                _toggleKeyBinder.LoadFromStored(trigger.ToggleKey);

                _remove = new Label
                {
                    AutoSize = true,
                    Text = "×",
                    Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point),
                    ForeColor = StaticColors.ForeGround,
                    BackColor = Color.Transparent,
                    Location = new Point(0, 24),
                    Cursor = Cursors.Hand,
                    TabStop = false,
                };
                _remove.Click += (_, _) => RemoveRequested?.Invoke(this, EventArgs.Empty);

                Controls.Add(_active);
                Controls.Add(_triggerKey);
                Controls.Add(_fireSequence);
                Controls.Add(_behavior);
                Controls.Add(_keyDelay);
                Controls.Add(_cycleDelay);
                Controls.Add(_toggleKey);
                Controls.Add(_remove);

                ApplyBehaviorVisibility();
            }

            public void RefreshActiveCheckbox() => _active.Checked = _trigger.Active;

            public MacroTrigger ToPersistedTrigger() => new()
            {
                Id = _trigger.Id,
                Active = _active.Checked,
                TriggerKey = _trigger.TriggerKey,
                FireSequence = _fireSequence.Text,
                Behavior = _trigger.Behavior,
                KeyDelayMs = _trigger.KeyDelayMs,
                CycleDelayMs = _trigger.CycleDelayMs,
                ToggleKey = _trigger.ToggleKey,
            };

            public MacroTrigger ToRuntimeTrigger()
            {
                bool keysReady = KeysAllowRuntime();

                return new MacroTrigger
                {
                    Id = _trigger.Id,
                    Active = _active.Checked && keysReady,
                    TriggerKey = _triggerKeyBinder.AllowsRuntime ? _trigger.TriggerKey : string.Empty,
                    FireSequence = _fireSequence.Text,
                    Behavior = _trigger.Behavior,
                    KeyDelayMs = _trigger.KeyDelayMs,
                    CycleDelayMs = _trigger.CycleDelayMs,
                    ToggleKey = _toggleKeyBinder.AllowsRuntime ? _trigger.ToggleKey : string.Empty,
                };
            }

            private bool KeysAllowRuntime() =>
                _trigger.Behavior switch
                {
                    MacroBehavior.Repeat => _toggleKeyBinder.AllowsRuntime,
                    _ => _triggerKeyBinder.AllowsRuntime,
                };

            public void SetWidth(int width)
            {
                Width = width;
                _remove.Location = new Point(width - _remove.Width - 4, 24);
                _remove.BringToFront();
            }

            private void ApplyBehaviorVisibility()
            {
                bool isRepeat = _trigger.Behavior == MacroBehavior.Repeat;
                bool isSingle = _trigger.Behavior == MacroBehavior.Single;

                _triggerKey.Visible = !isRepeat;
                _cycleDelay.Visible = !isSingle;
            }

            private void ValidateFireSequence()
            {
                string text = _fireSequence.Text;
                if (string.IsNullOrWhiteSpace(text))
                {
                    _fireSequence.ForeColor = Color.Red;
                    return;
                }

                _fireSequence.ForeColor = MacroFireSequence.IsValid(text)
                    ? StaticColors.ForeGround
                    : Color.Red;
            }

            private static FlatTextBox CreateKeyBox(Point location, Size size) =>
                new()
                {
                    Location = location,
                    Size = size,
                    Font = UiFont,
                    TextAlign = HorizontalAlignment.Center,
                };

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _triggerKeyBinder.Dispose();
                    _toggleKeyBinder.Dispose();
                    _fireValidateDebounce.Dispose();
                }

                base.Dispose(disposing);
            }

            private static FlatTextBox CreateNumericBox(Point location, Size size, int value, Action<int> setter)
            {
                var box = new FlatTextBox
                {
                    Location = location,
                    Size = size,
                    Font = UiFont,
                    TextAlign = HorizontalAlignment.Center,
                };
                box._textBox.Text = value.ToString();
                box._textBox.KeyUp += (_, _) =>
                {
                    if (int.TryParse(box._textBox.Text, out int parsed) && parsed >= 0)
                    {
                        setter(parsed);
                        box._textBox.ForeColor = StaticColors.ForeGround;
                    }
                    else
                    {
                        box._textBox.ForeColor = Color.Red;
                    }
                };

                return box;
            }
        }
    }
}
