namespace PoE.dlls.GameData
{
    internal static class AbyssJewelSubtypeTags
    {
        public const string Generic = "abyss_jewel";
        public const string Melee = "abyss_jewel_melee";
        public const string Ranged = "abyss_jewel_ranged";
        public const string Caster = "abyss_jewel_caster";
        public const string Summoner = "abyss_jewel_summoner";
        public const string SearingEye = "searing_eye_jewel";

        private static readonly string[] AllSubtypeTags =
        [
            Melee,
            Ranged,
            Caster,
            Summoner,
            SearingEye,
        ];

        public static bool IsAbyssJewelTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return false;

            if (string.Equals(tag, Generic, StringComparison.OrdinalIgnoreCase))
                return true;

            return IsSubtypeTag(tag);
        }

        public static bool IsSubtypeTag(string tag) =>
            AllSubtypeTags.Any(subtype => string.Equals(subtype, tag, StringComparison.OrdinalIgnoreCase));

        public static IReadOnlyList<string> GetAllMatchTags() =>
            AllSubtypeTags.Prepend(Generic).ToArray();

        public static IReadOnlyList<string> EnrichSpawnTags(string? modId, IReadOnlyList<string> tags)
        {
            var enriched = new List<string>(tags.Count + 3);
            foreach (string tag in tags)
            {
                if (!string.IsNullOrWhiteSpace(tag))
                    enriched.Add(tag.Trim());
            }

            if (!enriched.Any(t => string.Equals(t, Generic, StringComparison.OrdinalIgnoreCase)))
                enriched.Add(Generic);

            if (string.IsNullOrWhiteSpace(modId))
                return enriched;

            bool hasSummoner = enriched.Contains(Summoner, StringComparer.OrdinalIgnoreCase);
            bool hasMelee = enriched.Contains(Melee, StringComparer.OrdinalIgnoreCase);
            bool hasRanged = enriched.Contains(Ranged, StringComparer.OrdinalIgnoreCase);

            if (!hasSummoner
                && modId.Contains("Fire", StringComparison.OrdinalIgnoreCase)
                && (hasMelee || hasRanged))
            {
                enriched.Add(SearingEye);
            }

            return enriched;
        }

        public static IReadOnlyList<string> GetMatchTagsForFilter(string normalizedFilter)
        {
            if (string.Equals(normalizedFilter, Generic, StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalizedFilter, "abyss", StringComparison.OrdinalIgnoreCase))
            {
                return GetAllMatchTags();
            }

            if (IsSubtypeTag(normalizedFilter))
                return [normalizedFilter];

            return [];
        }
    }
}
