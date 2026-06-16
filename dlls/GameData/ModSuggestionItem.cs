namespace PoE.dlls.GameData
{
    public sealed class ModSuggestionItem
    {
        public required string ModName { get; init; }
        public required string ModContent { get; init; }

        public string GetDisplayText(string searchTerm, ModSuggestionBehavior? behavior = null)
        {
            behavior ??= ModSuggestionBehavior.Default;
            if (behavior.ShowNameAndDescription)
                return FormatNameAndDescription();

            return GetDisplayText(searchTerm);
        }

        public string GetInsertText(string searchTerm, ModSuggestionBehavior? behavior = null)
        {
            behavior ??= ModSuggestionBehavior.Default;
            if (behavior.InsertModNameOnly)
                return ModName;

            return GetInsertText(searchTerm);
        }

        private string FormatNameAndDescription()
        {
            if (string.IsNullOrEmpty(ModContent))
                return ModName;

            return $"{ModName} — {ModContent}";
        }

        public string GetDisplayText(string searchTerm)
        {
            string[] words = ModSearchQuery.SplitWords(searchTerm);
            if (words.Length == 0)
                return string.IsNullOrEmpty(ModContent) ? ModName : ModContent;

            bool contentHit = !string.IsNullOrEmpty(ModContent)
                && ModSearchQuery.AllWordsMatch(ModContent, words);
            bool nameHit = ModSearchQuery.AllWordsMatch(ModName, words);

            if (contentHit)
                return ModContent;

            if (nameHit)
                return ModName;

            return string.IsNullOrEmpty(ModContent) ? ModName : ModContent;
        }

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
