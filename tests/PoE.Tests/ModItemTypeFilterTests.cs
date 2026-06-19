using PoE.dlls.GameData;
using Xunit;

namespace PoE.Tests;

public class ModItemTypeFilterTests
{
    [Fact]
    public void SearchItemOnly_claw_includes_weapon_shared_and_excludes_spell_only_mods()
    {
        string dbPath = Path.Combine(Path.GetTempPath(), $"modcache_type_{Guid.NewGuid():N}.sqlite");
        try
        {
            using var database = new ModCacheDatabase(dbPath);
            database.Recreate(
            [
                new ModCatalogEntry(
                    "CritMulti",
                    "#% to Global Critical Strike Multiplier",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: 1,
                    ItemKind: ModItemKind.Item,
                    SpawnTags: "amulet,weapon"),
                new ModCatalogEntry(
                    "SpellGems",
                    "# to Level of all Spell Skill Gems",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: 1,
                    ItemKind: ModItemKind.Item,
                    SpawnTags: "dagger,sceptre,staff,wand"),
                new ModCatalogEntry(
                    "ClawLocal",
                    "Adds # to # Physical Damage to Claws",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: 1,
                    ItemKind: ModItemKind.Item,
                    SpawnTags: "claw"),
                new ModCatalogEntry(
                    "DefaultLife",
                    "# to maximum Life",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: 1,
                    ItemKind: ModItemKind.Item,
                    SpawnTags: "default"),
            ]);

            IReadOnlyList<ModSuggestionItem> clawCrit = database.SearchItemOnly("critical", "claw", 20, 0);
            IReadOnlyList<ModSuggestionItem> clawSpell = database.SearchItemOnly("spell", "claw", 20, 0);
            IReadOnlyList<ModSuggestionItem> clawLife = database.SearchItemOnly("life", "claw", 20, 0);

            Assert.Contains(clawCrit, item => item.ModContent.Contains("Critical Strike Multiplier", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(clawSpell, item => item.ModContent.Contains("Spell Skill Gems", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(clawLife, item => item.ModContent.Contains("maximum Life", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(database.SearchItemOnly("physical", "claw", 20, 0),
                item => item.ModContent.Contains("Physical Damage to Claws", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void Build_catalog_claw_filter_includes_crit_multi_excludes_spell_gems_when_game_present()
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

        string dbPath = Path.Combine(Path.GetTempPath(), $"modcache_type_{Guid.NewGuid():N}.sqlite");
        try
        {
            using var database = new ModCacheDatabase(dbPath);
            database.Recreate(entries);

            IReadOnlyList<ModSuggestionItem> clawCrit = database.SearchItemOnly("Global Critical Strike Multiplier", "claw", 30, 0);
            IReadOnlyList<ModSuggestionItem> clawSpell = database.SearchItemOnly("Level of all Spell Skill Gems", "claw", 30, 0);

            Assert.NotEmpty(clawCrit);
            Assert.DoesNotContain(
                clawSpell,
                item => item.ModContent.Contains("Level of all Spell Skill Gems", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}
