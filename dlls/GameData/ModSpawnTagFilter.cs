namespace PoE.dlls.GameData
{
    internal static class ModSpawnTagFilter
    {
        public static string? Normalize(string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return null;

            string trimmed = filter.Trim();
            if (ModSpawnTagDisplay.TryGetCanonicalTag(trimmed, out string canonical))
                return canonical;

            return trimmed;
        }

        public static bool TryResolveItemKind(string filter, out ModItemKind itemKind)
        {
            string normalized = Normalize(filter) ?? filter.Trim();
            switch (normalized.ToLowerInvariant())
            {
                case "cluster":
                case "cluster_jewel":
                    itemKind = ModItemKind.ClusterJewel;
                    return true;
                case "flask":
                    itemKind = ModItemKind.Flask;
                    return true;
                case "jewel":
                    itemKind = ModItemKind.Jewel;
                    return true;
                case "abyss_jewel":
                case "abyss":
                    itemKind = ModItemKind.AbyssJewel;
                    return true;
                default:
                    if (ClusterJewelSizeTags.IsClusterSizeTag(normalized)
                        || ClusterJewelSizeTags.IsAfflictionClusterTag(normalized))
                    {
                        itemKind = ModItemKind.ClusterJewel;
                        return true;
                    }

                    if (AbyssJewelSubtypeTags.IsSubtypeTag(normalized))
                    {
                        itemKind = ModItemKind.AbyssJewel;
                        return true;
                    }

                    itemKind = ModItemKind.Item;
                    return false;
            }
        }

        public static string BuildItemFilterSql(
            IReadOnlyList<string> matchTags,
            string itemKindParameterName,
            bool useItemKind)
        {
            var tagChecks = new List<string>();
            for (int i = 0; i < matchTags.Count; i++)
                tagChecks.Add($"(',' || lower(spawn_tags) || ',') LIKE '%,' || lower($mt{i}) || ',%'");

            if (useItemKind && tagChecks.Count > 0)
            {
                return $"""
                     AND item_kind = {itemKindParameterName}
                     AND ({string.Join(" OR ", tagChecks)})
                    """;
            }

            if (useItemKind)
                return $" AND item_kind = {itemKindParameterName}";

            if (tagChecks.Count == 0)
                return string.Empty;

            return $" AND ({string.Join(" OR ", tagChecks)})";
        }
    }
}
