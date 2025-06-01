using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Style
{
    public class Slider : Control
    {
        private int _minimum = 0;
        private int _maximum = 100;
        private int _value = 50;

        [Category("Behavior")]
        [Description("Minimum slider value.")]
        [DefaultValue(0)]
        public int Minimum
        {
            get => _minimum;
            set { _minimum = value; Invalidate(); }
        }

        [Category("Behavior")]
        [Description("Maximum slider value.")]
        [DefaultValue(100)]
        public int Maximum
        {
            get => _maximum;
            set { _maximum = value; Invalidate(); }
        }

        [Category("Behavior")]
        [Description("Current slider value.")]
        [DefaultValue(50)]
        public int Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = Math.Clamp(value, Minimum, Maximum);
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        public event EventHandler? ValueChanged;

        private bool _dragging = false;
        private Rectangle _thumbRect;

        public Slider()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.UserPaint, true);

            Width = 14;
            Height = 200;
            TabStop = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);

            int trackWidth = 4;
            int trackX = (Width - trackWidth) / 2;
            Rectangle trackRect = new Rectangle(trackX, 0, trackWidth, Height);
            using var trackBrush = new SolidBrush(Color.Gray);
            e.Graphics.FillRectangle(trackBrush, trackRect);

            float percent = (float)(Value - Minimum) / (Maximum - Minimum);
            int filledHeight = (int)(percent * Height);
            int filledY = Height - filledHeight;
            Rectangle fillRect = new Rectangle(trackX, filledY, trackWidth, filledHeight);

            using var fillBrush = new SolidBrush(StaticColors.ForeGround);
            e.Graphics.FillRectangle(fillBrush, fillRect);

            int thumbHeight = 10;
            int thumbY = filledY - thumbHeight / 2;
            thumbY = Math.Clamp(thumbY, 0, Height - thumbHeight);
            _thumbRect = new Rectangle(2, thumbY, Width - 4, thumbHeight);

            using var thumbBrush = new SolidBrush(StaticColors.ForeGround);
            e.Graphics.FillRectangle(thumbBrush, _thumbRect);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Focus();

            _dragging = _thumbRect.Contains(e.Location);
            SetValueFromMouse(e.Y);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_dragging)
                SetValueFromMouse(e.Y);
        }
        protected override bool IsInputKey(Keys keyData) => keyData is Keys.Left or Keys.Right or Keys.Up or Keys.Down or Keys.Tab;
        protected override void OnMouseUp(MouseEventArgs e) => _dragging = false;
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.Right:
                    Value = Math.Min(Maximum, Value + 1);
                    e.Handled = true;
                    break;
                case Keys.Down:
                case Keys.Left:
                    Value = Math.Max(Minimum, Value - 1);
                    e.Handled = true;
                    break;
                case Keys.Tab:
                    e.Handled = true;
                    break;
            }

            base.OnKeyDown(e);
        }
        private void SetValueFromMouse(int y)
        {
            float percent = 1f - Math.Clamp((float)y / Height, 0, 1);

            Value = Minimum + (int)(percent * (Maximum - Minimum));
        }
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode is Keys.Up or Keys.Down or Keys.Left or Keys.Right)
                e.IsInputKey = true;

            base.OnPreviewKeyDown(e);
        }
    }
}
