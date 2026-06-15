using PoE.dlls.InteropServices;
using PoE.dlls.Settings.Mods;
using PoE.dlls.Style;

namespace PoE
{
    public partial class Main
    {
        private void SetupResponsiveLayout()
        {
            tabControl_Main.Dock = DockStyle.Fill;
            MinimumSize = new Size(640, 420);
            MaximumSize = new Size(0, 0);

            tabPage_Main.Resize += (_, _) => LayoutMainTab();
            tabPage_Gamble.Resize += (_, _) => LayoutGambleTab();
            tabPage_Orbs.Resize += (_, _) => LayoutOrbsTab();
            tabPage_Settings.Resize += (_, _) => LayoutSettingsTab();
            Resize += (_, _) =>
            {
                LayoutMainTab();
                LayoutGambleTab();
                LayoutOrbsTab();
                LayoutSettingsTab();
            };
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
                _gambleTypeHelpIcon.BringToFront();
            }

            label_GambleType.BringToFront();
            comboBox_GambleType.BringToFront();

            int contentTop = rowY + rowHeight + margin;
            gamblePresetBar.Location = new Point(margin, contentTop);
            gamblePresetBar.Width = width - margin * 2;

            gambleRulesPanel.Location = new Point(margin, contentTop + gamblePresetBar.Height + 4);
            gambleRulesPanel.Size = new Size(width - margin * 2, Math.Max(120, height - gambleRulesPanel.Top - margin));
        }

        private void LayoutSettingsTab()
        {
            const int margin = 7;
            int width = tabPage_Settings.ClientSize.Width;
            int height = tabPage_Settings.ClientSize.Height;
            if (width <= 0 || height <= 0)
                return;

            int innerWidth = width - margin * 2;
            groupBox_GambleSettings.Location = new Point(margin, margin);
            groupBox_GambleSettings.Size = new Size(innerWidth, 158);

            groupBox_FlaskSettings.Location = new Point(margin, groupBox_GambleSettings.Bottom + margin);
            groupBox_FlaskSettings.Size = new Size(innerWidth, Math.Max(120, height - groupBox_FlaskSettings.Top - margin));
        }
    }
}
