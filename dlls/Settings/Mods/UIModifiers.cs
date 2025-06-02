using PoE.dlls.Gamble;
using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Settings.Mods
{
    public class UIModifiers
    {
        public GambleType GambleType { get; set; } = GambleType.Alt;

        public string GetCoorinatesKey = "F6";
        public string GamblerStart = "F7";
        public string GamblerStop = "F8";
        public int Delay { get; set; } = 40;
        public double Speed { get; set; } = 10.0;

        public IUIMods Mode { get; set; } = new UIAlt
        {
            Item = new Coordinates(0, 0),
            Base = new Coordinates(0, 0),
            Second = new Coordinates(0, 0),

            Priority1 = 0,
            Priority2 = 0,
            Priority3 = 0,
            Priority4 = 0,
            Priority5 = 0,
            Priority6 = 0,
            Priority7 = 0,
            Priority8 = 0,

            modifierType1 = ModifierType.Any,
            modifierType2 = ModifierType.Any,
            modifierType3 = ModifierType.Any,
            modifierType4 = ModifierType.Any,
            modifierType5 = ModifierType.Any,
            modifierType6 = ModifierType.Any,
            modifierType7 = ModifierType.Any,
            modifierType8 = ModifierType.Any,

            Tier1 = 1,
            Tier2 = 1,
            Tier3 = 1,
            Tier4 = 1,
            Tier5 = 1,
            Tier6 = 1,
            Tier7 = 1,
            Tier8 = 1
        };

        public UIAlt _uialt { get; set; } = new();
        public UIAlt_Aug _uialt_aug { get; set; } = new();
    }
}