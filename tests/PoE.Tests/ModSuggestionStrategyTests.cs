using PoE.dlls.GameData;
using PoE.dlls.Gamble;
using Xunit;

namespace PoE.Tests;

public class ModSuggestionStrategyTests
{
    [Theory]
    [InlineData(GambleType.Map, typeof(MapModSuggestionStrategy))]
    [InlineData(GambleType.MapExalt, typeof(MapModSuggestionStrategy))]
    [InlineData(GambleType.MapT17, typeof(MapModSuggestionStrategy))]
    [InlineData(GambleType.Alt, typeof(ItemModSuggestionStrategy))]
    [InlineData(GambleType.Chaos, typeof(ItemModSuggestionStrategy))]
    [InlineData(GambleType.Harvest, typeof(ItemModSuggestionStrategy))]
    [InlineData(GambleType.Eldritch, typeof(EldritchModSuggestionStrategy))]
    public void Resolver_picks_strategy_by_gamble_type(GambleType type, Type expectedStrategyType)
    {
        IModSuggestionStrategy strategy = ModSuggestionStrategyResolver.For(type);
        Assert.IsType(expectedStrategyType, strategy);
    }

    [Fact]
    public void Item_search_excludes_map_tagged_rows_when_cache_present()
    {
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PoE", "modcache.sqlite");
        if (!File.Exists(dbPath))
            return;

        using var database = new ModCacheDatabase();
        if (!database.HasEntries)
            return;

        IReadOnlyList<ModSuggestionItem> results = database.SearchItemOnly("life", 20, 0);
        Assert.NotEmpty(results);

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(1)
            FROM mod_suggestions
            WHERE is_map = 1
              AND (
                instr(lower(mod_name), 'life') > 0
                OR instr(lower(mod_content), 'life') > 0
              );
            """;
        long mapHits = (long)(command.ExecuteScalar() ?? 0L);
        if (mapHits == 0)
            return;

        Assert.DoesNotContain(results, item =>
        {
            command.CommandText = "SELECT is_map FROM mod_suggestions WHERE mod_name = $name AND mod_content = $content LIMIT 1;";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("$name", item.ModName);
            command.Parameters.AddWithValue("$content", item.ModContent);
            return Convert.ToInt64(command.ExecuteScalar()) == 1;
        });
    }
}
