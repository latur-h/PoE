using PoE.dlls.Flasks;
using PoE.dlls.Flasks.Base;
using PoE.dlls.Gamble;
using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.InteropServices;
using PoE.dlls.Gamble.UI;
using PoE.dlls.KeyBindings;
using PoE.dlls.Logger;
using PoE.dlls.Logger.UI;
using PoE.dlls.Settings;
using PoE.dlls.Settings.Mods;
using PoE.dlls.Style;
using Poss.Win.Automation.GlobalHotKeys;
using Poss.Win.Automation.Input;

namespace PoE
{
    public partial class Main : Form
    {
        private readonly InputSimulator _input;
        private readonly GlobalHotKeyManager _hotkeys;
        private readonly FlaskManager _flaskManager;
        private readonly UserSettings _userSettings;
        private GamblePresetBar gamblePresetBar = null!;
        private GambleRulesPanel gambleRulesPanel = null!;
        private ToolTip toolTip_Gamble = null!;
        private Settings _settings = null!;

        public Main(InputSimulator input, GlobalHotKeyManager hotkeys, FlaskManager flaskManager, UserSettings userSettings)
        {
            _input = input;
            _hotkeys = hotkeys;
            _flaskManager = flaskManager;
            _userSettings = userSettings;

            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            var logsPanel = new LogsPanel(AppLog.Buffer) { Dock = DockStyle.Fill };
            tabPage_Logs.Controls.Add(logsPanel);

            Console.SetOut(new ConsoleLogger(AppLog.Buffer));
            Console.SetError(new ConsoleLogger(AppLog.Buffer));

            _settings = _userSettings.LoadSettings();

            BackColor = StaticColors.BackGround;
            tabPage_Main.BackColor = StaticColors.BackGround;
            tabPage_Gamble.BackColor = StaticColors.BackGround;
            tabPage_Settings.BackColor = StaticColors.BackGround;
            tabPage_Logs.BackColor = StaticColors.BackGround;

            groupBox_GambleSettings.BackColor = StaticColors.BackGround;
            groupBox_GambleSettings.ForeColor = StaticColors.ForeGround;
            groupBox_FlaskSettings.BackColor = StaticColors.BackGround;
            groupBox_FlaskSettings.ForeColor = StaticColors.ForeGround;

            checkBox_Flask1.ForeColor = StaticColors.ForeGround;
            checkBox_Flask2.ForeColor = StaticColors.ForeGround;
            checkBox_Flask3.ForeColor = StaticColors.ForeGround;
            checkBox_Flask4.ForeColor = StaticColors.ForeGround;
            checkBox_Flask5.ForeColor = StaticColors.ForeGround;

            label_Active.ForeColor = StaticColors.ForeGround;
            label_Key.ForeColor = StaticColors.ForeGround;
            label_FlaskType.ForeColor = StaticColors.ForeGround;
            label_Percent.ForeColor = StaticColors.ForeGround;

            label_Flask1_Slider.ForeColor = StaticColors.ForeGround;
            label_Flask2_Slider.ForeColor = StaticColors.ForeGround;
            label_Flask3_Slider.ForeColor = StaticColors.ForeGround;
            label_Flask4_Slider.ForeColor = StaticColors.ForeGround;
            label_Flask5_Slider.ForeColor = StaticColors.ForeGround;

            slider_Flask1.ValueChanged += (s, e) =>
            {
                label_Flask1_Slider.Text = $"{slider_Flask1.Value}";

                _settings.Flasks["1"].Percent = slider_Flask1.Value;
            };
            slider_Flask2.ValueChanged += (s, e) =>
            {
                label_Flask2_Slider.Text = $"{slider_Flask2.Value}";

                _settings.Flasks["2"].Percent = slider_Flask2.Value;
            };
            slider_Flask3.ValueChanged += (s, e) =>
            {
                label_Flask3_Slider.Text = $"{slider_Flask3.Value}";

                _settings.Flasks["3"].Percent = slider_Flask3.Value;
            };
            slider_Flask4.ValueChanged += (s, e) =>
            {
                label_Flask4_Slider.Text = $"{slider_Flask4.Value}";

                _settings.Flasks["4"].Percent = slider_Flask4.Value;
            };
            slider_Flask5.ValueChanged += (s, e) =>
            {
                label_Flask5_Slider.Text = $"{slider_Flask5.Value}";

                _settings.Flasks["5"].Percent = slider_Flask5.Value;
            };

            checkBox_Flask1.CheckedChanged += (s, e) => _settings.Flasks["1"].Active = checkBox_Flask1.Checked;
            checkBox_Flask2.CheckedChanged += (s, e) => _settings.Flasks["2"].Active = checkBox_Flask2.Checked;
            checkBox_Flask3.CheckedChanged += (s, e) => _settings.Flasks["3"].Active = checkBox_Flask3.Checked;
            checkBox_Flask4.CheckedChanged += (s, e) => _settings.Flasks["4"].Active = checkBox_Flask4.Checked;
            checkBox_Flask5.CheckedChanged += (s, e) => _settings.Flasks["5"].Active = checkBox_Flask5.Checked;

            comboBox_Flask1.Items.AddRange(Enum.GetNames<FlaskType>());
            comboBox_Flask1.SelectedIndex = 0;
            comboBox_Flask1.SelectedIndexChanged += (s, e) =>
            {
                if (comboBox_Flask1.SelectedItem?.ToString() == FlaskType.Utility.ToString() || comboBox_Flask1.SelectedItem?.ToString() == FlaskType.Tincture.ToString())
                    groupBox_Flask1.Hide();
                else
                    groupBox_Flask1.Show();

                LabelPercent();

                _settings.Flasks["1"].FlaskType = comboBox_Flask1.SelectedItem?.ToString() ?? string.Empty;
            };

            comboBox_Flask2.Items.AddRange(Enum.GetNames<FlaskType>());
            comboBox_Flask2.SelectedIndex = 0;
            comboBox_Flask2.SelectedIndexChanged += (s, e) =>
            {
                if (comboBox_Flask2.SelectedItem?.ToString() == FlaskType.Utility.ToString() || comboBox_Flask2.SelectedItem?.ToString() == FlaskType.Tincture.ToString())
                    groupBox_Flask2.Hide();
                else
                    groupBox_Flask2.Show();

                LabelPercent();

                _settings.Flasks["2"].FlaskType = comboBox_Flask2.SelectedItem?.ToString() ?? string.Empty;
            };

            comboBox_Flask3.Items.AddRange(Enum.GetNames<FlaskType>());
            comboBox_Flask3.SelectedIndex = 0;
            comboBox_Flask3.SelectedIndexChanged += (s, e) =>
            {
                if (comboBox_Flask3.SelectedItem?.ToString() == FlaskType.Utility.ToString() || comboBox_Flask3.SelectedItem?.ToString() == FlaskType.Tincture.ToString())
                    groupBox_Flask3.Hide();
                else
                    groupBox_Flask3.Show();

                LabelPercent();

                _settings.Flasks["3"].FlaskType = comboBox_Flask3.SelectedItem?.ToString() ?? string.Empty;
            };

            comboBox_Flask4.Items.AddRange(Enum.GetNames<FlaskType>());
            comboBox_Flask4.SelectedIndex = 0;
            comboBox_Flask4.SelectedIndexChanged += (s, e) =>
            {
                if (comboBox_Flask4.SelectedItem?.ToString() == FlaskType.Utility.ToString() || comboBox_Flask4.SelectedItem?.ToString() == FlaskType.Tincture.ToString())
                    groupBox_Flask4.Hide();
                else
                    groupBox_Flask4.Show();

                LabelPercent();

                _settings.Flasks["4"].FlaskType = comboBox_Flask4.SelectedItem?.ToString() ?? string.Empty;
            };

            comboBox_Flask5.Items.AddRange(Enum.GetNames<FlaskType>());
            comboBox_Flask5.SelectedIndex = 0;
            comboBox_Flask5.SelectedIndexChanged += (s, e) =>
            {
                if (comboBox_Flask5.SelectedItem?.ToString() == FlaskType.Utility.ToString() || comboBox_Flask5.SelectedItem?.ToString() == FlaskType.Tincture.ToString())
                    groupBox_Flask5.Hide();
                else
                    groupBox_Flask5.Show();

                LabelPercent();

                _settings.Flasks["5"].FlaskType = comboBox_Flask5.SelectedItem?.ToString() ?? string.Empty;
            };

            textBox_Flask1._textBox.KeyDown += (s, e) => e.SuppressKeyPress = true;
            textBox_Flask1._textBox.KeyUp += (s, e) => BindFlaskKey(_settings.Flasks["1"], textBox_Flask1, e.KeyCode);

            textBox_Flask2._textBox.KeyDown += (s, e) => e.SuppressKeyPress = true;
            textBox_Flask2._textBox.KeyUp += (s, e) => BindFlaskKey(_settings.Flasks["2"], textBox_Flask2, e.KeyCode);

            textBox_Flask3._textBox.KeyDown += (s, e) => e.SuppressKeyPress = true;
            textBox_Flask3._textBox.KeyUp += (s, e) => BindFlaskKey(_settings.Flasks["3"], textBox_Flask3, e.KeyCode);

            textBox_Flask4._textBox.KeyDown += (s, e) => e.SuppressKeyPress = true;
            textBox_Flask4._textBox.KeyUp += (s, e) => BindFlaskKey(_settings.Flasks["4"], textBox_Flask4, e.KeyCode);

            textBox_Flask5._textBox.KeyDown += (s, e) => e.SuppressKeyPress = true;
            textBox_Flask5._textBox.KeyUp += (s, e) => BindFlaskKey(_settings.Flasks["5"], textBox_Flask5, e.KeyCode);

            checkBox_Flask1.Checked = _settings.Flasks["1"].Active;
            checkBox_Flask2.Checked = _settings.Flasks["2"].Active;
            checkBox_Flask3.Checked = _settings.Flasks["3"].Active;
            checkBox_Flask4.Checked = _settings.Flasks["4"].Active;
            checkBox_Flask5.Checked = _settings.Flasks["5"].Active;

            comboBox_Flask1.SelectedItem = _settings.Flasks["1"].FlaskType;
            comboBox_Flask2.SelectedItem = _settings.Flasks["2"].FlaskType;
            comboBox_Flask3.SelectedItem = _settings.Flasks["3"].FlaskType;
            comboBox_Flask4.SelectedItem = _settings.Flasks["4"].FlaskType;
            comboBox_Flask5.SelectedItem = _settings.Flasks["5"].FlaskType;

            slider_Flask1.Value = _settings.Flasks["1"].Percent;
            slider_Flask2.Value = _settings.Flasks["2"].Percent;
            slider_Flask3.Value = _settings.Flasks["3"].Percent;
            slider_Flask4.Value = _settings.Flasks["4"].Percent;
            slider_Flask5.Value = _settings.Flasks["5"].Percent;

            label_Flask1_Slider.Text = $"{slider_Flask1.Value}";
            label_Flask2_Slider.Text = $"{slider_Flask2.Value}";
            label_Flask3_Slider.Text = $"{slider_Flask3.Value}";
            label_Flask4_Slider.Text = $"{slider_Flask4.Value}";
            label_Flask5_Slider.Text = $"{slider_Flask5.Value}";

            label_GambleType.ForeColor = StaticColors.ForeGround;
            label_ItemXY.ForeColor = StaticColors.ForeGround;
            label_BaseXY.ForeColor = StaticColors.ForeGround;
            label_SecondXY.ForeColor = StaticColors.ForeGround;
            label_GamblerGetCoorinatesKey.ForeColor = StaticColors.ForeGround;
            label_GamblerStartKey.ForeColor = StaticColors.ForeGround;
            label_GamblerStopKey.ForeColor = StaticColors.ForeGround;
            label_GamblerDelay.ForeColor = StaticColors.ForeGround;
            label_GambleSpeed.ForeColor = StaticColors.ForeGround;
            label_FlaskRegisterKey.ForeColor = StaticColors.ForeGround;
            label_FlaskDrinkKey.ForeColor = StaticColors.ForeGround;
            label_FlaskStopKey.ForeColor = StaticColors.ForeGround;
            label_FlaskDelay.ForeColor = StaticColors.ForeGround;
            label_FlaskKeyPressDelay.ForeColor = StaticColors.ForeGround;
            label_FlaskHpMpCooldown.ForeColor = StaticColors.ForeGround;
            label_FlaskUtilityCooldown.ForeColor = StaticColors.ForeGround;
            label_FlaskTinctureCooldown.ForeColor = StaticColors.ForeGround;

            comboBox_GambleType.Items.AddRange(Enum.GetNames<GambleType>());
            comboBox_GambleType.SelectedIndex = 0;

            InitializeGamblePresetBar();
            InitializeGambleRulesPanel();
            SetupGambleModeHelp();

            comboBox_GambleType.SelectedIndexChanged += (s, e) =>
            {
                gambleRulesPanel.Commit();

                GambleType gambleType = Enum.Parse<GambleType>(comboBox_GambleType.SelectedItem?.ToString() ?? string.Empty);
                _settings.Modifiers.GambleType = gambleType;
                _settings.Modifiers.RefreshEditAdapter();
                LoadGambleModeIntoUi();
            };

            comboBox_GambleType.SelectedItem = _settings.Modifiers.GambleType.ToString();
            _settings.Modifiers.RefreshEditAdapter();
            LoadGambleModeIntoUi();
            textBox_GamblerDelay._textBox.Text = _settings.Modifiers.Delay.ToString();
            textBox_GambleSpeed._textBox.Text = _settings.Modifiers.Speed.ToString();
            textBox_FlaskDelay._textBox.Text = _settings.FlaskControls.Delay.ToString();
            textBox_FlaskKeyPressDelay._textBox.Text = _settings.FlaskControls.KeyPressDelay.ToString();
            textBox_FlaskHpMpCooldown._textBox.Text = _settings.FlaskControls.HpMpCooldown.ToString();
            textBox_FlaskUtilityCooldown._textBox.Text = _settings.FlaskControls.UtilityCooldown.ToString();
            textBox_FlaskTinctureCooldown._textBox.Text = _settings.FlaskControls.TinctureCooldown.ToString();

            textBox_ItemXY._textBox.KeyUp += (s, e) =>
            {
                if (textBox_ItemXY._textBox.Text.Contains(','))
                {
                    string[] coords = textBox_ItemXY._textBox.Text.Split(',');
                    if (coords.Length == 2 && int.TryParse(coords[0], out int x) && int.TryParse(coords[1], out int y))
                    {
                        Coordinates coordinates = new Coordinates(x, y);
                        _settings.Modifiers.Mode.Item = coordinates;

                        textBox_ItemXY._textBox.ForeColor = StaticColors.ForeGround;
                    }
                    else
                    {
                        textBox_ItemXY._textBox.ForeColor = Color.Red;
                    }
                }
                else
                {
                    textBox_ItemXY._textBox.ForeColor = Color.Red;
                }
            };
            textBox_BaseXY._textBox.KeyUp += (s, e) =>
            {
                if (textBox_BaseXY._textBox.Text.Contains(','))
                {
                    string[] coords = textBox_BaseXY._textBox.Text.Split(',');
                    if (coords.Length == 2 && int.TryParse(coords[0], out int x) && int.TryParse(coords[1], out int y))
                    {
                        Coordinates coordinates = new Coordinates(x, y);
                        _settings.Modifiers.Mode.Base = coordinates;

                        textBox_BaseXY._textBox.ForeColor = StaticColors.ForeGround;
                    }
                    else
                    {
                        textBox_BaseXY._textBox.ForeColor = Color.Red;
                    }
                }
                else
                {
                    textBox_BaseXY._textBox.ForeColor = Color.Red;
                }
            };
            textBox_SecondXY._textBox.KeyUp += (s, e) =>
            {
                if (textBox_SecondXY._textBox.Text.Contains(','))
                {
                    string[] coords = textBox_SecondXY._textBox.Text.Split(',');
                    if (coords.Length == 2 && int.TryParse(coords[0], out int x) && int.TryParse(coords[1], out int y))
                    {
                        Coordinates coordinates = new Coordinates(x, y);
                        _settings.Modifiers.Mode.Second = coordinates;

                        textBox_SecondXY._textBox.ForeColor = StaticColors.ForeGround;
                    }
                    else
                    {
                        textBox_SecondXY._textBox.ForeColor = Color.Red;
                    }
                }
                else
                {
                    textBox_SecondXY._textBox.ForeColor = Color.Red;
                }
            };

            textBox_GamblerGetCoordinatesKey._textBox.KeyDown += (s, e) =>
            {
                e.SuppressKeyPress = true;
            };
            textBox_GamblerGetCoordinatesKey._textBox.KeyUp += (s, e) =>
                BindHotkeySetting("Gambler get coordinates", ref _settings.Modifiers.GetCoorinatesKey, textBox_GamblerGetCoordinatesKey, e.KeyCode);
            textBox_GamblerStartKey._textBox.KeyDown += (s, e) =>
            {
                e.SuppressKeyPress = true;
            };
            textBox_GamblerStartKey._textBox.KeyUp += (s, e) =>
                BindHotkeySetting("Gambler start", ref _settings.Modifiers.GamblerStart, textBox_GamblerStartKey, e.KeyCode);
            textBox_GamblerStopKey._textBox.KeyDown += (s, e) =>
            {
                e.SuppressKeyPress = true;
            };
            textBox_GamblerStopKey._textBox.KeyUp += (s, e) =>
                BindHotkeySetting("Gambler stop", ref _settings.Modifiers.GamblerStop, textBox_GamblerStopKey, e.KeyCode);
            textBox_GamblerDelay._textBox.KeyUp += (s, e) =>
            {
                if (Delay_NumberOnly(textBox_GamblerDelay._textBox))
                    _settings.Modifiers.Delay = int.Parse(textBox_GamblerDelay._textBox.Text);
            };
            textBox_GambleSpeed._textBox.KeyUp += (s, e) =>
            {
                if (Speed_NumberOnly(textBox_GambleSpeed._textBox))
                    _settings.Modifiers.Speed = double.Parse(textBox_GambleSpeed._textBox.Text);
            };

            textBox_FlaskRegisterKey._textBox.KeyDown += (s, e) => e.SuppressKeyPress = true;
            textBox_FlaskRegisterKey._textBox.KeyUp += (s, e) =>
                BindHotkeySetting("Register Flask", ref _settings.FlaskControls.RegisterKey, textBox_FlaskRegisterKey, e.KeyCode);
            textBox_FlaskDrinkKey._textBox.KeyDown += (s, e) => e.SuppressKeyPress = true;
            textBox_FlaskDrinkKey._textBox.KeyUp += (s, e) =>
                BindHotkeySetting("Drink Flask", ref _settings.FlaskControls.DrinkKey, textBox_FlaskDrinkKey, e.KeyCode);
            textBox_FlaskStopKey._textBox.KeyDown += (s, e) => e.SuppressKeyPress = true;
            textBox_FlaskStopKey._textBox.KeyUp += (s, e) =>
                BindHotkeySetting("Stop Drinking", ref _settings.FlaskControls.StopKey, textBox_FlaskStopKey, e.KeyCode);
            BindFlaskDelayField(textBox_FlaskDelay, v => _settings.FlaskControls.Delay = v);
            BindFlaskDelayField(textBox_FlaskKeyPressDelay, v => _settings.FlaskControls.KeyPressDelay = v);
            BindFlaskDelayField(textBox_FlaskHpMpCooldown, v => _settings.FlaskControls.HpMpCooldown = v);
            BindFlaskDelayField(textBox_FlaskUtilityCooldown, v => _settings.FlaskControls.UtilityCooldown = v);
            BindFlaskDelayField(textBox_FlaskTinctureCooldown, v => _settings.FlaskControls.TinctureCooldown = v);

            button_Record1.Click += (s, e) =>
            {
                if (_getCoordinatesBase)
                    button_Record2.PerformClick();
                if (_getCoordinatesSecond)
                    button_Record3.PerformClick();

                if (_getCoordinatesItem)
                {
                    _getCoordinatesItem = false;

                    button_Record1.ForeColor = Color.Black;
                    button_Record1.Text = "Rec";
                }
                else
                {
                    _getCoordinatesItem = true;

                    button_Record1.ForeColor = Color.Red;
                    button_Record1.Text = "...";
                }
            };
            button_Record2.Click += (s, e) =>
            {
                if (_getCoordinatesItem)
                    button_Record1.PerformClick();
                if (_getCoordinatesSecond)
                    button_Record3.PerformClick();

                if (_getCoordinatesBase)
                {
                    _getCoordinatesBase = false;

                    button_Record2.ForeColor = Color.Black;
                    button_Record2.Text = "Rec";
                }
                else
                {
                    _getCoordinatesBase = true;

                    button_Record2.ForeColor = Color.Red;
                    button_Record2.Text = "...";
                }
            };
            button_Record3.Click += (s, e) =>
            {
                if (_getCoordinatesItem)
                    button_Record1.PerformClick();
                if (_getCoordinatesBase)
                    button_Record2.PerformClick();

                if (_getCoordinatesSecond)
                {
                    _getCoordinatesSecond = false;

                    button_Record3.ForeColor = Color.Black;
                    button_Record3.Text = "Rec";
                }
                else
                {
                    _getCoordinatesSecond = true;

                    button_Record3.ForeColor = Color.Red;
                    button_Record3.Text = "...";
                }
            };

            ValidateAllStoredKeys();

            SetupSettingsHints();

            _ = Init();
        }

