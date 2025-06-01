using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Style
{
    public class FlatTabControl : TabControl
    {
        public FlatTabControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

            DrawMode = TabDrawMode.OwnerDrawFixed;
            ItemSize = new Size(100, 32);
            SizeMode = TabSizeMode.Fixed;
            Appearance = TabAppearance.Normal;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            for (int i = 0; i < this.TabCount; i++)
            {
                DrawTabHeader(e.Graphics, i);
            }
        }
        private void DrawTabHeader(Graphics g, int index)
        {
            var tab = this.TabPages[index];
            var isSelected = (index == this.SelectedIndex);
            var bounds = this.GetTabRect(index);

            g.FillRectangle(new SolidBrush(StaticColors.BackGround), bounds);

            var font = new Font("Roboto", 16, FontStyle.Regular, GraphicsUnit.Pixel);
            var text = tab.Text;
            var color = isSelected ? StaticColors.TabControlSelectedForeGround : StaticColors.TabControlForeGround;

            TextRenderer.DrawText(g, text, font, bounds, color, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            if (isSelected)
            {
                using var underlinePen = new Pen(StaticColors.Underline, 1);
                for (int i = 0; i < 3; i++)
                    g.DrawLine(underlinePen, bounds.Left, bounds.Bottom - 1 - i, bounds.Right, bounds.Bottom - 1 - i);
            }
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(StaticColors.BackGround);
        }
        protected override bool ShowFocusCues => false;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }
    }

}
