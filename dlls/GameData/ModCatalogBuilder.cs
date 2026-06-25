using System.Diagnostics;

namespace PoE.dlls.GameData
{
    internal static class ModCatalogBuilder
    {
        private const int GenerationPrefix = 1;
        private const int GenerationSuffix = 2;
        private const int GenerationExarchImplicit = 28;
        private const int GenerationEaterImplicit = 29;

        private const string UniqueMonsterPresenceStat = "local_influence_mod_requires_unique_monster_presence";
        private const string CelestialBossPresenceStat = "local_influence_mod_requires_celestial_boss_presence";

        private static readonly HashSet<string> EldritchPresenceRequirementStats = new(StringComparer.OrdinalIgnoreCase)
        {
            UniqueMonsterPresenceStat,
            CelestialBossPresenceStat,
        };

        private static readonly string[] StatKeyColumns =
        [
            "StatsKey1", "StatsKey2", "StatsKey3", "StatsKey4", "StatsKey5", "StatsKey6",
        ];

        public static HashSet<ModCatalogEntry> Build(
            string schemaJsonPath,
            byte[] modsBytes,
            byte[] tagsBytes,
            byte[] statsBytes,
            IReadOnlyList<(string Path, byte[] Bytes)> statDescriptionFiles,
            byte[]? essencesBytes = null)
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

            var entries = new Dictionary<(string ModName, string ModContent, ModEldritchInfluence Eldritch), ModCatalogAccumulator>(ModEntryKeyComparer.Instance);
            int includedMods = 0;
            int mapMods = 0;
            int eldritchMods = 0;
            int flaskMods = 0;
            int clusterMods = 0;
            int abyssMods = 0;
            int essenceMods = 0;
            var scanTimer = Stopwatch.StartNew();
            long lastProgressMs = 0;

            GameDataLog.Info($"Scanning {mods.RowCount} mods (items, maps, flasks, jewels, abyss jewels, cluster jewels, eldritch implicits, essences)…");
            for (int row = 0; row < mods.RowCount; row++)
            {
                long elapsedMs = scanTimer.ElapsedMilliseconds;
                if (row > 0 && (row % 2000 == 0 || elapsedMs - lastProgressMs >= 5000))
                {
                    GameDataLog.Info($"Scanning mods… {row:N0} / {mods.RowCount:N0} ({100.0 * row / mods.RowCount:0.#}%), {includedMods:N0} matched so far.");
                    lastProgressMs = elapsedMs;
                }

                string? modId = mods.GetString(row, "Id");
                if (ModCatalogTagHelper.ShouldExcludeModId(modId))
                    continue;

                int domain = mods.GetInt32(row, "Domain");
                int generation = mods.GetInt32(row, "GenerationType");
                ModEldritchInfluence eldritchInfluence = generation switch
                {
                    GenerationExarchImplicit => ModEldritchInfluence.SearingExarch,
                    GenerationEaterImplicit => ModEldritchInfluence.EaterOfWorlds,
                    _ => ModEldritchInfluence.None,
                };

                IReadOnlyList<string> positiveSpawnTags = CollectPositiveSpawnTags(mods, tagIds, row);
                if (ModCatalogTagHelper.IsJewelDomain(domain))
                    positiveSpawnTags = EnsureJewelSpawnTag(positiveSpawnTags);
                else if (ModCatalogTagHelper.IsAbyssJewelDomain(domain))
                    positiveSpawnTags = AbyssJewelSubtypeTags.EnrichSpawnTags(modId, positiveSpawnTags);
                else if (ModCatalogTagHelper.IsFlaskDomain(domain)
                         || ModCatalogTagHelper.HasFlaskPositiveSpawn(positiveSpawnTags))
                    positiveSpawnTags = EnsureFlaskSpawnTag(positiveSpawnTags);

                if (eldritchInfluence != ModEldritchInfluence.None)
                {
                    if (!ShouldIncludeEldritchImplicit(mods, tagIds, row))
                        continue;

                    includedMods++;
                    eldritchMods++;
                    bool eldritchIsMap = IsMapMod(mods, tagIds, row, domain);
                    if (eldritchIsMap)
                        mapMods++;

                    string? eldritchName = ResolveCatalogModName(mods, row);
                    if (string.IsNullOrWhiteSpace(eldritchName))
                        continue;

                    IReadOnlyList<string> lines = BuildEldritchDescriptionLines(mods, statIds, statTemplates, row);
                    if (lines.Count == 0)
                        continue;

                    ModItemKind eldritchKind = ModCatalogTagHelper.ResolveItemKind(
                        domain, positiveSpawnTags, eldritchIsMap, eldritchInfluence);

                    foreach (string line in lines)
                    {
                        AddSuggestion(
                            entries,
                            eldritchName,
                            line,
                            eldritchIsMap,
                            eldritchInfluence,
                            domain,
                            eldritchKind,
                            positiveSpawnTags);
                    }

                    continue;
                }

                if (!ShouldIncludeMod(mods, tagIds, row, domain, positiveSpawnTags))
                    continue;

                includedMods++;
                bool isMap = IsMapMod(mods, tagIds, row, domain);
                if (isMap)
                    mapMods++;

                ModItemKind itemKind = ModCatalogTagHelper.ResolveItemKind(
                    domain, positiveSpawnTags, isMap, ModEldritchInfluence.None);

                if (itemKind == ModItemKind.Flask)
                    flaskMods++;
                else if (itemKind == ModItemKind.ClusterJewel)
                    clusterMods++;
                else if (itemKind == ModItemKind.AbyssJewel)
                    abyssMods++;

                string? name = mods.GetString(row, "Name")?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                AddSuggestion(entries, name, string.Empty, isMap, ModEldritchInfluence.None, domain, itemKind, positiveSpawnTags);

                foreach (string line in BuildDescriptionLines(mods, statIds, statTemplates, row))
                {
                    AddSuggestion(
                        entries,
                        name,
                        line,
                        isMap,
                        ModEldritchInfluence.None,
                        domain,
                        itemKind,
                        positiveSpawnTags);
                }
            }

