using PoE.dlls.GameData;
using Xunit;
using Xunit.Abstractions;

namespace PoE.Tests;

public class EssenceModsDatDiagTests
{
    private readonly ITestOutputHelper _output;

    public EssenceModsDatDiagTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Diagnose_essencemods_target_categories()
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
        if (!PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/essencemods.datc64", out byte[] essenceModsBytes, out _, out _))
        {
            _output.WriteLine("essencemods.datc64 not found");
            return;
        }

        Assert.True(PoEDataFileLocator.TryReadRequiredDatFile(
            archive, "data/essencetargetitemcategories.datc64", out byte[] catBytes, out _, out _));
        Assert.True(PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/mods.datc64", out byte[] modsBytes, out _, out _));
        Assert.True(PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/itemclasses.datc64", out byte[] icBytes, out _, out _));

        var essenceMods = LibDat2DatTable.Load(essenceModsBytes, "essencemods", schemaPath);
        var categories = LibDat2DatTable.Load(catBytes, "essencetargetitemcategories", schemaPath);
        var mods = LibDat2DatTable.Load(modsBytes, "mods", schemaPath);
        var itemClasses = LibDat2DatTable.Load(icBytes, "itemclasses", schemaPath);

        _output.WriteLine($"essencemods={essenceMods.RowCount}, categories={categories.RowCount}, itemclasses={itemClasses.RowCount}");

        for (int row = 0; row < categories.RowCount; row++)
        {
            string? id = categories.GetString(row, "Id");
            string? text = categories.GetString(row, "Text");
            IReadOnlyList<int> classKeys = categories.GetForeignKeyArray(row, "ItemClasses");
            var classNames = new List<string>();
            foreach (int key in classKeys.Take(12))
            {
                string? name = (uint)key < (uint)itemClasses.RowCount
                    ? itemClasses.GetString(key, "Id")
                    : null;
                if (!string.IsNullOrWhiteSpace(name))
                    classNames.Add(name);
            }

            _output.WriteLine($"category id={id} text={text} classes={string.Join(',', classNames)}");
        }

        DumpEssenceMod(essenceMods, categories, itemClasses, mods, "Life");
        DumpEssenceMod(essenceMods, categories, itemClasses, mods, "Wrath");
        DumpEssenceMod(essenceMods, categories, itemClasses, mods, "Contempt");
    }

    private void DumpEssenceMod(
        LibDat2DatTable essenceMods,
        LibDat2DatTable categories,
        LibDat2DatTable itemClasses,
        LibDat2DatTable mods,
        string essenceFragment)
    {
        _output.WriteLine($"=== essence rows containing '{essenceFragment}' ===");
        int shown = 0;
        for (int row = 0; row < essenceMods.RowCount && shown < 6; row++)
        {
            int? essenceKey = essenceMods.GetForeignKey(row, "Essence");
            int? categoryKey = essenceMods.GetForeignKey(row, "TargetItemCategory");
            int? modKey = essenceMods.GetForeignKey(row, "Mod");
            int? displayModKey = essenceMods.GetForeignKey(row, "DisplayMod");

            if (modKey is null)
                continue;

            string? modId = (uint)modKey.Value < (uint)mods.RowCount ? mods.GetString(modKey.Value, "Id") : null;
            if (modId is null || !modId.Contains(essenceFragment, StringComparison.OrdinalIgnoreCase))
                continue;

            string? catId = categoryKey is int ck && (uint)ck < (uint)categories.RowCount
                ? categories.GetString(ck, "Id")
                : null;
            string? catText = categoryKey is int ck2 && (uint)ck2 < (uint)categories.RowCount
                ? categories.GetString(ck2, "Text")
                : null;

            IReadOnlyList<int> classKeys = categoryKey is int ck3 && (uint)ck3 < (uint)categories.RowCount
                ? categories.GetForeignKeyArray(ck3, "ItemClasses")
                : [];

            var classNames = new List<string>();
            foreach (int key in classKeys)
            {
                string? name = (uint)key < (uint)itemClasses.RowCount
                    ? itemClasses.GetString(key, "Id")
                    : null;
                if (!string.IsNullOrWhiteSpace(name))
                    classNames.Add(name);
            }

            string? displayId = displayModKey is int dm && (uint)dm < (uint)mods.RowCount
                ? mods.GetString(dm, "Id")
                : null;
            _output.WriteLine($"  mod={modId} display={displayId} category={catId} ({catText}) classes={string.Join(',', classNames)}");
            shown++;
        }
    }
}
