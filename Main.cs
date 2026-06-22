using PoE.dlls.Automation;
using PoE.dlls.Flasks;
using PoE.dlls.Flasks.Base;
using PoE.dlls.Gamble;
using PoE.dlls.Gamble.UI;
using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.GameData;
using PoE.dlls.InteropServices;
using PoE.dlls.KeyBindings;
using PoE.dlls.Logger;
using PoE.dlls.Logger.UI;
using PoE.dlls.Macros;
using PoE.dlls.Settings;
using PoE.dlls.Settings.Mods;
using PoE.dlls.Style;
using Poss.Win.Automation.GlobalHotKeys;

namespace PoE
{
    public partial class Main : Form
    {
        private readonly InputSimulatorHost _inputHost;
        private readonly GlobalHotKeyManager _hotkeys;
        private readonly FlaskManager _flaskManager;
        private readonly UserSettings _userSettings;
        private readonly GameDataRefreshService _gameDataRefresh;
        private readonly ModSuggestionService _modSuggestions;
        private readonly ModCacheDatabase _modCacheDatabase;
        private readonly MacroEngine _macroEngine;
        private readonly MacroHotkeyBinder _macroHotkeyBinder;
        private GamblePresetBar gamblePresetBar = null!;
        private GambleRulesPanel gambleRulesPanel = null!;
        private Label? _gambleTypeHelpIcon;
        private ToolTip toolTip_Gamble = null!;
        private Settings _settings = null!;

