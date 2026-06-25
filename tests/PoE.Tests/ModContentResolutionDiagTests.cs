using PoE.dlls.GameData;
using Xunit;
using Xunit.Abstractions;

namespace PoE.Tests;

public class ModContentResolutionDiagTests
{
    private readonly ITestOutputHelper _output;

    public ModContentResolutionDiagTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Diagnose_mod_content_resolution_when_game_present()
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
        if (!PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/tags.datc64", out byte[] tagsBytes, out _, out _))
            return;
        if (!PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/stats.datc64", out byte[] statsBytes, out _, out _))
            return;

        IReadOnlyList<(string Path, byte[] Bytes)> statFiles = PoEDataFileLocator.ReadStatDescriptionFiles(archive);
        HashSet<ModCatalogEntry> entries = ModCatalogBuilder.Build(
            schemaPath,
            modsBytes,
            tagsBytes,
            statsBytes,
            statFiles);

        int total = entries.Count;
        int withContent = entries.Count(e => !string.IsNullOrWhiteSpace(e.ModContent));
        int nameOnly = total - withContent;

        _output.WriteLine($"total={total} withContent={withContent} nameOnly={nameOnly}");
        _output.WriteLine("sample with content:");
        foreach (ModCatalogEntry e in entries.Where(x => !string.IsNullOrWhiteSpace(x.ModContent)).Take(5))
            _output.WriteLine($"  {e.ModName} | {e.ModContent}");

        _output.WriteLine("english life samples:");
        foreach (ModCatalogEntry e in entries.Where(x => x.ModContent.Contains("Life", StringComparison.OrdinalIgnoreCase)).Take(5))
            _output.WriteLine($"  {e.ModName} | {e.ModContent}");

        Assert.Contains(entries, e => e.ModContent.Contains("maximum Life", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(entries, e => e.ModContent.Contains("to Strength", StringComparison.OrdinalIgnoreCase));

        _output.WriteLine("sample name-only:");
        foreach (ModCatalogEntry e in entries.Where(x => string.IsNullOrWhiteSpace(x.ModContent)).Take(10))
            _output.WriteLine($"  {e.ModName}");

        Assert.True(withContent > nameOnly, $"Expected more content rows ({withContent}) than name-only ({nameOnly}).");
    }
}
