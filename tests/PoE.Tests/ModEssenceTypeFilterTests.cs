using PoE.dlls.GameData;
using Xunit;

namespace PoE.Tests;

public class ModEssenceTypeFilterTests
{
    [Fact]
    public void SearchItemOnly_essence_mods_filter_by_item_type()
    {
        string dbPath = Path.Combine(Path.GetTempPath(), $"modcache_essence_{Guid.NewGuid():N}.sqlite");
        try
        {
            using var database = new ModCacheDatabase(dbPath);
            database.Recreate(
            [
                new ModCatalogEntry(
                    "Frigid",
                    "Adds Weapon Cold Damage",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: 1,
                    ItemKind: ModItemKind.Essence,
                    SpawnTags: "claw,weapon,one_hand_weapon"),
                new ModCatalogEntry(
                    "Frigid",
                    "Adds Armour Cold Damage",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: 1,
                    ItemKind: ModItemKind.Essence,
                    SpawnTags: "body_armour"),
                new ModCatalogEntry(
                    "Essence Life",
                    "Armour maximum Life",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: 1,
                    ItemKind: ModItemKind.Essence,
                    SpawnTags: "body_armour,helmet,gloves,boots,shield"),
                new ModCatalogEntry(
                    "Essence Life",
                    "Jewellery maximum Life",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: 1,
                    ItemKind: ModItemKind.Essence,
                    SpawnTags: "ring,amulet,belt"),
            ]);

            IReadOnlyList<ModSuggestionItem> clawCold = database.SearchItemOnly("cold damage", "claw", 20, 0);
            IReadOnlyList<ModSuggestionItem> bodyArmourCold = database.SearchItemOnly("cold damage", "body_armour", 20, 0);
            IReadOnlyList<ModSuggestionItem> clawLife = database.SearchItemOnly("maximum life", "claw", 20, 0);
            IReadOnlyList<ModSuggestionItem> bodyLife = database.SearchItemOnly("maximum life", "body_armour", 20, 0);
            IReadOnlyList<ModSuggestionItem> ringLife = database.SearchItemOnly("maximum life", "ring", 20, 0);

            Assert.Contains(clawCold, item => item.ModContent.Contains("Weapon Cold Damage", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(bodyArmourCold, item => item.ModContent.Contains("Weapon Cold Damage", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(bodyArmourCold, item => item.ModContent.Contains("Armour Cold Damage", StringComparison.OrdinalIgnoreCase));

            Assert.Empty(clawLife);
            Assert.Contains(bodyLife, item => item.ModContent.Contains("Armour maximum Life", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(ringLife, item => item.ModContent.Contains("Jewellery maximum Life", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(ringLife, item => item.ModContent.Contains("Armour maximum Life", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void Build_catalog_essence_mods_differ_by_item_type_when_game_present()
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

        Assert.True(PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/tags.datc64", out byte[] tagsBytes, out _, out _));
        Assert.True(PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/stats.datc64", out byte[] statsBytes, out _, out _));
        Assert.True(archive.TryReadGameFile("data/essences.datc64", out byte[] essencesBytes, out _, out _));

        HashSet<ModCatalogEntry> entries = ModCatalogBuilder.Build(
            schemaPath,
            modsBytes,
            tagsBytes,
            statsBytes,
            PoEDataFileLocator.ReadStatDescriptionFiles(archive),
            essencesBytes);

        Assert.Contains(
            entries,
            e => e.ItemKind == ModItemKind.Essence
                 && e.SpawnTags.Contains("claw", StringComparison.OrdinalIgnoreCase)
                 && e.ModContent.Contains("Cold Damage", StringComparison.OrdinalIgnoreCase));

        Assert.Contains(
            entries,
            e => e.ItemKind == ModItemKind.Essence
                 && e.SpawnTags.Contains("body_armour", StringComparison.OrdinalIgnoreCase)
                 && e.SpawnTags.Contains("claw", StringComparison.OrdinalIgnoreCase) == false
                 && e.ModContent.Contains("Cold Damage", StringComparison.OrdinalIgnoreCase));

        string dbPath = Path.Combine(Path.GetTempPath(), $"modcache_essence_{Guid.NewGuid():N}.sqlite");
        try
        {
            using var database = new ModCacheDatabase(dbPath);
            database.Recreate(entries);

            IReadOnlyList<ModSuggestionItem> clawCold = database.SearchItemOnly("Cold Damage", "claw", 30, 0);
            IReadOnlyList<ModSuggestionItem> bodyColdResist = database.SearchItemOnly("Cold Resistance", "body_armour", 30, 0);

            Assert.Contains(
                clawCold,
                item => item.ModContent.Contains("Cold Damage", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(
                clawCold,
                item => item.ModContent.Contains("Cold Resistance", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(
                bodyColdResist,
                item => item.ModContent.Contains("Cold Resistance", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}
