using Microsoft.Data.Sqlite;
using System.Text;

namespace PoE.dlls.GameData
{
    public sealed class ModCacheDatabase : IDisposable
    {
        private readonly string _dbPath;
        private readonly object _sync = new();
        private SqliteConnection? _connection;

        public ModCacheDatabase()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PoE");
            Directory.CreateDirectory(folder);
            _dbPath = Path.Combine(folder, "modcache.sqlite");
        }

        public bool HasEntries
        {
            get
            {
                lock (_sync)
                {
                    EnsureOpen();
                    using var command = _connection!.CreateCommand();
                    command.CommandText = "SELECT COUNT(1) FROM mod_suggestions;";
                    return Convert.ToInt64(command.ExecuteScalar()) > 0;
                }
            }
        }

        public void Recreate(IEnumerable<(string ModName, string ModContent)> entries)
        {
            lock (_sync)
            {
                Close();
                SqliteConnection.ClearAllPools();

                EnsureOpen(createFresh: true);

                using var transaction = _connection!.BeginTransaction();

                using (var clear = _connection.CreateCommand())
                {
                    clear.Transaction = transaction;
                    clear.CommandText = "DELETE FROM mod_suggestions;";
                    clear.ExecuteNonQuery();
                }

                using var insert = _connection.CreateCommand();
                insert.Transaction = transaction;
                insert.CommandText = """
                    INSERT OR IGNORE INTO mod_suggestions(mod_name, mod_content)
                    VALUES ($name, $content);
                    """;
                insert.Parameters.Add("$name", SqliteType.Text);
                insert.Parameters.Add("$content", SqliteType.Text);

                int written = 0;
                foreach (var (modName, modContent) in entries)
                {
                    if (string.IsNullOrWhiteSpace(modName))
                        continue;

                    insert.Parameters["$name"].Value = modName.Trim();
                    insert.Parameters["$content"].Value = modContent.Trim();
                    insert.ExecuteNonQuery();
                    written++;

                    if (written % 5000 == 0)
                        GameDataLog.Info($"Writing mod cache… {written:N0} rows saved.");
                }

                transaction.Commit();
                GameDataLog.Info($"Mod cache write complete — {written:N0} rows.");
            }
        }

        public IReadOnlyList<ModSuggestionItem> Search(string term, int limit = 50, int offset = 0)
        {
            string trimmed = term.Trim();
            string[] words = ModSearchQuery.SplitWords(trimmed);
            if (words.Length == 0 || trimmed.Length < 2)
                return [];

            lock (_sync)
            {
                EnsureOpen();
                using var command = _connection!.CreateCommand();

                var contentWordChecks = new StringBuilder();
                var nameWordChecks = new StringBuilder();
                for (int i = 0; i < words.Length; i++)
                {
                    string param = $"$w{i}";
                    contentWordChecks.Append($" AND instr(lower(mod_content), lower({param})) > 0");
                    nameWordChecks.Append($" AND instr(lower(mod_name), lower({param})) > 0");
                    command.Parameters.AddWithValue(param, words[i]);
                }

                command.Parameters.AddWithValue("$phrase", trimmed);
                command.Parameters.AddWithValue("$limit", limit);
                command.Parameters.AddWithValue("$offset", offset);

                command.CommandText = $"""
                    SELECT mod_name, mod_content
                    FROM mod_suggestions
                    WHERE (
                        (mod_content <> '' {contentWordChecks})
                        OR (mod_content = '' {nameWordChecks})
                    )
                    ORDER BY
                        CASE WHEN mod_content <> '' THEN 0 ELSE 1 END,
                        CASE WHEN mod_content <> '' AND instr(lower(mod_content), lower($phrase)) > 0 THEN 0 ELSE 1 END,
                        CASE WHEN mod_content <> '' THEN instr(lower(mod_content), lower($w0)) ELSE instr(lower(mod_name), lower($w0)) END,
                        length(mod_content),
                        mod_name,
                        mod_content
                    LIMIT $limit OFFSET $offset;
                    """;

                var results = new List<ModSuggestionItem>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new ModSuggestionItem
                    {
                        ModName = reader.GetString(0),
                        ModContent = reader.GetString(1),
                    });
                }

                return results;
            }
        }

        public int Count()
        {
            lock (_sync)
            {
                EnsureOpen();
                using var command = _connection!.CreateCommand();
                command.CommandText = "SELECT COUNT(1) FROM mod_suggestions;";
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private void EnsureOpen(bool createFresh = false)
        {
            if (_connection is not null)
                return;

            _connection = new SqliteConnection($"Data Source={_dbPath}");
            _connection.Open();

            if (createFresh || !TableExists())
            {
                using var command = _connection.CreateCommand();
                command.CommandText = """
                    DROP TABLE IF EXISTS suggestions;
                    CREATE TABLE IF NOT EXISTS mod_suggestions (
                        mod_name TEXT NOT NULL,
                        mod_content TEXT NOT NULL,
                        PRIMARY KEY (mod_name, mod_content)
                    );
                    CREATE INDEX IF NOT EXISTS idx_mod_suggestions_name ON mod_suggestions(mod_name);
                    CREATE INDEX IF NOT EXISTS idx_mod_suggestions_content ON mod_suggestions(mod_content);
                    """;
                command.ExecuteNonQuery();
            }
            else if (!ModSuggestionsTableExists())
            {
                MigrateLegacySchema();
            }
        }

        private bool TableExists()
        {
            using var command = _connection!.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name IN ('mod_suggestions', 'suggestions');";
            return command.ExecuteScalar() is not null;
        }

        private bool ModSuggestionsTableExists()
        {
            using var command = _connection!.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='mod_suggestions';";
            return command.ExecuteScalar() is not null;
        }

        private void MigrateLegacySchema()
        {
            using var command = _connection!.CreateCommand();
            command.CommandText = """
                DROP TABLE IF EXISTS suggestions;
                CREATE TABLE IF NOT EXISTS mod_suggestions (
                    mod_name TEXT NOT NULL,
                    mod_content TEXT NOT NULL,
                    PRIMARY KEY (mod_name, mod_content)
                );
                CREATE INDEX IF NOT EXISTS idx_mod_suggestions_name ON mod_suggestions(mod_name);
                CREATE INDEX IF NOT EXISTS idx_mod_suggestions_content ON mod_suggestions(mod_content);
                """;
            command.ExecuteNonQuery();
        }

        private void Close()
        {
            if (_connection is null)
                return;

            _connection.Dispose();
            _connection = null;
            SqliteConnection.ClearAllPools();
        }

        public void Dispose() => Close();
    }
}
