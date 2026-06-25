using PoE.dlls.GameData;
using Xunit;

namespace PoE.Tests;

public class ModCacheDatabaseSpawnTagTests
{
    [Fact]
    public void SearchItemOnly_filters_by_exact_spawn_tag()
    {
        string dbPath = Path.Combine(Path.GetTempPath(), $"modcache_spawn_{Guid.NewGuid():N}.sqlite");
        try
        {
            using var database = new ModCacheDatabase(dbPath);
            database.Recreate(
            [
                new ModCatalogEntry("FlaskLife", "#% increased Life Recovery from Flasks", false, ModEldritchInfluence.None,
                    ModDomain: 1, ItemKind: ModItemKind.Flask, SpawnTags: "default,flask"),
                new ModCatalogEntry("ClawDamage", "Adds # to # Physical Damage to Claws", false, ModEldritchInfluence.None,
                    ModDomain: 1, ItemKind: ModItemKind.Item, SpawnTags: "claw,weapon"),
                new ModCatalogEntry("CritMulti", "#% to Global Critical Strike Multiplier", false, ModEldritchInfluence.None,
                    ModDomain: 1, ItemKind: ModItemKind.Item, SpawnTags: "amulet,weapon"),
            ]);

            IReadOnlyList<ModSuggestionItem> flaskHits = database.SearchItemOnly("life", "flask", 20, 0);
            IReadOnlyList<ModSuggestionItem> clawHits = database.SearchItemOnly("physical", "claw", 20, 0);
            IReadOnlyList<ModSuggestionItem> clawShared = database.SearchItemOnly("critical", "claw", 20, 0);
            IReadOnlyList<ModSuggestionItem> allHits = database.SearchItemOnly("physical", null, 20, 0);

            Assert.Single(flaskHits);
            Assert.Equal("FlaskLife", flaskHits[0].ModName);
            Assert.Single(clawHits);
            Assert.Equal("ClawDamage", clawHits[0].ModName);
            Assert.Contains(clawShared, item => item.ModName == "CritMulti");
            Assert.Contains(allHits, item => item.ModName == "ClawDamage");
            Assert.DoesNotContain(allHits, item => item.ModName == "FlaskLife");
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void SearchItemOnly_generic_flask_filter_matches_life_flask_only_mods()
    {
        string dbPath = Path.Combine(Path.GetTempPath(), $"modcache_flask_{Guid.NewGuid():N}.sqlite");
        try
        {
            using var database = new ModCacheDatabase(dbPath);
            database.Recreate(
            [
                new ModCatalogEntry("LifeRecovery", "#% increased Life Recovery rate", false, ModEldritchInfluence.None,
                    ModDomain: ModCatalogTagHelper.DomainFlask, ItemKind: ModItemKind.Flask, SpawnTags: "default,flask,life_flask"),
                new ModCatalogEntry("UtilityEffect", "#% increased Effect", false, ModEldritchInfluence.None,
                    ModDomain: ModCatalogTagHelper.DomainFlask, ItemKind: ModItemKind.Flask, SpawnTags: "default,flask,utility_flask"),
                new ModCatalogEntry("ClawDamage", "Adds # to # Physical Damage to Claws", false, ModEldritchInfluence.None,
                    ModDomain: 1, ItemKind: ModItemKind.Item, SpawnTags: "claw,weapon"),
            ]);

            IReadOnlyList<ModSuggestionItem> lifeHits = database.SearchItemOnly("recovery", "flask", 20, 0);
            IReadOnlyList<ModSuggestionItem> effectHits = database.SearchItemOnly("effect", "flask", 20, 0);
            IReadOnlyList<ModSuggestionItem> lifeFlaskHits = database.SearchItemOnly("recovery", "life_flask", 20, 0);

            Assert.Single(lifeHits);
            Assert.Equal("LifeRecovery", lifeHits[0].ModName);
            Assert.Single(effectHits);
            Assert.Equal("UtilityEffect", effectHits[0].ModName);
            Assert.Single(lifeFlaskHits);
            Assert.Equal("LifeRecovery", lifeFlaskHits[0].ModName);
            Assert.True(database.SpawnTagExists("flask"));
            Assert.True(database.SpawnTagExists("Flask"));
            Assert.Contains(database.SearchSpawnTags("flask", 10), tag => string.Equals(tag, "flask", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void SpawnTagExists_and_SearchSpawnTags_use_index()
    {
        string dbPath = Path.Combine(Path.GetTempPath(), $"modcache_spawn_{Guid.NewGuid():N}.sqlite");
        try
        {
            using var database = new ModCacheDatabase(dbPath);
            database.Recreate(
            [
                new ModCatalogEntry("ClawDamage", "Adds # to # Physical Damage to Claws", false, ModEldritchInfluence.None,
                    ModDomain: 1, ItemKind: ModItemKind.Item, SpawnTags: "claw,weapon"),
            ]);

            Assert.True(database.SpawnTagExists("claw"));
            Assert.False(database.SpawnTagExists("flask"));

            IReadOnlyList<string> tags = database.SearchSpawnTags("cl", 10);
            Assert.Contains(tags, tag => string.Equals(tag, "claw", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}
