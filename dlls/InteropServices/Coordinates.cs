using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.InteropServices
{
    public struct Coordinates(int x, int y)
    {
        public int X { get; set; } = x;
        public int Y { get; set; } = y;
    }
}
