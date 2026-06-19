using PoE.dlls.GameData;
using Xunit;

namespace PoE.Tests;

public class ModClusterTypeFilterTests
{
    [Fact]
    public void SearchItemOnly_large_cluster_includes_minion_damage_affixes()
    {
        string dbPath = Path.Combine(Path.GetTempPath(), $"modcache_cluster_{Guid.NewGuid():N}.sqlite");
        try
        {
            using var database = new ModCacheDatabase(dbPath);
            database.Recreate(
            [
                new ModCatalogEntry(
                    "Small Passives",
                    "Added Small Passive Skills also grant: Minions have #% increased Attack and Cast Speed",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: ModCatalogTagHelper.DomainClusterJewel,
                    ItemKind: ModItemKind.ClusterJewel,
                    SpawnTags: "affliction_minion_damage"),
                new ModCatalogEntry(
                    "Large Generic",
                    "Added Small Passive Skills also grant: #% increased Armour",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: ModCatalogTagHelper.DomainClusterJewel,
                    ItemKind: ModItemKind.ClusterJewel,
                    SpawnTags: "expansion_jewel_large"),
                new ModCatalogEntry(
                    "Medium Life",
                    "Added Small Passive Skills also grant: #% increased maximum Life",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: ModCatalogTagHelper.DomainClusterJewel,
                    ItemKind: ModItemKind.ClusterJewel,
                    SpawnTags: "affliction_maximum_life"),
                new ModCatalogEntry(
                    "Small Reservation",
                    "Added Small Passive Skills also grant: #% increased Reservation Efficiency of Skills",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: ModCatalogTagHelper.DomainClusterJewel,
                    ItemKind: ModItemKind.ClusterJewel,
                    SpawnTags: "affliction_reservation_efficiency_small"),
            ]);

            IReadOnlyList<ModSuggestionItem> largeMinion = database.SearchItemOnly(
                "minion attack", "Large Cluster Jewel", 20, 0);
            IReadOnlyList<ModSuggestionItem> medium = database.SearchItemOnly(
                "maximum life", "Large Cluster Jewel", 20, 0);
            IReadOnlyList<ModSuggestionItem> minionType = database.SearchItemOnly(
                "minion attack", "Minion Damage", 20, 0);

            Assert.Contains(
                largeMinion,
                item => item.ModContent.Contains("Attack and Cast Speed", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(
                medium,
                item => item.ModContent.Contains("maximum Life", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(
                minionType,
                item => item.ModContent.Contains("Attack and Cast Speed", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void Build_catalog_large_cluster_search_finds_minion_mods_when_game_present()
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

        string dbPath = Path.Combine(Path.GetTempPath(), $"modcache_cluster_{Guid.NewGuid():N}.sqlite");
        try
        {
            using var database = new ModCacheDatabase(dbPath);
            database.Recreate(entries);

            IReadOnlyList<ModSuggestionItem> largeMinion = database.SearchItemOnly(
                "Minions have", "Large Cluster Jewel", 40, 0);

            Assert.Contains(
                largeMinion,
                item => item.ModContent.Contains("Attack and Cast Speed", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}
