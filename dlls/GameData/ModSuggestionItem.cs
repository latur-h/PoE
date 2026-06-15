namespace PoE.dlls.GameData
{
    public sealed class ModSuggestionItem
    {
        public required string ModName { get; init; }
        public required string ModContent { get; init; }

        public string DisplayText => string.IsNullOrEmpty(ModContent)
            ? ModName
            : $"{ModName} — {ModContent}";

        public string GetInsertText(string searchTerm)
        {
            string[] words = ModSearchQuery.SplitWords(searchTerm);
            if (words.Length == 0)
                return string.IsNullOrEmpty(ModContent) ? ModName : ModContent;

            bool contentHit = ModSearchQuery.AllWordsMatch(ModContent, words);
            bool nameHit = ModSearchQuery.AllWordsMatch(ModName, words);

            if (contentHit)
                return ModContent;

            if (nameHit)
                return ModName;

            return string.IsNullOrEmpty(ModContent) ? ModName : ModContent;
        }
    }
}
