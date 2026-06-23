using System.Drawing.Drawing2D;
using PoE.dlls.Gamble.Bulk;

namespace PoE.dlls.Gamble.UI
{
    internal sealed class GambleGridOverlayForm : Form
    {
        private const int WmMouseActivate = 0x0021;
        private const int MaNoActivate = 3;
        private const int BorderWidth = 2;
        private const int BoundsPadding = 4;

        private static readonly Color RedFill = Color.FromArgb(90, 198, 40, 40);
        private static readonly Color RedBorder = Color.FromArgb(220, 220, 70, 70);
        private static readonly Color GreenFill = Color.FromArgb(90, 46, 125, 50);
        private static readonly Color GreenBorder = Color.FromArgb(220, 70, 180, 90);
        private static readonly Color OrangeFill = Color.FromArgb(90, 220, 130, 40);
        private static readonly Color OrangeBorder = Color.FromArgb(220, 255, 170, 70);

        private IReadOnlyList<BulkMapHighlightEntry> _highlights = [];
        private Rectangle _screenBounds = Rectangle.Empty;

        public GambleGridOverlayForm(Form owner)
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

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
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

        public void Apply(IReadOnlyList<BulkMapHighlightEntry> highlights)
        {
            if (IsDisposed)
                return;

            _highlights = highlights;
            Rectangle bounds = ComputeScreenBounds(highlights);
            if (bounds.IsEmpty)
            {
                _screenBounds = Rectangle.Empty;
                if (Visible)
                    Hide();

                return;
            }

            bool boundsChanged = bounds != _screenBounds;
            _screenBounds = bounds;

            if (boundsChanged)
            {
                Location = bounds.Location;
                Size = bounds.Size;
            }

            if (!Visible)
                Show();

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Black);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Point origin = Location;
            foreach (BulkMapHighlightEntry highlight in _highlights)
            {
                Rectangle cell = highlight.Bounds;
                cell.Offset(-origin.X, -origin.Y);

                if (cell.Width <= 0 || cell.Height <= 0)
                    continue;

                GetColors(highlight.Color, out Color fill, out Color border);

                int diameter = Math.Min(cell.Width, cell.Height) - BorderWidth * 2;
                if (diameter <= 0)
                    continue;

                var circle = new Rectangle(
                    cell.X + (cell.Width - diameter) / 2,
                    cell.Y + (cell.Height - diameter) / 2,
                    diameter,
                    diameter);

                using var fillBrush = new SolidBrush(fill);
                using var borderPen = new Pen(border, BorderWidth);
                e.Graphics.FillEllipse(fillBrush, circle);
                e.Graphics.DrawEllipse(borderPen, circle);
            }
        }

        private static Rectangle ComputeScreenBounds(IReadOnlyList<BulkMapHighlightEntry> highlights)
        {
            if (highlights.Count == 0)
                return Rectangle.Empty;

            int left = int.MaxValue;
            int top = int.MaxValue;
            int right = int.MinValue;
            int bottom = int.MinValue;

            foreach (BulkMapHighlightEntry highlight in highlights)
            {
                Rectangle bounds = highlight.Bounds;
                left = Math.Min(left, bounds.Left);
                top = Math.Min(top, bounds.Top);
                right = Math.Max(right, bounds.Right);
                bottom = Math.Max(bottom, bounds.Bottom);
            }

            return Rectangle.FromLTRB(
                left - BoundsPadding,
                top - BoundsPadding,
                right + BoundsPadding,
                bottom + BoundsPadding);
        }

        private static void GetColors(BulkMapHighlightColor color, out Color fill, out Color border) =>
            (fill, border) = color switch
            {
                BulkMapHighlightColor.Red => (RedFill, RedBorder),
                BulkMapHighlightColor.Orange => (OrangeFill, OrangeBorder),
                _ => (GreenFill, GreenBorder),
            };
    }
}