            if (essencesBytes is not null)
                essenceMods = IncludeEssenceMods(
                    schemaJsonPath,
                    essencesBytes,
                    mods,
                    tagIds,
                    statIds,
                    statTemplates,
                    entries,
                    ref includedMods);

            GameDataLog.Info(
                $"Scan complete — {includedMods:N0} mods matched filters ({mapMods:N0} map-tagged, {eldritchMods:N0} eldritch implicit, {flaskMods:N0} flask, {clusterMods:N0} cluster jewel, {abyssMods:N0} abyss jewel, {essenceMods:N0} essence), {entries.Count:N0} suggestion rows.");

            if (includedMods == 0)
                WriteFilterDebugDump(mods, tagIds);

            return entries
                .Select(kv => new ModCatalogEntry(
                    kv.Key.ModName,
                    kv.Key.ModContent,
                    kv.Value.IsMap,
                    kv.Key.Eldritch,
                    kv.Value.ModDomain,
                    kv.Value.ItemKind,
                    ModCatalogTagHelper.FormatSpawnTags(kv.Value.SpawnTags)))
                .ToHashSet();
        }

        private static int IncludeEssenceMods(
            string schemaJsonPath,
            byte[] essencesBytes,
            LibDat2DatTable mods,
            string?[] tagIds,
            string?[] statIds,
            Dictionary<string, string> statTemplates,
            Dictionary<(string ModName, string ModContent, ModEldritchInfluence Eldritch), ModCatalogAccumulator> entries,
            ref int includedMods)
        {
            LibDat2DatTable essences;
            try
            {
                essences = LibDat2DatTable.Load(essencesBytes, "essences", schemaJsonPath);
            }
            catch (Exception ex)
            {
                GameDataLog.Info($"Could not load essences.dat for essence mod tags: {ex.Message}");
                return 0;
            }

            Dictionary<int, HashSet<string>> essenceTagsByModRow = EssenceModTagEnricher.BuildModRowTags(essences);
            int essenceModRows = 0;

            foreach (KeyValuePair<int, HashSet<string>> pair in essenceTagsByModRow)
            {
                int modRow = pair.Key;
                HashSet<string> essenceTags = pair.Value;
                if ((uint)modRow >= (uint)mods.RowCount || essenceTags.Count == 0)
                    continue;

                int domain = mods.GetInt32(modRow, "Domain");
                if (!ModCatalogTagHelper.AllowedDomains.Contains(domain))
                    continue;

                int generation = mods.GetInt32(modRow, "GenerationType");
                if (generation is not (GenerationPrefix or GenerationSuffix))
                    continue;

                IReadOnlyList<string> naturalTags = CollectPositiveSpawnTags(mods, tagIds, modRow);
                var combinedTags = new List<string>(naturalTags.Count + essenceTags.Count);
                combinedTags.AddRange(naturalTags);
                combinedTags.AddRange(essenceTags);

                string? name = mods.GetString(modRow, "Name")?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                includedMods++;
                essenceModRows++;

                ModItemKind itemKind = ModCatalogTagHelper.MergeItemKind(
                    ModCatalogTagHelper.ResolveItemKind(domain, combinedTags, false, ModEldritchInfluence.None),
                    ModItemKind.Essence);

                AddSuggestion(
                    entries,
                    name,
                    string.Empty,
                    false,
                    ModEldritchInfluence.None,
                    domain,
                    itemKind,
                    combinedTags);

                foreach (string line in BuildDescriptionLines(mods, statIds, statTemplates, modRow))
                {
                    AddSuggestion(
                        entries,
                        name,
                        line,
                        false,
                        ModEldritchInfluence.None,
                        domain,
                        itemKind,
                        combinedTags);
                }
            }

            return essenceModRows;
        }

