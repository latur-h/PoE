using PoE.dlls.GameData;
using Xunit;

namespace PoE.Tests;

public class EldritchPresenceModCatalogTests
{
    [Fact]
    public void Build_registers_presence_prefixed_eldritch_implicit_lines()
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

        int pinnacleRows = entries.Count(e =>
            e.EldritchInfluence != ModEldritchInfluence.None
            && e.ModContent.Contains("While a Pinnacle Atlas Boss is in your Presence", StringComparison.OrdinalIgnoreCase));
        int uniqueRows = entries.Count(e =>
            e.EldritchInfluence != ModEldritchInfluence.None
            && e.ModContent.Contains("While a Unique Enemy is in your Presence", StringComparison.OrdinalIgnoreCase));

        Assert.True(pinnacleRows > 0, "Expected pinnacle presence eldritch suggestion rows.");
        Assert.True(uniqueRows > 0, "Expected unique-enemy presence eldritch suggestion rows.");

        Assert.Contains(
            entries,
            e => e.EldritchInfluence == ModEldritchInfluence.SearingExarch
                 && string.Equals(
                     e.ModContent,
                     "While a Pinnacle Atlas Boss is in your Presence, #% increased Attack Speed",
                     StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            entries,
            e => e.EldritchInfluence == ModEldritchInfluence.SearingExarch
                 && string.Equals(
                     e.ModContent,
                     "While a Unique Enemy is in your Presence, #% increased Attack Speed",
                     StringComparison.OrdinalIgnoreCase));
    }
}
