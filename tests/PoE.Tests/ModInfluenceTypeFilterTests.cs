using PoE.dlls.GameData;
using Xunit;

namespace PoE.Tests;

public class ModInfluenceTypeFilterTests
{
    [Theory]
    [InlineData("bow,two_hand_weapon,weapon", "claw", false)]
    [InlineData("bow", "claw", false)]
    [InlineData("claw_shaper", "claw", true)]
    [InlineData("wand_shaper", "claw", false)]
    [InlineData("weapon", "claw", true)]
    [InlineData("amulet,weapon", "claw", true)]
    [InlineData("attack_dagger,attack_staff,weapon", "claw", true)]
    [InlineData("bow", "bow", true)]
    public void ModMatchesItemType_weapon_specific_and_influence_tags(string modTags, string filter, bool expected) =>
        Assert.Equal(expected, ModItemTypeTags.ModMatchesItemType(modTags, filter));

    [Fact]
    public void SearchItemOnly_claw_includes_shaper_excludes_bow_gems()
    {
        string dbPath = Path.Combine(Path.GetTempPath(), $"modcache_influence_{Guid.NewGuid():N}.sqlite");
        try
        {
            using var database = new ModCacheDatabase(dbPath);
            database.Recreate(
            [
                new ModCatalogEntry(
                    "Fletcher's",
                    "# to Level of Socketed Bow Gems",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: 1,
                    ItemKind: ModItemKind.Item,
                    SpawnTags: "bow"),
                new ModCatalogEntry(
                    "of Shaping",
                    "Socketed Gems are Supported by Level # Faster Attacks",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: 1,
                    ItemKind: ModItemKind.Item,
                    SpawnTags: "claw_shaper"),
                new ModCatalogEntry(
                    "of Shaping",
                    "#% increased Attack Speed",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: 1,
                    ItemKind: ModItemKind.Item,
                    SpawnTags: "claw_shaper"),
                new ModCatalogEntry(
                    "of the Underground",
                    "# to Level of Socketed Intelligence Gems",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: 1,
                    ItemKind: ModItemKind.Item,
                    SpawnTags: "bow,wand,staff"),
            ]);

            Assert.Empty(database.SearchItemOnly("Socketed Bow Gems", "claw", 20, 0));
            Assert.NotEmpty(database.SearchItemOnly("Faster Attacks", "claw", 20, 0));
            Assert.Empty(database.SearchItemOnly("Socketed Intelligence Gems", "claw", 20, 0));
            Assert.NotEmpty(database.SearchItemOnly("Socketed Intelligence Gems", "wand", 20, 0));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void Build_catalog_includes_shaper_claw_mods_when_game_present()
    {
        const string gameFolder = @"L:\PoE";
        if (!Directory.Exists(gameFolder))
            return;

        string schemaPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PoE",
            "schema.min.json");
        if (!File.Exists(schemaPath))
            return;

        using GameArchiveSession archive = new(gameFolder);
        if (!PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/mods.datc64", out byte[] modsBytes, out _, out _))
            return;

        Assert.True(PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/tags.datc64", out byte[] tagsBytes, out _, out _));
        Assert.True(PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/stats.datc64", out byte[] statsBytes, out _, out _));

        HashSet<ModCatalogEntry> entries = ModCatalogBuilder.Build(
            schemaPath, modsBytes, tagsBytes, statsBytes,
            PoEDataFileLocator.ReadStatDescriptionFiles(archive));

        Assert.Contains(
            entries,
            e => e.SpawnTags.Contains("claw_shaper", StringComparison.OrdinalIgnoreCase)
                 && e.ModContent.Contains("Faster Attacks", StringComparison.OrdinalIgnoreCase));

        Assert.DoesNotContain(
            entries,
            e => e.ModName.Contains("Underground", StringComparison.OrdinalIgnoreCase)
                 && e.ModContent.Contains("Socketed Intelligence Gems", StringComparison.OrdinalIgnoreCase));

        string dbPath = Path.Combine(Path.GetTempPath(), $"modcache_influence_{Guid.NewGuid():N}.sqlite");
        try
        {
            using var database = new ModCacheDatabase(dbPath);
            database.Recreate(entries);

            Assert.Empty(database.SearchItemOnly("Socketed Bow Gems", "claw", 30, 0));
            Assert.NotEmpty(database.SearchItemOnly("Faster Attacks", "claw", 30, 0));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}
