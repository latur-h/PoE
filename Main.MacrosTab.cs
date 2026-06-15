using PoE.dlls.Macros;
using PoE.dlls.Macros.UI;
using PoE.dlls.KeyBindings;
using PoE.dlls.Settings.Macros;
using PoE.dlls.Style;

namespace PoE
{
    public partial class Main
    {
        private FlatTextBox textBox_MacrosEnableKey = null!;
        private Label label_MacrosEnableKey = null!;
        private CheckBox checkBox_MacrosFeatureEnabled = null!;
        private Label label_MacrosGlobalSection = null!;
        private Label label_MacrosBuildSection = null!;
        private Panel? _macrosSectionSeparator;
        private MacrosPanel _globalMacrosPanel = null!;
        private MacroBuildPresetBar _macroBuildPresetBar = null!;
        private MacrosPanel _buildMacrosPanel = null!;
        private ToolTip toolTip_Macros = null!;
        private MacroKeyFieldBinder? _macrosEnableKeyBinder;
        private bool _macrosTabUiReady;
        private bool _suppressMacrosFeatureCheckbox;

        private void InitializeMacrosTab()
        {
            tabPage_Macros.BackColor = StaticColors.BackGround;
            tabPage_Macros.AutoScroll = false;

            label_MacrosEnableKey = new Label
            {
                AutoSize = true,
                Location = new Point(7, 10),
                Text = "Feature hotkey",
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Font = new Font("Segoe UI", 12F),
            };

            textBox_MacrosEnableKey = new FlatTextBox
            {
                Location = new Point(132, 6),
                Size = new Size(90, 30),
                Font = new Font("Segoe UI", 12F),
                TextAlign = HorizontalAlignment.Center,
            };
            _macrosEnableKeyBinder = new MacroKeyFieldBinder(
                textBox_MacrosEnableKey,
                value => _settings.Macros.EnableKey = value,
                OnMacrosEnableKeyChanged);

            checkBox_MacrosFeatureEnabled = new CheckBox
            {
                AutoSize = true,
                Location = new Point(230, 10),
                Text = "Enabled",
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Font = new Font("Segoe UI", 12F),
            };
            checkBox_MacrosFeatureEnabled.CheckedChanged += (_, _) => OnMacrosFeatureEnabledChanged();

            label_MacrosGlobalSection = new Label
            {
                AutoSize = true,
                Location = new Point(7, 44),
                Text = "Global (always active)",
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            };

            _globalMacrosPanel = new MacrosPanel
            {
                Location = new Point(7, 68),
                Size = new Size(920, 140),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };

            _macrosSectionSeparator = new Panel
            {
                Height = 1,
                BackColor = StaticColors.TabControlForeGround,
            };

            label_MacrosBuildSection = new Label
            {
                AutoSize = true,
                Text = "Build profile",
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            };

            _macroBuildPresetBar = new MacroBuildPresetBar
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            _macroBuildPresetBar.Bind(_settings.Macros);
            _macroBuildPresetBar.ProfileChanging += (_, _) => _buildMacrosPanel.Commit();
            _macroBuildPresetBar.ProfileChanged += (_, _) => LoadBuildMacrosIntoUi();

            _buildMacrosPanel = new MacrosPanel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            };
            _globalMacrosPanel.Changed += (_, _) => ApplyMacrosRuntime();
            _buildMacrosPanel.Changed += (_, _) => ApplyMacrosRuntime();

            tabPage_Macros.Controls.Add(label_MacrosEnableKey);
            tabPage_Macros.Controls.Add(textBox_MacrosEnableKey);
            tabPage_Macros.Controls.Add(checkBox_MacrosFeatureEnabled);
            tabPage_Macros.Controls.Add(label_MacrosGlobalSection);
            tabPage_Macros.Controls.Add(_globalMacrosPanel);
            tabPage_Macros.Controls.Add(_macrosSectionSeparator);
            tabPage_Macros.Controls.Add(label_MacrosBuildSection);
            tabPage_Macros.Controls.Add(_macroBuildPresetBar);
            tabPage_Macros.Controls.Add(_buildMacrosPanel);

            SetupMacrosHints();
            LoadMacrosTabIntoUi();
            _macrosTabUiReady = true;
        }

        private void SetupMacrosHints()
        {
            toolTip_Macros = new ToolTip(components);
            SettingsHintHelper.Configure(toolTip_Macros);

            SettingsHintHelper.Attach(
                toolTip_Macros,
                tabPage_Macros,
                label_MacrosEnableKey,
                textBox_MacrosEnableKey,
                "Hotkey to enable or disable the entire macro feature.");

            toolTip_Macros.SetToolTip(
                checkBox_MacrosFeatureEnabled,
                "Shows whether macros are currently enabled. Click to toggle, or use the feature hotkey.");

            toolTip_Macros.SetToolTip(
                label_MacrosGlobalSection,
                "Macros in this section are always armed. Recommended for always-on utilities such as mouse side-button spam.");

            toolTip_Macros.SetToolTip(
                label_MacrosBuildSection,
                "One build profile is active at a time. Switch profiles for different characters or builds.");

            toolTip_Macros.SetToolTip(
                _buildMacrosPanel,
                "Fire sequence: one action per line or separated by +, e.g.\n"
                + "LButton Down + LButton Up\n"
                + "or Ctrl Down + A Down + A Up + Ctrl Up");
        }

