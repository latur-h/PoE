using PoE.dlls.GameData;
using PoE.dlls.Gamble;
using PoE.dlls.Gamble.Modifiers;
using Xunit;
using Xunit.Abstractions;

namespace PoE.Tests
{
    public class SpellSkillGemsMatchTests
    {
        private const string RuleContent = "# to Level of all Spell Skill Gems";
        private static readonly Rule Rule = new(1, ModifierType.Any, 99, RuleContent);

        private static string LoadTextFixture()
        {
            string path = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", "..", "text.txt"));
            Assert.True(File.Exists(path), $"Fixture missing: {path}");
            return File.ReadAllText(path);
        }

        private readonly ITestOutputHelper _output;

        public SpellSkillGemsMatchTests(ITestOutputHelper output) => _output = output;

        [Fact]
        public void TextTxt_generic_spell_rule_must_not_match_fire_mod_line()
        {
            GambleModContentMatcher.ClearCatalogContext();
            Assert.False(GambleModContentMatcher.IsContentMatch(
                RuleContent,
                "+1 to Level of all Fire Spell Skill Gems"));
        }

        [Fact]
        public void TextTxt_priority1_rule_must_not_succeed_alt_aug_without_catalog()
        {
            string item = LoadTextFixture();
            GambleModContentMatcher.ClearCatalogContext();

            LogParsedModifiers(item);
            LogPerModMatches(item, useCatalog: false);

            AltAugResponse response = GambleRuleEvaluator.EvaluateAltAug(item, [Rule], logParse: false);
            _output.WriteLine($"EvaluateAltAug (no catalog): {response}");

            Assert.NotEqual(AltAugResponse.Success, response);
        }

        [Fact]
        public void TextTxt_priority1_rule_must_not_succeed_alt_aug_with_local_mod_catalog()
        {
            string item = LoadTextFixture();
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PoE",
                "modcache.sqlite");

            if (!File.Exists(dbPath))
            {
                _output.WriteLine($"Skipping catalog test — no mod cache at {dbPath}");
                return;
            }

            using var database = new ModCacheDatabase();
            GambleModContentMatcher.SetCatalogContext(database, GambleType.Alt_Aug);
            try
            {
                LogCatalogSpellTemplates(database);
                LogPerModMatches(item, useCatalog: true);

                AltAugResponse response = GambleRuleEvaluator.EvaluateAltAug(item, [Rule], logParse: false);
                _output.WriteLine($"EvaluateAltAug (with catalog): {response}");

                Assert.NotEqual(AltAugResponse.Success, response);
            }
            finally
            {
                GambleModContentMatcher.ClearCatalogContext();
            }
        }

        [Fact]
        public void TextTxt_no_modifier_matches_rule_including_crafted_name()
        {
            string item = LoadTextFixture();
            GambleModContentMatcher.ClearCatalogContext();

            List<Modifier> mods = GambleRuleEvaluator.ParseModifiers(item, logImplicitMods: false, logParse: false);
            foreach (Modifier mod in mods)
            {
                bool matched = GambleModContentMatcher.MatchesModRule(Rule, mod, matchNameToo: true);
                Assert.False(matched, $"Unexpected match on [{mod.Type}] name='{mod.Name}' content='{mod.Content}'");
            }
        }

        [Fact]
        public void TextTxt_catalog_template_match_also_rejects_fire_line()
        {
            GambleModContentMatcher.ClearCatalogContext();
            Assert.False(ModTemplateMatcher.TryMatch(
                RuleContent,
                "# to Level of all Spell Skill Gems",
                "+1 to Level of all Fire Spell Skill Gems"));
        }

        [Fact]
        public void TextTxt_legacy_regex_must_not_substring_match_fire_line()
        {
            string pattern = GambleModContentMatcher.ToRegexPattern(RuleContent);
            var match = System.Text.RegularExpressions.Regex.Match(
                "+1 to Level of all Fire Spell Skill Gems",
                pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            Assert.False(match.Success);
        }

        private void LogParsedModifiers(string item)
        {
            List<Modifier> mods = GambleRuleEvaluator.ParseModifiers(item, logImplicitMods: false, logParse: false);
            _output.WriteLine($"Parsed {mods.Count} modifier(s):");
            foreach (Modifier mod in mods)
                _output.WriteLine($"  [{mod.Type} T{mod.Tier}] name={mod.Name!} | {mod.Content}");
        }

        private void LogPerModMatches(string item, bool useCatalog)
        {
            if (!useCatalog)
                GambleModContentMatcher.ClearCatalogContext();

            List<Modifier> mods = GambleRuleEvaluator.ParseModifiers(item, logImplicitMods: false, logParse: false);
            _output.WriteLine($"Rule: {RuleContent}");
            foreach (Modifier mod in mods)
            {
                bool content = GambleModContentMatcher.IsContentMatch(RuleContent, mod.Content);
                bool name = GambleModContentMatcher.IsContentMatch(RuleContent, mod.Name);
                if (content || name)
                    _output.WriteLine($"  MATCH content={content} name={name} :: [{mod.Type}] {mod.Content}");
            }
        }

        private void LogCatalogSpellTemplates(ModCacheDatabase database)
        {
            string skeleton = ModTemplateNormalizer.ToSkeleton(RuleContent);
            _output.WriteLine($"Skeleton: {skeleton}");
            if (database.TryFindModTemplate(skeleton, GambleType.Alt_Aug, out string? template))
                _output.WriteLine($"Catalog template for skeleton: {template}");
            else
                _output.WriteLine("No catalog template for rule skeleton.");
        }
    }
}
