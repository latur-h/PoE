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
        private static readonly Regex NegateLineRegex = new(@"^\s*negate(?:\s+\d+)?\s*$", RegexOptions.IgnoreCase);
        private static readonly Regex LangHeaderRegex = new(
            @"\n[\s]*lang """,
            RegexOptions.Compiled);

        private static readonly Regex EnglishLangHeaderRegex = new(
            @"\n[\s]*lang ""English""",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex IntegerOnlyLineRegex = new(@"^\d+$", RegexOptions.None);

        public static (StatDescriptionCatalog Catalog, HashSet<string> MapStatIds) ParseEnglishTemplatesFromFiles(
            IReadOnlyList<(string Path, byte[] Bytes)> files)
        {
            var catalog = new StatDescriptionCatalog();
            var mapStatIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach ((string path, byte[] bytes) in files)
            {
                string text = Encoding.Unicode.GetString(bytes);
                if (text.Length > 0 && text[0] == '\uFEFF')
                    text = text[1..];

                var blocks = ParseFile(text).ToList();
                foreach (StatDescriptionBlock block in blocks)
                    catalog.AddBlock(block);

                bool isMapSource = path.Contains("map_stat_descriptions", StringComparison.OrdinalIgnoreCase)
                    || path.Contains("atlas_stat_descriptions", StringComparison.OrdinalIgnoreCase);

                if (!isMapSource)
                    continue;

                foreach (StatDescriptionBlock block in blocks)
                {
                    foreach (string statId in block.StatIds)
                    {
                        if (block.Translations.Any(t => IsUsefulTemplate(t.Template)))
                            mapStatIds.Add(statId);
                    }
                }
            }

            return (catalog, mapStatIds);
        }

        private static IEnumerable<StatDescriptionBlock> ParseFile(string data)
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
                {
                    StatDescriptionBlock? block = ParseDescriptionBlock(data, blockStart, blockEnd);
                    if (block is not null)
                        yield return block;
                }

                match = next;
            }
        }

        private static StatDescriptionBlock? ParseDescriptionBlock(string data, int blockStart, int blockEnd)
        {
            int offset = data.IndexOf('\n', blockStart);
            if (offset < 0 || offset >= blockEnd)
                return null;
            offset++;

            Match idCountMatch = IntRegex.Match(data, offset, blockEnd - offset);
            if (!idCountMatch.Success)
                return null;

            int idCount = int.Parse(idCountMatch.Value);
            offset = idCountMatch.Index + idCountMatch.Length;

            int lineEnd = data.IndexOf('\n', offset, blockEnd - offset);
            if (lineEnd < 0)
                lineEnd = blockEnd;

            string idLine = data[offset..lineEnd].Trim();
            var ids = IdTokenRegex.Matches(idLine).Select(m => m.Value).Take(idCount).ToList();
            if (ids.Count < idCount)
            {
                offset = lineEnd + 1;
                lineEnd = data.IndexOf('\n', offset, blockEnd - offset);
                if (lineEnd < 0)
                    lineEnd = blockEnd;

                idLine = data[offset..lineEnd].Trim();
                ids = IdTokenRegex.Matches(idLine).Select(m => m.Value).Take(idCount).ToList();
            }

            if (ids.Count == 0)
                return null;

            offset = lineEnd + 1;

            var translations = new List<StatTranslation>();
            (int defaultStart, int defaultEnd) = GetDefaultEnglishSection(data, offset, blockEnd);
            if (defaultEnd > defaultStart)
                ParseTranslationSection(data, defaultStart, defaultEnd, ids.Count, translations);

            (int explicitStart, int explicitEnd) = GetExplicitEnglishSection(data, offset, blockEnd);
            if (explicitEnd > explicitStart)
                ParseTranslationSection(data, explicitStart, explicitEnd, ids.Count, translations);

            if (translations.Count == 0)
                return null;

            return new StatDescriptionBlock
            {
                StatIds = ids,
                Translations = translations,
            };
        }

        private static (int Start, int End) GetDefaultEnglishSection(string data, int afterIds, int blockEnd)
        {
            Match langMatch = LangHeaderRegex.Match(data, afterIds, blockEnd - afterIds);
            int end = langMatch.Success && langMatch.Index < blockEnd ? langMatch.Index : blockEnd;

            int pos = afterIds;
            while (pos < end)
            {
                string line = ReadLine(data, pos, end, out int nextPos);
                pos = nextPos;
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("lang \"", StringComparison.OrdinalIgnoreCase))
                    return (afterIds, afterIds);

                break;
            }

            return (afterIds, end);
        }

        private static (int Start, int End) GetExplicitEnglishSection(string data, int afterIds, int blockEnd)
        {
            Match langMatch = EnglishLangHeaderRegex.Match(data, afterIds, blockEnd - afterIds);
            if (!langMatch.Success || langMatch.Index >= blockEnd)
                return (0, 0);

            int start = data.IndexOf('\n', langMatch.Index + 1, blockEnd - langMatch.Index - 1);
            if (start < 0)
                return (0, 0);
            start++;

            Match nextLang = LangHeaderRegex.Match(data, start, blockEnd - start);
            int end = nextLang.Success && nextLang.Index < blockEnd ? nextLang.Index : blockEnd;
            return (start, end);
        }

        private static void ParseTranslationSection(
            string data,
            int sectionStart,
            int sectionEnd,
            int statCount,
            List<StatTranslation> translations)
        {
            bool negate = false;
            int pos = sectionStart;
            while (pos < sectionEnd)
            {
                string line = ReadLine(data, pos, sectionEnd, out int nextPos);
                pos = nextPos;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (NegateLineRegex.IsMatch(line))
                {
                    negate = true;
                    continue;
                }

                if (line.StartsWith("canonical_line", StringComparison.OrdinalIgnoreCase)
                    || line.StartsWith("table_only", StringComparison.OrdinalIgnoreCase)
                    || line.StartsWith("lang \"", StringComparison.OrdinalIgnoreCase)
                    || IntegerOnlyLineRegex.IsMatch(line))
                {
                    continue;
                }

                Match translation = TranslationRegex.Match(line);
                if (!translation.Success)
                    continue;

                string template = SimplifyTemplate(translation.Groups["description"].Value);
                if (!IsUsefulTemplate(template))
                {
                    negate = false;
                    continue;
                }

                translations.Add(new StatTranslation
                {
                    Limits = StatLimit.ParseGroup(translation.Groups["minmax"].Value, statCount),
                    Negate = negate,
                    Template = template,
                });
                negate = false;
            }
        }

        private static string ReadLine(string data, int start, int end, out int nextPos)
        {
            int lineEnd = data.IndexOf('\n', start, end - start);
            if (lineEnd < 0)
                lineEnd = end;

            nextPos = lineEnd < end ? lineEnd + 1 : end;
            return data[start..lineEnd].Trim();
        }

        internal static bool IsUsefulTemplate(string template)
        {
            if (string.IsNullOrWhiteSpace(template))
                return false;

            if (template.StartsWith("No ", StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
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
