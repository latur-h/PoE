using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Settings.Mods
{
    public class UIAlt_Aug : IUIMods
    {
        public Coordinates Item { get; set; } = new Coordinates(0, 0);
        public Coordinates Base { get; set; } = new Coordinates(0, 0);
        public Coordinates Second { get; set; } = new Coordinates(0, 0);

        public decimal Priority1 { get; set; } = 0;
        public decimal Priority2 { get; set; } = 0;
        public decimal Priority3 { get; set; } = 0;
        public decimal Priority4 { get; set; } = 0;
        public decimal Priority5 { get; set; } = 0;
        public decimal Priority6 { get; set; } = 0;
        public decimal Priority7 { get; set; } = 0;
        public decimal Priority8 { get; set; } = 0;

        public ModifierType modifierType1 { get; set; } = ModifierType.Any;
        public ModifierType modifierType2 { get; set; } = ModifierType.Any;
        public ModifierType modifierType3 { get; set; } = ModifierType.Any;
        public ModifierType modifierType4 { get; set; } = ModifierType.Any;
        public ModifierType modifierType5 { get; set; } = ModifierType.Any;
        public ModifierType modifierType6 { get; set; } = ModifierType.Any;
        public ModifierType modifierType7 { get; set; } = ModifierType.Any;
        public ModifierType modifierType8 { get; set; } = ModifierType.Any;

        public int Tier1 { get; set; } = 1;
        public int Tier2 { get; set; } = 1;
        public int Tier3 { get; set; } = 1;
        public int Tier4 { get; set; } = 1;
        public int Tier5 { get; set; } = 1;
        public int Tier6 { get; set; } = 1;
        public int Tier7 { get; set; } = 1;
        public int Tier8 { get; set; } = 1;

        public string Content1 { get; set; } = string.Empty;
        public string Content2 { get; set; } = string.Empty;
        public string Content3 { get; set; } = string.Empty;
        public string Content4 { get; set; } = string.Empty;
        public string Content5 { get; set; } = string.Empty;
        public string Content6 { get; set; } = string.Empty;
        public string Content7 { get; set; } = string.Empty;
        public string Content8 { get; set; } = string.Empty;
    }
}
