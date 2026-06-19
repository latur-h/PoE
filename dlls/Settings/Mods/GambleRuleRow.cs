using PoE.dlls.Gamble.Modifiers;

namespace PoE.dlls.Settings.Mods
{
    public class GambleRuleRow
    {
        public decimal Priority { get; set; }
        public ModifierType ModifierType { get; set; } = ModifierType.Any;
        public string ItemTypeFilter { get; set; } = string.Empty;
        public int Tier { get; set; } = 1;
        public string Content { get; set; } = string.Empty;
        public EldritchInfluence? EldritchInfluence { get; set; }
    }
}
