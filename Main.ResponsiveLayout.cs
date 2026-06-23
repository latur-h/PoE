using PoE.dlls.InteropServices;
using PoE.dlls.Settings.Mods;
using PoE.dlls.Style;

namespace PoE
{
    public partial class Main
    {
        private Panel? _separatorUpdateGamble;
        private Panel? _separatorGambleFlask;
        private Panel? _separatorFlaskGameData;
        private Panel? _separatorGameDataInput;

        private const int SettingsUpdateHeight = 76;
        private const int SettingsGambleHeight = 190;
        private const int SettingsFlaskHeight = 158;
        private const int SettingsGameDataMinHeight = 150;
        private const int SettingsInputHeight = 62;
        private const int SettingsSectionGap = 10;

        private void SetupResponsiveLayout()
        {
            tabControl_Main.Dock = DockStyle.Fill;
            MinimumSize = new Size(640, 420);
            MaximumSize = new Size(0, 0);

            tabPage_Main.Resize += (_, _) => LayoutMainTab();
            tabPage_Gamble.Resize += (_, _) => LayoutGambleTab();
            tabPage_Orbs.Resize += (_, _) => LayoutOrbsTab();
            tabPage_Macros.Resize += (_, _) => LayoutMacrosTab();
            tabPage_Settings.Resize += (_, _) => LayoutSettingsTab();
            tabControl_Main.SelectedIndexChanged += (_, _) =>
            {
                if (tabControl_Main.SelectedTab == tabPage_Macros)
                    LayoutMacrosTab();
                else if (tabControl_Main.SelectedTab == tabPage_Gamble)
                    LayoutGambleTab();
            };
            Resize += (_, _) =>
            {
                LayoutMainTab();
                LayoutGambleTab();
                LayoutOrbsTab();
                if (tabControl_Main.SelectedTab == tabPage_Macros)
                    LayoutMacrosTab();
                LayoutSettingsTab();
            };
        }

        private const int DefaultWindowWidth = 944;
        private const int DefaultWindowHeight = 451;
        private const int MinClientWidth = 640;
        private const int MinClientHeight = 420;

        private void ApplySavedWindowSize()
        {
            int width = _settings.WindowWidth > 0 ? _settings.WindowWidth : DefaultWindowWidth;
            int height = _settings.WindowHeight > 0 ? _settings.WindowHeight : DefaultWindowHeight;

            var area = Screen.FromControl(this).WorkingArea;
            width = Math.Clamp(width, MinClientWidth, area.Width);
            height = Math.Clamp(height, MinClientHeight, area.Height);

            ClientSize = new Size(width, height);
        }

        private void SaveWindowSize()
        {
            if (WindowState != FormWindowState.Normal)
                return;

            _settings.WindowWidth = ClientSize.Width;
            _settings.WindowHeight = ClientSize.Height;
        }

        private static bool TryParseCoordinateText(string text, out Coordinates coordinates)
        {
            coordinates = default;
            if (!text.Contains(','))
                return false;

            string[] coords = text.Split(',');
            if (coords.Length != 2 || !int.TryParse(coords[0], out int x) || !int.TryParse(coords[1], out int y))
                return false;

            coordinates = new Coordinates(x, y);
            return true;
        }

