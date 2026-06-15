using Microsoft.Data.Sqlite;

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
                    command.CommandText = "SELECT COUNT(1) FROM suggestions;";
                    return Convert.ToInt64(command.ExecuteScalar()) > 0;
                }
            }
        }

        public void Recreate(IEnumerable<(ModSuggestionKind Kind, string Value)> entries)
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
                    clear.CommandText = "DELETE FROM suggestions;";
                    clear.ExecuteNonQuery();
                }

                using var insert = _connection.CreateCommand();
                insert.Transaction = transaction;
                insert.CommandText = """
                    INSERT OR IGNORE INTO suggestions(kind, value)
                    VALUES ($kind, $value);
                    """;
                insert.Parameters.Add("$kind", SqliteType.Integer);
                insert.Parameters.Add("$value", SqliteType.Text);

                int written = 0;
                foreach (var (kind, value) in entries)
                {
                    if (string.IsNullOrWhiteSpace(value))
                        continue;

                    insert.Parameters["$kind"].Value = (int)kind;
                    insert.Parameters["$value"].Value = value.Trim();
                    insert.ExecuteNonQuery();
                    written++;

                    if (written % 5000 == 0)
                        GameDataLog.Info($"Writing mod cache… {written:N0} rows saved.");
                }

                transaction.Commit();
                GameDataLog.Info($"Mod cache write complete — {written:N0} rows.");
            }
        }

        public IReadOnlyList<string> Search(string prefix, int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                return [];

            lock (_sync)
            {
                EnsureOpen();
                using var command = _connection!.CreateCommand();
                command.CommandText = """
                    SELECT value
                    FROM suggestions
                    WHERE value LIKE $prefix ESCAPE '\'
                    ORDER BY kind, length(value), value
                    LIMIT $limit;
                    """;
                command.Parameters.AddWithValue("$prefix", EscapeLike(prefix) + "%");
                command.Parameters.AddWithValue("$limit", limit);

                var results = new List<string>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    results.Add(reader.GetString(0));

                return results;
            }
        }

        public int Count()
        {
            lock (_sync)
            {
                EnsureOpen();
                using var command = _connection!.CreateCommand();
                command.CommandText = "SELECT COUNT(1) FROM suggestions;";
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
                    CREATE TABLE IF NOT EXISTS suggestions (
                        kind INTEGER NOT NULL,
                        value TEXT NOT NULL,
                        PRIMARY KEY(kind, value)
                    );
                    CREATE INDEX IF NOT EXISTS idx_suggestions_value ON suggestions(value);
                    """;
                command.ExecuteNonQuery();
            }
        }

        private bool TableExists()
        {
            using var command = _connection!.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='suggestions';";
            return command.ExecuteScalar() is not null;
        }

        private static string EscapeLike(string text) => text.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

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
