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

        private static readonly Regex ItemArticleRegex = new(
            @"^\s*(?<article>an|a)(?=\s|$)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool TryMatch(string ruleContent, string dbTemplate, string itemContent)
        {
            if (TryMatchCore(ruleContent, dbTemplate, itemContent, allowPluralRelaxation: false))
                return true;

            return TryMatchCore(ruleContent, dbTemplate, itemContent, allowPluralRelaxation: true);
        }

        private static bool TryMatchCore(
            string ruleContent,
            string dbTemplate,
            string itemContent,
            bool allowPluralRelaxation)
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
            {
                return string.Equals(dbTemplate.Trim(), item.Trim(), StringComparison.OrdinalIgnoreCase)
                    || (allowPluralRelaxation
                        && string.Equals(
                            RelaxTrailingWordPlural(dbTemplate.Trim()),
                            RelaxTrailingWordPlural(item.Trim()),
                            StringComparison.OrdinalIgnoreCase));
            }

            int rulePos = 0;
            int itemPos = 0;

            for (int i = 0; i < literals.Length; i++)
            {
                string literal = literals[i];
                if (!string.IsNullOrEmpty(literal))
                {
                    if (!TryConsumeLiteral(ruleContent, ref rulePos, literal, allowPluralRelaxation: false))
                        return false;

                    if (!TryConsumeItemLiteral(item, ref itemPos, literal, allowPluralRelaxation))
                        return false;
                }

                if (i >= slotCount)
                    continue;

                if (!TryParseRuleSlot(ruleContent, ref rulePos, out ModRuleSlot ruleSlot))
                    return false;

                if (!TryParseItemSlotValue(item, ref itemPos, ruleSlot))
                    return false;
            }

            SkipTrailingWhitespace(ruleContent, ref rulePos);
            SkipTrailingWhitespace(item, ref itemPos);
            return rulePos >= ruleContent.Length && itemPos >= item.Length;
        }

        private static bool TryConsumeLiteral(
            string text,
            ref int position,
            string literal,
            bool allowPluralRelaxation)
        {
            if (TryConsumeLiteralCore(text, ref position, literal))
                return true;

            if (!allowPluralRelaxation)
                return false;

            string? relaxed = RelaxTrailingWordPlural(literal);
            return relaxed is not null
                && !string.Equals(relaxed, literal, StringComparison.Ordinal)
                && TryConsumeLiteralCore(text, ref position, relaxed);
        }

        private static bool TryConsumeItemLiteral(
            string text,
            ref int position,
            string literal,
            bool allowPluralRelaxation)
        {
            if (TryConsumeLiteralCore(text, ref position, literal))
                return true;

            if (!allowPluralRelaxation)
                return false;

            string? relaxedLiteral = RelaxTrailingWordPlural(literal);
            if (relaxedLiteral is not null
                && !string.Equals(relaxedLiteral, literal, StringComparison.Ordinal)
                && TryConsumeLiteralCore(text, ref position, relaxedLiteral))
            {
                return true;
            }

            if (literal.Length == 0 || EndsWithLetterS(literal))
                return false;

            int afterLiteral = position + literal.Length;
            if (afterLiteral >= text.Length || text[afterLiteral] != 's')
                return false;

            if (afterLiteral + 1 < text.Length && char.IsLetter(text[afterLiteral + 1]))
                return false;

            if (!text.AsSpan(position).StartsWith(literal, StringComparison.OrdinalIgnoreCase))
                return false;

            position = afterLiteral + 1;
            return true;
        }

        private static bool TryConsumeLiteralCore(string text, ref int position, string literal)
        {
            if (position >= text.Length)
                return string.IsNullOrEmpty(literal);

            if (!text.AsSpan(position).StartsWith(literal, StringComparison.OrdinalIgnoreCase))
                return false;

            position += literal.Length;
            return true;
        }

        private static bool EndsWithLetterS(string text)
        {
            for (int i = text.Length - 1; i >= 0; i--)
            {
                if (char.IsLetter(text[i]))
                    return text[i] is 's' or 'S';
            }

            return false;
        }

        internal static string RelaxTrailingWordPlural(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            int end = text.Length - 1;
            while (end >= 0 && !char.IsLetter(text[end]))
                end--;

            if (end < 0 || text[end] is not ('s' or 'S'))
                return text;

            int start = end - 1;
            while (start >= 0 && char.IsLetter(text[start]))
                start--;

            if (end - start < 2)
                return text;

            return text[..end] + text[(end + 1)..];
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

        private static bool TryParseItemSlotValue(string text, ref int position, ModRuleSlot ruleSlot)
        {
            if (TryParseItemNumber(text, ref position, out int itemValue))
                return ruleSlot.Matches(itemValue);

            if (position >= text.Length)
                return false;

            string remaining = text[position..];
            Match match = ItemArticleRegex.Match(remaining);
            if (!match.Success || match.Index != 0)
                return false;

            position += match.Length;
            return ruleSlot.Matches(1);
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
