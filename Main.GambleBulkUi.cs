using PoE.dlls.Gamble;
using PoE.dlls.Gamble.Bulk;
using PoE.dlls.Gamble.UI;
using PoE.dlls.InteropServices;
using PoE.dlls.Logger;
using PoE.dlls.Settings.Mods;
using PoE.dlls.Style;

namespace PoE
{
    public partial class Main
    {
        private GroupBox? _groupBox_GambleBulk;
        private CheckBox? _checkBox_BulkInventory;
        private CheckBox? _checkBox_CorruptOnSuccess;
        private CheckBox? _checkBox_CorruptEightMods;
        private Label? _label_BrokenMapDisposition;
        private FlatComboBox? _comboBox_BrokenMapDisposition;
        private Label? _label_GambleGridStatus;
        private Label? _label_GambleCellAnchor;
        private Button? _button_GambleCellAnchorRec;
        private FlatTextBox? _textBox_GambleCellAnchor;
        private Label? _label_GambleNextX;
        private FlatTextBox? _textBox_GambleNextX;
        private Label? _label_GambleNextY;
        private FlatTextBox? _textBox_GambleNextY;
        private Button? _button_GambleNextCellRec;
        private Label? _label_GambleRefreshDelay;
        private FlatTextBox? _textBox_GambleRefreshDelay;
        private CheckBox? _checkBox_BulkFastEmptyColor;
        private Label? _label_GambleEmptySlotsStatus;
        private Button? _button_GambleEmptySlotsRegister;
        private Label? _label_GamblerGridPickKey;
        private FlatTextBox? _textBox_GamblerGridPickKey;
        private readonly GambleGridCapture _gambleGridCapture = new();
        private System.Windows.Forms.Timer? _gambleGridCaptureTimer;
        private bool _bulkAnchorCaptureArmed;
        private bool _gambleTabUiReady;
        private bool _showGambleBulkPanel;
        private bool _bulkNextCellCaptureArmed;

        private const int GambleBulkPanelHeight = 196;
        private const int GambleEmptyRegisterButtonWidth = 72;

        private void InitializeGambleBulkUi()
        {
            _groupBox_GambleBulk = new GroupBox
            {
                Text = "Map bulk",
                Font = new Font("Segoe UI", 12F),
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Location = new Point(7, 44),
                Size = new Size(400, GambleBulkPanelHeight),
                Visible = false,
            };

            _checkBox_BulkInventory = new CheckBox
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12F),
                ForeColor = StaticColors.ForeGround,
                Text = "Bulk inventory grid",
            };

