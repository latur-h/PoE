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
        private CheckBox checkBox_MacroOverlayEnabled = null!;
        private Label label_MacroOverlayCorner = null!;
        private FlatComboBox comboBox_MacroOverlayCorner = null!;
        private Label label_MacrosHelp = null!;
        private MacroProfileBar _macroProfileBar = null!;
        private MacrosPanel _macrosPanel = null!;
        private MacroOverlayForm? _macroOverlay;
        private ToolTip toolTip_Macros = null!;
        private MacroKeyFieldBinder? _macrosEnableKeyBinder;
        private bool _macrosTabUiReady;
        private bool _suppressMacrosFeatureCheckbox;
        private bool _suppressMacroOverlayCheckbox;
        private bool _suppressMacroOverlayCorner;

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

            checkBox_MacroOverlayEnabled = new CheckBox
            {
                AutoSize = true,
                Location = new Point(7, 42),
                Text = "Status overlay",
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Font = new Font("Segoe UI", 12F),
            };
            checkBox_MacroOverlayEnabled.CheckedChanged += (_, _) => OnMacroOverlayEnabledChanged();

            label_MacroOverlayCorner = new Label
            {
                AutoSize = true,
                Location = new Point(158, 46),
                Text = "Corner",
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Font = new Font("Segoe UI", 12F),
            };

            comboBox_MacroOverlayCorner = new FlatComboBox
            {
                Location = new Point(220, 42),
                Size = new Size(130, 30),
                Font = new Font("Segoe UI", 12F),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            comboBox_MacroOverlayCorner.Items.AddRange(["Top left", "Top right", "Bottom left", "Bottom right"]);
            comboBox_MacroOverlayCorner.SelectedIndexChanged += (_, _) => OnMacroOverlayCornerChanged();

            label_MacrosHelp = new Label
            {
                AutoSize = true,
                Location = new Point(318, 10),
                Text = "ⓘ",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Cursor = Cursors.Hand,
            };
            label_MacrosHelp.Click += (_, _) => MacroModeHelpDialog.ShowHelp(this);

            _macroProfileBar = new MacroProfileBar
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            _macroProfileBar.Bind(_settings.Macros);
            _macroProfileBar.ProfileChanging += (_, _) => _macrosPanel.Commit();
            _macroProfileBar.ProfileChanged += (_, _) =>
            {
                LoadSelectedProfileIntoUi();
                RefreshMacroOverlay();
            };

            _macrosPanel = new MacrosPanel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            };
            _macrosPanel.Changed += (_, _) => ApplyMacrosRuntime();
            _macrosPanel.CaptureArmed += (_, _) => ClearCoordinateRecording();

            tabPage_Macros.Controls.Add(label_MacrosEnableKey);
            tabPage_Macros.Controls.Add(textBox_MacrosEnableKey);
            tabPage_Macros.Controls.Add(checkBox_MacrosFeatureEnabled);
            tabPage_Macros.Controls.Add(checkBox_MacroOverlayEnabled);
            tabPage_Macros.Controls.Add(label_MacroOverlayCorner);
            tabPage_Macros.Controls.Add(comboBox_MacroOverlayCorner);
            tabPage_Macros.Controls.Add(label_MacrosHelp);
            tabPage_Macros.Controls.Add(_macroProfileBar);
            tabPage_Macros.Controls.Add(_macrosPanel);

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
                label_MacrosHelp,
                "Explain macro modes, JE/JNE pixel checks, and coordinate tools.");

            toolTip_Macros.SetToolTip(
                checkBox_MacrosFeatureEnabled,
                "Shows whether macros are currently enabled. Click to toggle, or use the feature hotkey.");

            toolTip_Macros.SetToolTip(
                checkBox_MacroOverlayEnabled,
                "Show a transparent on-screen overlay listing runtime macros. Green = on, red = off.");

            toolTip_Macros.SetToolTip(
                comboBox_MacroOverlayCorner,
                "Screen corner for the macro status overlay.");

            toolTip_Macros.SetToolTip(
                _macroProfileBar,
                "Global is always active at runtime. Pick another profile to also run its macros alongside Global.");

            toolTip_Macros.SetToolTip(
                _macrosPanel,
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

        private void OnMacroOverlayEnabledChanged()
        {
            if (_suppressMacroOverlayCheckbox)
                return;

            _settings.Macros.OverlayEnabled = checkBox_MacroOverlayEnabled.Checked;
            comboBox_MacroOverlayCorner.Enabled = checkBox_MacroOverlayEnabled.Checked;
            RefreshMacroOverlay();
        }

        private void OnMacroOverlayCornerChanged()
        {
            if (_suppressMacroOverlayCorner)
                return;

            _settings.Macros.OverlayCorner = ResolveOverlayCorner(comboBox_MacroOverlayCorner.SelectedIndex);
            RefreshMacroOverlay();
        }

        private static MacroOverlayCorner ResolveOverlayCorner(int selectedIndex) =>
            selectedIndex switch
            {
                1 => MacroOverlayCorner.TopRight,
                2 => MacroOverlayCorner.BottomLeft,
                3 => MacroOverlayCorner.BottomRight,
                _ => MacroOverlayCorner.TopLeft,
            };

        private static int OverlayCornerToIndex(MacroOverlayCorner corner) =>
            corner switch
            {
                MacroOverlayCorner.TopRight => 1,
                MacroOverlayCorner.BottomLeft => 2,
                MacroOverlayCorner.BottomRight => 3,
                _ => 0,
            };

        private void RefreshMacroOverlayCheckbox()
        {
            _suppressMacroOverlayCheckbox = true;
            checkBox_MacroOverlayEnabled.Checked = _settings.Macros.OverlayEnabled;
            _suppressMacroOverlayCheckbox = false;
        }

        private void RefreshMacroOverlayCorner()
        {
            _suppressMacroOverlayCorner = true;
            comboBox_MacroOverlayCorner.SelectedIndex = OverlayCornerToIndex(_settings.Macros.OverlayCorner);
            _suppressMacroOverlayCorner = false;
        }

        private void EnsureMacroOverlay()
        {
            if (_macroOverlay is not null && !_macroOverlay.IsDisposed)
                return;

            _macroOverlay = new MacroOverlayForm(this);
        }

        private void RefreshMacroOverlay()
        {
            if (!_macrosTabUiReady)
                return;

            if (!_settings.Macros.OverlayEnabled)
            {
                if (_macroOverlay is not null && !_macroOverlay.IsDisposed && _macroOverlay.Visible)
                    _macroOverlay.Hide();

                return;
            }

            EnsureMacroOverlay();
            _macroOverlay!.Apply(_settings.Macros, _macroEngine);
        }

        private void DisposeMacroOverlay()
        {
            if (_macroOverlay is null)
                return;

            _macroOverlay.Dispose();
            _macroOverlay = null;
        }

        private void OnMacrosFeatureEnabledChanged()
        {
            if (_suppressMacrosFeatureCheckbox)
                return;

            _settings.Macros.FeatureEnabled = checkBox_MacrosFeatureEnabled.Checked;
            _macroEngine.SetFeatureEnabled(checkBox_MacrosFeatureEnabled.Checked);
            ApplyMacrosRuntime();
            RefreshMacroOverlay();
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
            RefreshMacroOverlayCheckbox();
            RefreshMacroOverlayCorner();
            _macroProfileBar.RefreshProfiles();
            LoadSelectedProfileIntoUi();
            RefreshMacroOverlay();
        }

        private void LoadSelectedProfileIntoUi()
        {
            var profile = _macroProfileBar.GetSelectedProfile();
            if (profile is not null)
                _macrosPanel.Bind(profile, _settings.Macros);
        }

        private void ApplyMacrosRuntime()
        {
            MacroSettingsHelper.EnsureInitialized(_settings.Macros);
            _macrosPanel.Commit();

            var runtime = MacroRuntimeSettingsBuilder.Build(_settings.Macros);
            _macroEngine.ApplySettings(runtime);
            _macroHotkeyBinder.BindAll(_settings);
            RefreshMacroOverlay();
        }

        private bool TryCommitMacrosTab(bool showConflictDialog)
        {
            if (!_macrosTabUiReady)
                return true;

            _macrosPanel.Commit();
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

            _macrosPanel.Commit();
            _macrosPanel.RefreshActiveStates();
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
            SyncPersistedActivesFromEngine();
            _macrosPanel.SyncActiveFromEngine(_macroEngine);
            RefreshMacroOverlay();
        }

        private void SyncPersistedActivesFromEngine()
        {
            foreach (var (_, trigger) in MacroSettingsHelper.EnumerateTriggers(_settings.Macros, true, true))
            {
                MacroTrigger? resolved = _macroEngine.FindTrigger(trigger.Id);
                if (resolved is not null)
                    trigger.Active = resolved.Active;
            }
        }
    }
}
