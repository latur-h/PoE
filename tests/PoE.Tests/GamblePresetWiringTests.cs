using PoE.dlls.Gamble;
using PoE.dlls.Gamble.Modifiers;
using Xunit;

namespace PoE.Tests
{
    public class GamblePresetWiringTests
    {
        private const string FireSceptreSnippet = """
            { Prefix Modifier "Flame Shaper's" (Tier: 1) — Elemental, Fire, Caster, Gem }
            +1 to Level of all Fire Spell Skill Gems
            { Suffix Modifier "of the Apt" (Tier: 1) }
            32% reduced Attribute Requirements
            """;

        [Fact]
        public void Priority_zero_content_rules_do_not_count_as_success_in_alt_aug()
        {
            var ignoredRule = new Rule(
                0,
                ModifierType.Any,
                1,
                "# to Level of all Spell Skill Gems");

            AltAugResponse response = GambleRuleEvaluator.EvaluateAltAug(
                FireSceptreSnippet,
                [ignoredRule],
                logParse: false);

            Assert.NotEqual(AltAugResponse.Success, response);
        }

        [Fact]
        public void Priority_zero_content_rules_do_not_count_as_match_in_matches_rules()
        {
            var ignoredRule = new Rule(
                0,
                ModifierType.Any,
                1,
                "# to Level of all Spell Skill Gems");

            Assert.False(GambleRuleEvaluator.MatchesRules(
                FireSceptreSnippet,
                [ignoredRule],
                logParse: false));
        }

        [Fact]
        public void Priority_one_spell_rule_still_does_not_match_fire_mod_line()
        {
            var rule = new Rule(
                1,
                ModifierType.Any,
                99,
                "# to Level of all Spell Skill Gems");

            AltAugResponse response = GambleRuleEvaluator.EvaluateAltAug(
                FireSceptreSnippet,
                [rule],
                logParse: false);

            Assert.Equal(AltAugResponse.Alt, response);
        }
    }
}
