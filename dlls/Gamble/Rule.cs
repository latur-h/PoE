using PoE.dlls.Gamble.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Gamble
{
    public struct Rule(decimal priority, ModifierType type, int tier, string Content)
    {
        public decimal Priority { get; set; } = priority;
        public ModifierType Type { get; set; } = type;
        public int Tier { get; set; } = tier;
        public string Content { get; set; } = Content;
    }
}
