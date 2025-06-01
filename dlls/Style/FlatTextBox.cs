using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace PoE.dlls.Style
{
    public class FlatTextBox : UserControl
    {
        public readonly TextBox _textBox;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BorderColor { get; set; } = Color.Gray;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color FocusColor { get; set; } = StaticColors.ForeGround;

        public FlatTextBox()
        {
            AutoSize = false;
            BackColor = Color.Transparent;

            _textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = StaticColors.BackGround,
                ForeColor = StaticColors.ForeGround,
                Location = new Point(0, 0),
                Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0)
            };

            Controls.Add(_textBox);

            _textBox.GotFocus += (_, _) => Invalidate();
            _textBox.LostFocus += (_, _) => Invalidate();

            _textBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ReadOnly
        {
            get => _textBox.ReadOnly;
            set => _textBox.ReadOnly = value;
        }
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public HorizontalAlignment TextAlign
        {
            get => _textBox.TextAlign;
            set => _textBox.TextAlign = value;
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            _textBox.Width = Width;
            _textBox.Height = Height - 2;
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string TextValue
        {
            get => _textBox.Text;
            set => _textBox.Text = value;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return new Size(_textBox.Width, _textBox.Height + 2);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using var pen = new Pen(_textBox.Focused ? FocusColor : BorderColor, 1);
            int y = _textBox.Bottom + 1;

            e.Graphics.DrawLine(pen, 0, y, _textBox.Width, y);
        }
    }
}
