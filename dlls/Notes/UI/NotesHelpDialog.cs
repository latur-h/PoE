using PoE.dlls.Style;

namespace PoE.dlls.Notes.UI
{
    internal sealed class NotesHelpDialog : Form
    {
        private static readonly Font TitleFont = new("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
        private static readonly Font HeadingFont = new("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
        private static readonly Font BodyFont = new("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);

        private NotesHelpDialog()
        {
            Text = "Notes";
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
                Text = "Notes — markdown help",
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

            foreach ((string heading, string body) in NotesHelp.Sections())
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
            using var dialog = new NotesHelpDialog();
            dialog.ShowDialog(owner);
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
