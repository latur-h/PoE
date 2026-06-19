using Microsoft.Data.Sqlite;
using PoE.dlls.Gamble;
using System.Text;

namespace PoE.dlls.GameData
{
    public sealed class ModCacheDatabase : IDisposable
    {
        private readonly string _dbPath;
        private readonly object _sync = new();
        private SqliteConnection? _connection;
        private bool? _hasMapTaggedEntries;

        public ModCacheDatabase()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PoE");
            Directory.CreateDirectory(folder);
            _dbPath = Path.Combine(folder, "modcache.sqlite");
        }

        internal ModCacheDatabase(string dbPath) => _dbPath = dbPath;

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

        public void Recreate(IEnumerable<ModCatalogEntry> entries)
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
                    INSERT OR IGNORE INTO mod_suggestions(mod_name, mod_content, is_map, eldritch_influence, mod_domain, item_kind, spawn_tags)
                    VALUES ($name, $content, $isMap, $eldritch, $domain, $itemKind, $spawnTags);
                    """;
                insert.Parameters.Add("$name", SqliteType.Text);
                insert.Parameters.Add("$content", SqliteType.Text);
                insert.Parameters.Add("$isMap", SqliteType.Integer);
                insert.Parameters.Add("$eldritch", SqliteType.Integer);
                insert.Parameters.Add("$domain", SqliteType.Integer);
                insert.Parameters.Add("$itemKind", SqliteType.Integer);
                insert.Parameters.Add("$spawnTags", SqliteType.Text);

                int written = 0;
                var tagAccumulator = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var entry in entries)
                {
                    if (string.IsNullOrWhiteSpace(entry.ModName))
                        continue;

                    insert.Parameters["$name"].Value = entry.ModName.Trim();
                    insert.Parameters["$content"].Value = entry.ModContent.Trim();
                    insert.Parameters["$isMap"].Value = entry.IsMap ? 1 : 0;
                    insert.Parameters["$eldritch"].Value = (int)entry.EldritchInfluence;
                    insert.Parameters["$domain"].Value = entry.ModDomain;
                    insert.Parameters["$itemKind"].Value = (int)entry.ItemKind;
                    insert.Parameters["$spawnTags"].Value = entry.SpawnTags ?? string.Empty;
                    insert.ExecuteNonQuery();
                    written++;

                    if (!entry.IsMap && entry.EldritchInfluence == ModEldritchInfluence.None)
                        CollectSpawnTags(entry.SpawnTags, tagAccumulator);

                    if (written % 5000 == 0)
                        GameDataLog.Info($"Writing mod cache… {written:N0} rows saved.");
                }

                if (tagAccumulator.Any(ModCatalogTagHelper.IsFlaskSpawnTag))
                    tagAccumulator.Add("flask");

                RebuildSpawnTagIndex(transaction, tagAccumulator);

                transaction.Commit();
                _hasMapTaggedEntries = null;
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
                    SELECT MIN(mod_name) AS mod_name, mod_content
                    FROM mod_suggestions
                    WHERE (
                        (mod_content <> '' {contentWordChecks})
                        OR (mod_content = '' {nameWordChecks})
                    )
                    GROUP BY CASE WHEN mod_content <> '' THEN mod_content ELSE mod_name END
                    ORDER BY
                        CASE WHEN mod_content <> '' THEN 0 ELSE 1 END,
                        CASE WHEN mod_content <> '' AND instr(lower(mod_content), lower($phrase)) > 0 THEN 0 ELSE 1 END,
                        CASE WHEN mod_content <> '' THEN instr(lower(mod_content), lower($w0)) ELSE instr(lower(MIN(mod_name)), lower($w0)) END,
                        length(mod_content),
                        MIN(mod_name),
                        mod_content
                    LIMIT $limit OFFSET $offset;
                    """;