        public Main(
            InputSimulatorHost inputHost,
            GlobalHotKeyManager hotkeys,
            FlaskManager flaskManager,
            UserSettings userSettings,
            GameDataRefreshService gameDataRefresh,
            ModSuggestionService modSuggestions,
            ModCacheDatabase modCacheDatabase,
            MacroEngine macroEngine,
            MacroHotkeyBinder macroHotkeyBinder)
        {
            _inputHost = inputHost;
            _hotkeys = hotkeys;
            _flaskManager = flaskManager;
            _userSettings = userSettings;
            _gameDataRefresh = gameDataRefresh;
            _modSuggestions = modSuggestions;
            _modCacheDatabase = modCacheDatabase;
            _macroEngine = macroEngine;
            _macroHotkeyBinder = macroHotkeyBinder;
            _macroEngine.SettingsChanged += MacrosEngine_SettingsChanged;

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
            tabPage_Orbs.BackColor = StaticColors.BackGround;
            tabPage_Macros.BackColor = StaticColors.BackGround;
            tabPage_Settings.BackColor = StaticColors.BackGround;
            tabPage_Settings.AutoScroll = true;
            tabPage_Logs.BackColor = StaticColors.BackGround;

            groupBox_GambleSettings.BackColor = StaticColors.BackGround;
            groupBox_GambleSettings.ForeColor = StaticColors.ForeGround;
            groupBox_FlaskSettings.BackColor = StaticColors.BackGround;
            groupBox_FlaskSettings.ForeColor = StaticColors.ForeGround;

            foreach (var groupBox in new[] { groupBox_Flask1, groupBox_Flask2, groupBox_Flask3, groupBox_Flask4, groupBox_Flask5 })
            {
                groupBox.BackColor = StaticColors.BackGround;
                groupBox.ForeColor = StaticColors.ForeGround;
                groupBox.CenterTitle = true;
            }

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

            checkBox_Flask1.CheckedChanged += (_, _) => OnFlaskActiveChanged("1", checkBox_Flask1.Checked);
            checkBox_Flask2.CheckedChanged += (_, _) => OnFlaskActiveChanged("2", checkBox_Flask2.Checked);
            checkBox_Flask3.CheckedChanged += (_, _) => OnFlaskActiveChanged("3", checkBox_Flask3.Checked);
            checkBox_Flask4.CheckedChanged += (_, _) => OnFlaskActiveChanged("4", checkBox_Flask4.Checked);
            checkBox_Flask5.CheckedChanged += (_, _) => OnFlaskActiveChanged("5", checkBox_Flask5.Checked);

            comboBox_Flask1.Items.AddRange(Enum.GetNames<FlaskType>());
            comboBox_Flask1.SelectedIndex = 0;
            comboBox_Flask1.SelectedIndexChanged += (s, e) =>
            {
                ApplyFlaskPercentVisibility();
                _settings.Flasks["1"].FlaskType = comboBox_Flask1.SelectedItem?.ToString() ?? string.Empty;
            };

            comboBox_Flask2.Items.AddRange(Enum.GetNames<FlaskType>());
            comboBox_Flask2.SelectedIndex = 0;
            comboBox_Flask2.SelectedIndexChanged += (s, e) =>
            {
                ApplyFlaskPercentVisibility();
                _settings.Flasks["2"].FlaskType = comboBox_Flask2.SelectedItem?.ToString() ?? string.Empty;
            };

            comboBox_Flask3.Items.AddRange(Enum.GetNames<FlaskType>());
            comboBox_Flask3.SelectedIndex = 0;
            comboBox_Flask3.SelectedIndexChanged += (s, e) =>
            {
                ApplyFlaskPercentVisibility();
                _settings.Flasks["3"].FlaskType = comboBox_Flask3.SelectedItem?.ToString() ?? string.Empty;
            };

            comboBox_Flask4.Items.AddRange(Enum.GetNames<FlaskType>());
            comboBox_Flask4.SelectedIndex = 0;
            comboBox_Flask4.SelectedIndexChanged += (s, e) =>
            {
                ApplyFlaskPercentVisibility();
                _settings.Flasks["4"].FlaskType = comboBox_Flask4.SelectedItem?.ToString() ?? string.Empty;
            };

            comboBox_Flask5.Items.AddRange(Enum.GetNames<FlaskType>());
            comboBox_Flask5.SelectedIndex = 0;
            comboBox_Flask5.SelectedIndexChanged += (s, e) =>
            {
                ApplyFlaskPercentVisibility();
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

            ApplyFlaskPercentVisibility();

            label_GambleType.ForeColor = StaticColors.ForeGround;
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

            SetupResponsiveLayout();

            comboBox_GambleType.Items.AddRange(GambleTypeNames.All.Select(GambleTypeNames.DisplayName).ToArray());
            comboBox_GambleType.SelectedIndex = 0;

            InitializeGamblePresetBar();
            InitializeGambleRulesPanel();
            InitializeGambleBulkUi();
            InitializeGameDataSettingsUi();
            InitializeInputSettingsUi();
            InitializeSettingsSeparators();
            InitializeOrbsTab();
            InitializeMacrosTab();
            SetupGambleModeHelp();

            tabControl_Main.Selecting += MacrosTab_Selecting;

            comboBox_GambleType.SelectedIndexChanged += (s, e) =>
            {
                if (!_gambleTabUiReady)
                    return;

                gambleRulesPanel.Commit();

                int index = comboBox_GambleType.SelectedIndex;
                if (index < 0 || index >= GambleTypeNames.All.Length)
                    return;

                _settings.Modifiers.GambleType = GambleTypeNames.All[index];
                _settings.Modifiers.RefreshEditAdapter();
                LoadGambleModeIntoUi();
            };

            comboBox_GambleType.SelectedIndex = GambleTypeNames.IndexOf(_settings.Modifiers.GambleType);
            _settings.Modifiers.RefreshEditAdapter();
            textBox_GamblerDelay._textBox.Text = _settings.Modifiers.Delay.ToString();
            textBox_GambleSpeed._textBox.Text = _settings.Modifiers.Speed.ToString();
            textBox_FlaskDelay._textBox.Text = _settings.FlaskControls.Delay.ToString();
            textBox_FlaskKeyPressDelay._textBox.Text = _settings.FlaskControls.KeyPressDelay.ToString();
            textBox_FlaskHpMpCooldown._textBox.Text = _settings.FlaskControls.HpMpCooldown.ToString();
            textBox_FlaskUtilityCooldown._textBox.Text = _settings.FlaskControls.UtilityCooldown.ToString();
            textBox_FlaskTinctureCooldown._textBox.Text = _settings.FlaskControls.TinctureCooldown.ToString();

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

            ValidateAllStoredKeys();

            SetupSettingsHints();

            FinalizeGambleTabUi();

            ApplySavedWindowSize();

            LayoutMainTab();
            LayoutOrbsTab();
            LayoutMacrosTab();
            LayoutSettingsTab();

            _ = Init();
        }

        private void LoadGambleModeIntoUi()
        {
            var store = _settings.Modifiers.GetModeStore(_settings.Modifiers.GambleType);

            gamblePresetBar.RefreshPresets();
            gambleRulesPanel.PurgeViewsExcept(store.Presets);
            gambleRulesPanel.Bind(_settings.Modifiers.GetActivePreset());
            gambleRulesPanel.RefreshGambleTypeLayout();
            UpdateGambleBulkPanelVisibility();
        }

        private void InitializeGamblePresetBar()
        {
            gamblePresetBar = new GamblePresetBar
            {
                Location = new Point(7, 40),
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
            gambleRulesPanel = new GambleRulesPanel(_modSuggestions, () => _settings.Modifiers.GambleType)
            {
                Location = new Point(7, 74),
                Size = new Size(603, 294),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            };
            tabPage_Gamble.Controls.Add(gambleRulesPanel);
        }

        private void SetupGambleModeHelp()
        {
            toolTip_Gamble = new ToolTip(components);
            SettingsHintHelper.Configure(toolTip_Gamble);

            _gambleTypeHelpIcon = SettingsHintHelper.Attach(
                toolTip_Gamble,
                tabPage_Gamble,
                label_GambleType,
                comboBox_GambleType,
                "How this gamble mode works",
                () => GambleModeHelpDialog.ShowForMode(this, _settings.Modifiers.GambleType));

            SetupGambleBulkHints(toolTip_Gamble);
        }

        private void SetupSettingsHints()
        {
            SettingsHintHelper.Configure(toolTip_Settings);

            SettingsHintHelper.Attach(toolTip_Settings, groupBox_GambleSettings, label_GamblerGetCoorinatesKey, textBox_GamblerGetCoordinatesKey,
                "Hotkey to capture the mouse position for the active Rec slot on the Orbs tab (items and orbs are shared across gamble modes).");
            if (_label_GamblerGridPickKey is not null && _textBox_GamblerGridPickKey is not null)
            {
                SettingsHintHelper.Attach(toolTip_Settings, groupBox_GambleSettings, _label_GamblerGridPickKey, _textBox_GamblerGridPickKey,
                    GambleBulkHelp.Short.GridArea + " Full details: Gamble tab → ? help.");
            }
            SettingsHintHelper.Attach(toolTip_Settings, groupBox_GambleSettings, label_GamblerStartKey, textBox_GamblerStartKey,
                "Hotkey to start the selected gamble routine.");
            SettingsHintHelper.Attach(toolTip_Settings, groupBox_GambleSettings, label_GamblerStopKey, textBox_GamblerStopKey,
                "Hotkey to stop the running gamble routine.");
            SettingsHintHelper.Attach(toolTip_Settings, groupBox_GambleSettings, label_GamblerDelay, textBox_GamblerDelay,
                "Pause between every gamble input (click/key gaps, mouse steps). All modes. " +
                "Map modes also use Gamble tab → Bulk → Refresh ms after slams before copy — see Gamble tab ? help. " +
                "Without network lag, try Delay 20 ms with Refresh ms 10; lower both if stable, raise Delay if orbs miss or Refresh ms if copy is stale.");
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
            if (_textBox_GamblerGridPickKey is not null)
                InitHotkeySetting(ref _settings.Modifiers.GamblerGridPickKey, _textBox_GamblerGridPickKey);
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
            if (!TryCommitMacrosTab(showConflictDialog: true))
            {
                e.Cancel = true;
                return;
            }

            _hotkeys.Stop();
            DisposeMacroOverlay();
            DisposeOverlayMonitoring();
            _macroEngine.Stop();
            gambleRulesPanel.Commit();
            SaveWindowSize();

            _userSettings.SaveSettings();
        }

        private static bool FlaskTypeUsesPercent(string? flaskType) =>
            flaskType != FlaskType.Utility.ToString() && flaskType != FlaskType.Tincture.ToString();

        private void ApplyFlaskPercentVisibility()
        {
            SetFlaskPercentVisible(comboBox_Flask1, slider_Flask1, label_Flask1_Slider);
            SetFlaskPercentVisible(comboBox_Flask2, slider_Flask2, label_Flask2_Slider);
            SetFlaskPercentVisible(comboBox_Flask3, slider_Flask3, label_Flask3_Slider);
            SetFlaskPercentVisible(comboBox_Flask4, slider_Flask4, label_Flask4_Slider);
            SetFlaskPercentVisible(comboBox_Flask5, slider_Flask5, label_Flask5_Slider);
            LabelPercent();
        }

        private static void SetFlaskPercentVisible(FlatComboBox combo, Slider slider, Label label)
        {
            bool visible = FlaskTypeUsesPercent(combo.SelectedItem?.ToString());
            slider.Visible = visible;
            label.Visible = visible;
        }

        private void LabelPercent()
        {
            label_Percent.Visible = slider_Flask1.Visible
                || slider_Flask2.Visible
                || slider_Flask3.Visible
                || slider_Flask4.Visible
                || slider_Flask5.Visible;
        }
    }
}
