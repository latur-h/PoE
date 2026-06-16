using System.Diagnostics;

namespace PoE.dlls.GameData
{
    internal static class ModCatalogBuilder
    {
        // LibDat2 schema enums are 1-based (ITEM=1, AREA=5, ATLAS=11, CRUCIBLE_MAP=33). Raw dat can be 0-based (ITEM=0, ATLAS=10).
        private static readonly HashSet<int> AllowedDomains = [0, 1, 5, 10, 11, 33];
        private const int GenerationPrefix = 1;
        private const int GenerationSuffix = 2;

        private static readonly HashSet<string> MapSpawnTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "map",
            "atlas",
            "white_map",
            "red_map",
            "yellow_map",
            "blue_map",
            "ancient_map",
            "memory_map",
            "expedition_logbook",
            "low_tier_map",
            "mid_tier_map",
            "top_tier_map",
            "uber_tier_map",
            "maven_map",
            "primordial_map",
            "has_uber_map_prefix",
            "has_uber_map_suffix",
            "crucible_map_low",
            "crucible_map_high",
        };

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

        public static HashSet<(string ModName, string ModContent, bool IsMap)> Build(
            string schemaJsonPath,
            byte[] modsBytes,
            byte[] tagsBytes,
            byte[] statsBytes,
            IReadOnlyList<(string Path, byte[] Bytes)> statDescriptionFiles)
        {
            var mods = LibDat2DatTable.Load(modsBytes, "mods", schemaJsonPath);
            var tags = LibDat2DatTable.Load(tagsBytes, "tags", schemaJsonPath);
            var stats = LibDat2DatTable.Load(statsBytes, "stats", schemaJsonPath);

            GameDataLog.Info($"Loaded dat tables — Mods: {mods.RowCount} rows, Tags: {tags.RowCount}, Stats: {stats.RowCount}.");

            GameDataLog.Info("Parsing English stat descriptions…");
            (Dictionary<string, string> statTemplates, HashSet<string> mapStatIds) =
                StatDescriptionParser.ParseEnglishTemplatesFromFiles(statDescriptionFiles);
            GameDataLog.Info($"Stat description templates: {statTemplates.Count} ({mapStatIds.Count} map/atlas stat ids).");

            string?[] tagIds = BuildTagIds(tags);
            string?[] statIds = BuildStatIds(stats);

            var entries = new Dictionary<(string ModName, string ModContent), bool>(ModEntryKeyComparer.Instance);
            int includedMods = 0;
            int mapMods = 0;
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
                bool isMap = IsMapMod(mods, tagIds, row);
                if (isMap)
                    mapMods++;

                string? name = mods.GetString(row, "Name")?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                AddSuggestion(entries, name, string.Empty, isMap);

                foreach (string line in BuildDescriptionLines(mods, statIds, statTemplates, row))
                    AddSuggestion(entries, name, line, isMap);
            }

            GameDataLog.Info($"Scan complete — {includedMods:N0} mods matched filters ({mapMods:N0} map-tagged), {entries.Count:N0} suggestion rows.");
            if (includedMods == 0)
                WriteFilterDebugDump(mods, tagIds);

            return entries
                .Select(kv => (kv.Key.ModName, kv.Key.ModContent, kv.Value))
                .ToHashSet();
        }

        private static void AddSuggestion(
            Dictionary<(string ModName, string ModContent), bool> entries,
            string modName,
            string modContent,
            bool isMap)
        {
            var key = (modName, modContent);
            if (entries.TryGetValue(key, out bool existing))
                entries[key] = existing || isMap;
            else
                entries[key] = isMap;
        }

        private sealed class ModEntryKeyComparer : IEqualityComparer<(string ModName, string ModContent)>
        {
            public static ModEntryKeyComparer Instance { get; } = new();

            public bool Equals((string ModName, string ModContent) x, (string ModName, string ModContent) y) =>
                string.Equals(x.ModName, y.ModName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.ModContent, y.ModContent, StringComparison.OrdinalIgnoreCase);

            public int GetHashCode((string ModName, string ModContent) obj) =>
                HashCode.Combine(
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.ModName),
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.ModContent));
        }

        private static bool IsMapDomain(int domain) => domain is 5 or 11 or 33;

        private static bool IsMapMod(
            LibDat2DatTable mods,
            string?[] tagIds,
            int row)
        {
            if (IsMapDomain(mods.GetInt32(row, "Domain")))
                return true;

            return HasActiveSpawnTag(mods, tagIds, row, MapSpawnTags);
        }

        private static bool HasActiveSpawnTag(
            LibDat2DatTable mods,
            string?[] tagIds,
            int row,
            HashSet<string> tags)
        {
            IReadOnlyList<int> spawnTags = mods.GetForeignKeyArray(row, "SpawnWeight_TagsKeys");
            IReadOnlyList<int> spawnValues = mods.GetInt32Array(row, "SpawnWeight_Values");
            if (spawnTags.Count == 0 || spawnValues.Count == 0)
                return false;

            int count = Math.Min(spawnTags.Count, spawnValues.Count);
            for (int i = 0; i < count; i++)
            {
                if (spawnValues[i] <= 0)
                    continue;

                string? tag = TagId(tagIds, spawnTags[i]);
                if (tag is not null && tags.Contains(tag))
                    return true;
            }

            return false;
        }

        private static bool HasAnyNonUniquePositiveSpawn(LibDat2DatTable mods, string?[] tagIds, int row)
        {
            IReadOnlyList<int> spawnTags = mods.GetForeignKeyArray(row, "SpawnWeight_TagsKeys");
            IReadOnlyList<int> spawnValues = mods.GetInt32Array(row, "SpawnWeight_Values");
            if (spawnTags.Count == 0 || spawnValues.Count == 0)
                return false;

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

                return true;
            }

            return false;
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

            if (IsMapDomain(domain))
                return HasAnyNonUniquePositiveSpawn(mods, tagIds, row);

            IReadOnlyList<int> spawnTags = mods.GetForeignKeyArray(row, "SpawnWeight_TagsKeys");
            IReadOnlyList<int> spawnValues = mods.GetInt32Array(row, "SpawnWeight_Values");
            if (spawnTags.Count == 0 || spawnValues.Count == 0)
                return false;

            bool hasAllowedSpawn = false;
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
                if (ItemOrMapTags.Contains(tag) || MapSpawnTags.Contains(tag))
                    hasAllowedSpawn = true;
            }

            return hasAllowedSpawn && !onlyUniqueSpawn;
        }

        private static IReadOnlyList<string> BuildDescriptionLines(
            LibDat2DatTable mods,
            string?[] statIds,
            Dictionary<string, string> statTemplates,
            int row)
        {
            var lines = new List<string>(StatKeyColumns.Length);
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
                    lines.Add(template);
            }

            return lines;
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
