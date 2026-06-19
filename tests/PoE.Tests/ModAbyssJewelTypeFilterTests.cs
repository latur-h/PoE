using PoE.dlls.GameData;
using Xunit;

namespace PoE.Tests;

public class ModAbyssJewelTypeFilterTests
{
    [Fact]
    public void SearchItemOnly_ghastly_includes_summoner_mods_only()
    {
        string dbPath = Path.Combine(Path.GetTempPath(), $"modcache_abyss_{Guid.NewGuid():N}.sqlite");
        try
        {
            using var database = new ModCacheDatabase(dbPath);
            database.Recreate(
            [
                new ModCatalogEntry(
                    "Heated",
                    "Minions deal # to # added Fire Damage",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: ModCatalogTagHelper.DomainAbyssJewel,
                    ItemKind: ModItemKind.AbyssJewel,
                    SpawnTags: "abyss_jewel,abyss_jewel_summoner"),
                new ModCatalogEntry(
                    "Heated",
                    "Adds # to # Fire Damage to Attacks",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: ModCatalogTagHelper.DomainAbyssJewel,
                    ItemKind: ModItemKind.AbyssJewel,
                    SpawnTags: "abyss_jewel,abyss_jewel_melee,searing_eye_jewel"),
                new ModCatalogEntry(
                    "of Enchanting",
                    "#% increased Cast Speed",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: ModCatalogTagHelper.DomainAbyssJewel,
                    ItemKind: ModItemKind.AbyssJewel,
                    SpawnTags: "abyss_jewel,abyss_jewel_caster"),
            ]);

            IReadOnlyList<ModSuggestionItem> ghastly = database.SearchItemOnly("fire", "Ghastly Eye Jewel", 20, 0);
            IReadOnlyList<ModSuggestionItem> searing = database.SearchItemOnly("fire", "Searing Eye Jewel", 20, 0);
            IReadOnlyList<ModSuggestionItem> allAbyss = database.SearchItemOnly("fire", "Abyss Jewel", 20, 0);

            Assert.Contains(
                ghastly,
                item => item.ModContent.Contains("Minions deal", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(
                ghastly,
                item => item.ModContent.Contains("Attacks", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(
                searing,
                item => item.ModContent.Contains("Attacks", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(
                searing,
                item => item.ModContent.Contains("Minions deal", StringComparison.OrdinalIgnoreCase));
            Assert.True(allAbyss.Count >= 2);
            Assert.True(database.SpawnTagExists("Ghastly Eye Jewel"));
            Assert.True(database.SpawnTagExists("Searing Eye Jewel"));
            Assert.True(database.SpawnTagExists("Abyss Jewel"));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void Build_catalog_abyss_jewel_filters_when_game_present()
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

        int abyssRows = entries.Count(e => e.ItemKind == ModItemKind.AbyssJewel);
        Assert.True(abyssRows > 0, $"Expected abyss jewel suggestion rows, got 0 of {entries.Count}.");

        string dbPath = Path.Combine(Path.GetTempPath(), $"modcache_abyss_{Guid.NewGuid():N}.sqlite");
        try
        {
            using var database = new ModCacheDatabase(dbPath);
            database.Recreate(entries);

            Assert.True(database.SpawnTagExists("Abyss Jewel"));
            Assert.True(database.SpawnTagExists("Ghastly Eye Jewel"));

            IReadOnlyList<ModSuggestionItem> ghastly = database.SearchItemOnly(
                "Minions deal", "Ghastly Eye Jewel", 40, 0);
            IReadOnlyList<ModSuggestionItem> searing = database.SearchItemOnly(
                "Fire Damage to Attacks", "Searing Eye Jewel", 40, 0);

            Assert.NotEmpty(ghastly);
            Assert.NotEmpty(searing);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}
