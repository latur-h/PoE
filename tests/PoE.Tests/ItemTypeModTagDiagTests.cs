using PoE.dlls.GameData;
using Xunit;
using Xunit.Abstractions;

namespace PoE.Tests;

public class ItemTypeModTagDiagTests
{
    private readonly ITestOutputHelper _output;

    public ItemTypeModTagDiagTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Diagnose_mod_spawn_tags_for_claw_relevant_content()
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

        IReadOnlyList<(string Path, byte[] Bytes)> statFiles = PoEDataFileLocator.ReadStatDescriptionFiles(archive);
        HashSet<ModCatalogEntry> entries = ModCatalogBuilder.Build(
            schemaPath, modsBytes, tagsBytes, statsBytes, statFiles);

        Dump(entries, "Critical Strike Multiplier");
        Dump(entries, "Spell Skill Gems");
        Dump(entries, "to Level of all Fire Spell Skill Gems");

        var tags = LibDat2DatTable.Load(tagsBytes, "tags", schemaPath);
        _output.WriteLine("=== tag row for claw ===");
        for (int i = 0; i < tags.RowCount; i++)
        {
            if (!string.Equals(tags.GetString(i, "Id"), "claw", StringComparison.OrdinalIgnoreCase))
                continue;

            _output.WriteLine($"claw row={i}");
            foreach (string col in new[] { "TagsKeys", "Tag", "Id" })
            {
                try
                {
                    var arr = tags.GetForeignKeyArray(i, col);
                    if (arr.Count > 0)
                        _output.WriteLine($"  {col}=[{string.Join(',', arr)}]");
                }
                catch { /* column may not exist */ }
            }
        }
    }

    private void Dump(HashSet<ModCatalogEntry> entries, string fragment)
    {
        _output.WriteLine($"=== '{fragment}' ===");
        foreach (ModCatalogEntry entry in entries
                     .Where(e => !e.IsMap && e.EldritchInfluence == ModEldritchInfluence.None)
                     .Where(e => e.ModContent.Contains(fragment, StringComparison.OrdinalIgnoreCase)
                                 || e.ModName.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                     .Take(6))
        {
            _output.WriteLine($"{entry.ModName} | {entry.ModContent} | tags={entry.SpawnTags}");
        }
    }
}
