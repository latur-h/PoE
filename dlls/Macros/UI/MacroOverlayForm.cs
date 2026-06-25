using PoE.dlls.Settings.Macros;
using PoE.dlls.Style;

namespace PoE.dlls.Macros.UI
{
    internal sealed class MacroOverlayForm : Form
    {
        private const int ScreenMargin = 8;
        private const int StatusRowHeight = 22;
        private const int SectionRowHeight = 20;
        private const int RowGap = 2;
        private const int SectionGap = 4;
        private const int HorizontalPadding = 8;
        private const int MinWidth = 140;
        private const int MaxWidth = 420;

        private const int WmMouseActivate = 0x0021;
        private const int MaNoActivate = 3;

        private static readonly Color OnColor = Color.FromArgb(210, 46, 125, 50);
        private static readonly Color OffColor = Color.FromArgb(210, 198, 40, 40);
        private static readonly Color WarningColor = Color.FromArgb(210, 218, 145, 0);
        private static readonly Color SectionColor = Color.FromArgb(210, 50, 50, 50);
        private static readonly Font StatusFont = new("Segoe UI", 9F, FontStyle.Bold);
        private static readonly Font SectionFont = new("Segoe UI", 8.25F, FontStyle.Bold);

        private IReadOnlyList<OverlayRow> _rows = [];
        private MacroOverlayCorner _corner = MacroOverlayCorner.TopLeft;

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
            DoubleBuffered = true;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            UpdateStyles();
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                const int wsExNoActivate = 0x08000000;
                const int wsExTransparent = 0x00000020;
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= wsExNoActivate | wsExTransparent;
                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmMouseActivate)
            {
                m.Result = (IntPtr)MaNoActivate;
                return;
            }

            base.WndProc(ref m);
        }

        public void Apply(IReadOnlyList<OverlayRow> rows, MacroOverlayCorner corner)
        {
            if (IsDisposed)
                return;

            _rows = rows;
            _corner = corner;
            RecalculateLayout();

            if (!Visible)
                Show();

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(Color.Black);

            int y = 0;
            foreach (OverlayRow row in _rows)
            {
                int rowHeight = row.Kind == OverlayRowKind.Section ? SectionRowHeight : StatusRowHeight;
                if (row.Kind == OverlayRowKind.Section && y > 0)
                    y += SectionGap;

                var bounds = new Rectangle(0, y, Width, rowHeight);
                Color backColor = row.Kind switch
                {
                    OverlayRowKind.Section => SectionColor,
                    OverlayRowKind.Status when row.State == OverlayRowState.On => OnColor,
                    OverlayRowKind.Status when row.State == OverlayRowState.Warning => WarningColor,
                    _ => OffColor,
                };

                Color foreColor = row.Kind == OverlayRowKind.Section
                    ? StaticColors.ForeGround
                    : Color.White;

                Font font = row.Kind == OverlayRowKind.Section ? SectionFont : StatusFont;

                using var backBrush = new SolidBrush(backColor);
                e.Graphics.FillRectangle(backBrush, bounds);

                TextRenderer.DrawText(
                    e.Graphics,
                    row.Label,
                    font,
                    new Rectangle(bounds.Left + HorizontalPadding, bounds.Top, bounds.Width - HorizontalPadding * 2, bounds.Height),
                    foreColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

                y += rowHeight + RowGap;
            }
        }

        private void RecalculateLayout()
        {
            if (_rows.Count == 0)
            {
                Size = new Size(MinWidth, StatusRowHeight);
                PositionAtCorner(_corner);
                return;
            }

            int maxTextWidth = 0;
            using var measure = CreateGraphics();
            foreach (OverlayRow row in _rows)
            {
                Font font = row.Kind == OverlayRowKind.Section ? SectionFont : StatusFont;
                Size textSize = TextRenderer.MeasureText(measure, row.Label, font, Size.Empty, TextFormatFlags.NoPadding);
                maxTextWidth = Math.Max(maxTextWidth, textSize.Width);
            }

            int width = Math.Clamp(maxTextWidth + HorizontalPadding * 2, MinWidth, MaxWidth);
            int height = MeasureContentHeight(width);
            Size = new Size(width, height);
            PositionAtCorner(_corner);
        }

        private int MeasureContentHeight(int width)
        {
            int y = 0;
            using var measure = CreateGraphics();

            foreach (OverlayRow row in _rows)
            {
                int rowHeight = row.Kind == OverlayRowKind.Section ? SectionRowHeight : StatusRowHeight;
                if (row.Kind == OverlayRowKind.Section && y > 0)
                    y += SectionGap;

                y += rowHeight + RowGap;
            }

            return Math.Max(StatusRowHeight, y - RowGap);
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
