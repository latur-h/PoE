using PoE.dlls.Macros;
using PoE.dlls.Settings.Macros;

namespace PoE.dlls.Macros.UI
{
    internal sealed class MacroOverlayForm : Form
    {
        private const int ScreenMargin = 8;
        private const int RowHeight = 22;
        private const int RowGap = 2;
        private const int HorizontalPadding = 8;
        private const int MinWidth = 140;
        private const int MaxWidth = 420;

        private static readonly Color OnColor = Color.FromArgb(210, 46, 125, 50);
        private static readonly Color OffColor = Color.FromArgb(210, 198, 40, 40);
        private static readonly Font RowFont = new("Segoe UI", 9F, FontStyle.Bold);

        private readonly Panel _rowsHost;
        private readonly List<Panel> _rowPanels = [];

        public MacroOverlayForm(Form owner)
        {
            Owner = owner;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            BackColor = Color.Black;
            TransparencyKey = Color.Black;
            AutoSize = false;

            _rowsHost = new Panel
            {
                AutoSize = false,
                BackColor = Color.Black,
                Location = Point.Empty,
            };
            Controls.Add(_rowsHost);
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                const int wsExNoActivate = 0x08000000;
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= wsExNoActivate;
                return cp;
            }
        }

        public void Apply(MacroSettings settings, MacroEngine engine)
        {
            if (IsDisposed)
                return;

            if (!settings.OverlayEnabled)
            {
                if (Visible)
                    Hide();

                return;
            }

            IReadOnlyList<MacroOverlayDisplayHelper.MacroOverlayRow> rows =
                MacroOverlayDisplayHelper.BuildRows(settings, engine);

            RebuildRows(rows);
            PositionAtCorner(settings.OverlayCorner);

            if (!Visible)
                Show();
        }

        private void RebuildRows(IReadOnlyList<MacroOverlayDisplayHelper.MacroOverlayRow> rows)
        {
            foreach (Panel row in _rowPanels)
                row.Dispose();

            _rowPanels.Clear();
            _rowsHost.Controls.Clear();

            if (rows.Count == 0)
            {
                Size = new Size(MinWidth, RowHeight);
                _rowsHost.Size = Size;
                return;
            }

            int maxTextWidth = 0;
            using var measure = CreateGraphics();
            foreach (var row in rows)
            {
                Size textSize = TextRenderer.MeasureText(measure, row.Label, RowFont, Size.Empty, TextFormatFlags.NoPadding);
                maxTextWidth = Math.Max(maxTextWidth, textSize.Width);
            }

            int width = Math.Clamp(maxTextWidth + HorizontalPadding * 2, MinWidth, MaxWidth);
            int y = 0;

            foreach (var row in rows)
            {
                var rowPanel = new Panel
                {
                    Location = new Point(0, y),
                    Size = new Size(width, RowHeight),
                    BackColor = row.IsOn ? OnColor : OffColor,
                };

                var label = new Label
                {
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    Text = row.Label,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = RowFont,
                    Padding = new Padding(HorizontalPadding, 0, HorizontalPadding, 0),
                };

                rowPanel.Controls.Add(label);
                _rowsHost.Controls.Add(rowPanel);
                _rowPanels.Add(rowPanel);
                y += RowHeight + RowGap;
            }

            int height = Math.Max(RowHeight, y - RowGap);
            _rowsHost.Size = new Size(width, height);
            Size = _rowsHost.Size;
        }

        private void PositionAtCorner(MacroOverlayCorner corner)
        {
            Rectangle area = Screen.PrimaryScreen?.WorkingArea ?? SystemInformation.WorkingArea;
            Point location = corner switch
            {
                MacroOverlayCorner.TopRight => new Point(area.Right - Width - ScreenMargin, area.Top + ScreenMargin),
                MacroOverlayCorner.BottomLeft => new Point(area.Left + ScreenMargin, area.Bottom - Height - ScreenMargin),
                MacroOverlayCorner.BottomRight => new Point(area.Right - Width - ScreenMargin, area.Bottom - Height - ScreenMargin),
                _ => new Point(area.Left + ScreenMargin, area.Top + ScreenMargin),
            };

            Location = location;
        }
    }
}
