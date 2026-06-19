using Microsoft.Data.Sqlite;
using PoE.dlls.GameData;
using Xunit;

namespace PoE.Tests;

public class ModCacheDatabaseMetadataTests
{
    [Fact]
    public void Recreate_persists_mod_domain_item_kind_and_spawn_tags()
    {
        string dbPath = Path.Combine(Path.GetTempPath(), $"modcache_test_{Guid.NewGuid():N}.sqlite");
        try
        {
            using var database = new ModCacheDatabase(dbPath);
            var entries = new[]
            {
                new ModCatalogEntry(
                    "FlaskLife",
                    "#% increased Life Recovery from Flasks",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: 1,
                    ItemKind: ModItemKind.Flask,
                    SpawnTags: "default,flask"),
                new ModCatalogEntry(
                    "FeastingFiends",
                    "1 Added Passive Skill is Feasting Fiends",
                    false,
                    ModEldritchInfluence.None,
                    ModDomain: ModCatalogTagHelper.DomainClusterJewel,
                    ItemKind: ModItemKind.ClusterJewel,
                    SpawnTags: "affliction_minion_life,expansion_jewel_large"),
            };

            database.Recreate(entries);

            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT mod_domain, item_kind, spawn_tags
                FROM mod_suggestions
                WHERE mod_name = $name
                LIMIT 1;
                """;
            command.Parameters.AddWithValue("$name", "FlaskLife");
            using var reader = command.ExecuteReader();
            Assert.True(reader.Read());
            Assert.Equal(1, reader.GetInt32(0));
            Assert.Equal((int)ModItemKind.Flask, reader.GetInt32(1));
            Assert.Equal("default,flask", reader.GetString(2));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}
