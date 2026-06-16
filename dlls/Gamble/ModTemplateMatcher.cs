using System.Globalization;
using System.Text.RegularExpressions;

namespace PoE.dlls.Gamble
{
    internal readonly record struct ModRuleSlot(ModComparisonOperator Operator, int? Threshold)
    {
        public bool IsWildcard => Operator == ModComparisonOperator.Any;

        public bool Matches(int value)
        {
            if (IsWildcard || Threshold is not int threshold)
                return true;

            return Operator switch
            {
                ModComparisonOperator.Equal => value == threshold,
                ModComparisonOperator.GreaterOrEqual => value >= threshold,
                ModComparisonOperator.LessOrEqual => value <= threshold,
                ModComparisonOperator.Greater => value > threshold,
                ModComparisonOperator.Less => value < threshold,
                _ => true,
            };
        }
    }

    internal static class ModTemplateMatcher
    {
        private static readonly Regex RuleSlotRegex = new(
            @"^\s*(?:(?<op><=|>=|<|>|=)\s*)?(?:#|(?<num>-?\d+))",
            RegexOptions.Compiled);

        private static readonly Regex ItemNumberRegex = new(
            @"^\s*(?<num>-?\d+)",
            RegexOptions.Compiled);

        public static bool TryMatch(string ruleContent, string dbTemplate, string itemContent)
        {
            if (string.IsNullOrWhiteSpace(ruleContent)
                || string.IsNullOrWhiteSpace(dbTemplate)
                || string.IsNullOrWhiteSpace(itemContent))
            {
                return false;
            }

            string item = GambleModContentMatcher.NormalizeItemModContent(itemContent);
            string[] literals = dbTemplate.Split('#');
            int slotCount = Math.Max(0, literals.Length - 1);

            if (slotCount == 0)
                return string.Equals(dbTemplate.Trim(), item.Trim(), StringComparison.OrdinalIgnoreCase);

            int rulePos = 0;
            int itemPos = 0;

            for (int i = 0; i < literals.Length; i++)
            {
                string literal = literals[i];
                if (!string.IsNullOrEmpty(literal))
                {
                    if (!TryConsumeLiteral(ruleContent, ref rulePos, literal))
                        return false;

                    if (!TryConsumeLiteral(item, ref itemPos, literal))
                        return false;
                }

                if (i >= slotCount)
                    continue;

                if (!TryParseRuleSlot(ruleContent, ref rulePos, out ModRuleSlot ruleSlot))
                    return false;

                if (!TryParseItemNumber(item, ref itemPos, out int itemValue))
                    return false;

                if (!ruleSlot.Matches(itemValue))
                    return false;
            }

            SkipTrailingWhitespace(ruleContent, ref rulePos);
            SkipTrailingWhitespace(item, ref itemPos);
            return rulePos >= ruleContent.Length && itemPos >= item.Length;
        }

        private static bool TryConsumeLiteral(string text, ref int position, string literal)
        {
            if (position >= text.Length)
                return string.IsNullOrEmpty(literal);

            if (!text.AsSpan(position).StartsWith(literal, StringComparison.OrdinalIgnoreCase))
                return false;

            position += literal.Length;
            return true;
        }

        private static bool TryParseRuleSlot(string text, ref int position, out ModRuleSlot slot)
        {
            slot = default;
            if (position >= text.Length)
                return false;

            string remaining = text[position..];
            Match match = RuleSlotRegex.Match(remaining);
            if (!match.Success || match.Index != 0)
                return false;

            if (match.Value.Contains('#', StringComparison.Ordinal))
            {
                slot = new ModRuleSlot(ModComparisonOperator.Any, null);
                position += match.Length;
                return true;
            }

            if (!match.Groups["num"].Success
                || !int.TryParse(match.Groups["num"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int threshold))
            {
                return false;
            }

            ModComparisonOperator op = ModComparisonOperator.Equal;
            if (match.Groups["op"].Success)
            {
                op = match.Groups["op"].Value switch
                {
                    ">=" => ModComparisonOperator.GreaterOrEqual,
                    "<=" => ModComparisonOperator.LessOrEqual,
                    ">" => ModComparisonOperator.Greater,
                    "<" => ModComparisonOperator.Less,
                    _ => ModComparisonOperator.Equal,
                };
            }

            slot = new ModRuleSlot(op, threshold);
            position += match.Length;
            return true;
        }

        private static bool TryParseItemNumber(string text, ref int position, out int value)
        {
            value = 0;
            if (position >= text.Length)
                return false;

            string remaining = text[position..];
            Match match = ItemNumberRegex.Match(remaining);
            if (!match.Success || match.Index != 0 || !match.Groups["num"].Success)
                return false;

            if (!int.TryParse(match.Groups["num"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                return false;

            position += match.Length;
            return true;
        }

        private static void SkipWhitespace(string text, ref int position)
        {
            while (position < text.Length && char.IsWhiteSpace(text[position]))
                position++;
        }

        private static void SkipTrailingWhitespace(string text, ref int position)
        {
            SkipWhitespace(text, ref position);
        }
    }
}
