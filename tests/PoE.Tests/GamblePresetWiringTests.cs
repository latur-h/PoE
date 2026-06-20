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

        [Fact]
        public void Reject_rule_fails_matches_rules_before_required_check()
        {
            var reject = new Rule(-1, ModifierType.Any, 99, "Attribute Requirements");
            var required = new Rule(1, ModifierType.Any, 99, "Fire Spell Skill Gems");

            Assert.False(GambleRuleEvaluator.MatchesRules(
                FireSceptreSnippet,
                [required, reject],
                logParse: false));
        }

        [Fact]
        public void Reject_rule_passes_when_mod_not_present()
        {
            var reject = new Rule(-1, ModifierType.Any, 99, "maximum Life");
            var required = new Rule(1, ModifierType.Any, 99, "Fire Spell Skill Gems");

            Assert.True(GambleRuleEvaluator.MatchesRules(
                FireSceptreSnippet,
                [required, reject],
                logParse: false));
        }

        [Fact]
        public void Reject_only_setup_passes_when_no_excluded_mod()
        {
            var reject = new Rule(-1, ModifierType.Any, 99, "maximum Life");

            Assert.True(GambleRuleEvaluator.MatchesRules(
                FireSceptreSnippet,
                [reject],
                logParse: false));
        }

        [Fact]
        public void Reject_rule_matches_mod_name()
        {
            var reject = new Rule(-1, ModifierType.Any, 99, "Apt");

            Assert.False(GambleRuleEvaluator.MatchesRules(
                FireSceptreSnippet,
                [reject],
                logParse: false));
        }

        [Fact]
        public void Required_rule_matches_mod_name()
        {
            var required = new Rule(1, ModifierType.Any, 99, "Flame Shaper");

            Assert.True(GambleRuleEvaluator.MatchesRules(
                FireSceptreSnippet,
                [required],
                logParse: false));
        }

        [Fact]
        public void Reject_rule_returns_alt_in_alt_aug_before_success()
        {
            var reject = new Rule(-1, ModifierType.Any, 99, "Attribute Requirements");
            var required = new Rule(1, ModifierType.Any, 99, "Fire Spell Skill Gems");

            AltAugResponse response = GambleRuleEvaluator.EvaluateAltAug(
                FireSceptreSnippet,
                [required, reject],
                logParse: false);

            Assert.Equal(AltAugResponse.Alt, response);
        }
    }
}
