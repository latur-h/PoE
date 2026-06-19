using PoE.dlls.GameData;
using Xunit;

namespace PoE.Tests;

public class ModEldritchTypeFilterTests
{
    [Fact]
    public void SearchEldritchImplicit_excludes_weapon_item_types()
    {
        string dbPath = Path.Combine(Path.GetTempPath(), $"modcache_eldritch_{Guid.NewGuid():N}.sqlite");
        try
        {
            using var database = new ModCacheDatabase(dbPath);
            database.Recreate(
            [
                new ModCatalogEntry(
                    "Exarch",
                    "#% increased Action Speed",
                    false,
                    ModEldritchInfluence.SearingExarch,
                    ModDomain: 1,
                    ItemKind: ModItemKind.Eldritch,
                    SpawnTags: "boots"),
                new ModCatalogEntry(
                    "Eater",
                    "#% increased Effect of Arcane Surge on you",
                    false,
                    ModEldritchInfluence.EaterOfWorlds,
                    ModDomain: 1,
                    ItemKind: ModItemKind.Eldritch,
                    SpawnTags: "helmet"),
            ]);

            Assert.Empty(database.SearchEldritchImplicit(ModEldritchInfluence.SearingExarch, "action speed", "claw", 20, 0));
            Assert.Empty(database.SearchEldritchImplicit(ModEldritchInfluence.SearingExarch, "action speed", "wand", 20, 0));
            Assert.NotEmpty(database.SearchEldritchImplicit(ModEldritchInfluence.SearingExarch, "action speed", "boots", 20, 0));
            Assert.NotEmpty(database.SearchEldritchImplicit(ModEldritchInfluence.EaterOfWorlds, "arcane surge", "helmet", 20, 0));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Theory]
    [InlineData("claw", false)]
    [InlineData("wand", false)]
    [InlineData("bow", false)]
    [InlineData("body_armour", true)]
    [InlineData("helmet", true)]
    [InlineData("gloves", true)]
    [InlineData("boots", true)]
    [InlineData("amulet", true)]
    [InlineData("", true)]
    public void IsEldritchEligibleItemType_matches_armour_only(string itemType, bool expected) =>
        Assert.Equal(expected, ModItemTypeTags.IsEldritchEligibleItemType(
            string.IsNullOrEmpty(itemType) ? null : itemType));

    [Fact]
    public void SearchSpawnTags_eldritch_armour_scope_lists_armour_only()
    {
        string dbPath = Path.Combine(Path.GetTempPath(), $"modcache_eldritch_tags_{Guid.NewGuid():N}.sqlite");
        try
        {
            using var database = new ModCacheDatabase(dbPath);
            database.Recreate(
            [
                new ModCatalogEntry(
                    "Exarch",
                    "#% increased Action Speed",
                    false,
                    ModEldritchInfluence.SearingExarch,
                    ModDomain: 1,
                    ItemKind: ModItemKind.Eldritch,
                    SpawnTags: "boots"),
                new ModCatalogEntry(
                    "Claw",
                    "#% increased Attack Speed",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: 1,
                    ItemKind: ModItemKind.Item,
                    SpawnTags: "claw"),
            ]);

            IReadOnlyList<string> armourTags = database.SearchSpawnTags("o", 20, eldritchArmourOnly: true);
            IReadOnlyList<string> allTags = database.SearchSpawnTags("claw", 20, eldritchArmourOnly: false);

            Assert.Contains(armourTags, tag => string.Equals(tag, "boots", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(armourTags, tag => string.Equals(tag, "claw", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(allTags, tag => string.Equals(tag, "claw", StringComparison.OrdinalIgnoreCase));
            Assert.True(database.SpawnTagExistsForEldritchArmour("Boots"));
            Assert.False(database.SpawnTagExistsForEldritchArmour("Claw"));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}