                return ReadSuggestionItems(command);
            }
        }

        public IReadOnlyList<ModSuggestionItem> SearchItemOnly(
            string term,
            string? spawnTagFilter = null,
            int limit = 50,
            int offset = 0)
        {
            string trimmed = term.Trim();
            string[] words = ModSearchQuery.SplitWords(trimmed);
            if (words.Length == 0 || trimmed.Length < 2)
                return [];

            string? normalizedFilter = ModSpawnTagFilter.Normalize(spawnTagFilter);
            ModItemKind kind = ModItemKind.Item;
            bool useItemKind = normalizedFilter is not null
                && ModSpawnTagFilter.TryResolveItemKind(normalizedFilter, out kind);
            int itemKindFilter = useItemKind ? (int)kind : -1;
            IReadOnlyList<string> matchTags = ModItemTypeTags.GetMatchTags(normalizedFilter);

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

                string itemFilterSql = normalizedFilter is null
                    ? string.Empty
                    : ModSpawnTagFilter.BuildItemFilterSql(matchTags, "$itemKindFilter", useItemKind);

                if (normalizedFilter is not null)
                {
                    if (useItemKind)
                        command.Parameters.AddWithValue("$itemKindFilter", itemKindFilter);

                    for (int i = 0; i < matchTags.Count; i++)
                        command.Parameters.AddWithValue($"$mt{i}", matchTags[i]);
                }

                command.CommandText = $"""
                    SELECT MIN(mod_name) AS mod_name, mod_content
                    FROM mod_suggestions
                    WHERE is_map = 0
                      AND eldritch_influence = 0
                      {itemFilterSql}
                      AND (
                        (mod_content <> '' {contentWordChecks})
                        OR (mod_content = '' {nameWordChecks})
                      )
                    GROUP BY CASE WHEN mod_content <> '' THEN mod_content ELSE mod_name END
                    ORDER BY
                        CASE WHEN mod_content <> '' THEN 0 ELSE 1 END,
                        CASE WHEN mod_content <> '' AND instr(lower(mod_content), lower($phrase)) > 0 THEN 0 ELSE 1 END,
                        CASE WHEN mod_content <> '' THEN instr(lower(mod_content), lower($w0)) ELSE instr(lower(MIN(mod_name)), lower($w0)) END,
                        length(mod_content),
                        MIN(mod_name),
                        mod_content
                    LIMIT $limit OFFSET $offset;
                    """;

                return ReadSuggestionItems(command);
            }
        }

        public bool SpawnTagExists(string tag)
        {
            string? normalized = ModSpawnTagFilter.Normalize(tag);
            if (normalized is null)
                return true;

            if (!ModItemTypeTags.HasKnownProfile(normalized))
            {
                lock (_sync)
                {
                    EnsureOpen();
                    EnsureSpawnTagIndex();
                    using var command = _connection!.CreateCommand();
                    command.CommandText = "SELECT EXISTS(SELECT 1 FROM spawn_tag_index WHERE lower(tag) = lower($tag));";
                    command.Parameters.AddWithValue("$tag", normalized);
                    return Convert.ToInt64(command.ExecuteScalar()) > 0;
                }
            }

            return HasAnyModForItemTypeFilter(normalized);
        }

        private bool HasAnyModForItemTypeFilter(string normalizedFilter)
        {
            ModItemKind kind = ModItemKind.Item;
            bool useItemKind = ModSpawnTagFilter.TryResolveItemKind(normalizedFilter, out kind);
            IReadOnlyList<string> matchTags = ModItemTypeTags.GetMatchTags(normalizedFilter);
            string filterSql = ModSpawnTagFilter.BuildItemFilterSql(matchTags, "$itemKindFilter", useItemKind);
            if (string.IsNullOrEmpty(filterSql))
                return false;

            lock (_sync)
            {
                EnsureOpen();
                using var command = _connection!.CreateCommand();
                if (useItemKind)
                    command.Parameters.AddWithValue("$itemKindFilter", (int)kind);

                for (int i = 0; i < matchTags.Count; i++)
                    command.Parameters.AddWithValue($"$mt{i}", matchTags[i]);

                command.CommandText = $"""
                    SELECT EXISTS(
                        SELECT 1
                        FROM mod_suggestions
                        WHERE is_map = 0
                          AND eldritch_influence = 0
                          {filterSql}
                    );
                    """;
                return Convert.ToInt64(command.ExecuteScalar()) > 0;
            }
        }

        public IReadOnlyList<string> SearchSpawnTags(string term, int limit = 50) =>
            SearchSpawnTags(term, limit, eldritchArmourOnly: false);

        public IReadOnlyList<string> SearchSpawnTags(string term, int limit, bool eldritchArmourOnly)
        {
            string trimmed = term.Trim();
            if (trimmed.Length == 0)
                return [];

            lock (_sync)
            {
                EnsureOpen();
                EnsureSpawnTagIndex();

                var results = new List<string>();
                using var command = _connection!.CreateCommand();
                command.Parameters.AddWithValue("$term", trimmed);
                command.Parameters.AddWithValue("$limit", limit);

                command.CommandText = """
                    SELECT tag
                    FROM spawn_tag_index
                    WHERE instr(lower(tag), lower($term)) > 0
                    ORDER BY
                        CASE WHEN lower(tag) = lower($term) THEN 0 ELSE 1 END,
                        CASE WHEN lower(tag) LIKE lower($term) || '%' THEN 0 ELSE 1 END,
                        length(tag),
                        tag
                    LIMIT $limit;
                    """;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string tag = reader.GetString(0);
                    if (ModCatalogTagHelper.IsIndexableSpawnTag(tag))
                        results.Add(tag);
                }

                if (!eldritchArmourOnly)
                {
                    AppendAliasMatches(trimmed, results, limit);
                    AppendDisplayNameMatches(_connection!, trimmed, results, limit);
                }
                else
                {
                    AppendEldritchArmourMatches(trimmed, results, limit);
                }

                IEnumerable<string> ordered = results
                    .Where(tag => !eldritchArmourOnly || ModItemTypeTags.IsEldritchEligibleItemType(tag))
                    .OrderBy(tag => ModSpawnTagDisplay.GetDisplayName(tag), StringComparer.OrdinalIgnoreCase)
                    .Take(limit);

                return ordered.ToList();
            }
        }

        public bool SpawnTagExistsForEldritchArmour(string tag)
        {
            string? normalized = ModSpawnTagFilter.Normalize(tag);
            if (normalized is null)
                return true;

            if (!ModItemTypeTags.IsEldritchEligibleItemType(normalized))
                return false;

            return HasEldritchImplicitForItemType(normalized);
        }

        private bool HasEldritchImplicitForItemType(string normalizedFilter)
        {
            IReadOnlyList<string> matchTags = ModItemTypeTags.GetMatchTags(normalizedFilter);
            if (matchTags.Count == 0)
                return false;

            string filterSql = ModSpawnTagFilter.BuildItemFilterSql(matchTags, "$itemKindFilter", useItemKind: false);
            if (string.IsNullOrEmpty(filterSql))
                return false;

            lock (_sync)
            {
                EnsureOpen();
                using var command = _connection!.CreateCommand();
                for (int i = 0; i < matchTags.Count; i++)
                    command.Parameters.AddWithValue($"$mt{i}", matchTags[i]);

                command.CommandText = $"""
                    SELECT EXISTS(
                        SELECT 1
                        FROM mod_suggestions
                        WHERE eldritch_influence > 0
                          AND mod_content <> ''
                          {filterSql}
                    );
                    """;
                return Convert.ToInt64(command.ExecuteScalar()) > 0;
            }
        }

        private static void AppendEldritchArmourMatches(string term, List<string> results, int limit)
        {
            var seen = new HashSet<string>(results, StringComparer.OrdinalIgnoreCase);
            foreach (string tag in ModItemTypeTags.EldritchEligibleItemTypes)
            {
                if (results.Count >= limit)
                    return;

                if (!seen.Add(tag))
                    continue;

                if (ModSpawnTagDisplay.MatchesSearch(tag, term))
                    results.Add(tag);
            }
        }

        private static void AppendDisplayNameMatches(SqliteConnection connection, string term, List<string> results, int limit)
        {
            if (results.Count >= limit)
                return;

            var seen = new HashSet<string>(results, StringComparer.OrdinalIgnoreCase);
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT tag FROM spawn_tag_index ORDER BY tag;";
            using var reader = command.ExecuteReader();
            while (reader.Read() && results.Count < limit)
            {
                string tag = reader.GetString(0);
                if (!ModCatalogTagHelper.IsIndexableSpawnTag(tag))
                    continue;

                if (!seen.Add(tag))
                    continue;

                if (ModSpawnTagDisplay.MatchesSearch(tag, term))
                    results.Add(tag);
            }
        }

        private static void AppendAliasMatches(string term, List<string> results, int limit)
        {
            string lower = term.ToLowerInvariant();
            string[] aliases =
            [
                "cluster", "cluster_jewel", "flask", "jewel", "abyss_jewel",
                "life_flask", "mana_flask", "utility_flask", "hybrid_flask",
                "expansion_jewel_large", "expansion_jewel_medium", "expansion_jewel_small",
                "affliction_minion_damage",
                "abyss_jewel_summoner", "abyss_jewel_melee", "abyss_jewel_ranged", "abyss_jewel_caster",
                "searing_eye_jewel",
            ];
            foreach (string alias in aliases)
            {
                if (!alias.Contains(lower, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (results.Any(existing => string.Equals(existing, alias, StringComparison.OrdinalIgnoreCase)))
                    continue;

                results.Add(alias);
                if (results.Count >= limit)
                    return;
            }
        }

        private static void CollectSpawnTags(string? spawnTags, HashSet<string> accumulator)
        {
            if (string.IsNullOrWhiteSpace(spawnTags))
                return;

            foreach (string part in spawnTags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!string.IsNullOrWhiteSpace(part) && ModCatalogTagHelper.IsIndexableSpawnTag(part))
                    accumulator.Add(part.Trim());
            }
        }

        private void RebuildSpawnTagIndex(SqliteTransaction transaction, HashSet<string> tags)
        {
            using var clear = _connection!.CreateCommand();
            clear.Transaction = transaction;
            clear.CommandText = "DELETE FROM spawn_tag_index;";
            clear.ExecuteNonQuery();

            using var insert = _connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = "INSERT OR IGNORE INTO spawn_tag_index(tag) VALUES ($tag);";
            insert.Parameters.Add("$tag", SqliteType.Text);

            foreach (string tag in tags.OrderBy(t => t, StringComparer.OrdinalIgnoreCase))
            {
                insert.Parameters["$tag"].Value = tag;
                insert.ExecuteNonQuery();
            }
        }

        public IReadOnlyList<ModSuggestionItem> SearchEldritchImplicit(
            ModEldritchInfluence influence,
            string term,
            string? spawnTagFilter = null,
            int limit = 50,
            int offset = 0)
        {
            if (influence is ModEldritchInfluence.None)
                return [];

            string trimmed = term.Trim();
            string[] words = ModSearchQuery.SplitWords(trimmed);
            if (words.Length == 0 || trimmed.Length < 2)
                return [];

            string? normalizedFilter = ModSpawnTagFilter.Normalize(spawnTagFilter);
            if (normalizedFilter is not null && !ModItemTypeTags.IsEldritchEligibleItemType(normalizedFilter))
                return [];

            IReadOnlyList<string> matchTags = ModItemTypeTags.GetMatchTags(normalizedFilter);

            lock (_sync)
            {
                EnsureOpen();
                using var command = _connection!.CreateCommand();

                var contentWordChecks = new StringBuilder();
                for (int i = 0; i < words.Length; i++)
                {
                    string param = $"$w{i}";
                    contentWordChecks.Append($" AND instr(lower(mod_content), lower({param})) > 0");
                    command.Parameters.AddWithValue(param, words[i]);
                }

                command.Parameters.AddWithValue("$phrase", trimmed);
                command.Parameters.AddWithValue("$limit", limit);
                command.Parameters.AddWithValue("$offset", offset);
                command.Parameters.AddWithValue("$eldritch", (int)influence);

                string itemFilterSql = normalizedFilter is null
                    ? string.Empty
                    : ModSpawnTagFilter.BuildItemFilterSql(matchTags, "$itemKindFilter", useItemKind: false);

                if (normalizedFilter is not null)
                {
                    for (int i = 0; i < matchTags.Count; i++)
                        command.Parameters.AddWithValue($"$mt{i}", matchTags[i]);
                }

                command.CommandText = $"""
                    SELECT MIN(mod_name) AS mod_name, mod_content
                    FROM mod_suggestions
                    WHERE eldritch_influence = $eldritch
                      AND mod_content <> ''
                      {itemFilterSql}
                      AND (1 = 1 {contentWordChecks})
                    GROUP BY mod_content
                    ORDER BY
                        CASE WHEN instr(lower(mod_content), lower($phrase)) > 0 THEN 0 ELSE 1 END,
                        instr(lower(mod_content), lower($w0)),
                        length(mod_content),
                        mod_content
                    LIMIT $limit OFFSET $offset;
                    """;

                return ReadSuggestionItems(command);
            }
        }

        public bool TryFindModTemplate(string skeleton, GambleType gambleType, out string template)
        {
            template = string.Empty;
            if (string.IsNullOrWhiteSpace(skeleton))
                return false;

            string eldritchFilter = gambleType == GambleType.Eldritch
                ? "eldritch_influence > 0"
                : "eldritch_influence = 0";

            lock (_sync)
            {
                EnsureOpen();
                using var command = _connection!.CreateCommand();
                command.CommandText = $"""
                    SELECT mod_content
                    FROM mod_suggestions
                    WHERE mod_content <> ''
                      AND is_map = 0
                      AND {eldritchFilter}
                      AND lower(mod_content) = lower($skeleton)
                    LIMIT 1;
                    """;
                command.Parameters.AddWithValue("$skeleton", skeleton.Trim());

                object? result = command.ExecuteScalar();
                if (result is not string content || string.IsNullOrWhiteSpace(content))
                    return false;

                template = content;
                return true;
            }
        }

        public bool HasEldritchTaggedEntries(ModEldritchInfluence influence)
        {
            if (influence is ModEldritchInfluence.None)
                return false;

            lock (_sync)
            {
                EnsureOpen();
                using var command = _connection!.CreateCommand();
                command.CommandText = "SELECT EXISTS(SELECT 1 FROM mod_suggestions WHERE eldritch_influence = $eldritch);";
                command.Parameters.AddWithValue("$eldritch", (int)influence);
                return Convert.ToInt64(command.ExecuteScalar()) > 0;
            }
        }

        public IReadOnlyList<ModSuggestionItem> SearchMapGrouped(string term, int limit = 50, int offset = 0)
        {
            if (!HasMapTaggedEntries())
                return [];

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
                var pickContentWordChecks = new StringBuilder();
                for (int i = 0; i < words.Length; i++)
                {
                    string param = $"$w{i}";
                    contentWordChecks.Append($" AND instr(lower(mod_content), lower({param})) > 0");
                    nameWordChecks.Append($" AND instr(lower(mod_name), lower({param})) > 0");
                    pickContentWordChecks.Append($" AND instr(lower(s.mod_content), lower({param})) > 0");
                    command.Parameters.AddWithValue(param, words[i]);
                }

                command.Parameters.AddWithValue("$phrase", trimmed);
                command.Parameters.AddWithValue("$limit", limit);
                command.Parameters.AddWithValue("$offset", offset);

                command.CommandText = $"""
                    SELECT
                        grouped.mod_name,
                        COALESCE((
                            SELECT s.mod_content
                            FROM mod_suggestions s
                            WHERE s.mod_name = grouped.mod_name
                              AND s.is_map = 1
                              AND s.eldritch_influence = 0
                              AND s.mod_content <> ''
                            ORDER BY
                                CASE WHEN instr(lower(s.mod_content), lower($phrase)) > 0 THEN 0 ELSE 1 END,
                                CASE WHEN (1 = 1 {pickContentWordChecks}) THEN 0 ELSE 1 END,
                                length(s.mod_content),
                                s.mod_content
                            LIMIT 1
                        ), '') AS mod_content
                    FROM (
                        SELECT mod_name
                        FROM mod_suggestions
                        WHERE is_map = 1
                          AND eldritch_influence = 0
                          AND (
                            (1 = 1 {nameWordChecks})
                            OR (mod_content <> '' {contentWordChecks})
                          )
                        GROUP BY mod_name
                        ORDER BY
                            CASE WHEN instr(lower(mod_name), lower($phrase)) > 0 THEN 0 ELSE 1 END,
                            instr(lower(mod_name), lower($w0)),
                            mod_name
                        LIMIT $limit OFFSET $offset
                    ) grouped;
                    """;

                return ReadSuggestionItems(command);
            }
        }

        private static List<ModSuggestionItem> ReadSuggestionItems(SqliteCommand command)
        {
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

        public bool HasMapTaggedEntries()
        {
            lock (_sync)
            {
                if (_hasMapTaggedEntries is bool cached)
                    return cached;

                EnsureOpen();
                using var command = _connection!.CreateCommand();
                command.CommandText = "SELECT EXISTS(SELECT 1 FROM mod_suggestions WHERE is_map = 1);";
                _hasMapTaggedEntries = Convert.ToInt64(command.ExecuteScalar()) > 0;
                return _hasMapTaggedEntries.Value;
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
                        is_map INTEGER NOT NULL DEFAULT 0,
                        eldritch_influence INTEGER NOT NULL DEFAULT 0,
                        mod_domain INTEGER NOT NULL DEFAULT 0,
                        item_kind INTEGER NOT NULL DEFAULT 0,
                        spawn_tags TEXT NOT NULL DEFAULT '',
                        PRIMARY KEY (mod_name, mod_content, eldritch_influence)
                    );
                    CREATE INDEX IF NOT EXISTS idx_mod_suggestions_name ON mod_suggestions(mod_name);
                    CREATE INDEX IF NOT EXISTS idx_mod_suggestions_content ON mod_suggestions(mod_content);
                    CREATE INDEX IF NOT EXISTS idx_mod_suggestions_map ON mod_suggestions(is_map);
                    CREATE INDEX IF NOT EXISTS idx_mod_suggestions_eldritch ON mod_suggestions(eldritch_influence);
                    CREATE INDEX IF NOT EXISTS idx_mod_suggestions_item_kind ON mod_suggestions(item_kind);
                    CREATE TABLE IF NOT EXISTS spawn_tag_index (
                        tag TEXT PRIMARY KEY
                    );
                    CREATE INDEX IF NOT EXISTS idx_spawn_tag_index_tag ON spawn_tag_index(tag);
                    """;
                command.ExecuteNonQuery();
            }
            else if (!ModSuggestionsTableExists())
            {
                MigrateLegacySchema();
            }
            else
            {
                EnsureMapColumn();
                EnsureEldritchColumn();
                EnsureMetadataColumns();
                EnsureSpawnTagIndex();
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
                    is_map INTEGER NOT NULL DEFAULT 0,
                    PRIMARY KEY (mod_name, mod_content)
                );
                CREATE INDEX IF NOT EXISTS idx_mod_suggestions_name ON mod_suggestions(mod_name);
                CREATE INDEX IF NOT EXISTS idx_mod_suggestions_content ON mod_suggestions(mod_content);
                CREATE INDEX IF NOT EXISTS idx_mod_suggestions_map ON mod_suggestions(is_map);
                """;
            command.ExecuteNonQuery();
        }

        private void EnsureMapColumn()
        {
            if (HasColumn("is_map"))
                return;

            using var alter = _connection!.CreateCommand();
            alter.CommandText = """
                ALTER TABLE mod_suggestions ADD COLUMN is_map INTEGER NOT NULL DEFAULT 0;
                CREATE INDEX IF NOT EXISTS idx_mod_suggestions_map ON mod_suggestions(is_map);
                """;
            alter.ExecuteNonQuery();
            _hasMapTaggedEntries = null;
            GameDataLog.Info("Mod cache upgraded with is_map column — refresh mod list to populate map tags.");
        }

        private void EnsureEldritchColumn()
        {
            if (HasColumn("eldritch_influence"))
                return;

            using var alter = _connection!.CreateCommand();
            alter.CommandText = """
                ALTER TABLE mod_suggestions ADD COLUMN eldritch_influence INTEGER NOT NULL DEFAULT 0;
                CREATE INDEX IF NOT EXISTS idx_mod_suggestions_eldritch ON mod_suggestions(eldritch_influence);
                """;
            alter.ExecuteNonQuery();
            GameDataLog.Info("Mod cache upgraded with eldritch_influence column — refresh mod list to populate eldritch implicits.");
        }

        private void EnsureMetadataColumns()
        {
            bool added = false;

            if (!HasColumn("mod_domain"))
            {
                using var alter = _connection!.CreateCommand();
                alter.CommandText = "ALTER TABLE mod_suggestions ADD COLUMN mod_domain INTEGER NOT NULL DEFAULT 0;";
                alter.ExecuteNonQuery();
                added = true;
            }

            if (!HasColumn("item_kind"))
            {
                using var alter = _connection!.CreateCommand();
                alter.CommandText = """
                    ALTER TABLE mod_suggestions ADD COLUMN item_kind INTEGER NOT NULL DEFAULT 0;
                    CREATE INDEX IF NOT EXISTS idx_mod_suggestions_item_kind ON mod_suggestions(item_kind);
                    """;
                alter.ExecuteNonQuery();
                added = true;
            }

            if (!HasColumn("spawn_tags"))
            {
                using var alter = _connection!.CreateCommand();
                alter.CommandText = "ALTER TABLE mod_suggestions ADD COLUMN spawn_tags TEXT NOT NULL DEFAULT '';";
                alter.ExecuteNonQuery();
                added = true;
            }

            if (added)
                GameDataLog.Info("Mod cache upgraded with mod_domain, item_kind, and spawn_tags — refresh mod list to populate metadata.");
        }

        private void EnsureSpawnTagIndex()
        {
            using var command = _connection!.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='spawn_tag_index';";
            if (command.ExecuteScalar() is not null)
                return;

            using var create = _connection.CreateCommand();
            create.CommandText = """
                CREATE TABLE IF NOT EXISTS spawn_tag_index (
                    tag TEXT PRIMARY KEY
                );
                CREATE INDEX IF NOT EXISTS idx_spawn_tag_index_tag ON spawn_tag_index(tag);
                """;
            create.ExecuteNonQuery();
            GameDataLog.Info("Mod cache upgraded with spawn_tag_index — refresh mod list to populate item-type filters.");
        }

        private bool HasColumn(string columnName)
        {
            using var command = _connection!.CreateCommand();
            command.CommandText = "PRAGMA table_info(mod_suggestions);";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
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
