using PoE.dlls.GameData;
using Xunit;
using Xunit.Abstractions;

namespace PoE.Tests;

public class JewelModExtractionDiagTests
{
    public JewelModExtractionDiagTests(ITestOutputHelper output) => _output = output;
    private readonly ITestOutputHelper _output;

    [Fact]
    public void Diagnose_jewel_mods()
    {
        const string gameFolder = @"L:\PoE";
        if (!Directory.Exists(gameFolder)) return;
        string schemaPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PoE", "schema.min.json");
        if (!File.Exists(schemaPath)) return;

        using GameArchiveSession archive = new(gameFolder);
        Assert.True(PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/mods.datc64", out byte[] modsBytes, out _, out _));
        Assert.True(PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/tags.datc64", out byte[] tagsBytes, out _, out _));
        var mods = LibDat2DatTable.Load(modsBytes, "mods", schemaPath);
        var tags = LibDat2DatTable.Load(tagsBytes, "tags", schemaPath);
        var tagIds = new string?[tags.RowCount];
        for (int i = 0; i < tags.RowCount; i++) tagIds[i] = tags.GetString(i, "Id");

        var domainCounts = new Dictionary<int, int>();
        int samples = 0;
        for (int row = 0; row < mods.RowCount; row++)
        {
            int gen = mods.GetInt32(row, "GenerationType");
            if (gen is not (1 or 2)) continue;
            var positive = new List<string>();
            var spawnTags = mods.GetForeignKeyArray(row, "SpawnWeight_TagsKeys");
            var spawnVals = mods.GetInt32Array(row, "SpawnWeight_Values");
            for (int i = 0; i < Math.Min(spawnTags.Count, spawnVals.Count); i++)
            {
                if (spawnVals[i] <= 0) continue;
                string? tag = (uint)spawnTags[i] < (uint)tagIds.Length ? tagIds[spawnTags[i]] : null;
                if (!string.IsNullOrWhiteSpace(tag)) positive.Add(tag);
            }
            if (!positive.Any(t => string.Equals(t, "jewel", StringComparison.OrdinalIgnoreCase)))
                continue;

            int domain = mods.GetInt32(row, "Domain");
            domainCounts.TryGetValue(domain, out int c);
            domainCounts[domain] = c + 1;
            if (samples < 8)
            {
                samples++;
                _output.WriteLine($"row={row} domain={domain} id={mods.GetString(row,"Id")} tags={string.Join(',', positive)}");
            }
        }
        foreach (var kv in domainCounts.OrderBy(k => k.Key))
            _output.WriteLine($"domain {kv.Key}: {kv.Value} jewel-tagged prefix/suffix mods");
    }
}
