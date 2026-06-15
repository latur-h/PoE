using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
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

            string normalized = NormalizeItemModContent(itemModContent);
            return CreateRulePattern(ruleContent).IsMatch(normalized);
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

        private static string ReplaceWithHighestNonRangeValue(Match match)
        {
            int leading = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            var rangeValues = ParseRangeNumbers(match.Groups[2].Value);
            var rangeSet = new HashSet<int>(rangeValues);

            // Rolled value sits before the range; use it when it is not a range bound.
            if (!rangeSet.Contains(leading))
                return leading.ToString(CultureInfo.InvariantCulture);

            // Rare overlap: pick the highest number in this token that is not a range bound.
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
