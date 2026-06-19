namespace PoE.dlls.GameData
{
    internal static class ModCatalogTagHelper
    {
        // LibDat2 schema labels vary by patch; domain 10 holds regular jewel affixes in current game data.
        public const int DomainFlask = 2;
        public const int DomainJewel = 10;
        public const int DomainJewelLegacy = 16;
        public const int DomainAbyssJewel = 13;
        public const int DomainClusterJewelRaw = 20;
        public const int DomainClusterJewel = 21;

        public static readonly HashSet<int> AllowedDomains =
        [
            0, 1, 2, 5, 10, 11, 16, 33,
            DomainAbyssJewel,
            DomainClusterJewelRaw, DomainClusterJewel,
        ];

        public static readonly HashSet<string> EldritchItemSpawnTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "gloves",
            "boots",
            "helmet",
            "body_armour",
            "amulet",
        };

        public static readonly HashSet<string> MapSpawnTags = new(StringComparer.OrdinalIgnoreCase)
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

        public static readonly HashSet<string> UniqueOnlyTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "unique",
            "unique_map",
            "unique_league",
        };

        public static readonly HashSet<string> ItemOrMapTags = new(StringComparer.OrdinalIgnoreCase)
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
            "flask",
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

        public static bool IsMapDomain(int domain) => domain is 5 or 11 or 33;

        public static bool IsFlaskDomain(int domain) => domain is DomainFlask;

        public static bool IsJewelDomain(int domain) => domain is DomainJewel or DomainJewelLegacy;

        public static bool IsClusterJewelDomain(int domain) => domain is DomainClusterJewelRaw or DomainClusterJewel;

        public static bool IsAbyssJewelDomain(int domain) => domain is DomainAbyssJewel;

        public static bool HasAbyssJewelPositiveSpawn(IReadOnlyList<string> positiveSpawnTags) =>
            positiveSpawnTags.Any(AbyssJewelSubtypeTags.IsAbyssJewelTag);

        public static bool IsFlaskSpawnTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return false;

            if (tag.StartsWith("affliction_", StringComparison.OrdinalIgnoreCase))
                return false;

            return tag.ToLowerInvariant() switch
            {
                "flask" or "life_flask" or "mana_flask" or "utility_flask" or "hybrid_flask"
                    or "critical_utility_flask" or "tincture" => true,
                _ => tag.EndsWith("_flask", StringComparison.OrdinalIgnoreCase)
                    && !tag.StartsWith("expedition_", StringComparison.OrdinalIgnoreCase),
            };
        }

        public static bool HasFlaskPositiveSpawn(IReadOnlyList<string> positiveSpawnTags) =>
            positiveSpawnTags.Any(IsFlaskSpawnTag);

        public static bool IsIndexableSpawnTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag) || UniqueOnlyTags.Contains(tag))
                return false;

            if (tag.StartsWith("old_do_not_use_", StringComparison.OrdinalIgnoreCase))
                return false;

            if (IsFlaskSpawnTag(tag))
                return true;

            if (tag.StartsWith("affliction_", StringComparison.OrdinalIgnoreCase))
                return false;

            if (tag.StartsWith("expansion_jewel_", StringComparison.OrdinalIgnoreCase))
                return true;

            if (AbyssJewelSubtypeTags.IsAbyssJewelTag(tag))
                return true;

            return IsAllowedSpawnTag(tag);
        }

        private static readonly string[] InfluenceTagSuffixes =
        [
            "_shaper", "_elder", "_crusader", "_hunter", "_redeemer", "_warlord",
        ];

        private static readonly string[] ExcludedModIdPrefixes =
        [
            "Delve", "Synthesis", "Bestiary", "Incursion", "BlightEnchantment",
            "HellscapeUpside", "HellscapeDownside",
        ];

        public static bool IsInfluenceSpawnTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return false;

            foreach (string suffix in InfluenceTagSuffixes)
            {
                if (tag.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public static bool ShouldExcludeModId(string? modId)
        {
            if (string.IsNullOrWhiteSpace(modId))
                return false;

            foreach (string prefix in ExcludedModIdPrefixes)
            {
                if (modId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public static bool IsAllowedSpawnTag(string tag)
        {
            if (ItemOrMapTags.Contains(tag) || MapSpawnTags.Contains(tag))
                return true;

            if (IsInfluenceSpawnTag(tag))
                return true;

            if (string.Equals(tag, "flask", StringComparison.OrdinalIgnoreCase))
                return true;

            if (IsFlaskSpawnTag(tag))
                return true;

            if (tag.StartsWith("affliction_", StringComparison.OrdinalIgnoreCase))
                return true;

            if (AbyssJewelSubtypeTags.IsAbyssJewelTag(tag))
                return true;

            return tag.StartsWith("expansion_jewel_", StringComparison.OrdinalIgnoreCase);
        }

        public static bool HasAllowedPositiveSpawn(IReadOnlyList<string> positiveSpawnTags) =>
            positiveSpawnTags.Any(IsAllowedSpawnTag);

        public static ModItemKind ResolveItemKind(
            int modDomain,
            IReadOnlyList<string> positiveSpawnTags,
            bool isMap,
            ModEldritchInfluence eldritchInfluence)
        {
            if (eldritchInfluence != ModEldritchInfluence.None)
                return ModItemKind.Eldritch;

            if (isMap || IsMapDomain(modDomain))
                return ModItemKind.Map;

            if (IsClusterJewelDomain(modDomain)
                || positiveSpawnTags.Any(t => t.StartsWith("affliction_", StringComparison.OrdinalIgnoreCase))
                || positiveSpawnTags.Any(t => t.StartsWith("expansion_jewel_", StringComparison.OrdinalIgnoreCase)))
            {
                return ModItemKind.ClusterJewel;
            }

            if (IsAbyssJewelDomain(modDomain)
                || positiveSpawnTags.Any(AbyssJewelSubtypeTags.IsAbyssJewelTag))
            {
                return ModItemKind.AbyssJewel;
            }

            if (IsJewelDomain(modDomain)
                || positiveSpawnTags.Any(t => string.Equals(t, "jewel", StringComparison.OrdinalIgnoreCase)))
            {
                return ModItemKind.Jewel;
            }

            if (IsFlaskDomain(modDomain) || HasFlaskPositiveSpawn(positiveSpawnTags))
                return ModItemKind.Flask;

            if (positiveSpawnTags.Any(t => string.Equals(t, "flask", StringComparison.OrdinalIgnoreCase)))
                return ModItemKind.Flask;

            return ModItemKind.Item;
        }

        public static string FormatSpawnTags(IEnumerable<string> tags)
        {
            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string tag in tags)
            {
                if (!string.IsNullOrWhiteSpace(tag))
                    unique.Add(tag.Trim());
            }

            if (unique.Count == 0)
                return string.Empty;

            return string.Join(',', unique.OrderBy(t => t, StringComparer.OrdinalIgnoreCase));
        }

        public static ModItemKind MergeItemKind(ModItemKind existing, ModItemKind incoming) =>
            (int)incoming > (int)existing ? incoming : existing;
    }
}