        private void LayoutMainTab()
        {
            const int margin = 8;
            const int leftLabelsWidth = 92;
            const int groupTop = 8;
            const int controlWidth = 90;
            const int innerPadding = 6;
            const int rowActive = 30;
            const int rowKey = 56;
            const int rowType = 94;
            const int rowPercent = 132;
            const int sliderTop = 168;

            int width = tabPage_Main.ClientSize.Width;
            int height = tabPage_Main.ClientSize.Height;
            if (width <= 0 || height <= 0)
                return;

            int availableWidth = width - leftLabelsWidth - margin;
            int columnWidth = Math.Max(controlWidth + 4, availableWidth / 5);
            int groupHeight = Math.Max(220, height - groupTop - margin);
            int innerWidth = controlWidth - innerPadding * 2;

            label_Active.Location = new Point(margin, groupTop + rowActive);
            label_Key.Location = new Point(margin, groupTop + rowKey);
            label_FlaskType.Location = new Point(margin, groupTop + rowType);
            label_Percent.Location = new Point(margin, groupTop + rowPercent);

            CheckBox[] checks = [checkBox_Flask1, checkBox_Flask2, checkBox_Flask3, checkBox_Flask4, checkBox_Flask5];
            FlatTextBox[] keys = [textBox_Flask1, textBox_Flask2, textBox_Flask3, textBox_Flask4, textBox_Flask5];
            FlatComboBox[] types = [comboBox_Flask1, comboBox_Flask2, comboBox_Flask3, comboBox_Flask4, comboBox_Flask5];
            GroupBox[] groups = [groupBox_Flask1, groupBox_Flask2, groupBox_Flask3, groupBox_Flask4, groupBox_Flask5];
            Slider[] sliders = [slider_Flask1, slider_Flask2, slider_Flask3, slider_Flask4, slider_Flask5];
            Label[] sliderLabels = [label_Flask1_Slider, label_Flask2_Slider, label_Flask3_Slider, label_Flask4_Slider, label_Flask5_Slider];

            for (int i = 0; i < 5; i++)
            {
                int x = leftLabelsWidth + i * columnWidth + Math.Max(0, (columnWidth - controlWidth) / 2);
                groups[i].Location = new Point(x, groupTop);
                groups[i].Size = new Size(controlWidth, groupHeight);

                checks[i].Location = new Point((controlWidth - checks[i].Width) / 2, rowActive);
                keys[i].Location = new Point(innerPadding, rowKey);
                keys[i].Size = new Size(innerWidth, 30);
                types[i].Location = new Point(innerPadding, rowType);
                types[i].Size = new Size(innerWidth, 30);

                int sliderHeight = Math.Max(80, groupHeight - sliderTop - 28);
                sliders[i].Location = new Point((controlWidth - sliders[i].Width) / 2, sliderTop);
                sliders[i].Size = new Size(sliders[i].Width, sliderHeight);
                sliderLabels[i].Location = new Point((controlWidth - sliderLabels[i].Width) / 2, sliderTop + sliderHeight + 4);
            }
        }

