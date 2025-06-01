using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Style
{
    public class FlatGroupBox : GroupBox
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            Size textSize = TextRenderer.MeasureText(Text, Font);
            Rectangle textRect = new Rectangle(6, 0, textSize.Width, textSize.Height);

            using var bgBrush = new SolidBrush(BackColor);
            e.Graphics.FillRectangle(bgBrush, ClientRectangle);

            TextRenderer.DrawText(e.Graphics, Text, Font, textRect, ForeColor);
        }
    }
}
