using System.Text;
using System.Text.RegularExpressions;

namespace PoE.dlls.GameData
{
    internal static class ModTemplateNormalizer
    {
        private static readonly Regex OperatorNumberRegex = new(
            @"(?<op><=|>=|<|>|=)\s*(?<num>-?\d+)",
            RegexOptions.Compiled);

        private static readonly Regex BareNumberRegex = new(
            @"-?\d+",
            RegexOptions.Compiled);

        private static readonly Regex WhitespaceRegex = new(
            @"\s+",
            RegexOptions.Compiled);

        /// <summary>
        /// Strips comparison operators and replaces all numbers with # for DB template lookup.
        /// </summary>
        public static string ToSkeleton(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            string normalized = OperatorNumberRegex.Replace(text, "#");
            normalized = BareNumberRegex.Replace(normalized, "#");
            normalized = WhitespaceRegex.Replace(normalized.Trim(), " ");
            return normalized;
        }

        /// <summary>
        /// Converts suggestion text to a # template for rule entry.
        /// </summary>
        public static string ToHashTemplate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return ToSkeleton(text);
        }
    }
}