        private void LoadGambleModeIntoUi()
        {
            var mode = _settings.Modifiers.Mode;

            textBox_ItemXY._textBox.Text = $"{mode.Item.X}, {mode.Item.Y}";
            textBox_BaseXY._textBox.Text = $"{mode.Base.X}, {mode.Base.Y}";
            textBox_SecondXY._textBox.Text = $"{mode.Second.X}, {mode.Second.Y}";

            gamblePresetBar.RefreshPresets();
            var store = _settings.Modifiers.GetModeStore(_settings.Modifiers.GambleType);
            gambleRulesPanel.PurgeViewsExcept(store.Presets);
            gambleRulesPanel.Bind(_settings.Modifiers.GetActivePreset());

            UpdateGambleSecondCoordinateVisibility();
        }

        private void InitializeGamblePresetBar()
        {
            gamblePresetBar = new GamblePresetBar
            {
                Location = new Point(7, 64),
                Size = new Size(603, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            gamblePresetBar.Bind(_settings.Modifiers);
            gamblePresetBar.PresetChanging += (_, _) => gambleRulesPanel.Commit();
            gamblePresetBar.PresetRemoved += (_, preset) => gambleRulesPanel.DropPresetView(preset);
            gamblePresetBar.PresetChanged += (_, _) =>
            {
                _settings.Modifiers.RefreshEditAdapter();
                gambleRulesPanel.Bind(_settings.Modifiers.GetActivePreset());
            };
            tabPage_Gamble.Controls.Add(gamblePresetBar);
        }

        private void InitializeGambleRulesPanel()
        {
            gambleRulesPanel = new GambleRulesPanel
            {
                Location = new Point(7, 98),
                Size = new Size(603, 294),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            };
            tabPage_Gamble.Controls.Add(gambleRulesPanel);
        }

        private void SetupGambleModeHelp()
        {
            toolTip_Gamble = new ToolTip(components);
            SettingsHintHelper.Configure(toolTip_Gamble);

            SettingsHintHelper.Attach(
                toolTip_Gamble,
                tabPage_Gamble,
                label_GambleType,
                comboBox_GambleType,
                "How this gamble mode works",
                () => GambleModeHelpDialog.ShowForMode(this, _settings.Modifiers.GambleType));
        }

        private void UpdateGambleSecondCoordinateVisibility()
        {
            bool showSecond = GambleModeLayout.UsesSecond(_settings.Modifiers.GambleType);
            label_SecondXY.Visible = showSecond;
            textBox_SecondXY.Visible = showSecond;
            button_Record3.Visible = showSecond;
        }

        private void SetupSettingsHints()
        {
            SettingsHintHelper.Configure(toolTip_Settings);

            SettingsHintHelper.Attach(toolTip_Settings, groupBox_GambleSettings, label_GamblerGetCoorinatesKey, textBox_GamblerGetCoordinatesKey,
                "Hotkey to capture the current mouse position as item, orb, or second-slot coordinates while recording on the Gamble tab.");
            SettingsHintHelper.Attach(toolTip_Settings, groupBox_GambleSettings, label_GamblerStartKey, textBox_GamblerStartKey,
                "Hotkey to start the selected gamble routine.");
            SettingsHintHelper.Attach(toolTip_Settings, groupBox_GambleSettings, label_GamblerStopKey, textBox_GamblerStopKey,
                "Hotkey to stop the running gamble routine.");
            SettingsHintHelper.Attach(toolTip_Settings, groupBox_GambleSettings, label_GamblerDelay, textBox_GamblerDelay,
                "Pause in milliseconds between gamble actions (clicks, keys, clipboard). Increase if item text fails to copy or orbs miss.");
            SettingsHintHelper.Attach(toolTip_Settings, groupBox_GambleSettings, label_GambleSpeed, textBox_GambleSpeed,
                "Mouse movement speed when traveling between stash positions. Higher is faster. Does not change click or key timing.");

            SettingsHintHelper.Attach(toolTip_Settings, groupBox_FlaskSettings, label_FlaskRegisterKey, textBox_FlaskRegisterKey,
                "Hotkey to register active flasks and snapshot their bar pixel colors for drinking detection.");
            SettingsHintHelper.Attach(toolTip_Settings, groupBox_FlaskSettings, label_FlaskDrinkKey, textBox_FlaskDrinkKey,
                "Hotkey to start the automatic flask drinking loop.");
            SettingsHintHelper.Attach(toolTip_Settings, groupBox_FlaskSettings, label_FlaskStopKey, textBox_FlaskStopKey,
                "Hotkey to stop the flask drinking loop.");
            SettingsHintHelper.Attach(toolTip_Settings, groupBox_FlaskSettings, label_FlaskDelay, textBox_FlaskDelay,
                "How often the drink loop runs, in milliseconds. The loop still waits this long when PoE is not focused, but skips drinking.");
            SettingsHintHelper.Attach(toolTip_Settings, groupBox_FlaskSettings, label_FlaskKeyPressDelay, textBox_FlaskKeyPressDelay,
                "Delay in milliseconds between flask key down and key up when a drink is sent.");
            SettingsHintHelper.Attach(toolTip_Settings, groupBox_FlaskSettings, label_FlaskHpMpCooldown, textBox_FlaskHpMpCooldown,
                "Minimum time in milliseconds before the same life (HP) or mana (MP) flask can fire again after a successful drink.");
            SettingsHintHelper.Attach(toolTip_Settings, groupBox_FlaskSettings, label_FlaskUtilityCooldown, textBox_FlaskUtilityCooldown,
                "Minimum time in milliseconds before the same utility flask can fire again after a successful drink.");
            SettingsHintHelper.Attach(toolTip_Settings, groupBox_FlaskSettings, label_FlaskTinctureCooldown, textBox_FlaskTinctureCooldown,
                "Minimum time in milliseconds before the same tincture flask can fire again after a successful drink.");
        }

        private void ValidateAllStoredKeys()
        {
            InitFlaskKey(_settings.Flasks["1"], textBox_Flask1);
            InitFlaskKey(_settings.Flasks["2"], textBox_Flask2);
            InitFlaskKey(_settings.Flasks["3"], textBox_Flask3);
            InitFlaskKey(_settings.Flasks["4"], textBox_Flask4);
            InitFlaskKey(_settings.Flasks["5"], textBox_Flask5);

            InitHotkeySetting(ref _settings.Modifiers.GetCoorinatesKey, textBox_GamblerGetCoordinatesKey);
            InitHotkeySetting(ref _settings.Modifiers.GamblerStart, textBox_GamblerStartKey);
            InitHotkeySetting(ref _settings.Modifiers.GamblerStop, textBox_GamblerStopKey);

            InitHotkeySetting(ref _settings.FlaskControls.RegisterKey, textBox_FlaskRegisterKey);
            InitHotkeySetting(ref _settings.FlaskControls.DrinkKey, textBox_FlaskDrinkKey);
            InitHotkeySetting(ref _settings.FlaskControls.StopKey, textBox_FlaskStopKey);
            ApplyFlaskSettings();
        }

        private void BindFlaskDelayField(FlatTextBox textBox, Action<int> setter)
        {
            textBox._textBox.KeyUp += (s, e) =>
            {
                if (Delay_NumberOnly(textBox._textBox))
                {
                    setter(int.Parse(textBox._textBox.Text));
                    ApplyFlaskSettings();
                }
            };
        }

        private void ApplyFlaskSettings()
        {
            _flaskManager.ApplySettings(_settings.FlaskControls);
        }

        private static void InitFlaskKey(UIFlask flask, FlatTextBox textBox)
        {
            if (KeyBindingHelper.TryResolveStored(flask.Key, out string sendKey, out string displayKey))
            {
                flask.Key = sendKey;
                textBox._textBox.Text = displayKey;
                textBox._textBox.ForeColor = StaticColors.ForeGround;
            }
            else
            {
                textBox._textBox.ForeColor = Color.Red;
            }
        }

        private static void BindFlaskKey(UIFlask flask, FlatTextBox textBox, Keys keyCode)
        {
            if (!KeyBindingHelper.TryBindFromWinForms(keyCode, out string sendKey, out string displayKey))
            {
                textBox._textBox.ForeColor = Color.Red;
                return;
            }

            flask.Key = sendKey;
            textBox._textBox.Text = displayKey;
            textBox._textBox.ForeColor = StaticColors.ForeGround;
        }

        private void InitHotkeySetting(ref string setting, FlatTextBox textBox)
        {
            if (KeyBindingHelper.TryResolveStored(setting, out string sendKey, out string displayKey))
            {
                setting = sendKey;
                textBox._textBox.Text = displayKey;
                textBox._textBox.ForeColor = StaticColors.ForeGround;
            }
            else
            {
                textBox._textBox.ForeColor = Color.Red;
            }
        }

        private void BindHotkeySetting(string hotkeyId, ref string setting, FlatTextBox textBox, Keys keyCode)
        {
            if (!KeyBindingHelper.TryBindFromWinForms(keyCode, out string sendKey, out string displayKey))
            {
                textBox._textBox.ForeColor = Color.Red;
                return;
            }

            _hotkeys.Change(hotkeyId, sendKey);
            setting = sendKey;
            textBox._textBox.Text = displayKey;
            textBox._textBox.ForeColor = StaticColors.ForeGround;
        }

        private bool Delay_NumberOnly(TextBox textBox)
        {
            if (int.TryParse(textBox.Text, out int value))
            {
                textBox.ForeColor = StaticColors.ForeGround;
                return true;
            }
            else
            {
                textBox.ForeColor = Color.Red;
                return false;
            }
        }
        private bool Speed_NumberOnly(TextBox textBox)
        {
            if (double.TryParse(textBox.Text, out double value))
            {
                textBox.ForeColor = StaticColors.ForeGround;
                return true;
            }
            else
            {
                textBox.ForeColor = Color.Red;
                return false;
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            _hotkeys.Stop();
            gambleRulesPanel.Commit();

            _userSettings.SaveSettings();
        }

        private void LabelPercent()
        {
            if (!groupBox_Flask1.Visible && !groupBox_Flask2.Visible && !groupBox_Flask3.Visible && !groupBox_Flask4.Visible && !groupBox_Flask5.Visible)
                label_Percent.Visible = false;
            else
                label_Percent.Visible = true;
        }
    }
}
