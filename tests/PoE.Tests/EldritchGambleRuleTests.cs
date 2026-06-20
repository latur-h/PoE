using PoE.dlls.Gamble;
using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.Settings.Mods;
using Xunit;

namespace PoE.Tests
{
    public class EldritchGambleRuleTests
    {
        private const string EaterGlovesSnippet = """
            { Eater of Worlds Implicit Modifier (Lesser) }
            Projectiles Pierce an additional Target
            --------
            { Prefix Modifier "Athlete's" (Tier: 1) — Life }
            +125(115-129) to maximum Life
            """;

        [Fact]
        public void ParseModifiers_eldritch_implicit_sets_influence_name_from_header()
        {
            List<Modifier> mods = GambleRuleEvaluator.ParseModifiers(
                EaterGlovesSnippet,
                logImplicitMods: false,
                logParse: false);

            Modifier eaterImplicit = Assert.Single(
                mods,
                m => m.Type == ModifierType.Implicit && m.Content.Contains("Projectiles Pierce"));

            Assert.Equal("Eater of Worlds", eaterImplicit.Name);
            Assert.Equal("Projectiles Pierce an additional Target", eaterImplicit.Content);
        }

        [Fact]
        public void Eldritch_eater_slot_matches_required_rule_for_single_eater_implicit()
        {
            List<Modifier> mods = GambleRuleEvaluator.ParseModifiers(
                EaterGlovesSnippet,
                logImplicitMods: false,
                logParse: false);

            Modifier eaterImplicit = Assert.Single(
                mods,
                m => m.Type == ModifierType.Implicit && m.Content.Contains("Projectiles Pierce"));

            var eaterRule = new Rule(
                1,
                ModifierType.Any,
                99,
                "Projectiles Pierce an additional Target",
                EldritchInfluence.EaterOfWorlds);

            Assert.True(GambleModContentMatcher.MatchesModRule(eaterRule, eaterImplicit));
            Assert.Contains("Eater of Worlds", eaterImplicit.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