        private void LayoutGambleTab()
        {
            if (!_gambleTabUiReady || gamblePresetBar is null || gambleRulesPanel is null)
                return;

            const int margin = 7;
            const int rowHeight = 30;
            const int labelGap = 8;
            const int iconGap = 6;
            const int preferredTypeWidth = 180;
            const int iconWidth = 20;

            int width = tabPage_Gamble.ClientSize.Width;
            int height = tabPage_Gamble.ClientSize.Height;
            if (width <= 0 || height <= 0)
                return;

            int rowY = margin;
            label_GambleType.Location = new Point(margin, rowY + (rowHeight - label_GambleType.Height) / 2);

            int comboX = label_GambleType.Right + labelGap;
            int comboWidth = Math.Max(120, Math.Min(preferredTypeWidth, width - comboX - iconGap - iconWidth - margin));
            comboBox_GambleType.Location = new Point(comboX, rowY);
            comboBox_GambleType.Size = new Size(comboWidth, rowHeight);

            if (_gambleTypeHelpIcon is not null)
            {
                _gambleTypeHelpIcon.Location = new Point(
                    comboBox_GambleType.Right + iconGap,
                    rowY + (rowHeight - _gambleTypeHelpIcon.Height) / 2);
            }

            int bulkOffset = _showGambleBulkPanel ? GambleBulkPanelHeight + margin : 0;

            if (_groupBox_GambleBulk is not null && _showGambleBulkPanel)
            {
                int bulkY = rowY + rowHeight + margin;
                _groupBox_GambleBulk.Location = new Point(margin, bulkY);
                _groupBox_GambleBulk.Size = new Size(width - margin * 2, GambleBulkPanelHeight);

                if (_checkBox_BulkInventory is not null)
                    _checkBox_BulkInventory.Location = new Point(10, 24);
                if (_checkBox_CorruptOnSuccess is not null)
                    _checkBox_CorruptOnSuccess.Location = new Point(10, 50);
                if (_checkBox_CorruptEightMods is not null)
                    _checkBox_CorruptEightMods.Location = new Point(10, 72);
                if (_label_BrokenMapDisposition is not null)
                    _label_BrokenMapDisposition.Location = new Point(10, 98);
                if (_comboBox_BrokenMapDisposition is not null)
                    _comboBox_BrokenMapDisposition.Location = new Point(68, 94);
                if (_label_GambleGridStatus is not null)
                {
                    _label_GambleGridStatus.Location = new Point(10, 122);
                    _label_GambleGridStatus.MaximumSize = new Size(width - 24, 0);
                }

                if (_label_GambleCellAnchor is not null)
                    _label_GambleCellAnchor.Location = new Point(220, 26);
                if (_button_GambleCellAnchorRec is not null)
                    _button_GambleCellAnchorRec.Location = new Point(262, 22);
                if (_textBox_GambleCellAnchor is not null)
                    _textBox_GambleCellAnchor.Location = new Point(316, 22);

                if (_label_GambleNextX is not null)
                    _label_GambleNextX.Location = new Point(220, 58);
                if (_textBox_GambleNextX is not null)
                    _textBox_GambleNextX.Location = new Point(278, 54);
                if (_label_GambleNextY is not null)
                    _label_GambleNextY.Location = new Point(338, 58);
                if (_textBox_GambleNextY is not null)
                    _textBox_GambleNextY.Location = new Point(396, 54);
                if (_button_GambleNextCellRec is not null)
                    _button_GambleNextCellRec.Location = new Point(454, 54);
                if (_label_GambleRefreshDelay is not null)
                    _label_GambleRefreshDelay.Location = new Point(220, 88);
                if (_textBox_GambleRefreshDelay is not null)
                    _textBox_GambleRefreshDelay.Location = new Point(310, 84);
                if (_checkBox_BulkFastEmptyColor is not null)
                    _checkBox_BulkFastEmptyColor.Location = new Point(10, 146);

                int bulkInnerRight = width - margin * 2 - 10;
                if (_button_GambleEmptySlotsRegister is not null)
                    _button_GambleEmptySlotsRegister.Location = new Point(
                        bulkInnerRight - GambleEmptyRegisterButtonWidth,
                        162);
                if (_label_GambleEmptySlotsStatus is not null)
                {
                    int registerLeft = bulkInnerRight - GambleEmptyRegisterButtonWidth - 8;
                    _label_GambleEmptySlotsStatus.Location = new Point(10, 166);
                    _label_GambleEmptySlotsStatus.MaximumSize = new Size(Math.Max(120, registerLeft - 10), 0);
                }
            }

            int contentTop = rowY + rowHeight + margin + bulkOffset;
            gamblePresetBar.Location = new Point(margin, contentTop);
            gamblePresetBar.Width = width - margin * 2;

            gambleRulesPanel.Location = new Point(margin, contentTop + gamblePresetBar.Height + 4);
            gambleRulesPanel.Size = new Size(width - margin * 2, Math.Max(120, height - gambleRulesPanel.Top - margin));

            if (_groupBox_GambleBulk is not null)
            {
                _groupBox_GambleBulk.Visible = _showGambleBulkPanel;
                _groupBox_GambleBulk.SendToBack();
            }

            label_GambleType.BringToFront();
            comboBox_GambleType.BringToFront();
            _gambleTypeHelpIcon?.BringToFront();
            gamblePresetBar.BringToFront();
            gambleRulesPanel.BringToFront();
        }

