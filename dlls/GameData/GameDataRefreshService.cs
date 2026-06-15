namespace PoE.dlls.GameData
{
    public sealed class GameDataRefreshService
    {
        private readonly ModCacheDatabase _database;
        private readonly PoESchemaProvider _schemaProvider = new();
        private int _refreshRunning;

        public GameDataRefreshService(ModCacheDatabase database) => _database = database;

        public bool IsRefreshRunning => Volatile.Read(ref _refreshRunning) != 0;

        public GameDataRefreshResult Refresh(string gameFolder)
        {
            if (Interlocked.CompareExchange(ref _refreshRunning, 1, 0) != 0)
            {
                GameDataLog.Info("Refresh skipped — another refresh is already running.");
                return GameDataRefreshResult.Fail("Mod cache refresh is already in progress.");
            }

            try
            {
                return RefreshCore(gameFolder);
            }
            finally
            {
                Interlocked.Exchange(ref _refreshRunning, 0);
            }
        }

        private GameDataRefreshResult RefreshCore(string gameFolder)
        {
            GameDataLog.Info("Starting mod cache refresh…");

            if (string.IsNullOrWhiteSpace(gameFolder) || !Directory.Exists(gameFolder))
                return Fail("Game folder is missing or invalid.");

            GameDataLog.Info($"Game folder: {gameFolder}");

            try
            {
                GameDataLog.Info("Downloading dat schema (schema.min.json)…");
                _schemaProvider.RefreshSchema();
                PoESchema schema = _schemaProvider.GetSchema();
                GameDataLog.Info($"Schema loaded ({schema.Tables.Count} tables).");
            }
            catch (Exception ex)
            {
                return Fail($"Could not download PoE dat schema: {ex.Message}", ex);
            }

            string schemaPath = _schemaProvider.SchemaPath;

            using GameArchiveSession archive = new(gameFolder);

            if (!ReadDatFile(archive, "data/mods.datc64", out byte[] modsBytes))
                return Fail("Could not read data/mods.datc64 from the game folder.");

            if (!ReadDatFile(archive, "data/tags.datc64", out byte[] tagsBytes))
                return Fail("Could not read data/tags.datc64 from the game folder.");

            if (!ReadDatFile(archive, "data/stats.datc64", out byte[] statsBytes))
                return Fail("Could not read data/stats.datc64 from the game folder.");

            IReadOnlyList<(string Path, byte[] Bytes)> statDescriptionFiles =
                PoEDataFileLocator.ReadStatDescriptionFiles(archive);
            if (statDescriptionFiles.Count == 0)
                return Fail("Could not read stat description files from Metadata/StatDescriptions.");

            foreach (var (path, bytes) in statDescriptionFiles)
                GameDataLog.Info($"Read {path} ({FormatBytes(bytes.Length)}).");

            HashSet<(string ModName, string ModContent)> uniqueEntries;
            try
            {
                uniqueEntries = ModCatalogBuilder.Build(
                    schemaPath,
                    modsBytes,
                    tagsBytes,
                    statsBytes,
                    statDescriptionFiles.Select(f => f.Bytes).ToList());
            }
            catch (Exception ex)
            {
                return Fail($"Failed to parse game mod data: {ex.Message}", ex);
            }

            if (uniqueEntries.Count == 0)
                return Fail("No item or map modifier names/descriptions matched the current filters.");

            int nameRows = uniqueEntries.Count(e => string.IsNullOrEmpty(e.ModContent));
            int contentRows = uniqueEntries.Count - nameRows;
            GameDataLog.Info($"Extracted {nameRows:N0} name rows and {contentRows:N0} stat-line rows ({uniqueEntries.Count:N0} total suggestions).");

            GameDataLog.Info("Writing SQLite mod cache…");
            try
            {
                _database.Recreate(uniqueEntries);
            }
            catch (Exception ex)
            {
                return Fail($"Could not write mod cache: {ex.Message}", ex);
            }

            GameDataLog.Info($"Mod cache saved to {GetCachePathHint()}.");

            int filesRead = 3 + statDescriptionFiles.Count;
            GameDataLog.Info($"Refresh complete — {uniqueEntries.Count} suggestions from {filesRead} file(s).");
            return GameDataRefreshResult.Ok(uniqueEntries.Count, filesRead);
        }

        private static bool ReadDatFile(GameArchiveSession archive, string path, out byte[] bytes)
        {
            GameDataLog.Info($"Reading {path}…");
            if (!PoEDataFileLocator.TryReadRequiredDatFile(archive, path, out bytes, out string? source, out string? error))
            {
                GameDataLog.Error($"Failed to read {path}.");
                if (!string.IsNullOrWhiteSpace(error))
                {
                    foreach (string line in error.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
                        GameDataLog.Error(line.TrimStart());
                }

                return false;
            }

            GameDataLog.Info($"Read {path} ({FormatBytes(bytes.Length)}) from {source}.");
            return true;
        }

        private static GameDataRefreshResult Fail(string message, Exception? ex = null)
        {
            if (ex is not null)
                GameDataLog.Error(message, ex);
            else
                GameDataLog.Error(message);

            return GameDataRefreshResult.Fail(message);
        }

        private static string FormatBytes(long bytes) =>
            bytes switch
            {
                < 1024 => $"{bytes} B",
                < 1024 * 1024 => $"{bytes / 1024.0:0.#} KB",
                _ => $"{bytes / (1024.0 * 1024.0):0.#} MB",
            };

        private static string GetCachePathHint()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PoE");
            return Path.Combine(folder, "modcache.sqlite");
        }
    }

    public readonly record struct GameDataRefreshResult(bool Success, string Message, int EntryCount, int FilesRead)
    {
        public static GameDataRefreshResult Ok(int entryCount, int filesRead) =>
            new(true, $"Loaded {entryCount} modifier names/descriptions from {filesRead} file(s).", entryCount, filesRead);

        public static GameDataRefreshResult Fail(string message) =>
            new(false, message, 0, 0);
    }
}
