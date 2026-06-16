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
        private Label? _label_GambleGridStatus;
        private Label? _label_GambleCellAnchor;
        private Button? _button_GambleCellAnchorRec;
        private FlatTextBox? _textBox_GambleCellAnchor;
        private Label? _label_GambleNextX;
        private FlatTextBox? _textBox_GambleNextX;
        private Label? _label_GambleNextY;
        private FlatTextBox? _textBox_GambleNextY;
        private Button? _button_GambleNextCellRec;
        private Label? _label_GamblerGridPickKey;
        private FlatTextBox? _textBox_GamblerGridPickKey;
        private readonly GambleGridCapture _gambleGridCapture = new();
        private System.Windows.Forms.Timer? _gambleGridCaptureTimer;
        private bool _bulkAnchorCaptureArmed;
        private bool _gambleTabUiReady;
        private bool _showGambleBulkPanel;
        private bool _bulkNextCellCaptureArmed;

        private const int GambleBulkPanelHeight = 118;

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
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9F),
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
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9F),
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
            _groupBox_GambleBulk.Controls.Add(_label_GambleGridStatus);
            _groupBox_GambleBulk.Controls.Add(_label_GambleCellAnchor);
            _groupBox_GambleBulk.Controls.Add(_button_GambleCellAnchorRec);
            _groupBox_GambleBulk.Controls.Add(_textBox_GambleCellAnchor);
            _groupBox_GambleBulk.Controls.Add(_label_GambleNextX);
            _groupBox_GambleBulk.Controls.Add(_textBox_GambleNextX);
            _groupBox_GambleBulk.Controls.Add(_label_GambleNextY);
            _groupBox_GambleBulk.Controls.Add(_textBox_GambleNextY);
            _groupBox_GambleBulk.Controls.Add(_button_GambleNextCellRec);
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
                _settings.Modifiers.MapBulk.CorruptOnSuccess = _checkBox_CorruptOnSuccess.Checked;

            _button_GambleCellAnchorRec.Click += (_, _) => ToggleBulkFirstCellCapture();
            _button_GambleNextCellRec.Click += (_, _) => ToggleBulkNextCellCapture();

            _textBox_GambleCellAnchor._textBox.KeyUp += (_, _) =>
            {
                if (TryParseCoordinateText(_textBox_GambleCellAnchor._textBox.Text, out var coordinates))
                {
                    _settings.Modifiers.MapBulk.CellAnchor = coordinates;
                    _textBox_GambleCellAnchor._textBox.ForeColor = StaticColors.ForeGround;
                    RefreshGambleBulkStatusLabel();
                }
                else
                {
                    _textBox_GambleCellAnchor._textBox.ForeColor = Color.Red;
                }
            };

            _textBox_GambleNextX._textBox.KeyUp += (_, _) => TryApplyBulkStepField(_textBox_GambleNextX, v => _settings.Modifiers.MapBulk.NextX = v, allowZero: false);
            _textBox_GambleNextY._textBox.KeyUp += (_, _) => TryApplyBulkStepField(_textBox_GambleNextY, v => _settings.Modifiers.MapBulk.NextY = v, allowZero: true);

            _textBox_GamblerGridPickKey._textBox.KeyDown += (_, e) => e.SuppressKeyPress = true;
            _textBox_GamblerGridPickKey._textBox.KeyUp += (_, e) =>
                BindHotkeySetting("Gambler grid pick", ref _settings.Modifiers.GamblerGridPickKey, _textBox_GamblerGridPickKey, e.KeyCode);

            _gambleGridCaptureTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _gambleGridCaptureTimer.Tick += (_, _) =>
            {
                if (_gambleGridCapture.Poll(_settings.Modifiers.MapBulk))
                    RefreshGambleBulkStatusLabel();
            };
            _gambleGridCaptureTimer.Start();

            LoadGambleBulkIntoUi();
        }

        internal void SetupGambleBulkHints(ToolTip toolTip)
        {
            if (_checkBox_BulkInventory is null
                || _checkBox_CorruptOnSuccess is null
                || _label_GambleGridStatus is null
                || _label_GambleCellAnchor is null
                || _button_GambleCellAnchorRec is null
                || _textBox_GambleCellAnchor is null
                || _label_GambleNextX is null
                || _textBox_GambleNextX is null
                || _label_GambleNextY is null
                || _textBox_GambleNextY is null
                || _button_GambleNextCellRec is null)
                return;

            if (_groupBox_GambleBulk is not null)
                toolTip.SetToolTip(_groupBox_GambleBulk, "Bulk map rolling — hover each control for a short hint. Open ? on Gamble type for full details.");
            toolTip.SetToolTip(_checkBox_BulkInventory, GambleBulkHelp.Short.BulkInventory);
            toolTip.SetToolTip(_checkBox_CorruptOnSuccess, GambleBulkHelp.Short.CorruptOnSuccess);
            toolTip.SetToolTip(_label_GambleGridStatus, GambleBulkHelp.Short.GridArea);
            toolTip.SetToolTip(_label_GambleCellAnchor, GambleBulkHelp.Short.FirstCell);
            toolTip.SetToolTip(_button_GambleCellAnchorRec, GambleBulkHelp.Short.FirstCell);
            toolTip.SetToolTip(_textBox_GambleCellAnchor, GambleBulkHelp.Short.FirstCell);
            toolTip.SetToolTip(_label_GambleNextX, GambleBulkHelp.Short.NextX);
            toolTip.SetToolTip(_textBox_GambleNextX, GambleBulkHelp.Short.NextX);
            toolTip.SetToolTip(_label_GambleNextY, GambleBulkHelp.Short.NextY);
            toolTip.SetToolTip(_textBox_GambleNextY, GambleBulkHelp.Short.NextY);
            toolTip.SetToolTip(_button_GambleNextCellRec, GambleBulkHelp.Short.NextCellRec);
        }

        private void FinalizeGambleTabUi()
        {
            _gambleTabUiReady = true;
            LoadGambleModeIntoUi();
            Shown += (_, _) => LayoutGambleTab();
        }

        private void LoadGambleBulkIntoUi()
        {
            if (_checkBox_BulkInventory is null)
                return;

            var bulk = _settings.Modifiers.MapBulk;
            _checkBox_BulkInventory.Checked = bulk.BulkInventory;
            _checkBox_CorruptOnSuccess!.Checked = bulk.CorruptOnSuccess;
            _textBox_GambleCellAnchor!._textBox.Text = $"{bulk.CellAnchor.X}, {bulk.CellAnchor.Y}";
            _textBox_GambleCellAnchor._textBox.ForeColor = StaticColors.ForeGround;
            _textBox_GambleNextX!._textBox.Text = bulk.NextX.ToString();
            _textBox_GambleNextX._textBox.ForeColor = StaticColors.ForeGround;
            _textBox_GambleNextY!._textBox.Text = bulk.NextY.ToString();
            _textBox_GambleNextY._textBox.ForeColor = StaticColors.ForeGround;
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
                return;
            }

            int cells = GambleGridCalculator.BuildCellCenters(bulk).Count;
            if (!bulk.IsConfigured)
            {
                string missing = !bulk.HasCellStep ? "set Next X / Next Y" : "pick First cell";
                _label_GambleGridStatus.Text = $"Grid area set — {missing} ({cells} cells when ready)";
                return;
            }

            _label_GambleGridStatus.Text = $"Grid ready: {cells} cells";
        }

        private void TryApplyBulkStepField(FlatTextBox textBox, Action<int> setter, bool allowZero)
        {
            if (int.TryParse(textBox._textBox.Text, out int value) && (allowZero ? value >= 0 : value > 0))
            {
                setter(value);
                textBox._textBox.ForeColor = StaticColors.ForeGround;
                RefreshGambleBulkStatusLabel();
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
            _button_GambleCellAnchorRec.ForeColor = Color.Black;
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
            _button_GambleNextCellRec.ForeColor = Color.Black;
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
    }
}
