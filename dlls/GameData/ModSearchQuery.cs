namespace PoE.dlls.GameData
{
    internal static class ModSearchQuery
    {
        public static string[] SplitWords(string term) =>
            term.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        public static bool AllWordsMatch(string haystack, IReadOnlyList<string> words)
        {
            if (string.IsNullOrEmpty(haystack))
                return false;

            foreach (string word in words)
            {
                if (!haystack.Contains(word, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }
    }
}