        private void LayoutMacrosTab()
        {
            if (!_macrosTabUiReady)
                return;

            const int margin = 7;
            const int topBarHeight = 78;
            const int profileBarHeight = 34;

            int width = tabPage_Macros.ClientSize.Width;
            int height = tabPage_Macros.ClientSize.Height;
            if (width <= 0 || height <= 0)
                return;

            int innerWidth = Math.Max(400, width - margin * 2);
            int y = margin;

            label_MacrosEnableKey.Location = new Point(margin, y + 6);
            textBox_MacrosEnableKey.Location = new Point(132, y + 2);
            checkBox_MacrosFeatureEnabled.Location = new Point(230, y + 8);
            label_MacrosHelp.Location = new Point(318, y + 6);

            checkBox_MacroOverlayEnabled.Location = new Point(margin, y + 38);
            label_MacroOverlayCorner.Location = new Point(158, y + 42);
            comboBox_MacroOverlayCorner.Location = new Point(220, y + 38);
            comboBox_MacroOverlayCorner.Enabled = checkBox_MacroOverlayEnabled.Checked;

            y += topBarHeight;

            _macroProfileBar.Location = new Point(margin, y);
            _macroProfileBar.Size = new Size(innerWidth, profileBarHeight);
            y += profileBarHeight + 6;

            var panelLocation = new Point(margin, y);
            var panelSize = new Size(innerWidth, Math.Max(160, height - y - margin));
            bool panelBoundsChanged = _macrosPanel.Location != panelLocation || _macrosPanel.Size != panelSize;
            if (_macrosPanel.Location != panelLocation)
                _macrosPanel.Location = panelLocation;
            if (_macrosPanel.Size != panelSize)
                _macrosPanel.Size = panelSize;
            if (panelBoundsChanged)
                _macrosPanel.RequestLayout();
        }

        private void LayoutSettingsTab()
        {
            const int margin = 7;
            int width = tabPage_Settings.ClientSize.Width;
            if (width <= 0)
                return;

            if (groupBox_Input is null || groupBox_GameData is null || groupBox_Update is null)
                return;

            int innerWidth = Math.Max(200, width - margin * 2);
            int y = margin;

            y = PlaceSettingsSection(groupBox_Update, margin, y, innerWidth, SettingsUpdateHeight);
            y = PlaceSettingsSeparator(_separatorUpdateGamble, margin, y, innerWidth);
            y = PlaceSettingsSection(groupBox_GambleSettings, margin, y, innerWidth, SettingsGambleHeight);
            y = PlaceSettingsSeparator(_separatorGambleFlask, margin, y, innerWidth);
            y = PlaceSettingsSection(groupBox_FlaskSettings, margin, y, innerWidth, SettingsFlaskHeight);
            y = PlaceSettingsSeparator(_separatorFlaskGameData, margin, y, innerWidth);
            int gameDataHeight = Math.Max(SettingsGameDataMinHeight, LayoutGameDataSettingsGroup(innerWidth));
            y = PlaceSettingsSection(groupBox_GameData, margin, y, innerWidth, gameDataHeight);
            y = PlaceSettingsSeparator(_separatorGameDataInput, margin, y, innerWidth);
            PlaceSettingsSection(groupBox_Input, margin, y, innerWidth, SettingsInputHeight);
            LayoutInputSettingsGroup();
            LayoutAppUpdateSettingsGroup();
        }

        private void InitializeSettingsSeparators()
        {
            if (_separatorGambleFlask is not null)
                return;

            _separatorUpdateGamble = CreateSettingsSeparator();
            _separatorGambleFlask = CreateSettingsSeparator();
            _separatorFlaskGameData = CreateSettingsSeparator();
            _separatorGameDataInput = CreateSettingsSeparator();

            tabPage_Settings.Controls.Add(_separatorUpdateGamble);
            tabPage_Settings.Controls.Add(_separatorGambleFlask);
            tabPage_Settings.Controls.Add(_separatorFlaskGameData);
            tabPage_Settings.Controls.Add(_separatorGameDataInput);
        }

        private static Panel CreateSettingsSeparator() =>
            new()
            {
                Height = 1,
                BackColor = StaticColors.TabControlForeGround,
            };

        private static int PlaceSettingsSection(Control section, int x, int y, int width, int height)
        {
            section.Location = new Point(x, y);
            section.Size = new Size(width, height);
            return y + height;
        }

        private static int PlaceSettingsSeparator(Panel? separator, int x, int y, int width)
        {
            if (separator is null)
                return y;

            y += SettingsSectionGap;
            separator.Location = new Point(x, y);
            separator.Size = new Size(width, 1);
            separator.Visible = true;
            return y + 1 + SettingsSectionGap;
        }
    }
}