        private void OnMacrosEnableKeyChanged(object? sender, EventArgs e)
        {
            if (_macrosEnableKeyBinder is null || !_macrosEnableKeyBinder.AllowsRuntime)
                return;

            if (KeyBindingHelper.TryResolveStored(_settings.Macros.EnableKey, out string sendKey, out _))
                _hotkeys.Change("Macros enable", sendKey);
        }

        private void OnMacrosFeatureEnabledChanged()
        {
            if (_suppressMacrosFeatureCheckbox)
                return;

            _settings.Macros.FeatureEnabled = checkBox_MacrosFeatureEnabled.Checked;
            _macroEngine.SetFeatureEnabled(checkBox_MacrosFeatureEnabled.Checked);
            ApplyMacrosRuntime();
        }

        private void RefreshMacrosFeatureCheckbox()
        {
            _suppressMacrosFeatureCheckbox = true;
            checkBox_MacrosFeatureEnabled.Checked = _settings.Macros.FeatureEnabled;
            _suppressMacrosFeatureCheckbox = false;
        }

        private void LoadMacrosTabIntoUi()
        {
            MacroSettingsHelper.EnsureInitialized(_settings.Macros);
            _macrosEnableKeyBinder?.LoadFromStored(_settings.Macros.EnableKey);
            RefreshMacrosFeatureCheckbox();

            _globalMacrosPanel.Bind(_settings.Macros.GlobalProfile);
            _macroBuildPresetBar.RefreshProfiles();
            LoadBuildMacrosIntoUi();
        }

        private void LoadBuildMacrosIntoUi()
        {
            var profile = MacroSettingsHelper.GetActiveBuildProfile(_settings.Macros);
            if (profile is not null)
                _buildMacrosPanel.Bind(profile);
        }

        private void ApplyMacrosRuntime()
        {
            MacroSettingsHelper.EnsureInitialized(_settings.Macros);

            var runtime = MacroRuntimeSettingsBuilder.Build(
                _settings.Macros,
                _globalMacrosPanel.GetRuntimeTriggers(),
                _buildMacrosPanel.GetRuntimeTriggers());

            _macroEngine.ApplySettings(runtime);
            _macroHotkeyBinder.BindAll(_settings);
        }

        private bool TryCommitMacrosTab(bool showConflictDialog)
        {
            if (!_macrosTabUiReady)
                return true;

            _globalMacrosPanel.Commit();
            _buildMacrosPanel.Commit();
            MacroSettingsHelper.EnsureInitialized(_settings.Macros);

            var conflicts = MacroKeyConflictChecker.FindConflicts(_settings);
            if (conflicts.Count == 0)
            {
                ApplyMacrosRuntime();
                return true;
            }

            if (!showConflictDialog)
            {
                ApplyMacrosRuntime();
                return true;
            }

            string message = BuildMacroConflictMessage(conflicts);
            DialogResult result = MessageBox.Show(
                this,
                message,
                "Macro key conflicts",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning);

            if (result == DialogResult.OK)
            {
                ApplyMacrosRuntime();
                return true;
            }

            foreach (var trigger in conflicts.SelectMany(c => c.MacroTriggers).Distinct())
            {
                if (trigger.Active)
                    trigger.Active = false;
            }

            _globalMacrosPanel.Commit();
            _buildMacrosPanel.Commit();
            _globalMacrosPanel.RefreshActiveStates();
            _buildMacrosPanel.RefreshActiveStates();
            ApplyMacrosRuntime();
            return false;
        }

        private static string BuildMacroConflictMessage(IReadOnlyList<MacroKeyConflict> conflicts)
        {
            var lines = new List<string>
            {
                "The same key is assigned to multiple actions:",
                string.Empty,
            };

            foreach (var conflict in conflicts)
            {
                lines.Add($"• {conflict.Key}");
                foreach (string label in conflict.Labels)
                    lines.Add($"    - {label}");
                lines.Add(string.Empty);
            }

            lines.Add("OK keeps these bindings (multiple handlers may run).");
            lines.Add("Cancel deactivates conflicting macros that are currently active.");
            return string.Join(Environment.NewLine, lines);
        }

        private void MacrosTab_Selecting(object? sender, TabControlCancelEventArgs e)
        {
            if (tabControl_Main.SelectedTab != tabPage_Macros || e.TabPage == tabPage_Macros)
                return;

            if (!TryCommitMacrosTab(showConflictDialog: true))
                e.Cancel = true;
        }

        private void MacrosEngine_SettingsChanged()
        {
            if (InvokeRequired)
            {
                BeginInvoke(MacrosEngine_SettingsChanged);
                return;
            }

            _settings.Macros.FeatureEnabled = _macroEngine.FeatureEnabled;
            RefreshMacrosFeatureCheckbox();
            _globalMacrosPanel.RefreshActiveStates();
            _buildMacrosPanel.RefreshActiveStates();
        }
    }
}
