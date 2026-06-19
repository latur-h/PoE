using PoE.dlls.GameData;
using Xunit;
using Xunit.Abstractions;

namespace PoE.Tests;

public class FlaskModExtractionDiagTests
{
    private readonly ITestOutputHelper _output;

    public FlaskModExtractionDiagTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Diagnose_flask_spawn_tags_in_game_data()
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

        var flaskRelatedTags = tagIds
            .Where(t => t is not null && t.Contains("flask", StringComparison.OrdinalIgnoreCase))
            .OrderBy(t => t, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _output.WriteLine("=== tags containing 'flask' ===");
        foreach (string? tag in flaskRelatedTags)
            _output.WriteLine(tag);

        int samples = 0;
        for (int row = 0; row < mods.RowCount && samples < 20; row++)
        {
            int gen = mods.GetInt32(row, "GenerationType");
            if (gen is not (1 or 2))
                continue;

            var positive = CollectPositive(mods, tagIds, row);
            bool equipmentFlask = positive.Any(t =>
                t.Contains("flask", StringComparison.OrdinalIgnoreCase)
                && !t.StartsWith("affliction_", StringComparison.OrdinalIgnoreCase));

            if (!equipmentFlask)
                continue;

            samples++;
            _output.WriteLine(
                $"row={row} domain={mods.GetInt32(row, "Domain")} gen={gen} id={mods.GetString(row, "Id")} name={mods.GetString(row, "Name")}");
            _output.WriteLine($"  tags={string.Join(',', positive)}");
        }

        _output.WriteLine($"equipment flask mod samples: {samples}");
    }

    private static List<string> CollectPositive(LibDat2DatTable mods, string?[] tagIds, int row)
    {
        var positive = new List<string>();
        var spawnTags = mods.GetForeignKeyArray(row, "SpawnWeight_TagsKeys");
        var spawnVals = mods.GetInt32Array(row, "SpawnWeight_Values");
        int count = Math.Min(spawnTags.Count, spawnVals.Count);
        for (int i = 0; i < count; i++)
        {
            if (spawnVals[i] <= 0)
                continue;

            string? tag = (uint)spawnTags[i] < (uint)tagIds.Length ? tagIds[spawnTags[i]] : null;
            if (!string.IsNullOrWhiteSpace(tag))
                positive.Add(tag);
        }

        return positive;
    }
}
