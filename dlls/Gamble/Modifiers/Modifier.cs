using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Gamble.Modifiers
{
    public struct Modifier
    {
        public ModifierType Type { get; set; }
        public int Tier { get; set; }
        public string Name { get; set; }

        public string Content { get; set; }

        public Modifier(ModifierType type, int tier, string name, string content)
        {
            Type = type;
            Tier = tier;
            Name = name;
            Content = content;
        }
    }
}
