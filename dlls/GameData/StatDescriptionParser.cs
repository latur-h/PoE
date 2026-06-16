using System.Text;
using System.Text.RegularExpressions;

namespace PoE.dlls.GameData
{
    internal static class StatDescriptionParser
    {
        private static readonly Regex TokenRegex = new(
            @"(?:^""(?<header>.*)""$)|(?:^include ""(?<include>.*)"")|(?:^no_description (?<no_description>[\w+%]*)$)|(?<description>^description[\s]*(?<identifier>[\S]*)[\s]*$)",
            RegexOptions.Multiline);

        private static readonly Regex TranslationRegex = new(
            @"^[\s]*(?<minmax>(?:[0-9\-\|#!]+[ \t]+)+)""(?<description>.*)""(?<quantifier>(?:[ \t]*[\w%]+)*)[\s]*$",
            RegexOptions.Multiline);

        private static readonly Regex IdTokenRegex = new(@"([\S]+)", RegexOptions.None);
        private static readonly Regex IntRegex = new(@"[0-9]+", RegexOptions.None);

        public static Dictionary<string, string> ParseEnglishTemplates(IEnumerable<byte[]> files)
        {
            (Dictionary<string, string> templates, _) = ParseEnglishTemplatesFromFiles(
                files.Select(bytes => (string.Empty, bytes)).ToList());
            return templates;
        }

        public static (Dictionary<string, string> Templates, HashSet<string> MapStatIds) ParseEnglishTemplatesFromFiles(
            IReadOnlyList<(string Path, byte[] Bytes)> files)
        {
            var templates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var mapStatIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach ((string path, byte[] bytes) in files)
            {
                var perFile = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                string text = Encoding.Unicode.GetString(bytes);
                if (text.Length > 0 && text[0] == '\uFEFF')
                    text = text[1..];

                ParseFile(text, perFile);

                bool isMapSource = path.Contains("map_stat_descriptions", StringComparison.OrdinalIgnoreCase)
                    || path.Contains("atlas_stat_descriptions", StringComparison.OrdinalIgnoreCase);

                foreach ((string id, string template) in perFile)
                {
                    TryAssignTemplate(templates, id, template);
                    if (isMapSource && IsUsefulTemplate(template))
                        mapStatIds.Add(id);
                }
            }

            return (templates, mapStatIds);
        }

        private static void ParseFile(string data, Dictionary<string, string> map)
        {
            int offset = 0;
            Match match = TokenRegex.Match(data, offset);
            while (match.Success)
            {
                int blockStart = match.Index;
                offset = match.Index + match.Length;
                Match next = TokenRegex.Match(data, offset);
                int blockEnd = next.Success ? next.Index : data.Length;

                if (match.Groups["description"].Success)
                    ParseDescriptionBlock(data, blockStart, blockEnd, map);

                match = next;
            }
        }

        private static void ParseDescriptionBlock(string data, int blockStart, int blockEnd, Dictionary<string, string> map)
        {
            int offset = data.IndexOf('\n', blockStart);
            if (offset < 0 || offset >= blockEnd)
                return;
            offset++;

            Match idCountMatch = IntRegex.Match(data, offset, blockEnd - offset);
            if (!idCountMatch.Success)
                return;

            int idCount = int.Parse(idCountMatch.Value);
            offset = idCountMatch.Index + idCountMatch.Length;

            int lineEnd = data.IndexOf('\n', offset, blockEnd - offset);
            if (lineEnd < 0)
                lineEnd = blockEnd;

            string idLine = data[offset..lineEnd];
            var ids = IdTokenRegex.Matches(idLine).Select(m => m.Value).Take(idCount).ToList();
            offset = lineEnd + 1;

            Match langCountMatch = IntRegex.Match(data, offset, blockEnd - offset);
            if (!langCountMatch.Success)
                return;

            int translationCount = int.Parse(langCountMatch.Value);
            offset = langCountMatch.Index + langCountMatch.Length;

            int englishStart = offset;
            int englishEnd = blockEnd;
            int langIndex = data.IndexOf("lang \"English\"", offset, StringComparison.Ordinal);
            if (langIndex >= 0 && langIndex < blockEnd)
            {
                englishStart = data.IndexOf('\n', langIndex);
                if (englishStart < 0)
                    return;
                englishStart++;

                int nextLang = data.IndexOf("\nlang \"", englishStart, StringComparison.Ordinal);
                if (nextLang >= 0 && nextLang < blockEnd)
                    englishEnd = nextLang;
            }
            else
            {
                int nextLang = data.IndexOf("\nlang \"", offset, StringComparison.Ordinal);
                if (nextLang >= 0 && nextLang < blockEnd)
                    englishEnd = nextLang;
            }

            Match translation = TranslationRegex.Match(data, englishStart, englishEnd - englishStart);
            for (int i = 0; i < translationCount && translation.Success; i++)
            {
                string template = SimplifyTemplate(translation.Groups["description"].Value);
                foreach (string id in ids)
                    TryAssignTemplate(map, id, template);

                englishStart = translation.Index + translation.Length;
                translation = TranslationRegex.Match(data, englishStart, englishEnd - englishStart);
            }
        }

        private static void TryAssignTemplate(Dictionary<string, string> map, string id, string template)
        {
            if (!IsUsefulTemplate(template))
                return;

            if (!map.TryGetValue(id, out string? existing) || IsBetterTemplate(template, existing))
                map[id] = template;
        }

        private static bool IsUsefulTemplate(string template)
        {
            if (string.IsNullOrWhiteSpace(template))
                return false;

            if (template.StartsWith("No ", StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        private static bool IsBetterTemplate(string candidate, string existing)
        {
            if (!IsUsefulTemplate(existing))
                return true;

            bool candidateHasValue = candidate.Contains('#') || candidate.Contains('%');
            bool existingHasValue = existing.Contains('#') || existing.Contains('%');
            if (candidateHasValue != existingHasValue)
                return candidateHasValue;

            return candidate.Length > existing.Length;
        }

        internal static string SimplifyTemplate(string template)
        {
            if (string.IsNullOrWhiteSpace(template))
                return string.Empty;

            string text = Regex.Replace(template, @"\{[0-9]+(?::[^}]+)?\}", "#");
            text = Regex.Replace(text, @"\(\d+-\d+\)", string.Empty);
            return text.Trim();
        }
    }
}