        private static void AddSuggestion(
            Dictionary<(string ModName, string ModContent, ModEldritchInfluence Eldritch), ModCatalogAccumulator> entries,
            string modName,
            string modContent,
            bool isMap,
            ModEldritchInfluence eldritch,
            int modDomain,
            ModItemKind itemKind,
            IReadOnlyList<string> positiveSpawnTags)
        {
            var key = (modName, modContent, eldritch);
            if (!entries.TryGetValue(key, out ModCatalogAccumulator? accumulator))
            {
                accumulator = new ModCatalogAccumulator();
                entries[key] = accumulator;
            }

            accumulator.Merge(isMap, modDomain, itemKind, positiveSpawnTags);
        }

        private sealed class ModCatalogAccumulator
        {
            public bool IsMap { get; private set; }

            public int ModDomain { get; private set; }

            public ModItemKind ItemKind { get; private set; }

            public HashSet<string> SpawnTags { get; } = new(StringComparer.OrdinalIgnoreCase);

            public void Merge(bool isMap, int modDomain, ModItemKind itemKind, IEnumerable<string> positiveSpawnTags)
            {
                IsMap |= isMap;
                ModDomain = modDomain;
                ItemKind = ModCatalogTagHelper.MergeItemKind(ItemKind, itemKind);

                foreach (string tag in positiveSpawnTags)
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                        SpawnTags.Add(tag.Trim());
                }
            }
        }

        private sealed class ModEntryKeyComparer : IEqualityComparer<(string ModName, string ModContent, ModEldritchInfluence Eldritch)>
        {
            public static ModEntryKeyComparer Instance { get; } = new();

            public bool Equals((string ModName, string ModContent, ModEldritchInfluence Eldritch) x, (string ModName, string ModContent, ModEldritchInfluence Eldritch) y) =>
                string.Equals(x.ModName, y.ModName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.ModContent, y.ModContent, StringComparison.OrdinalIgnoreCase)
                && x.Eldritch == y.Eldritch;

            public int GetHashCode((string ModName, string ModContent, ModEldritchInfluence Eldritch) obj) =>
                HashCode.Combine(
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.ModName),
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.ModContent),
                    obj.Eldritch);
        }

        private static bool ShouldIncludeEldritchImplicit(
            LibDat2DatTable mods,
            string?[] tagIds,
            int row)
        {
            if (HasActiveSpawnTag(mods, tagIds, row, ModCatalogTagHelper.EldritchItemSpawnTags))
                return true;

            return HasActiveSpawnTag(mods, tagIds, row, ModCatalogTagHelper.MapSpawnTags);
        }

        private static string? ResolveCatalogModName(LibDat2DatTable mods, int row)
        {
            string? name = mods.GetString(row, "Name")?.Trim();
            if (!string.IsNullOrWhiteSpace(name))
                return name;

            return mods.GetString(row, "Id")?.Trim();
        }

        private static bool IsMapMod(
            LibDat2DatTable mods,
            string?[] tagIds,
            int row,
            int domain)
        {
            if (ModCatalogTagHelper.IsMapDomain(domain))
                return true;

            return HasActiveSpawnTag(mods, tagIds, row, ModCatalogTagHelper.MapSpawnTags);
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

        private static List<string> CollectPositiveSpawnTags(LibDat2DatTable mods, string?[] tagIds, int row)
        {
            var positive = new List<string>();
            IReadOnlyList<int> spawnTags = mods.GetForeignKeyArray(row, "SpawnWeight_TagsKeys");
            IReadOnlyList<int> spawnValues = mods.GetInt32Array(row, "SpawnWeight_Values");
            if (spawnTags.Count == 0 || spawnValues.Count == 0)
                return positive;

            int count = Math.Min(spawnTags.Count, spawnValues.Count);
            for (int i = 0; i < count; i++)
            {
                if (spawnValues[i] <= 0)
                    continue;

                string? tag = TagId(tagIds, spawnTags[i]);
                if (string.IsNullOrWhiteSpace(tag) || ModCatalogTagHelper.UniqueOnlyTags.Contains(tag))
                    continue;

                positive.Add(tag);
            }

            return positive;
        }

        private static IReadOnlyList<string> EnsureJewelSpawnTag(IReadOnlyList<string> tags)
        {
            foreach (string tag in tags)
            {
                if (string.Equals(tag, "jewel", StringComparison.OrdinalIgnoreCase))
                    return tags;
            }

            var withJewel = new List<string>(tags.Count + 1);
            withJewel.AddRange(tags);
            withJewel.Add("jewel");
            return withJewel;
        }

        private static IReadOnlyList<string> EnsureFlaskSpawnTag(IReadOnlyList<string> tags)
        {
            if (tags.Count == 0)
                return tags;

            bool hasGenericFlask = tags.Any(t => string.Equals(t, "flask", StringComparison.OrdinalIgnoreCase));
            if (hasGenericFlask)
                return tags;

            bool hasFlaskTag = tags.Any(ModCatalogTagHelper.IsFlaskSpawnTag);
            if (!hasFlaskTag && !tags.Any(t => string.Equals(t, "default", StringComparison.OrdinalIgnoreCase)))
                return tags;

            var enriched = new List<string>(tags.Count + 1);
            enriched.AddRange(tags);
            enriched.Add("flask");
            return enriched;
        }

        private static bool HasAnyNonUniquePositiveSpawn(LibDat2DatTable mods, string?[] tagIds, int row) =>
            CollectPositiveSpawnTags(mods, tagIds, row).Count > 0;

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
                    if (domain is not (1 or 11 or ModCatalogTagHelper.DomainClusterJewel or ModCatalogTagHelper.DomainClusterJewelRaw) || gen is not (1 or 2))
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

        private static bool ShouldIncludeMod(
            LibDat2DatTable mods,
            string?[] tagIds,
            int row,
            int domain,
            IReadOnlyList<string> positiveSpawnTags)
        {
            if (!ModCatalogTagHelper.AllowedDomains.Contains(domain))
                return false;

            int generation = mods.GetInt32(row, "GenerationType");
            if (generation is not (GenerationPrefix or GenerationSuffix))
                return false;

            if (ModCatalogTagHelper.IsMapDomain(domain))
                return HasAnyNonUniquePositiveSpawn(mods, tagIds, row);

            if (ModCatalogTagHelper.IsClusterJewelDomain(domain))
                return positiveSpawnTags.Count > 0;

            if (ModCatalogTagHelper.IsFlaskDomain(domain))
                return positiveSpawnTags.Count > 0;

            if (ModCatalogTagHelper.IsJewelDomain(domain))
                return positiveSpawnTags.Count > 0;

            if (ModCatalogTagHelper.IsAbyssJewelDomain(domain))
                return ModCatalogTagHelper.HasAbyssJewelPositiveSpawn(positiveSpawnTags);

            return ModCatalogTagHelper.HasAllowedPositiveSpawn(positiveSpawnTags);
        }

        private static IReadOnlyList<string> BuildEldritchDescriptionLines(
            LibDat2DatTable mods,
            string?[] statIds,
            Dictionary<string, string> statTemplates,
            int row)
        {
            var lines = new List<string>(StatKeyColumns.Length * 2);
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var effectTemplates = new List<string>();
            var modStatIds = new List<string?>(StatKeyColumns.Length);

            foreach (string column in StatKeyColumns)
            {
                int? statRow = mods.GetForeignKey(row, column);
                if (statRow is null or < 0)
                    continue;

                string? statId = StatId(statIds, statRow.Value);
                modStatIds.Add(statId);
                if (string.IsNullOrWhiteSpace(statId) || EldritchPresenceRequirementStats.Contains(statId))
                    continue;

                if (!statTemplates.TryGetValue(statId, out string? template) || string.IsNullOrWhiteSpace(template))
                    continue;

                if (seen.Add(template))
                {
                    lines.Add(template);
                    effectTemplates.Add(template);
                }
            }

            string? presencePrefix = ResolveEldritchPresencePrefix(modStatIds);
            if (presencePrefix is null)
                return lines;

            foreach (string template in effectTemplates)
            {
                string presenceLine = $"{presencePrefix}, {template}";
                if (seen.Add(presenceLine))
                    lines.Add(presenceLine);
            }

            return lines;
        }

        private static string? ResolveEldritchPresencePrefix(IReadOnlyList<string?> modStatIds)
        {
            bool pinnacle = false;
            bool unique = false;
            foreach (string? statId in modStatIds)
            {
                if (string.IsNullOrWhiteSpace(statId))
                    continue;

                if (string.Equals(statId, CelestialBossPresenceStat, StringComparison.OrdinalIgnoreCase))
                    pinnacle = true;
                else if (string.Equals(statId, UniqueMonsterPresenceStat, StringComparison.OrdinalIgnoreCase))
                    unique = true;
            }

            if (pinnacle)
                return "While a Pinnacle Atlas Boss is in your Presence";

            if (unique)
                return "While a Unique Enemy is in your Presence";

            return null;
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
