using PoE.dlls.InteropServices;
using PoE.dlls.Macros;
using PoE.dlls.Settings.Macros;
using PoE.dlls.Style;

namespace PoE.dlls.Macros.UI
{
    internal sealed class MacroRowControl : Panel
    {
        private const int CompactRowHeight = 72;
        private const int ExpandedRowHeight = 110;
        private const int ColumnGap = 6;
        private const int RemoveReserve = 22;
        private const int MinFireWidth = 72;
        private const int ActiveX = 0;
        private const int TriggerX = 30;
        private const int TriggerWidth = 68;
        private const int FireX = 104;
        private const int BehaviorWidth = 82;
        private const int KeyDelayWidth = 48;
        private const int CycleDelayWidth = 48;
        private const int LockWidth = 48;
        private const int ToggleWidth = 62;
        private const int PixelRowHeight = 34;

        private static readonly Font UiFont = new("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
        private static readonly string[] BehaviorNames = Enum.GetNames<MacroBehavior>();

        private MacroTrigger _trigger;
        private MacroSettings? _macroSettings;
        private readonly CheckBox _active;
        private readonly FlatTextBox _triggerKey;
        private readonly TextBox _fireSequence;
        private readonly FlatComboBox _behavior;
        private readonly FlatTextBox _keyDelay;
        private readonly FlatTextBox _cycleDelay;
        private readonly FlatTextBox _lockDelay;
        private readonly FlatTextBox _toggleKey;
        private readonly Panel _pixelRow;
        private readonly FlatTextBox _coordText;
        private readonly Button _recButton;
        private readonly Button _mouseButton;
        private readonly FlatTextBox _colorText;
        private readonly Button _pickButton;
        private readonly Button _pickMouseButton;
        private readonly FlatComboBox _rememberCombo;
        private readonly Label _remove;
        private readonly MacroKeyFieldBinder _triggerKeyBinder;
        private readonly MacroKeyFieldBinder _toggleKeyBinder;
        private readonly System.Windows.Forms.Timer _fireValidateDebounce;

        public event EventHandler? RemoveRequested;
        public event EventHandler? Changed;
        public event EventHandler? CaptureArmed;
        public event EventHandler? RowHeightChanged;

        public MacroRowControl(MacroTrigger trigger, MacroSettings? macroSettings)
        {
            _trigger = trigger;
            _macroSettings = macroSettings;
            BackColor = StaticColors.BackGround;
            Height = CompactRowHeight;

            _active = new CheckBox
            {
                Location = new Point(ActiveX + 4, 22),
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

            _triggerKey = CreateKeyBox(new Point(TriggerX, 14), new Size(TriggerWidth, 30));
            _triggerKeyBinder = new MacroKeyFieldBinder(_triggerKey, v => _trigger.TriggerKey = v, (_, _) => Changed?.Invoke(this, EventArgs.Empty));
            _triggerKeyBinder.LoadFromStored(trigger.TriggerKey);

            _fireSequence = new TextBox
            {
                Location = new Point(FireX, 6),
                Size = new Size(MinFireWidth, 58),
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
            _fireValidateDebounce.Tick += (_, _) => { _fireValidateDebounce.Stop(); ValidateFireSequence(); };
            _fireSequence.TextChanged += (_, _) =>
            {
                _trigger.FireSequence = _fireSequence.Text;
                _fireValidateDebounce.Stop();
                _fireValidateDebounce.Start();
                Changed?.Invoke(this, EventArgs.Empty);
            };
            ValidateFireSequence();

            _behavior = new FlatComboBox { Location = new Point(0, 14), Size = new Size(BehaviorWidth, 30), Font = UiFont };
            _behavior.Items.AddRange(BehaviorNames);
            _behavior.SelectedItem = trigger.Behavior.ToString();
            _behavior.SelectedIndexChanged += (_, _) =>
            {
                if (_behavior.SelectedItem is string name)
                    _trigger.Behavior = Enum.Parse<MacroBehavior>(name);
                ApplyBehaviorVisibility();
                Changed?.Invoke(this, EventArgs.Empty);
            };

            _keyDelay = CreateNumericBox(new Point(0, 14), new Size(KeyDelayWidth, 30), trigger.KeyDelayMs, v => { _trigger.KeyDelayMs = v; Changed?.Invoke(this, EventArgs.Empty); });
            _cycleDelay = CreateNumericBox(new Point(0, 14), new Size(CycleDelayWidth, 30), trigger.CycleDelayMs, v => { _trigger.CycleDelayMs = v; Changed?.Invoke(this, EventArgs.Empty); });
            _lockDelay = CreateNumericBox(new Point(0, 14), new Size(LockWidth, 30), trigger.LockMs, v => { _trigger.LockMs = v; Changed?.Invoke(this, EventArgs.Empty); });

            _toggleKey = CreateKeyBox(new Point(0, 14), new Size(ToggleWidth, 30));
            _toggleKeyBinder = new MacroKeyFieldBinder(_toggleKey, v => _trigger.ToggleKey = v, (_, _) => Changed?.Invoke(this, EventArgs.Empty));
            _toggleKeyBinder.LoadFromStored(trigger.ToggleKey);

            _pixelRow = new Panel { Location = new Point(TriggerX, CompactRowHeight - PixelRowHeight), Size = new Size(400, PixelRowHeight), BackColor = StaticColors.BackGround };
            _coordText = new FlatTextBox { Location = new Point(0, 2), Size = new Size(100, 30), Font = UiFont, TextAlign = HorizontalAlignment.Center };
            _coordText._textBox.Text = $"{trigger.PixelX}, {trigger.PixelY}";
            _coordText._textBox.TextChanged += (_, _) => OnCoordTextChanged();

            _recButton = CreateSmallButton("Rec", 106);
            _recButton.Click += (_, _) =>
            {
                RequestCoordinateCapture();
                CaptureArmed?.Invoke(this, EventArgs.Empty);
            };

            _mouseButton = CreateSmallButton("Mouse", 152);
            _mouseButton.Click += (_, _) => { CaptureArmed?.Invoke(this, EventArgs.Empty); RequestMouseCapture(); };

            _colorText = new FlatTextBox { Location = new Point(206, 2), Size = new Size(72, 30), Font = UiFont, TextAlign = HorizontalAlignment.Center };
            _colorText._textBox.Text = trigger.ExpectedColor;
            _colorText._textBox.TextChanged += (_, _) => OnColorTextChanged();

            _pickButton = CreateSmallButton("Pick", 284);
            _pickButton.Click += (_, _) => PickColorAtCoordinate();

            _pickMouseButton = CreateSmallButton("MPick", 330);
            _pickMouseButton.Click += (_, _) => { CaptureArmed?.Invoke(this, EventArgs.Empty); RequestMouseCapture(); };

            _rememberCombo = new FlatComboBox { Location = new Point(386, 2), Size = new Size(120, 30), Font = UiFont, DropDownStyle = ComboBoxStyle.DropDownList };
            _rememberCombo.SelectedIndexChanged += (_, _) =>
            {
                if (_rememberCombo.SelectedItem is string hex)
                {
                    _colorText._textBox.Text = hex;
                    OnColorTextChanged();
                }
            };
            RefreshRememberedColors();

            _pixelRow.Controls.AddRange([_coordText, _recButton, _mouseButton, _colorText, _pickButton, _pickMouseButton, _rememberCombo]);

            _remove = new Label
            {
                AutoSize = true,
                Text = "×",
                Font = new Font("Segoe UI", 14F),
                ForeColor = StaticColors.ForeGround,
                BackColor = Color.Transparent,
                Location = new Point(0, 18),
                Cursor = Cursors.Hand,
            };
            _remove.Click += (_, _) => RemoveRequested?.Invoke(this, EventArgs.Empty);

            Controls.AddRange([_active, _triggerKey, _fireSequence, _behavior, _keyDelay, _cycleDelay, _lockDelay, _toggleKey, _pixelRow, _remove]);
            ApplyBehaviorVisibility();
            ValidateCoordColor();
        }

        public MacroCaptureMode PendingCapture { get; private set; } = MacroCaptureMode.None;

        public void SetMacroSettings(MacroSettings settings)
        {
            _macroSettings = settings;
            RefreshRememberedColors();
        }

        public Guid TriggerId => _trigger.Id;

        public void RefreshActiveCheckbox() => _active.Checked = _trigger.Active;

        public void SetActive(bool active)
        {
            _trigger.Active = active;
            _active.Checked = active;
        }

        public void RequestCoordinateCapture()
        {
            PendingCapture = MacroCaptureMode.Coordinate;
            _recButton.ForeColor = Color.Red;
            _recButton.Text = "...";
        }

        public void RequestMouseCapture()
        {
            PendingCapture = MacroCaptureMode.MouseSample;
            _mouseButton.ForeColor = Color.Red;
            _pickMouseButton.ForeColor = Color.Red;
        }

        public void ClearCaptureUi()
        {
            PendingCapture = MacroCaptureMode.None;
            _recButton.ForeColor = StaticColors.ForeGround;
            _recButton.Text = "Rec";
            _mouseButton.ForeColor = StaticColors.ForeGround;
            _pickMouseButton.ForeColor = StaticColors.ForeGround;
        }

        public bool TryApplyCapture(Coordinates coordinates, MacroSettings settings)
        {
            if (PendingCapture == MacroCaptureMode.Coordinate)
            {
                ApplyCoordinate(coordinates);
                ClearCaptureUi();
                return true;
            }

            if (PendingCapture == MacroCaptureMode.MouseSample)
            {
                Color sampled = InteropHelper.GetColorAt(coordinates.X, coordinates.Y);
                ApplyMouseSample(coordinates, sampled, settings);
                ClearCaptureUi();
                return true;
            }

            return false;
        }

        public void ApplyCoordinate(Coordinates coordinates)
        {
            _trigger.PixelX = coordinates.X;
            _trigger.PixelY = coordinates.Y;
            _coordText._textBox.Text = $"{coordinates.X}, {coordinates.Y}";
            ValidateCoordColor();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void ApplyMouseSample(Coordinates coordinates, Color color, MacroSettings settings)
        {
            ApplyCoordinate(coordinates);
            string hex = MacroColorHelper.ToHex(color);
            _colorText._textBox.Text = hex;
            _trigger.ExpectedColor = hex;
            MacroColorHelper.RememberColor(settings, hex);
            RefreshRememberedColors();
            ValidateCoordColor();
            Changed?.Invoke(this, EventArgs.Empty);
        }

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
            PixelX = _trigger.PixelX,
            PixelY = _trigger.PixelY,
            ExpectedColor = _trigger.ExpectedColor,
            LockMs = _trigger.LockMs,
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
                PixelX = _trigger.PixelX,
                PixelY = _trigger.PixelY,
                ExpectedColor = _trigger.ExpectedColor,
                LockMs = _trigger.LockMs,
            };
        }

        public void SetWidth(int width)
        {
            if (Width == width && _appliedLayoutWidth == width)
                return;

            _appliedLayoutWidth = width;
            if (Width != width)
                Width = width;

            ApplyColumnLayout(width);
        }

        private int _appliedLayoutWidth = -1;

        private void ApplyColumnLayout(int width)
        {
            int removeX = Math.Max(FireX + MinFireWidth + BehaviorWidth, width - RemoveReserve);
            if (_remove.Location.X != removeX)
                _remove.Location = new Point(removeX, 18);

            int x = removeX - ColumnGap;
            x = PlaceFieldIfVisible(_toggleKey, x, ToggleWidth);
            x = PlaceFieldIfVisible(_lockDelay, x, LockWidth);
            x = PlaceFieldIfVisible(_cycleDelay, x, CycleDelayWidth);
            x = PlaceFieldIfVisible(_keyDelay, x, KeyDelayWidth);
            x = PlaceField(_behavior, x, BehaviorWidth);

            int fireWidth = Math.Max(MinFireWidth, x - ColumnGap - FireX);
            _fireSequence.Location = new Point(FireX, _fireSequence.Location.Y);
            _fireSequence.Width = fireWidth;

            _pixelRow.Width = Math.Max(TriggerWidth, removeX - TriggerX);
            _pixelRow.Location = new Point(TriggerX, Height - PixelRowHeight);
            LayoutPixelRowControls();
        }

        private static int PlaceField(Control control, int rightEdge, int fieldWidth)
        {
            rightEdge -= fieldWidth;
            control.Location = new Point(rightEdge, 14);
            control.Width = fieldWidth;
            return rightEdge - ColumnGap;
        }

        private static int PlaceFieldIfVisible(Control control, int rightEdge, int fieldWidth) =>
            control.Visible ? PlaceField(control, rightEdge, fieldWidth) : rightEdge;

        private void LayoutPixelRowControls()
        {
            int x = 0;
            const int coordWidth = 88;
            const int colorWidth = 68;
            const int buttonWidth = 40;
            const int rememberWidth = 96;
            const int gap = 4;

            _coordText.Location = new Point(x, 2);
            _coordText.Width = coordWidth;
            x += coordWidth + gap;

            _recButton.Location = new Point(x, 2);
            _recButton.Width = buttonWidth;
            x += buttonWidth + gap;

            _mouseButton.Location = new Point(x, 2);
            _mouseButton.Width = buttonWidth;
            x += buttonWidth + gap;

            _colorText.Location = new Point(x, 2);
            _colorText.Width = colorWidth;
            x += colorWidth + gap;

            _pickButton.Location = new Point(x, 2);
            _pickButton.Width = buttonWidth;
            x += buttonWidth + gap;

            _pickMouseButton.Location = new Point(x, 2);
            _pickMouseButton.Width = buttonWidth;
            x += buttonWidth + gap;

            int rememberX = Math.Max(x, _pixelRow.ClientSize.Width - rememberWidth);
            _rememberCombo.Location = new Point(rememberX, 2);
            _rememberCombo.Width = Math.Min(rememberWidth, Math.Max(72, _pixelRow.ClientSize.Width - x));
        }

        private bool KeysAllowRuntime() => _trigger.Behavior switch
        {
            MacroBehavior.Repeat => true,
            MacroBehavior.JE or MacroBehavior.JNE => IsPixelConfigValid(),
            _ => _triggerKeyBinder.AllowsRuntime,
        };

        private bool IsPixelConfigValid() =>
            TryParseCoord(_coordText._textBox.Text, out _, out _)
            && MacroColorHelper.TryParseHex(_colorText._textBox.Text, out _);

        private void ApplyBehaviorVisibility()
        {
            bool pixelMode = _trigger.Behavior is MacroBehavior.JE or MacroBehavior.JNE;
            bool hideTrigger = _trigger.Behavior is MacroBehavior.Repeat or MacroBehavior.JE or MacroBehavior.JNE;
            bool hideCycle = _trigger.Behavior == MacroBehavior.Single;

            _triggerKey.Visible = !hideTrigger;
            _cycleDelay.Visible = !hideCycle;
            _lockDelay.Visible = pixelMode;
            _pixelRow.Visible = pixelMode;

            int previousHeight = Height;
            Height = pixelMode ? ExpandedRowHeight : CompactRowHeight;
            _fireSequence.Height = pixelMode ? 40 : 58;

            if (Width > 0)
                ApplyColumnLayout(Width);

            if (Height != previousHeight)
                RowHeightChanged?.Invoke(this, EventArgs.Empty);
        }

        private void PickColorAtCoordinate()
        {
            if (!TryParseCoord(_coordText._textBox.Text, out int x, out int y))
                return;

            Color sampled = InteropHelper.GetColorAt(x, y);
            string hex = MacroColorHelper.ToHex(sampled);
            _colorText._textBox.Text = hex;
            _trigger.ExpectedColor = hex;
            if (_macroSettings is not null)
            {
                MacroColorHelper.RememberColor(_macroSettings, hex);
                RefreshRememberedColors();
            }

            ValidateCoordColor();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private void OnCoordTextChanged()
        {
            if (TryParseCoord(_coordText._textBox.Text, out int x, out int y))
            {
                _trigger.PixelX = x;
                _trigger.PixelY = y;
                _coordText._textBox.ForeColor = StaticColors.ForeGround;
            }
            else if (!string.IsNullOrWhiteSpace(_coordText._textBox.Text))
                _coordText._textBox.ForeColor = Color.Red;

            Changed?.Invoke(this, EventArgs.Empty);
        }

        private void OnColorTextChanged()
        {
            if (MacroColorHelper.TryParseHex(_colorText._textBox.Text, out _))
            {
                _trigger.ExpectedColor = _colorText._textBox.Text.Trim();
                _colorText._textBox.ForeColor = StaticColors.ForeGround;
            }
            else if (!string.IsNullOrWhiteSpace(_colorText._textBox.Text))
                _colorText._textBox.ForeColor = Color.Red;

            Changed?.Invoke(this, EventArgs.Empty);
        }

        private void ValidateCoordColor()
        {
            if (string.IsNullOrWhiteSpace(_coordText._textBox.Text))
                _coordText._textBox.ForeColor = StaticColors.ForeGround;
            else if (!TryParseCoord(_coordText._textBox.Text, out _, out _))
                _coordText._textBox.ForeColor = Color.Red;

            if (string.IsNullOrWhiteSpace(_colorText._textBox.Text))
                _colorText._textBox.ForeColor = StaticColors.ForeGround;
            else if (!MacroColorHelper.TryParseHex(_colorText._textBox.Text, out _))
                _colorText._textBox.ForeColor = Color.Red;
        }

        private void ValidateFireSequence()
        {
            _fireSequence.ForeColor = string.IsNullOrWhiteSpace(_fireSequence.Text) || !MacroFireSequence.IsValid(_fireSequence.Text)
                ? Color.Red
                : StaticColors.ForeGround;
        }

        private void RefreshRememberedColors()
        {
            _rememberCombo.BeginUpdate();
            try
            {
                _rememberCombo.Items.Clear();
                if (_macroSettings?.RememberedColors is null)
                    return;

                foreach (string color in _macroSettings.RememberedColors)
                    _rememberCombo.Items.Add(color);
            }
            finally
            {
                _rememberCombo.EndUpdate();
            }
        }

        private static bool TryParseCoord(string text, out int x, out int y)
        {
            x = 0;
            y = 0;
            if (!text.Contains(','))
                return false;

            string[] parts = text.Split(',');
            return parts.Length == 2
                && int.TryParse(parts[0].Trim(), out x)
                && int.TryParse(parts[1].Trim(), out y)
                && x >= 0
                && y >= 0;
        }

        private static Button CreateSmallButton(string text, int x) => new()
        {
            Location = new Point(x, 2),
            Size = new Size(40, 30),
            Text = text,
            FlatStyle = FlatStyle.Flat,
            BackColor = StaticColors.BackGround,
            ForeColor = StaticColors.ForeGround,
            Font = new Font("Segoe UI", 9F),
            TabStop = false,
        };

        private static FlatTextBox CreateKeyBox(Point location, Size size) =>
            new() { Location = location, Size = size, Font = UiFont, TextAlign = HorizontalAlignment.Center };

        private static FlatTextBox CreateNumericBox(Point location, Size size, int value, Action<int> setter)
        {
            var box = new FlatTextBox { Location = location, Size = size, Font = UiFont, TextAlign = HorizontalAlignment.Center };
            box._textBox.Text = value.ToString();
            box._textBox.KeyUp += (_, _) =>
            {
                if (int.TryParse(box._textBox.Text, out int parsed) && parsed >= 0)
                {
                    setter(parsed);
                    box._textBox.ForeColor = StaticColors.ForeGround;
                }
                else
                    box._textBox.ForeColor = Color.Red;
            };
            return box;
        }

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
    }

    internal enum MacroCaptureMode
    {
        None,
        Coordinate,
        MouseSample,
    }
}
