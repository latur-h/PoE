using PoE.dlls.Style;

namespace PoE.dlls.Macros.UI
{
    internal sealed class MacroModeHelpDialog : Form
    {
        private static readonly Font TitleFont = new("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
        private static readonly Font HeadingFont = new("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
        private static readonly Font BodyFont = new("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);

        private MacroModeHelpDialog()
        {
            Text = "Macro modes";
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(560, 520);
            MinimumSize = new Size(420, 320);
            BackColor = StaticColors.BackGround;
            ForeColor = StaticColors.ForeGround;
            Font = BodyFont;

            var titleLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(12, 10, 12, 0),
                Text = "Macro modes",
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Font = TitleFont,
            };

            var contentPanel = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Location = new Point(0, 0),
                Width = 520,
                BackColor = StaticColors.BackGround,
            };

            var scrollHost = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = StaticColors.BackGround,
                Padding = new Padding(12, 0, 12, 12),
            };
            scrollHost.Controls.Add(contentPanel);

            var closeButton = new Button
            {
                Text = "Close",
                DialogResult = DialogResult.OK,
                Dock = DockStyle.Bottom,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = StaticColors.BackGround,
                ForeColor = StaticColors.ForeGround,
                Cursor = Cursors.Hand,
            };
            closeButton.FlatAppearance.BorderColor = StaticColors.ForeGround;
            AcceptButton = closeButton;

            Controls.Add(scrollHost);
            Controls.Add(closeButton);
            Controls.Add(titleLabel);

            int y = 0;
            const int sectionGap = 14;
            int panelWidth = 520;

            foreach ((string heading, string body) in Sections())
            {
                var headingLabel = new Label
                {
                    AutoSize = false,
                    Location = new Point(0, y),
                    Size = new Size(panelWidth, 22),
                    Text = heading,
                    Font = HeadingFont,
                    ForeColor = StaticColors.ForeGround,
                    BackColor = StaticColors.BackGround,
                };
                contentPanel.Controls.Add(headingLabel);
                y += 24;

                var bodyLabel = new Label
                {
                    AutoSize = false,
                    Location = new Point(0, y),
                    Size = new Size(panelWidth, 10),
                    Text = body,
                    Font = BodyFont,
                    ForeColor = StaticColors.ForeGround,
                    BackColor = StaticColors.BackGround,
                };
                int bodyHeight = MeasureMultilineHeight(bodyLabel.Text, bodyLabel.Font, panelWidth);
                bodyLabel.Height = bodyHeight;
                contentPanel.Controls.Add(bodyLabel);
                y += bodyHeight + sectionGap;
            }

            contentPanel.Width = panelWidth;
            contentPanel.Height = y;
        }

        public static void ShowHelp(IWin32Window owner)
        {
            using var dialog = new MacroModeHelpDialog();
            dialog.ShowDialog(owner);
        }

        private static IEnumerable<(string Heading, string Body)> Sections()
        {
            yield return ("Single", "Fires once when the trigger key is pressed. Use the On checkbox or toggle hotkey to arm or disarm.");
            yield return ("Loop", "While the trigger key is held, repeats the fire sequence every Cycle ms.");
            yield return ("Repeat", "When armed (On checkbox or toggle hotkey), fires continuously every Cycle ms. No trigger key.");
            yield return ("JE (jump if equal)", "While armed, checks a screen pixel every Cycle ms when the game window is focused. Fires when the pixel strictly matches #RRGGBB. Lock ms prevents multi-fire after a successful cycle. If the color stops matching, the current cycle finishes but no new cycle starts.");
            yield return ("JNE (jump if not equal)", "Same as JE, but fires while the pixel does not match the expected color.");
            yield return ("Fire sequence", "One InputSimulator stroke per line or separated by +, for example:\nLButton Down + LButton Up\nCtrl Down + A Down + A Up + Ctrl Up");
            yield return ("Pixel tools", "Rec arms the shared coordinate hotkey (same as Orbs) for X,Y only. Mouse / MPick arms it for X,Y plus color at that point. Pick samples color at the typed coordinates. Remember stores picked colors for quick reuse.");
            yield return ("Profiles", "Global is the first profile and is always active at runtime. Select Global to run only global macros. Select any other profile to run Global plus that profile. Use + / − to manage build profiles.");
            yield return ("Toggle hotkey", "Flips the On checkbox (active state) for any mode, including Repeat and JE/JNE.");
        }

        private static int MeasureMultilineHeight(string text, Font font, int width)
        {
            var size = TextRenderer.MeasureText(
                text,
                font,
                new Size(width, int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);

            return Math.Max(20, size.Height + 4);
        }
    }
}
