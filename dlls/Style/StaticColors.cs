using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Style
{
    internal static class StaticColors
    {
        public static readonly Color BackGround = Color.FromArgb(31, 31, 31);
        public static readonly Color ForeGround = ColorTranslator.FromHtml("#8CDCDA");

        /// <summary>Standard <see cref="Button"/> text on the system control background.</summary>
        public static readonly Color ButtonForeGround = Color.Black;

        public static readonly Color TabControlSelectedForeGround = Color.FromArgb(179, 182, 203);
        public static readonly Color TabControlForeGround = Color.FromArgb(124, 127, 151);
        public static readonly Color Underline = Color.FromArgb(139, 177, 234);
    }
}
