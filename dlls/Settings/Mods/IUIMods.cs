using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Settings.Mods
{
    public interface IUIMods
    {
        public Coordinates Item { get; set; }
        public Coordinates Base { get; set; }
        public Coordinates Second { get; set; }

        public decimal Priority1 { get; set; }
        public decimal Priority2 { get; set; }
        public decimal Priority3 { get; set; }
        public decimal Priority4 { get; set; }
        public decimal Priority5 { get; set; }
        public decimal Priority6 { get; set; }
        public decimal Priority7 { get; set; }
        public decimal Priority8 { get; set; }

        public ModifierType modifierType1 { get; set; }
        public ModifierType modifierType2 { get; set; }
        public ModifierType modifierType3 { get; set; }
        public ModifierType modifierType4 { get; set; }
        public ModifierType modifierType5 { get; set; }
        public ModifierType modifierType6 { get; set; }
        public ModifierType modifierType7 { get; set; }
        public ModifierType modifierType8 { get; set; }

        public int Tier1 { get; set; }
        public int Tier2 { get; set; }
        public int Tier3 { get; set; }
        public int Tier4 { get; set; }
        public int Tier5 { get; set; }
        public int Tier6 { get; set; }
        public int Tier7 { get; set; }
        public int Tier8 { get; set; }

        public string Content1 { get; set; }
        public string Content2 { get; set; }
        public string Content3 { get; set; }
        public string Content4 { get; set; }
        public string Content5 { get; set; }
        public string Content6 { get; set; }
        public string Content7 { get; set; }
        public string Content8 { get; set; }
    }
}
