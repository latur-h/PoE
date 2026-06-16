using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.Settings.Mods;

namespace PoE.dlls.Gamble
{
    public struct Rule(decimal priority, ModifierType type, int tier, string Content, EldritchInfluence? eldritchInfluence = null)
    {
        public decimal Priority { get; set; } = priority;
        public ModifierType Type { get; set; } = type;
        public int Tier { get; set; } = tier;
        public string Content { get; set; } = Content;
        public EldritchInfluence? EldritchInfluence { get; set; } = eldritchInfluence;
    }
}
