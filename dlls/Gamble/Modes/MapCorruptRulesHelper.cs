using PoE.dlls.Gamble.Modifiers;

namespace PoE.dlls.Gamble.Modes
{
    internal static class MapCorruptRulesHelper
    {
        public const int PostCorruptMinimumAffixes = 8;

        public const string EightAffixStatRule = "affixes:8;";

        public static List<Rule> RulesForPostCorruptEvaluation(
            IReadOnlyList<Rule> rules,
            bool requireEightAffixes)
        {
            if (!requireEightAffixes)
                return rules.ToList();

            var augmented = rules.ToList();
            augmented.Add(new Rule(0, ModifierType.Prefix, 0, EightAffixStatRule));
            return augmented;
        }
    }
}
