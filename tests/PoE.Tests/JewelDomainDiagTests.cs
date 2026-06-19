using PoE.dlls.GameData;
using Xunit;
using Xunit.Abstractions;

namespace PoE.Tests;

public class JewelDomainDiagTests
{
    private readonly ITestOutputHelper _output;

    public JewelDomainDiagTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Diagnose_domain_16_jewel_mods()
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

        var mods = LibDat2DatTable.Load(modsBytes, "mods", schemaPath);
        var tags = LibDat2DatTable.Load(tagsBytes, "tags", schemaPath);
        var tagIds = new string?[tags.RowCount];
        for (int i = 0; i < tags.RowCount; i++)
            tagIds[i] = tags.GetString(i, "Id");

        int domain16 = 0;
        int shown = 0;
        for (int row = 0; row < mods.RowCount; row++)
        {
            int domain = mods.GetInt32(row, "Domain");
            if (domain != ModCatalogTagHelper.DomainJewelLegacy)
                continue;

            int gen = mods.GetInt32(row, "GenerationType");
            if (gen is not (1 or 2))
                continue;

            domain16++;
            if (shown >= 10)
                continue;

            var positive = new List<string>();
            IReadOnlyList<int> spawnTags = mods.GetForeignKeyArray(row, "SpawnWeight_TagsKeys");
            IReadOnlyList<int> spawnValues = mods.GetInt32Array(row, "SpawnWeight_Values");
            for (int i = 0; i < Math.Min(spawnTags.Count, spawnValues.Count); i++)
            {
                if (spawnValues[i] <= 0)
                    continue;

                string? tag = (uint)spawnTags[i] < (uint)tagIds.Length ? tagIds[spawnTags[i]] : null;
                if (!string.IsNullOrWhiteSpace(tag))
                    positive.Add(tag);
            }

            shown++;
            _output.WriteLine($"  id={mods.GetString(row, "Id")} name={mods.GetString(row, "Name")} tags={string.Join(',', positive)}");
        }

        _output.WriteLine($"domain 16 prefix/suffix mods={domain16}");
    }
}
