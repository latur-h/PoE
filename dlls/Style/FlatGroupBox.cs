using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Style
{
    public class FlatGroupBox : GroupBox
    {
        [DefaultValue(false)]
        public bool CenterTitle { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            Size textSize = TextRenderer.MeasureText(Text, Font);
            int textX = CenterTitle
                ? Math.Max(0, (ClientSize.Width - textSize.Width) / 2)
                : 6;
            Rectangle textRect = new Rectangle(textX, 0, textSize.Width, textSize.Height);

            using var bgBrush = new SolidBrush(BackColor);
            e.Graphics.FillRectangle(bgBrush, ClientRectangle);

            TextRenderer.DrawText(e.Graphics, Text, Font, textRect, ForeColor);
        }
    }
}
