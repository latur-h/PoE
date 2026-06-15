using PoE.dlls.Gamble;
using PoE.dlls.Style;

namespace PoE.dlls.Gamble.UI
{
    internal sealed class GambleModeHelpDialog : Form
    {
        private static readonly Font TitleFont = new("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
        private static readonly Font HeadingFont = new("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
        private static readonly Font BodyFont = new("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);

        private readonly Label _titleLabel;
        private readonly Panel _scrollHost;
        private readonly Panel _contentPanel;

        public GambleModeHelpDialog()
        {
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(520, 480);
            MinimumSize = new Size(420, 320);
            BackColor = StaticColors.BackGround;
            ForeColor = StaticColors.ForeGround;
            Font = BodyFont;

            _titleLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(12, 10, 12, 0),
                ForeColor = StaticColors.ForeGround,
                BackColor = StaticColors.BackGround,
                Font = TitleFont,
            };

            _contentPanel = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Location = new Point(0, 0),
                Width = 480,
                BackColor = StaticColors.BackGround,
            };

            _scrollHost = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = StaticColors.BackGround,
                Padding = new Padding(12, 0, 12, 12),
            };
            _scrollHost.Controls.Add(_contentPanel);

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
            Controls.Add(_scrollHost);
            Controls.Add(closeButton);
            Controls.Add(_titleLabel);
        }

        public void SetContent(GambleModeHelpContent content)
        {
            Text = content.Title;
            _titleLabel.Text = content.Title;
            _contentPanel.Controls.Clear();
            _contentPanel.SuspendLayout();

            int y = 0;
            const int sectionGap = 14;
            int panelWidth = Math.Max(360, _scrollHost.ClientSize.Width - 36);

            foreach (GambleModeHelpSection section in content.Sections)
            {
                var heading = new Label
                {
                    AutoSize = false,
                    Location = new Point(0, y),
                    Size = new Size(panelWidth, 22),
                    Text = section.Heading,
                    Font = HeadingFont,
                    ForeColor = StaticColors.ForeGround,
                    BackColor = StaticColors.BackGround,
                };
                _contentPanel.Controls.Add(heading);
                y += 24;

                var body = new Label
                {
                    AutoSize = false,
                    Location = new Point(0, y),
                    Size = new Size(panelWidth, 10),
                    Text = section.Body,
                    Font = BodyFont,
                    ForeColor = StaticColors.ForeGround,
                    BackColor = StaticColors.BackGround,
                };
                int bodyHeight = MeasureMultilineHeight(body.Text, body.Font, panelWidth);
                body.Height = bodyHeight;
                _contentPanel.Controls.Add(body);
                y += bodyHeight + sectionGap;
            }

            _contentPanel.Width = panelWidth;
            _contentPanel.Height = y;
            _contentPanel.ResumeLayout(true);
        }

        public static void ShowForMode(IWin32Window owner, GambleType type)
        {
            using var dialog = new GambleModeHelpDialog();
            dialog.SetContent(GambleModeHelp.For(type));
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
