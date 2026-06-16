using PoE.dlls.GameData;
using PoE.dlls.Gamble;
using Xunit;

namespace PoE.Tests
{
    public class GambleModContentMatcherTests
    {
        [Fact]
        public void Hash_in_rule_matches_rolled_percent_value()
        {
            Assert.True(GambleModContentMatcher.IsContentMatch(
                "#% increased Physical Damage",
                "65% increased Physical Damage"));
        }

        [Fact]
        public void Hash_matches_after_range_normalization()
        {
            Assert.True(GambleModContentMatcher.IsContentMatch(
                "#% more Monster Life",
                "44(40-49)% more Monster Life"));
        }

        [Fact]
        public void Normalize_keeps_highest_non_range_value()
        {
            string normalized = GambleModContentMatcher.NormalizeItemModContent("29(25-30)% more Monster Life");
            Assert.Equal("29% more Monster Life", normalized);
        }

        [Fact]
        public void Normalize_handles_negative_range_values()
        {
            string normalized = GambleModContentMatcher.NormalizeItemModContent("-9(-12--9)% to all maximum Resistances");
            Assert.Equal("-9% to all maximum Resistances", normalized);
        }

        [Fact]
        public void Percent_stays_literal_in_pattern()
        {
            Assert.True(GambleModContentMatcher.IsContentMatch("#%", "18% of Physical Damage"));
        }

        [Fact]
        public void Skeleton_strips_operators_and_numbers()
        {
            Assert.Equal(
                "Adds # to # Physical Damage",
                ModTemplateNormalizer.ToSkeleton("Adds >=5 to # Physical Damage"));
        }

        [Fact]
        public void Bare_number_in_rule_becomes_exact_equals_in_template_match()
        {
            Assert.True(ModTemplateMatcher.TryMatch(
                "Adds 5 to 15 Physical Damage",
                "Adds # to # Physical Damage",
                "Adds 5 to 15 Physical Damage"));

            Assert.False(ModTemplateMatcher.TryMatch(
                "Adds 5 to 15 Physical Damage",
                "Adds # to # Physical Damage",
                "Adds 6 to 15 Physical Damage"));
        }

        [Fact]
        public void Template_match_supports_greater_or_equal_thresholds()
        {
            Assert.True(ModTemplateMatcher.TryMatch(
                "Adds >=5 to >=15 Physical Damage",
                "Adds # to # Physical Damage",
                "Adds 8 to 22 Physical Damage"));

            Assert.False(ModTemplateMatcher.TryMatch(
                "Adds >=5 to >=15 Physical Damage",
                "Adds # to # Physical Damage",
                "Adds 3 to 22 Physical Damage"));
        }

        [Fact]
        public void Template_match_supports_wildcard_and_threshold_mix()
        {
            Assert.True(ModTemplateMatcher.TryMatch(
                "Adds # to >=15 Physical Damage",
                "Adds # to # Physical Damage",
                "Adds 2 to 20 Physical Damage"));

            Assert.False(ModTemplateMatcher.TryMatch(
                "Adds # to >=15 Physical Damage",
                "Adds # to # Physical Damage",
                "Adds 2 to 10 Physical Damage"));
        }

        [Fact]
        public void Template_match_supports_less_or_equal_for_negative_mods()
        {
            Assert.True(ModTemplateMatcher.TryMatch(
                "<= -9% to all maximum Resistances",
                "#% to all maximum Resistances",
                "-9% to all maximum Resistances"));

            Assert.False(ModTemplateMatcher.TryMatch(
                "<= -9% to all maximum Resistances",
                "#% to all maximum Resistances",
                "-8% to all maximum Resistances"));
        }

        [Fact]
        public void Catalog_path_uses_db_template_when_skeleton_matches()
        {
            string dbPath = Path.Combine(Path.GetTempPath(), $"poe_modcache_test_{Guid.NewGuid():N}.sqlite");
            try
            {
                using var database = new ModCacheDatabase(dbPath);
                database.Recreate(
                [
                    new ModCatalogEntry("AddsPhysicalDamage", "Adds # to # Physical Damage", false, ModEldritchInfluence.None),
                ]);

                GambleModContentMatcher.SetCatalogContext(database, GambleType.Chaos);
                try
                {
                    Assert.True(GambleModContentMatcher.IsContentMatch(
                        "Adds >=5 to >=15 Physical Damage",
                        "Adds 8 to 22 Physical Damage"));
                }
                finally
                {
                    GambleModContentMatcher.ClearCatalogContext();
                }
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void Catalog_miss_falls_back_to_legacy_hash_regex()
        {
            string dbPath = Path.Combine(Path.GetTempPath(), $"poe_modcache_test_{Guid.NewGuid():N}.sqlite");
            try
            {
                using var database = new ModCacheDatabase(dbPath);
                database.Recreate([]);

                GambleModContentMatcher.SetCatalogContext(database, GambleType.Chaos);
                try
                {
                    Assert.True(GambleModContentMatcher.IsContentMatch(
                        "#% increased Physical Damage",
                        "65% increased Physical Damage"));
                }
                finally
                {
                    GambleModContentMatcher.ClearCatalogContext();
                }
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ToHashTemplate_replaces_numbers_for_autocomplete_insert()
        {
            Assert.Equal(
                "#% increased Physical Damage",
                ModTemplateNormalizer.ToHashTemplate("65% increased Physical Damage"));
        }
    }
}
