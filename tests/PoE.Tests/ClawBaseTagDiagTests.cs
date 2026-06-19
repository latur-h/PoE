using PoE.dlls.GameData;
using Xunit;
using Xunit.Abstractions;

namespace PoE.Tests;

public class ClawBaseTagDiagTests
{
    private readonly ITestOutputHelper _output;

    public ClawBaseTagDiagTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Diagnose_claw_base_item_tags()
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
        if (!PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/baseitemtypes.datc64", out byte[] baseBytes, out _, out _))
        {
            _output.WriteLine("no baseitemtypes");
            return;
        }

        Assert.True(PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/tags.datc64", out byte[] tagsBytes, out _, out _));
        var bases = LibDat2DatTable.Load(baseBytes, "baseitemtypes", schemaPath);
        var tags = LibDat2DatTable.Load(tagsBytes, "tags", schemaPath);
        var tagIds = new string?[tags.RowCount];
        for (int i = 0; i < tags.RowCount; i++)
            tagIds[i] = tags.GetString(i, "Id");

        var clawTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int samples = 0;
        for (int row = 0; row < bases.RowCount; row++)
        {
            string? name = bases.GetString(row, "Name");
            string? id = bases.GetString(row, "Id");
            if (name is null || !name.Contains("Claw", StringComparison.OrdinalIgnoreCase))
                continue;

            samples++;
            var tagKeys = bases.GetForeignKeyArray(row, "TagsKeys");
            foreach (int key in tagKeys)
            {
                string? tag = (uint)key < (uint)tagIds.Length ? tagIds[key] : null;
                if (!string.IsNullOrWhiteSpace(tag))
                    clawTags.Add(tag);
            }

            if (samples <= 3)
                _output.WriteLine($"base={name} id={id} tags={string.Join(',', tagKeys.Select(k => (uint)k < (uint)tagIds.Length ? tagIds[k] : "?"))}");
        }

        _output.WriteLine($"claw bases={samples}");
        _output.WriteLine($"union tags: {string.Join(", ", clawTags.OrderBy(t => t))}");
    }
}
