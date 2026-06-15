using PoE.dlls.InteropServices;
using PoE.dlls.Settings.Mods;
using PoE.dlls.Style;

namespace PoE
{
    public partial class Main
    {
        private Label label_ThirdXY = null!;
        private FlatTextBox textBox_ThirdXY = null!;
        private Button button_Record4 = null!;

        private void SetupResponsiveLayout()
        {
            tabControl_Main.Dock = DockStyle.Fill;
            MinimumSize = new Size(640, 420);
            MaximumSize = new Size(0, 0);

            tabPage_Main.Resize += (_, _) => LayoutMainTab();
            tabPage_Gamble.Resize += (_, _) => LayoutGambleTab();
            tabPage_Settings.Resize += (_, _) => LayoutSettingsTab();
            Resize += (_, _) =>
            {
                LayoutMainTab();
                LayoutGambleTab();
                LayoutSettingsTab();
            };
        }

        private void InitializeGambleThirdCoordinate()
        {
            label_ThirdXY = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 12F),
                Text = "Exalt",
                ForeColor = StaticColors.ForeGround,
            };

            textBox_ThirdXY = new FlatTextBox
            {
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12F),
                Size = new Size(97, 30),
                TextAlign = HorizontalAlignment.Center,
            };

            button_Record4 = new Button
            {
                Size = new Size(48, 23),
                Text = "Rec",
                UseVisualStyleBackColor = true,
            };

            tabPage_Gamble.Controls.Add(label_ThirdXY);
            tabPage_Gamble.Controls.Add(textBox_ThirdXY);
            tabPage_Gamble.Controls.Add(button_Record4);

            textBox_ThirdXY._textBox.KeyUp += (_, _) =>
            {
                if (TryParseCoordinateText(textBox_ThirdXY._textBox.Text, out var coordinates))
                {
                    _settings.Modifiers.GetModeStore(_settings.Modifiers.GambleType).Third = coordinates;
                    textBox_ThirdXY._textBox.ForeColor = StaticColors.ForeGround;
                }
                else
                {
                    textBox_ThirdXY._textBox.ForeColor = Color.Red;
                }
            };

            button_Record4.Click += (_, _) =>
            {
                if (_getCoordinatesItem)
                    button_Record1.PerformClick();
                if (_getCoordinatesBase)
                    button_Record2.PerformClick();
                if (_getCoordinatesSecond)
                    button_Record3.PerformClick();

                if (_getCoordinatesThird)
                {
                    _getCoordinatesThird = false;
                    button_Record4.ForeColor = Color.Black;
                    button_Record4.Text = "Rec";
                }
                else
                {
                    _getCoordinatesThird = true;
                    button_Record4.ForeColor = Color.Red;
                    button_Record4.Text = "...";
                }
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
            const int controlWidth = 85;
            const int headerTop = 22;
            const int keyTop = 55;
            const int typeTop = 98;
            const int sliderTop = 146;

            int width = tabPage_Main.ClientSize.Width;
            int height = tabPage_Main.ClientSize.Height;
            if (width <= 0 || height <= 0)
                return;

            int columnWidth = Math.Max(90, (width - margin * 2) / 5);
            int sliderHeight = Math.Max(120, height - sliderTop - margin);

            CheckBox[] checks = [checkBox_Flask1, checkBox_Flask2, checkBox_Flask3, checkBox_Flask4, checkBox_Flask5];
            FlatTextBox[] keys = [textBox_Flask1, textBox_Flask2, textBox_Flask3, textBox_Flask4, textBox_Flask5];
            FlatComboBox[] types = [comboBox_Flask1, comboBox_Flask2, comboBox_Flask3, comboBox_Flask4, comboBox_Flask5];
            GroupBox[] groups = [groupBox_Flask1, groupBox_Flask2, groupBox_Flask3, groupBox_Flask4, groupBox_Flask5];

            for (int i = 0; i < 5; i++)
            {
                int x = margin + i * columnWidth + Math.Max(0, (columnWidth - controlWidth) / 2);
                checks[i].Location = new Point(x + (controlWidth - checks[i].Width) / 2, headerTop);
                keys[i].Location = new Point(x, keyTop);
                types[i].Location = new Point(x, typeTop);
                groups[i].Location = new Point(x, sliderTop);
                groups[i].Size = new Size(controlWidth, sliderHeight);
            }
        }

        private void LayoutGambleTab()
        {
            const int margin = 7;
            const int rowTop = 26;
            const int labelTop = 2;
            const int recWidth = 48;
            const int typeWidth = 150;

            int width = tabPage_Gamble.ClientSize.Width;
            int height = tabPage_Gamble.ClientSize.Height;
            if (width <= 0 || height <= 0)
                return;

            comboBox_GambleType.Location = new Point(margin, rowTop);
            comboBox_GambleType.Size = new Size(typeWidth, 30);
            label_GambleType.Location = new Point(margin, labelTop);

            var slots = new List<(Label Label, FlatTextBox TextBox, Button Record, bool Visible)>
            {
                (label_ItemXY, textBox_ItemXY, button_Record1, true),
                (label_BaseXY, textBox_BaseXY, button_Record2, true),
                (label_SecondXY, textBox_SecondXY, button_Record3, GambleModeLayout.UsesSecond(_settings.Modifiers.GambleType)),
                (label_ThirdXY, textBox_ThirdXY, button_Record4, GambleModeLayout.UsesThird(_settings.Modifiers.GambleType)),
            };

            int visibleCount = slots.Count(s => s.Visible);
            int startX = margin + typeWidth + 12;
            int available = width - startX - margin;
            int slotWidth = visibleCount > 0 ? Math.Max(130, available / visibleCount) : available;
            int index = 0;

            foreach (var slot in slots)
            {
                slot.Label.Visible = slot.Visible;
                slot.TextBox.Visible = slot.Visible;
                slot.Record.Visible = slot.Visible;

                if (!slot.Visible)
                    continue;

                int x = startX + index * slotWidth;
                slot.Label.Location = new Point(x + 8, labelTop);
                slot.Record.Location = new Point(x, rowTop);
                slot.TextBox.Location = new Point(x + recWidth + 4, rowTop);
                slot.TextBox.Size = new Size(Math.Max(70, slotWidth - recWidth - 8), 30);
                index++;
            }

            int contentTop = 64;
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
