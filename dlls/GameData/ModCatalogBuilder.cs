using System.Diagnostics;

namespace PoE.dlls.GameData
{
    internal static class ModCatalogBuilder
    {
        // ModDomains / ModGenerationType use indexing=1 in schema (ITEM=1, ATLAS=11, PREFIX=1, SUFFIX=2).
        private static readonly HashSet<int> AllowedDomains = [1, 11];
        private const int GenerationPrefix = 1;
        private const int GenerationSuffix = 2;

        private static readonly HashSet<string> UniqueOnlyTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "unique",
            "unique_map",
            "unique_league",
        };

        private static readonly HashSet<string> ItemOrMapTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "default",
            "ring",
            "amulet",
            "belt",
            "gloves",
            "boots",
            "helmet",
            "body_armour",
            "shield",
            "quiver",
            "weapon",
            "map",
            "jewel",
            "str_armour",
            "dex_armour",
            "int_armour",
            "str_dex_armour",
            "str_int_armour",
            "dex_int_armour",
            "str_dex_int_armour",
            "two_hand_weapon",
            "one_hand_weapon",
            "onehand",
            "twohand",
            "bow",
            "claw",
            "dagger",
            "rune_dagger",
            "wand",
            "staff",
            "warstaff",
            "axe",
            "mace",
            "sword",
        };

        private static readonly string[] StatKeyColumns =
        [
            "StatsKey1", "StatsKey2", "StatsKey3", "StatsKey4", "StatsKey5", "StatsKey6",
        ];

        public static HashSet<(ModSuggestionKind Kind, string Value)> Build(
            string schemaJsonPath,
            byte[] modsBytes,
            byte[] tagsBytes,
            byte[] statsBytes,
            IReadOnlyList<byte[]> statDescriptionFiles)
        {
            var mods = LibDat2DatTable.Load(modsBytes, "mods", schemaJsonPath);
            var tags = LibDat2DatTable.Load(tagsBytes, "tags", schemaJsonPath);
            var stats = LibDat2DatTable.Load(statsBytes, "stats", schemaJsonPath);

            GameDataLog.Info($"Loaded dat tables — Mods: {mods.RowCount} rows, Tags: {tags.RowCount}, Stats: {stats.RowCount}.");

            GameDataLog.Info("Parsing English stat descriptions…");
            var statTemplates = StatDescriptionParser.ParseEnglishTemplates(statDescriptionFiles);
            GameDataLog.Info($"Stat description templates: {statTemplates.Count}.");

            string?[] tagIds = BuildTagIds(tags);
            string?[] statIds = BuildStatIds(stats);

            var entries = new HashSet<(ModSuggestionKind Kind, string Value)>();
            int includedMods = 0;
            var scanTimer = Stopwatch.StartNew();
            long lastProgressMs = 0;

            GameDataLog.Info($"Scanning {mods.RowCount} mods (items & maps, prefix/suffix only)…");
            for (int row = 0; row < mods.RowCount; row++)
            {
                long elapsedMs = scanTimer.ElapsedMilliseconds;
                if (row > 0 && (row % 2000 == 0 || elapsedMs - lastProgressMs >= 5000))
                {
                    GameDataLog.Info($"Scanning mods… {row:N0} / {mods.RowCount:N0} ({100.0 * row / mods.RowCount:0.#}%), {includedMods:N0} matched so far.");
                    lastProgressMs = elapsedMs;
                }

                if (!ShouldIncludeMod(mods, tagIds, row))
                    continue;

                includedMods++;

                string? name = mods.GetString(row, "Name")?.Trim();
                if (!string.IsNullOrWhiteSpace(name))
                    entries.Add((ModSuggestionKind.ModName, name));

                string description = BuildDescription(mods, statIds, statTemplates, row);
                if (!string.IsNullOrWhiteSpace(description))
                    entries.Add((ModSuggestionKind.ModDescription, description));
            }

            GameDataLog.Info($"Scan complete — {includedMods:N0} mods matched filters, {entries.Count:N0} unique suggestions.");
            if (includedMods == 0)
                WriteFilterDebugDump(mods, tagIds);

            return entries;
        }

        private static void WriteFilterDebugDump(LibDat2DatTable mods, string?[] tagIds)
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PoE", "mod_filter_debug.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                var lines = new List<string>
                {
                    "No mods matched filters. Sample raw rows (Domain 1=ITEM, 11=ATLAS; Gen 1=PREFIX, 2=SUFFIX):",
                    "",
                };

                int samples = 0;
                for (int row = 0; row < mods.RowCount && samples < 40; row++)
                {
                    int domain = mods.GetInt32(row, "Domain");
                    int gen = mods.GetInt32(row, "GenerationType");
                    if (domain is not (1 or 11) || gen is not (1 or 2))
                        continue;

                    string? id = mods.GetString(row, "Id");
                    string? name = mods.GetString(row, "Name");
                    var spawnTags = mods.GetForeignKeyArray(row, "SpawnWeight_TagsKeys");
                    var spawnVals = mods.GetInt32Array(row, "SpawnWeight_Values");

                    lines.Add($"Row {row}: Id={id} Name={name} Domain={domain} Gen={gen}");
                    int n = Math.Min(spawnTags.Count, spawnVals.Count);
                    for (int i = 0; i < Math.Min(n, 6); i++)
                    {
                        string? tag = TagId(tagIds, spawnTags[i]);
                        lines.Add($"  spawn[{i}] tagKey={spawnTags[i]} tag={tag} weight={spawnVals[i]}");
                    }

                    lines.Add("");
                    samples++;
                }

                File.WriteAllLines(path, lines);
                GameDataLog.Info($"Wrote filter debug dump to {path}.");
            }
            catch (Exception ex)
            {
                GameDataLog.Error($"Could not write filter debug dump: {ex.Message}");
            }
        }

        private static bool ShouldIncludeMod(LibDat2DatTable mods, string?[] tagIds, int row)
        {
            int domain = mods.GetInt32(row, "Domain");
            if (!AllowedDomains.Contains(domain))
                return false;

            int generation = mods.GetInt32(row, "GenerationType");
            if (generation is not (GenerationPrefix or GenerationSuffix))
                return false;

            IReadOnlyList<int> spawnTags = mods.GetForeignKeyArray(row, "SpawnWeight_TagsKeys");
            IReadOnlyList<int> spawnValues = mods.GetInt32Array(row, "SpawnWeight_Values");
            if (spawnTags.Count == 0 || spawnValues.Count == 0)
                return false;

            bool hasItemOrMapSpawn = false;
            bool onlyUniqueSpawn = true;

            int count = Math.Min(spawnTags.Count, spawnValues.Count);
            for (int i = 0; i < count; i++)
            {
                if (spawnValues[i] <= 0)
                    continue;

                string? tag = TagId(tagIds, spawnTags[i]);
                if (string.IsNullOrWhiteSpace(tag))
                    continue;

                if (UniqueOnlyTags.Contains(tag))
                    continue;

                onlyUniqueSpawn = false;
                if (ItemOrMapTags.Contains(tag))
                    hasItemOrMapSpawn = true;
            }

            return hasItemOrMapSpawn && !onlyUniqueSpawn;
        }

        private static string BuildDescription(
            LibDat2DatTable mods,
            string?[] statIds,
            Dictionary<string, string> statTemplates,
            int row)
        {
            var parts = new List<string>(StatKeyColumns.Length);
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string column in StatKeyColumns)
            {
                int? statRow = mods.GetForeignKey(row, column);
                if (statRow is null or < 0)
                    continue;

                string? statId = StatId(statIds, statRow.Value);
                if (string.IsNullOrWhiteSpace(statId))
                    continue;

                if (!statTemplates.TryGetValue(statId, out string? template) || string.IsNullOrWhiteSpace(template))
                    continue;

                if (seen.Add(template))
                    parts.Add(template);
            }

            return string.Join(" / ", parts);
        }

        private static string?[] BuildTagIds(LibDat2DatTable tags)
        {
            var ids = new string?[tags.RowCount];
            for (int i = 0; i < tags.RowCount; i++)
                ids[i] = tags.GetString(i, "Id");
            return ids;
        }

        private static string?[] BuildStatIds(LibDat2DatTable stats)
        {
            var ids = new string?[stats.RowCount];
            for (int i = 0; i < stats.RowCount; i++)
                ids[i] = stats.GetString(i, "Id");
            return ids;
        }

        private static string? TagId(string?[] tagIds, int row) =>
            (uint)row < (uint)tagIds.Length ? tagIds[row] : null;

        private static string? StatId(string?[] statIds, int row) =>
            (uint)row < (uint)statIds.Length ? statIds[row] : null;
    }
}
