using System.Text.RegularExpressions;

namespace PoE.dlls.GameData
{
    internal sealed class StatDescriptionCatalog
    {
        private readonly Dictionary<string, List<StatDescriptionBlock>> _blocksByStatId =
            new(StringComparer.OrdinalIgnoreCase);

        public int StatCount => _blocksByStatId.Count;

        public void AddBlock(StatDescriptionBlock block)
        {
            foreach (string statId in block.StatIds)
            {
                if (!_blocksByStatId.TryGetValue(statId, out List<StatDescriptionBlock>? blocks))
                {
                    blocks = [];
                    _blocksByStatId[statId] = blocks;
                }

                blocks.Add(block);
            }
        }

        public bool HasUsefulTemplate(string statId) =>
            _blocksByStatId.TryGetValue(statId, out List<StatDescriptionBlock>? blocks)
            && blocks.Any(b => b.Translations.Any(t => StatDescriptionParser.IsUsefulTemplate(t.Template)));

        public IEnumerable<string> EnumerateStatIds() => _blocksByStatId.Keys;

        public string? ResolveTemplate(
            string statId,
            int statMin,
            int statMax,
            IReadOnlyDictionary<string, (int Min, int Max)> modStats)
        {
            if (!_blocksByStatId.TryGetValue(statId, out List<StatDescriptionBlock>? blocks))
                return null;

            string? best = TryResolve(blocks, statId, statMin, statMax, modStats, requireSiblingLimits: true);
            return best ?? TryResolve(blocks, statId, statMin, statMax, modStats, requireSiblingLimits: false);
        }

        private static string? TryResolve(
            List<StatDescriptionBlock> blocks,
            string statId,
            int statMin,
            int statMax,
            IReadOnlyDictionary<string, (int Min, int Max)> modStats,
            bool requireSiblingLimits)
        {
            string? best = null;
            foreach (StatDescriptionBlock block in blocks)
            {
                int statIndex = block.StatIds.FindIndex(id => string.Equals(id, statId, StringComparison.OrdinalIgnoreCase));
                if (statIndex < 0)
                    continue;

                foreach (StatTranslation translation in block.Translations)
                {
                    if (requireSiblingLimits && !TranslationMatchesMod(block, translation, modStats))
                        continue;

                    if (!ValueRangeMatchesLimit(statMin, statMax, translation.Limits[statIndex]))
                        continue;

                    if (best is null || IsBetterResolvedTemplate(translation.Template, best))
                        best = translation.Template;
                }
            }

            return best;
        }

        private static bool TranslationMatchesMod(
            StatDescriptionBlock block,
            StatTranslation translation,
            IReadOnlyDictionary<string, (int Min, int Max)> modStats)
        {
            for (int i = 0; i < block.StatIds.Count; i++)
            {
                string blockStatId = block.StatIds[i];
                if (modStats.TryGetValue(blockStatId, out (int Min, int Max) range))
                {
                    if (!ValueRangeMatchesLimit(range.Min, range.Max, translation.Limits[i]))
                        return false;
                }
                else if (!IsExactZeroLimit(translation.Limits[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ValueRangeMatchesLimit(int modMin, int modMax, StatLimit limit)
        {
            if (limit.NotZero)
                return modMin != 0 || modMax != 0;

            if (IsExactZeroLimit(limit))
                return modMin == 0 && modMax == 0;

            int limitMin = limit.Min ?? int.MinValue / 2;
            int limitMax = limit.Max ?? int.MaxValue / 2;

            return modMin <= limitMax && modMax >= limitMin;
        }

        private static bool IsExactZeroLimit(StatLimit limit) =>
            limit is { NotZero: false, Min: 0, Max: 0 };

        private static bool IsBetterResolvedTemplate(string candidate, string existing)
        {
            bool candidateCombined = candidate.Contains("to this value when used", StringComparison.OrdinalIgnoreCase);
            bool existingCombined = existing.Contains("to this value when used", StringComparison.OrdinalIgnoreCase);
            if (candidateCombined != existingCombined)
                return !candidateCombined;

            bool candidateLatin = ContainsLatinLetters(candidate);
            bool existingLatin = ContainsLatinLetters(existing);
            if (candidateLatin != existingLatin)
                return candidateLatin;

            return candidate.Length > existing.Length;
        }

        private static bool ContainsLatinLetters(string text) =>
            text.Any(static c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z');
    }

    internal sealed class StatDescriptionBlock
    {
        public required List<string> StatIds { get; init; }

        public required List<StatTranslation> Translations { get; init; }
    }

    internal sealed class StatTranslation
    {
        public required List<StatLimit> Limits { get; init; }

        public bool Negate { get; init; }

        public required string Template { get; init; }
    }

    internal sealed class StatLimit
    {
        public int? Min { get; init; }

        public int? Max { get; init; }

        public bool NotZero { get; init; }

        public static StatLimit ParseToken(string token)
        {
            if (token.StartsWith('!'))
            {
                return new StatLimit
                {
                    NotZero = true,
                    Min = int.TryParse(token[1..], out int compareValue) ? compareValue : 0,
                    Max = int.TryParse(token[1..], out int compareValue2) ? compareValue2 : 0,
                };
            }

            int pipe = token.IndexOf('|');
            if (pipe >= 0)
            {
                string left = token[..pipe];
                string right = token[(pipe + 1)..];
                return new StatLimit
                {
                    Min = ParseBound(left),
                    Max = ParseBound(right),
                };
            }

            if (int.TryParse(token, out int exact))
                return new StatLimit { Min = exact, Max = exact };

            return new StatLimit();
        }

        private static int? ParseBound(string token) =>
            token switch
            {
                "#" => null,
                _ when int.TryParse(token, out int value) => value,
                _ => null,
            };

        public static List<StatLimit> ParseGroup(string minmax, int statCount)
        {
            var tokens = Regex.Matches(minmax, @"[!]?[\d#|\-]+")
                .Select(m => m.Value)
                .Where(t => t.Length > 0)
                .ToList();

            var limits = new List<StatLimit>(statCount);
            int tokenIndex = 0;
            for (int statIndex = 0; statIndex < statCount; statIndex++)
            {
                if (tokenIndex >= tokens.Count)
                {
                    limits.Add(new StatLimit { Min = 0, Max = 0 });
                    continue;
                }

                bool isLastStat = statIndex == statCount - 1;
                if (isLastStat
                    && tokens.Count - tokenIndex == 2
                    && tokens[tokenIndex] == "0"
                    && tokens[tokenIndex + 1] == "0")
                {
                    limits.Add(new StatLimit { Min = 0, Max = 0 });
                    break;
                }

                limits.Add(ParseToken(tokens[tokenIndex++]));
            }

            while (limits.Count < statCount)
                limits.Add(new StatLimit { Min = 0, Max = 0 });

            return limits;
        }
    }
}
