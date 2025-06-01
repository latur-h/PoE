using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Flasks.Base
{
    internal class Flask
    {
        public FlaskType Type { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public int X_Bottom { get; set; }
        public int Y_Bottom { get; set; }

        public Color Top { get; set; }
        public Color Bottom { get; set; }

        public string Key { get; set; }

        public DateTimeOffset LastUsed { get; set; }

        public Flask(FlaskType type, int x, int y, int x_bottom, int y_bottom, Color top, Color bottom, string key)
        {
            Type = type;

            X = x;
            Y = y;

            X_Bottom = x_bottom;
            Y_Bottom = y_bottom;

            Top = top;
            Bottom = bottom;

            Key = key;

            LastUsed = DateTimeOffset.MinValue;
        }

        public Flask(FlaskType type, int x, int y, Color pixel, string key)
        {
            Type = type;

            X = x;
            Y = y;

            X_Bottom = 0;
            Y_Bottom = 0;

            Top = pixel;
            Bottom = Color.Empty; // Assuming Bottom is not used in this constructor

            Key = key;
            LastUsed = DateTimeOffset.MinValue;
        }
    }
}
