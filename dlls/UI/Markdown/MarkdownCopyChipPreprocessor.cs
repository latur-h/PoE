using System.Net;
using System.Text.RegularExpressions;

namespace PoE.dlls.UI.Markdown
{
    public static partial class MarkdownCopyChipPreprocessor
    {
        [GeneratedRegex(@"\[\[copy\]\s*([^\]]+)\]", RegexOptions.IgnoreCase)]
        private static partial Regex CopyChipPattern();

        public static string ExpandCopyChips(string markdown) =>
            CopyChipPattern().Replace(markdown, match =>
            {
                string text = match.Groups[1].Value.Trim();
                if (text.Length == 0)
                    return match.Value;

                string encoded = WebUtility.HtmlEncode(text);
                return $"""<span class="copy-chip" data-copy="{encoded}" title="Click to copy">{encoded}<span class="copy-icon">&#x29C9;</span></span>""";
            });
    }
}
