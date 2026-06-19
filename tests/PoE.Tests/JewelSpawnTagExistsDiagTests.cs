using PoE.dlls.GameData;
using Xunit;
using Xunit.Abstractions;

namespace PoE.Tests;

public class JewelSpawnTagExistsDiagTests
{
    private readonly ITestOutputHelper _output;

    public JewelSpawnTagExistsDiagTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Diagnose_jewel_filter_validation()
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
        Assert.True(PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/mods.datc64", out byte[] modsBytes, out _, out _));
        Assert.True(PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/tags.datc64", out byte[] tagsBytes, out _, out _));
        Assert.True(PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/stats.datc64", out byte[] statsBytes, out _, out _));

        HashSet<ModCatalogEntry> entries = ModCatalogBuilder.Build(
            schemaPath, modsBytes, tagsBytes, statsBytes,
            PoEDataFileLocator.ReadStatDescriptionFiles(archive));

        int jewelKind = entries.Count(e => e.ItemKind == ModItemKind.Jewel);
        int jewelTag = entries.Count(e => e.SpawnTags.Contains("jewel", StringComparison.OrdinalIgnoreCase));
        int jewelKindAndTag = entries.Count(e => e.ItemKind == ModItemKind.Jewel
            && e.SpawnTags.Contains("jewel", StringComparison.OrdinalIgnoreCase));

        _output.WriteLine($"jewel item_kind rows={jewelKind}, jewel spawn tag rows={jewelTag}, both={jewelKindAndTag}");

        foreach (ModCatalogEntry e in entries
                     .Where(x => x.ItemKind == ModItemKind.Jewel || x.SpawnTags.Contains("jewel", StringComparison.OrdinalIgnoreCase))
                     .Take(8))
        {
            _output.WriteLine($"  kind={e.ItemKind} tags={e.SpawnTags} | {e.ModName} | {e.ModContent}");
        }

        string dbPath = Path.Combine(Path.GetTempPath(), $"modcache_jewel_{Guid.NewGuid():N}.sqlite");
        try
        {
            using var database = new ModCacheDatabase(dbPath);
            database.Recreate(entries);

            bool existsJewel = database.SpawnTagExists("Jewel");
            bool existsJewelCanon = database.SpawnTagExists("jewel");
            int searchCount = database.SearchItemOnly("life", "jewel", 10, 0).Count;

            _output.WriteLine($"SpawnTagExists('Jewel')={existsJewel}");
            _output.WriteLine($"SpawnTagExists('jewel')={existsJewelCanon}");
            _output.WriteLine($"SearchItemOnly life+jewel={searchCount}");
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}
