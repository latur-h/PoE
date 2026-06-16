using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using PoE.dlls.GameData;
using PoE.dlls.Gamble.Modifiers;

namespace PoE.dlls.Gamble
{
    internal static class GambleModContentMatcher
    {
        private static readonly Regex ValueBeforeRangeRegex = new(
            @"([-+]?\d+)\(([-+]?\d+(?:--?[-+]?\d+)+)\)",
            RegexOptions.Compiled);

        private static readonly Regex RemainingRangeRegex = new(
            @"\(([-+]?\d+(?:--?[-+]?\d+)+)\)",
            RegexOptions.Compiled);

        private static readonly Regex RangeNumberRegex = new(
            @"[-+]?\d+",
            RegexOptions.Compiled);

        [ThreadStatic]
        private static ModCacheDatabase? t_database;

        [ThreadStatic]
        private static GambleType? t_gambleType;

        private static readonly AsyncLocal<ModCacheDatabase?> s_asyncDatabase = new();
        private static readonly AsyncLocal<GambleType?> s_asyncGambleType = new();

        internal static void SetCatalogContext(ModCacheDatabase? database, GambleType gambleType)
        {
            t_database = database;
            t_gambleType = gambleType;
            s_asyncDatabase.Value = database;
            s_asyncGambleType.Value = gambleType;
        }

        internal static void ClearCatalogContext()
        {
            t_database = null;
            t_gambleType = null;
            s_asyncDatabase.Value = null;
            s_asyncGambleType.Value = null;
        }

        private static ModCacheDatabase? GetDatabase() => s_asyncDatabase.Value ?? t_database;

        private static GambleType? GetGambleType() => s_asyncGambleType.Value ?? t_gambleType;

        /// <summary>
        /// Normalizes clipboard mod text: drops tier roll ranges in parentheses and keeps the
        /// highest numeric value that is not a range bound (the rolled value PoE shows first).
        /// </summary>
        public static string NormalizeItemModContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return string.Empty;

            content = ValueBeforeRangeRegex.Replace(content, ReplaceWithHighestNonRangeValue);
            content = RemainingRangeRegex.Replace(content, string.Empty);
            return content.Trim();
        }

        public static string ToRegexPattern(string ruleContent)
        {
            if (string.IsNullOrEmpty(ruleContent))
                return string.Empty;

            var pattern = new StringBuilder(ruleContent.Length + 16);
            foreach (char c in ruleContent)
                pattern.Append(c == '#' ? @"\d+" : c);

            return pattern.ToString();
        }

        public static Regex CreateRulePattern(string ruleContent) =>
            new(ToRegexPattern(ruleContent), RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool IsContentMatch(string ruleContent, string itemModContent)
        {
            if (string.IsNullOrEmpty(ruleContent))
                return false;

            string skeleton = ModTemplateNormalizer.ToSkeleton(ruleContent);

            ModCacheDatabase? database = GetDatabase();
            GambleType? gambleType = GetGambleType();

            if (UsesCatalogMatching(gambleType)
                && database is not null
                && gambleType is GambleType type
                && database.TryFindModTemplate(skeleton, type, out string? template)
                && ModTemplateMatcher.TryMatch(ruleContent, template, itemModContent))
            {
                return true;
            }

            if (skeleton.Contains('#', StringComparison.Ordinal)
                && ModTemplateMatcher.TryMatch(ruleContent, skeleton, itemModContent))
            {
                return true;
            }

            return IsLegacyRegexMatch(ruleContent, itemModContent);
        }

        public static bool MatchesModRule(Rule rule, Modifier mod, bool matchNameToo = false)
        {
            if (rule.Type != ModifierType.Any && mod.Type != rule.Type)
                return false;

            if (mod.Tier > rule.Tier)
                return false;

            if (IsContentMatch(rule.Content, mod.Content))
                return true;

            return matchNameToo && IsContentMatch(rule.Content, mod.Name);
        }

        private static bool UsesCatalogMatching(GambleType? gambleType) =>
            gambleType is not (
                GambleType.Map
                or GambleType.MapExalt
                or GambleType.MapT17
                or null);

        private static bool IsLegacyRegexMatch(string ruleContent, string itemModContent)
        {
            string normalized = NormalizeItemModContent(itemModContent);
            return CreateRulePattern(ruleContent).IsMatch(normalized);
        }

        private static string ReplaceWithHighestNonRangeValue(Match match)
        {
            int leading = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            var rangeValues = ParseRangeNumbers(match.Groups[2].Value);
            var rangeSet = new HashSet<int>(rangeValues);

            if (!rangeSet.Contains(leading))
                return leading.ToString(CultureInfo.InvariantCulture);

            int best = leading;
            foreach (int value in rangeValues)
            {
                if (!rangeSet.Contains(value))
                    best = Math.Max(best, value);
            }

            return best.ToString(CultureInfo.InvariantCulture);
        }

        private static List<int> ParseRangeNumbers(string rangeBody)
        {
            var numbers = new List<int>();
            foreach (Match number in RangeNumberRegex.Matches(rangeBody))
            {
                if (int.TryParse(number.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
                    numbers.Add(value);
            }

            return numbers;
        }
    }
}
