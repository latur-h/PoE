using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Style
{
    public class FlatComboBox : ComboBox
    {
        public FlatComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            FlatStyle = FlatStyle.Flat;

            SetStyle(ControlStyles.UserPaint, true);
        }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            Color bgColor = e.State.HasFlag(DrawItemState.Selected)
                ? Color.FromArgb(50, 100, 180)
                : Color.FromArgb(31, 31, 31);

            using var backgroundBrush = new SolidBrush(bgColor);
            e.Graphics.FillRectangle(backgroundBrush, e.Bounds);

            string text = GetItemText(Items[e.Index])!;
            using var textBrush = new SolidBrush(StaticColors.ForeGround);

            var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            e.Graphics.DrawString(text, Font, textBrush, e.Bounds, format);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            using var b = new SolidBrush(StaticColors.BackGround);
            e.Graphics.FillRectangle(b, this.ClientRectangle);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!DroppedDown)
                DroppedDown = true;
        }
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            SelectionLength = 0;
        }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0xF)
            {
                using Graphics g = Graphics.FromHwnd(Handle);
                
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;                
                
                string text = GetItemText(SelectedItem)!;
                using var brush = new SolidBrush(StaticColors.ForeGround);
                
                using var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };

                SizeF textSize = TextRenderer.MeasureText(text, Font);
                int arrowPadding = 6;
                int arrowWidth = 6;
                int totalWidth = (int)Math.Ceiling(textSize.Width) + arrowPadding + arrowWidth;

                int startX = (Width - totalWidth) / 2;
                int centerY = Height / 2;

                Rectangle textRect = new Rectangle(startX, 0, (int)textSize.Width, Height);
                g.DrawString(text, Font, brush, textRect, format);

                int arrowX = textRect.Right + arrowPadding;
                Point[] triangle =
                [
                    new Point(arrowX, centerY - 2),
                    new Point(arrowX + arrowWidth, centerY - 2),
                    new Point(arrowX + arrowWidth / 2, centerY + 2),
                ];

                using var arrowBrush = new SolidBrush(StaticColors.ForeGround);
                g.FillPolygon(arrowBrush, triangle);
            }
        }
        protected override void OnEnter(EventArgs e) => SelectionLength = 0;
    }
}
