using PoE.dlls.Gamble;
using PoE.dlls.Gamble.Modes;
using PoE.dlls.Gamble.Modifiers;
using Xunit;

namespace PoE.Tests;

public class MapRulesEvaluatorTests
{
    private const string SixModMapWithAugmentedStats = """
        Item Class: Maps
        Rarity: Rare
        Savage Journey
        Map (Tier 16)
        --------
        Item Quantity: +72% (augmented)
        Item Rarity: +43% (augmented)
        Monster Pack Size: +28% (augmented)
        --------
        Item Level: 85
        --------
        Monster Level: 83
        --------
        Area contains an Empowered Mirage which covers the entire Map (enchant)
        --------
        { Prefix Modifier "Shocking" (Tier: 1) — Damage, Physical, Elemental, Lightning }
        Monsters deal 97(90-110)% extra Physical Damage as Lightning
        { Prefix Modifier "Conflagrating" (Tier: 1) — Elemental, Fire, Ailment }
        All Monster Damage from Hits always Ignites — Unscalable Value
        { Prefix Modifier "Ceremonial" (Tier: 1) }
        Area contains many Totems — Unscalable Value
        { Suffix Modifier "of Enfeeblement" (Tier: 1) — Caster, Curse }
        Players are Cursed with Enfeeble
        { Suffix Modifier "of Endurance" (Tier: 1) }
        Monsters gain an Endurance Charge on Hit
        { Suffix Modifier "of Bloodlines" (Tier: 1) }
        27(20-30)% increased Magic Monsters
        --------
        Travel to a Map of this tier or lower by using this in a personal Map Device. Maps can only be used once.
        """;

    [Fact]
    public void Evaluate_counts_six_affixes_with_augmented_stat_lines_present()
    {
        var result = MapRulesEvaluator.Evaluate(SixModMapWithAugmentedStats, [], logMods: false);

        Assert.True(result.IsMap);
        Assert.True(result.IsRare);
        Assert.Equal(6, result.ModCount);
        Assert.Equal(6, result.AffixModCount);
    }

    [Fact]
    public void Evaluate_keeps_affix_count_when_stat_rules_fail()
    {
        var rules = new List<Rule>
        {
            new(0, ModifierType.Any, 0, "q99r99ps99"),
        };

        var result = MapRulesEvaluator.Evaluate(SixModMapWithAugmentedStats, rules, logMods: false);

        Assert.Equal(MapRuleFailure.Stats, result.Failure);
        Assert.False(result.RulesPassed);
        Assert.Equal(6, result.ModCount);
        Assert.Equal(6, result.AffixModCount);
    }

    [Fact]
    public void CountAffixMods_ignores_implicit_blocks()
    {
        var modifiers = new List<Modifier>
        {
            new(ModifierType.Prefix, 1, "A", "line"),
            new(ModifierType.Suffix, 1, "B", "line"),
            new(ModifierType.Implicit, 0, "Quantity", "line"),
        };

        Assert.Equal(2, MapRulesEvaluator.CountAffixMods(modifiers));
    }
}
