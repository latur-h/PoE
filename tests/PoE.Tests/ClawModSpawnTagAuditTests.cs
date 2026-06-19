using PoE.dlls.GameData;
using Xunit;
using Xunit.Abstractions;

namespace PoE.Tests;

public class ClawModSpawnTagAuditTests
{
    private readonly ITestOutputHelper _output;

    public ClawModSpawnTagAuditTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Audit_spawn_tags_for_claw_relevant_and_wrong_mods()
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

        string[] needles =
        [
            "maximum Life",
            "Minions have",
            "Global Critical Strike Multiplier",
            "Adds # to # Physical Damage",
            "Life per Enemy Killed",
            "Level of Socketed Gems",
            "Elemental Damage with Attack Skills",
        ];

        foreach (string needle in needles)
        {
            _output.WriteLine($"=== {needle} ===");
            foreach (ModCatalogEntry e in entries
                         .Where(x => !x.IsMap && x.EldritchInfluence == ModEldritchInfluence.None)
                         .Where(x => x.ModContent.Contains(needle, StringComparison.OrdinalIgnoreCase)
                                     || x.ModName.Contains(needle, StringComparison.OrdinalIgnoreCase))
                         .Take(8))
            {
                bool clawMatch = ModItemTypeTags.ModMatchesItemType(e.SpawnTags, "claw");
                _output.WriteLine($"  claw={clawMatch} tags={e.SpawnTags} | {e.ModContent}");
            }
        }
    }
}
