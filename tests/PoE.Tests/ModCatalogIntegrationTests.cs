using PoE.dlls.GameData;
using Xunit;

namespace PoE.Tests;

public class ModCatalogIntegrationTests
{
    [Fact]
    public void Build_tags_map_mods_when_game_folder_present()
    {
        const string gameFolder = @"L:\PoE";
        if (!Directory.Exists(gameFolder))
            return;

        string schemaPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PoE",
            "schema.min.json");
        Assert.True(File.Exists(schemaPath), "schema.min.json missing — run Refresh mod list once in the app.");

        using GameArchiveSession archive = new(gameFolder);

        Assert.True(
            PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/mods.datc64", out byte[] modsBytes, out _, out _));
        Assert.True(
            PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/tags.datc64", out byte[] tagsBytes, out _, out _));
        Assert.True(
            PoEDataFileLocator.TryReadRequiredDatFile(archive, "data/stats.datc64", out byte[] statsBytes, out _, out _));

        IReadOnlyList<(string Path, byte[] Bytes)> statFiles = PoEDataFileLocator.ReadStatDescriptionFiles(archive);
        Assert.NotEmpty(statFiles);

        HashSet<ModCatalogEntry> entries = ModCatalogBuilder.Build(
            schemaPath,
            modsBytes,
            tagsBytes,
            statsBytes,
            statFiles);

        int mapRows = entries.Count(e => e.IsMap);
        int mapNames = entries.Where(e => e.IsMap).Select(e => e.ModName).Distinct(StringComparer.OrdinalIgnoreCase).Count();

        Assert.True(entries.Count > 0, "No mod suggestions extracted.");
        Assert.True(mapRows > 0, $"Expected map-tagged suggestion rows, got 0 of {entries.Count} (unique map names: {mapNames}).");

        HashSet<string> mapNameSet = entries
            .Where(e => e.IsMap)
            .Select(e => e.ModName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Contains("Punishing", mapNameSet);
        Assert.Contains("Mirrored", mapNameSet);
        Assert.DoesNotContain(
            entries,
            e => e.IsMap && e.ModContent.Contains("wielding a Staff", StringComparison.OrdinalIgnoreCase));

        int exarchRows = entries.Count(e => e.EldritchInfluence == ModEldritchInfluence.SearingExarch);
        int eaterRows = entries.Count(e => e.EldritchInfluence == ModEldritchInfluence.EaterOfWorlds);
        Assert.True(exarchRows > 0, $"Expected Searing Exarch implicit rows, got 0 of {entries.Count}.");
        Assert.True(eaterRows > 0, $"Expected Eater of Worlds implicit rows, got 0 of {entries.Count}.");
        Assert.Contains(
            entries,
            e => e.EldritchInfluence == ModEldritchInfluence.SearingExarch
                 && e.ModContent.Contains("Action Speed", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            entries,
            e => e.EldritchInfluence == ModEldritchInfluence.EaterOfWorlds
                 && e.ModContent.Contains("Arcane Surge", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            entries,
            e => e.EldritchInfluence == ModEldritchInfluence.SearingExarch
                 && e.ModContent.Contains("While a Pinnacle Atlas Boss is in your Presence", StringComparison.OrdinalIgnoreCase)
                 && e.ModContent.Contains("Attack Speed", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            entries,
            e => e.EldritchInfluence == ModEldritchInfluence.SearingExarch
                 && e.ModContent.Contains("While a Unique Enemy is in your Presence", StringComparison.OrdinalIgnoreCase)
                 && e.ModContent.Contains("Attack Speed", StringComparison.OrdinalIgnoreCase));

        int flaskRows = entries.Count(e => e.ItemKind == ModItemKind.Flask);
        int jewelRows = entries.Count(e => e.ItemKind == ModItemKind.Jewel);
        int clusterRows = entries.Count(e => e.ItemKind == ModItemKind.ClusterJewel);
        int abyssRows = entries.Count(e => e.ItemKind == ModItemKind.AbyssJewel);
        Assert.True(flaskRows > 0, $"Expected flask suggestion rows, got 0 of {entries.Count}.");
        Assert.True(jewelRows > 0, $"Expected jewel suggestion rows, got 0 of {entries.Count}.");
        Assert.True(clusterRows > 0, $"Expected cluster jewel suggestion rows, got 0 of {entries.Count}.");
        Assert.True(abyssRows > 0, $"Expected abyss jewel suggestion rows, got 0 of {entries.Count}.");

        Assert.Contains(
            entries,
            e => e.ItemKind == ModItemKind.Flask
                 && (e.SpawnTags.Contains("utility_flask", StringComparison.OrdinalIgnoreCase)
                     || e.SpawnTags.Contains("life_flask", StringComparison.OrdinalIgnoreCase)
                     || e.SpawnTags.Contains("mana_flask", StringComparison.OrdinalIgnoreCase)));

        Assert.Contains(
            entries,
            e => e.ItemKind == ModItemKind.Flask
                 && e.ModContent.Contains("reduced Effect of Curses", StringComparison.OrdinalIgnoreCase));

        Assert.Contains(
            entries,
            e => e.ItemKind == ModItemKind.Flask
                 && e.ModContent.Contains("increased effect", StringComparison.OrdinalIgnoreCase)
                 && !e.ModContent.Contains("to this value when used", StringComparison.OrdinalIgnoreCase));

        Assert.Contains(
            entries,
            e => e.ItemKind == ModItemKind.Flask
                 && e.ModContent.Contains("reduced Duration", StringComparison.OrdinalIgnoreCase)
                 && !e.ModContent.Contains("to this value when used", StringComparison.OrdinalIgnoreCase));

        Assert.DoesNotContain(
            entries,
            e => e.ItemKind == ModItemKind.Flask
                 && e.ModContent.Contains("to this value when used", StringComparison.OrdinalIgnoreCase));

        Assert.Contains(
            entries,
            e => e.ItemKind == ModItemKind.ClusterJewel
                 && (e.ModContent.Contains("Feasting Fiends", StringComparison.OrdinalIgnoreCase)
                     || e.ModName.Contains("Feasting Fiends", StringComparison.OrdinalIgnoreCase)));

        Assert.Contains(
            entries,
            e => e.ItemKind == ModItemKind.ClusterJewel
                 && !string.IsNullOrWhiteSpace(e.SpawnTags)
                 && e.SpawnTags.Contains("affliction_", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Refresh_populates_map_tags_in_local_cache()
    {
        if (Environment.GetEnvironmentVariable("POE_REFRESH_MOD_CACHE") != "1")
            return;

        const string gameFolder = @"L:\PoE";
        if (!Directory.Exists(gameFolder))
            return;

        var service = new GameDataRefreshService(new ModCacheDatabase());
        GameDataRefreshResult result = service.Refresh(gameFolder);
        Assert.True(result.Success, result.Message);
        Assert.True(new ModCacheDatabase().HasMapTaggedEntries());
    }
}