            _checkBox_CorruptOnSuccess = new CheckBox
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12F),
                ForeColor = StaticColors.ForeGround,
                Text = "Corrupt on success (Vaal)",
            };

            _checkBox_CorruptEightMods = new CheckBox
            {
                AutoSize = true,
                Enabled = false,
                Font = new Font("Segoe UI", 12F),
                ForeColor = StaticColors.ForeGround,
                Text = "8 mods",
            };

            _label_BrokenMapDisposition = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11F),
                ForeColor = StaticColors.ForeGround,
                Text = "Broken",
            };

            _comboBox_BrokenMapDisposition = new FlatComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11F),
                Size = new Size(100, 30),
            };
            _comboBox_BrokenMapDisposition.Items.AddRange(["Stash", "Highlight"]);

            _label_GambleGridStatus = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11F),
                ForeColor = StaticColors.ForeGround,
                Text = "Grid: not configured",
            };

            _label_GambleCellAnchor = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11F),
                ForeColor = StaticColors.ForeGround,
                Text = "First",
            };

            _button_GambleCellAnchorRec = new Button
            {
                Size = new Size(48, 30),
                Text = "Rec",
                Font = new Font("Segoe UI", 9F),
                ForeColor = StaticColors.ButtonForeGround,
                UseVisualStyleBackColor = true,
            };

            _textBox_GambleCellAnchor = new FlatTextBox
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12F),
                Size = new Size(100, 30),
                TextAlign = HorizontalAlignment.Center,
            };

            _label_GambleNextX = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11F),
                ForeColor = StaticColors.ForeGround,
                Text = "Next X",
            };

            _textBox_GambleNextX = new FlatTextBox
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12F),
                Size = new Size(52, 30),
                TextAlign = HorizontalAlignment.Center,
            };

            _label_GambleNextY = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11F),
                ForeColor = StaticColors.ForeGround,
                Text = "Next Y",
            };

            _textBox_GambleNextY = new FlatTextBox
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12F),
                Size = new Size(52, 30),
                TextAlign = HorizontalAlignment.Center,
            };

            _button_GambleNextCellRec = new Button
            {
                Size = new Size(48, 30),
                Text = "Rec",
                Font = new Font("Segoe UI", 9F),
                ForeColor = StaticColors.ButtonForeGround,
                UseVisualStyleBackColor = true,
            };

            _label_GambleRefreshDelay = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11F),
                ForeColor = StaticColors.ForeGround,
                Text = "Refresh ms",
            };

            _textBox_GambleRefreshDelay = new FlatTextBox
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12F),
                Size = new Size(52, 30),
                TextAlign = HorizontalAlignment.Center,
            };

            _checkBox_BulkFastEmptyColor = new CheckBox
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12F),
                ForeColor = StaticColors.ForeGround,
                Text = "Fast empty check",
            };

            _label_GambleEmptySlotsStatus = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11F),
                ForeColor = StaticColors.ForeGround,
                Text = "Empty slots: not registered",
                MaximumSize = new Size(260, 0),
            };

            _button_GambleEmptySlotsRegister = new Button
            {
                Size = new Size(72, 30),
                Text = "Register",
                Font = new Font("Segoe UI", 9F),
                ForeColor = StaticColors.ButtonForeGround,
                UseVisualStyleBackColor = true,
            };

            _label_GamblerGridPickKey = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12F),
                ForeColor = StaticColors.ForeGround,
                Text = "Grid",
            };

            _textBox_GamblerGridPickKey = new FlatTextBox
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12F),
                Size = new Size(85, 30),
                TextAlign = HorizontalAlignment.Center,
            };

            _groupBox_GambleBulk.Controls.Add(_checkBox_BulkInventory);
            _groupBox_GambleBulk.Controls.Add(_checkBox_CorruptOnSuccess);
            _groupBox_GambleBulk.Controls.Add(_checkBox_CorruptEightMods);
            _groupBox_GambleBulk.Controls.Add(_label_BrokenMapDisposition);
            _groupBox_GambleBulk.Controls.Add(_comboBox_BrokenMapDisposition);
            _groupBox_GambleBulk.Controls.Add(_label_GambleGridStatus);
            _groupBox_GambleBulk.Controls.Add(_label_GambleCellAnchor);
            _groupBox_GambleBulk.Controls.Add(_button_GambleCellAnchorRec);
            _groupBox_GambleBulk.Controls.Add(_textBox_GambleCellAnchor);
            _groupBox_GambleBulk.Controls.Add(_label_GambleNextX);
            _groupBox_GambleBulk.Controls.Add(_textBox_GambleNextX);
            _groupBox_GambleBulk.Controls.Add(_label_GambleNextY);
            _groupBox_GambleBulk.Controls.Add(_textBox_GambleNextY);
            _groupBox_GambleBulk.Controls.Add(_button_GambleNextCellRec);
            _groupBox_GambleBulk.Controls.Add(_label_GambleRefreshDelay);
            _groupBox_GambleBulk.Controls.Add(_textBox_GambleRefreshDelay);
            _groupBox_GambleBulk.Controls.Add(_checkBox_BulkFastEmptyColor);
            _groupBox_GambleBulk.Controls.Add(_label_GambleEmptySlotsStatus);
            _groupBox_GambleBulk.Controls.Add(_button_GambleEmptySlotsRegister);
            tabPage_Gamble.Controls.Add(_groupBox_GambleBulk);
            tabPage_Gamble.Controls.SetChildIndex(_groupBox_GambleBulk, 0);

            groupBox_GambleSettings.Controls.Add(_label_GamblerGridPickKey);
            groupBox_GambleSettings.Controls.Add(_textBox_GamblerGridPickKey);
            _label_GamblerGridPickKey.Location = new Point(224, 78);
            _textBox_GamblerGridPickKey.Location = new Point(224, 102);

            _checkBox_BulkInventory.CheckedChanged += (_, _) =>
            {
                _settings.Modifiers.MapBulk.BulkInventory = _checkBox_BulkInventory.Checked;
                RefreshGambleBulkStatusLabel();
            };

            _checkBox_CorruptOnSuccess.CheckedChanged += (_, _) =>
            {
                _settings.Modifiers.MapBulk.CorruptOnSuccess = _checkBox_CorruptOnSuccess.Checked;
                if (!_checkBox_CorruptOnSuccess.Checked)
                {
                    _checkBox_CorruptEightMods!.Checked = false;
                    _settings.Modifiers.MapBulk.CorruptRequireEightMods = false;
                }

                _checkBox_CorruptEightMods!.Enabled = _checkBox_CorruptOnSuccess.Checked;
            };

            _checkBox_CorruptEightMods.CheckedChanged += (_, _) =>
                _settings.Modifiers.MapBulk.CorruptRequireEightMods = _checkBox_CorruptEightMods.Checked;

            _comboBox_BrokenMapDisposition.SelectedIndexChanged += (_, _) =>
            {
                _settings.Modifiers.MapBulk.BrokenMapDisposition = ResolveBrokenMapDisposition(_comboBox_BrokenMapDisposition.SelectedIndex);
                if (_settings.Modifiers.MapBulk.BrokenMapDisposition == BulkMapBrokenDisposition.Stash)
                    ClearBulkMapHighlight();
            };

            _checkBox_BulkFastEmptyColor.CheckedChanged += (_, _) =>
                _settings.Modifiers.MapBulk.FastEmptyColorCheck = _checkBox_BulkFastEmptyColor.Checked;

            _button_GambleCellAnchorRec.Click += (_, _) => ToggleBulkFirstCellCapture();
            _button_GambleNextCellRec.Click += (_, _) => ToggleBulkNextCellCapture();
            _button_GambleEmptySlotsRegister.Click += (_, _) => RegisterBulkEmptySlots();

            _textBox_GambleCellAnchor._textBox.KeyUp += (_, _) =>
            {
                if (TryParseCoordinateText(_textBox_GambleCellAnchor._textBox.Text, out var coordinates))
                {
                    _settings.Modifiers.MapBulk.CellAnchor = coordinates;
                    _textBox_GambleCellAnchor._textBox.ForeColor = StaticColors.ForeGround;
                    RefreshGambleBulkStatusLabel();
                    RefreshEmptySlotRegistrationLabel();
                }
                else
                {
                    _textBox_GambleCellAnchor._textBox.ForeColor = Color.Red;
                }
            };

            _textBox_GambleNextX._textBox.KeyUp += (_, _) => TryApplyBulkStepField(_textBox_GambleNextX, v => _settings.Modifiers.MapBulk.NextX = v, allowZero: false);
            _textBox_GambleNextY._textBox.KeyUp += (_, _) => TryApplyBulkStepField(_textBox_GambleNextY, v => _settings.Modifiers.MapBulk.NextY = v, allowZero: true);
            _textBox_GambleRefreshDelay!._textBox.KeyUp += (_, _) =>
                TryApplyBulkStepField(_textBox_GambleRefreshDelay, v => _settings.Modifiers.MapBulk.RefreshDelayMs = v, allowZero: false);

            _textBox_GamblerGridPickKey._textBox.KeyDown += (_, e) => e.SuppressKeyPress = true;
            _textBox_GamblerGridPickKey._textBox.KeyUp += (_, e) =>
                BindHotkeySetting("Gambler grid pick", ref _settings.Modifiers.GamblerGridPickKey, _textBox_GamblerGridPickKey, e.KeyCode);

            _gambleGridCaptureTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _gambleGridCaptureTimer.Tick += (_, _) =>
            {
                if (_gambleGridCapture.Poll(_settings.Modifiers.MapBulk))
                {
                    BulkEmptySlotHelper.ClearRegistrationIfStale(_settings.Modifiers.MapBulk);
                    RefreshGambleBulkStatusLabel();
                    RefreshEmptySlotRegistrationLabel();
                }
            };
            _gambleGridCaptureTimer.Start();

            LoadGambleBulkIntoUi();
        }

        internal void SetupGambleBulkHints(ToolTip toolTip)
        {
            if (_checkBox_BulkInventory is null
                || _checkBox_CorruptOnSuccess is null
                || _checkBox_CorruptEightMods is null
                || _label_BrokenMapDisposition is null
                || _comboBox_BrokenMapDisposition is null
                || _label_GambleGridStatus is null
                || _label_GambleCellAnchor is null
                || _button_GambleCellAnchorRec is null
                || _textBox_GambleCellAnchor is null
                || _label_GambleNextX is null
                || _textBox_GambleNextX is null
                || _label_GambleNextY is null
                || _textBox_GambleNextY is null
                || _button_GambleNextCellRec is null
                || _label_GambleRefreshDelay is null
                || _textBox_GambleRefreshDelay is null
                || _checkBox_BulkFastEmptyColor is null
                || _label_GambleEmptySlotsStatus is null
                || _button_GambleEmptySlotsRegister is null)
                return;

            if (_groupBox_GambleBulk is not null)
                toolTip.SetToolTip(_groupBox_GambleBulk, "Bulk map rolling — hover each control for a short hint. Open ? on Gamble type for full details.");
            toolTip.SetToolTip(_checkBox_BulkInventory, GambleBulkHelp.Short.BulkInventory);
            toolTip.SetToolTip(_checkBox_CorruptOnSuccess, GambleBulkHelp.Short.CorruptOnSuccess);
            toolTip.SetToolTip(_checkBox_CorruptEightMods, GambleBulkHelp.Short.CorruptRequireEightMods);
            toolTip.SetToolTip(_label_BrokenMapDisposition, GambleBulkHelp.Short.BrokenMapDisposition);
            toolTip.SetToolTip(_comboBox_BrokenMapDisposition, GambleBulkHelp.Short.BrokenMapDisposition);
            toolTip.SetToolTip(_label_GambleGridStatus, GambleBulkHelp.Short.GridArea);
            toolTip.SetToolTip(_label_GambleCellAnchor, GambleBulkHelp.Short.FirstCell);
            toolTip.SetToolTip(_button_GambleCellAnchorRec, GambleBulkHelp.Short.FirstCell);
            toolTip.SetToolTip(_textBox_GambleCellAnchor, GambleBulkHelp.Short.FirstCell);
            toolTip.SetToolTip(_label_GambleNextX, GambleBulkHelp.Short.NextX);
            toolTip.SetToolTip(_textBox_GambleNextX, GambleBulkHelp.Short.NextX);
            toolTip.SetToolTip(_label_GambleNextY, GambleBulkHelp.Short.NextY);
            toolTip.SetToolTip(_textBox_GambleNextY, GambleBulkHelp.Short.NextY);
            toolTip.SetToolTip(_button_GambleNextCellRec, GambleBulkHelp.Short.NextCellRec);
            toolTip.SetToolTip(_label_GambleRefreshDelay, GambleBulkHelp.Short.RefreshDelay);
            toolTip.SetToolTip(_textBox_GambleRefreshDelay, GambleBulkHelp.Short.RefreshDelay);
            toolTip.SetToolTip(_checkBox_BulkFastEmptyColor, GambleBulkHelp.Short.FastEmptyColorCheck);
            toolTip.SetToolTip(_label_GambleEmptySlotsStatus, GambleBulkHelp.Short.EmptySlotRegistration);
            toolTip.SetToolTip(_button_GambleEmptySlotsRegister, GambleBulkHelp.Short.EmptySlotRegister);
        }

        private void FinalizeGambleTabUi()
        {
            _gambleTabUiReady = true;
            LoadGambleModeIntoUi();
            Shown += (_, _) =>
            {
                LayoutGambleTab();
                PreloadTabContentCaches();
            };
        }

        private void LoadGambleBulkIntoUi()
        {
            if (_checkBox_BulkInventory is null)
                return;

            var bulk = _settings.Modifiers.MapBulk;
            _checkBox_BulkInventory.Checked = bulk.BulkInventory;
            _checkBox_CorruptOnSuccess!.Checked = bulk.CorruptOnSuccess;
            _checkBox_CorruptEightMods!.Checked = bulk.CorruptRequireEightMods;
            _checkBox_CorruptEightMods.Enabled = bulk.CorruptOnSuccess;
            _comboBox_BrokenMapDisposition!.SelectedIndex = BrokenMapDispositionToIndex(bulk.BrokenMapDisposition);
            _textBox_GambleCellAnchor!._textBox.Text = $"{bulk.CellAnchor.X}, {bulk.CellAnchor.Y}";
            _textBox_GambleCellAnchor._textBox.ForeColor = StaticColors.ForeGround;
            _textBox_GambleNextX!._textBox.Text = bulk.NextX.ToString();
            _textBox_GambleNextX._textBox.ForeColor = StaticColors.ForeGround;
            _textBox_GambleNextY!._textBox.Text = bulk.NextY.ToString();
            _textBox_GambleNextY._textBox.ForeColor = StaticColors.ForeGround;
            _textBox_GambleRefreshDelay!._textBox.Text = bulk.RefreshDelayMs.ToString();
            _textBox_GambleRefreshDelay._textBox.ForeColor = StaticColors.ForeGround;
            _checkBox_BulkFastEmptyColor!.Checked = bulk.FastEmptyColorCheck;
            BulkEmptySlotHelper.ClearRegistrationIfStale(bulk);
            RefreshEmptySlotRegistrationLabel();
            InitHotkeySetting(ref _settings.Modifiers.GamblerGridPickKey, _textBox_GamblerGridPickKey!);
            RefreshGambleBulkStatusLabel();
        }

        private void UpdateGambleBulkPanelVisibility()
        {
            if (_groupBox_GambleBulk is null)
                return;

            _showGambleBulkPanel = _settings.Modifiers.GambleType is GambleType.Map or GambleType.MapExalt or GambleType.MapT17;

            if (_label_GamblerGridPickKey is not null)
                _label_GamblerGridPickKey.Visible = _showGambleBulkPanel;
            if (_textBox_GamblerGridPickKey is not null)
                _textBox_GamblerGridPickKey.Visible = _showGambleBulkPanel;

            if (!_gambleTabUiReady)
            {
                if (_groupBox_GambleBulk is not null)
                    _groupBox_GambleBulk.Visible = false;
                return;
            }

            LayoutGambleTab();
        }

        private void RefreshGambleBulkStatusLabel()
        {
            if (_label_GambleGridStatus is null)
                return;

            var bulk = _settings.Modifiers.MapBulk;
            if (!bulk.HasGridArea)
            {
                _label_GambleGridStatus.Text = "Grid: press Grid hotkey, drag LMB over stash area";
                RefreshEmptySlotRegistrationLabel();
                return;
            }

            int cells = GambleGridCalculator.BuildCellCenters(bulk).Count;
            if (!bulk.IsConfigured)
            {
                string missing = !bulk.HasCellStep ? "set Next X / Next Y" : "pick First cell";
                _label_GambleGridStatus.Text = $"Grid area set — {missing} ({cells} cells when ready)";
                RefreshEmptySlotRegistrationLabel();
                return;
            }

            _label_GambleGridStatus.Text = $"Grid ready: {cells} cells";
            RefreshEmptySlotRegistrationLabel();
        }

        private void RefreshEmptySlotRegistrationLabel()
        {
            if (_label_GambleEmptySlotsStatus is null || _button_GambleEmptySlotsRegister is null)
                return;

            var bulk = _settings.Modifiers.MapBulk;
            BulkEmptySlotHelper.ClearRegistrationIfStale(bulk);

            if (!bulk.IsConfigured)
            {
                _label_GambleEmptySlotsStatus.Text = "Empty slots: configure grid first";
                _label_GambleEmptySlotsStatus.ForeColor = StaticColors.ForeGround;
                _button_GambleEmptySlotsRegister.Enabled = false;
                return;
            }

            _button_GambleEmptySlotsRegister.Enabled = true;

            if (BulkEmptySlotHelper.IsRegistrationValid(bulk))
            {
                int cells = bulk.EmptySlotSignatures.Count;
                _label_GambleEmptySlotsStatus.Text = $"Empty slots: registered ({cells})";
                _label_GambleEmptySlotsStatus.ForeColor = Color.LimeGreen;
                return;
            }

            _label_GambleEmptySlotsStatus.Text = "Empty slots: not registered";
            _label_GambleEmptySlotsStatus.ForeColor = StaticColors.ForeGround;
        }

        private void RegisterBulkEmptySlots()
        {
            var bulk = _settings.Modifiers.MapBulk;
            if (!BulkEmptySlotHelper.TryRegister(bulk, out string error))
            {
                GamblerLog.Warn(error);
                RefreshEmptySlotRegistrationLabel();
                return;
            }

            int cells = bulk.EmptySlotSignatures.Count;
            GamblerLog.Info($"Registered empty slots for {cells} grid cell(s).");
            RefreshEmptySlotRegistrationLabel();
        }

        private void TryApplyBulkStepField(FlatTextBox textBox, Action<int> setter, bool allowZero)
        {
            if (int.TryParse(textBox._textBox.Text, out int value) && (allowZero ? value >= 0 : value > 0))
            {
                setter(value);
                textBox._textBox.ForeColor = StaticColors.ForeGround;
                RefreshGambleBulkStatusLabel();
                RefreshEmptySlotRegistrationLabel();
                return;
            }

            textBox._textBox.ForeColor = Color.Red;
        }

        private void ToggleBulkFirstCellCapture()
        {
            if (_button_GambleCellAnchorRec is null)
                return;

            if (_bulkAnchorCaptureArmed)
            {
                StopBulkFirstCellCapture();
                return;
            }

            StopBulkNextCellCapture();
            ClearCoordinateRecording();
            DisarmMacroCoordinateCapture();
            _bulkAnchorCaptureArmed = true;
            _button_GambleCellAnchorRec.ForeColor = Color.Red;
            _button_GambleCellAnchorRec.Text = "...";
        }

        private void StopBulkFirstCellCapture()
        {
            if (_button_GambleCellAnchorRec is null)
                return;

            _bulkAnchorCaptureArmed = false;
            _button_GambleCellAnchorRec.ForeColor = StaticColors.ButtonForeGround;
            _button_GambleCellAnchorRec.Text = "Rec";
        }

        private void ToggleBulkNextCellCapture()
        {
            if (_button_GambleNextCellRec is null)
                return;

            if (_bulkNextCellCaptureArmed)
            {
                StopBulkNextCellCapture();
                return;
            }

            StopBulkFirstCellCapture();
            ClearCoordinateRecording();
            DisarmMacroCoordinateCapture();
            _bulkNextCellCaptureArmed = true;
            _button_GambleNextCellRec.ForeColor = Color.Red;
            _button_GambleNextCellRec.Text = "...";
        }

        private void StopBulkNextCellCapture()
        {
            if (_button_GambleNextCellRec is null)
                return;

            _bulkNextCellCaptureArmed = false;
            _button_GambleNextCellRec.ForeColor = StaticColors.ButtonForeGround;
            _button_GambleNextCellRec.Text = "Rec";
        }

        private bool TryApplyBulkAnchorCapture()
        {
            if (_bulkAnchorCaptureArmed)
                return TryApplyBulkFirstCellCapture();

            if (_bulkNextCellCaptureArmed)
                return TryApplyBulkNextCellCapture();

            return false;
        }

        private bool TryApplyBulkFirstCellCapture()
        {
            if (!_bulkAnchorCaptureArmed || _textBox_GambleCellAnchor is null)
                return false;

            var coordinates = InteropHelper.GetMousePos();
            _settings.Modifiers.MapBulk.CellAnchor = coordinates;
            _textBox_GambleCellAnchor._textBox.Text = $"{coordinates.X}, {coordinates.Y}";
            _textBox_GambleCellAnchor._textBox.ForeColor = StaticColors.ForeGround;
            StopBulkFirstCellCapture();
            RefreshGambleBulkStatusLabel();
            RefreshEmptySlotRegistrationLabel();
            return true;
        }

        private bool TryApplyBulkNextCellCapture()
        {
            if (!_bulkNextCellCaptureArmed || _textBox_GambleNextX is null || _textBox_GambleNextY is null)
                return false;

            var anchor = _settings.Modifiers.MapBulk.CellAnchor;
            if (anchor.X <= 0 || anchor.Y <= 0)
            {
                GamblerLog.Warn("Set the First cell before capturing the next map.");
                StopBulkNextCellCapture();
                return true;
            }

            var next = InteropHelper.GetMousePos();
            int deltaX = next.X - anchor.X;
            int deltaY = next.Y - anchor.Y;
            if (deltaX == 0 && deltaY == 0)
            {
                GamblerLog.Warn("Next map must be to the right or below the first cell.");
                StopBulkNextCellCapture();
                return true;
            }

            if (deltaX > 0)
                _settings.Modifiers.MapBulk.NextX = deltaX;
            if (deltaY > 0)
                _settings.Modifiers.MapBulk.NextY = deltaY;
            else if (deltaX > 0)
                _settings.Modifiers.MapBulk.NextY = 0;
            _textBox_GambleNextX._textBox.Text = _settings.Modifiers.MapBulk.NextX.ToString();
            _textBox_GambleNextY._textBox.Text = _settings.Modifiers.MapBulk.NextY.ToString();
            _textBox_GambleNextX._textBox.ForeColor = StaticColors.ForeGround;
            _textBox_GambleNextY._textBox.ForeColor = StaticColors.ForeGround;
            StopBulkNextCellCapture();
            RefreshGambleBulkStatusLabel();
            RefreshEmptySlotRegistrationLabel();
            return true;
        }

        private Task GamblerGridPick()
        {
            Invoke(() =>
            {
                ClearCoordinateRecording();
                DisarmMacroCoordinateCapture();
                StopBulkFirstCellCapture();
                StopBulkNextCellCapture();
                _gambleGridCapture.ArmRectangleCapture();
            });

            return Task.CompletedTask;
        }

        private static int BrokenMapDispositionToIndex(BulkMapBrokenDisposition disposition) =>
            disposition == BulkMapBrokenDisposition.Highlight ? 1 : 0;

        private static BulkMapBrokenDisposition ResolveBrokenMapDisposition(int selectedIndex) =>
            selectedIndex == 1 ? BulkMapBrokenDisposition.Highlight : BulkMapBrokenDisposition.Stash;
    }
}
