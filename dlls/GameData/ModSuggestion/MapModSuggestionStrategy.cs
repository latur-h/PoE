namespace PoE.dlls.GameData
{
    public sealed class MapModSuggestionStrategy : IModSuggestionStrategy
    {
        public static MapModSuggestionStrategy Instance { get; } = new();

        public IReadOnlyList<ModSuggestionItem> Search(ModCacheDatabase database, string term, int limit, int offset) =>
            database.SearchMapGrouped(term, limit, offset);

        public string FormatDisplay(ModSuggestionItem item, string searchTerm)
        {
            if (string.IsNullOrEmpty(item.ModContent))
                return item.ModName;

            return $"{item.ModName} — {item.ModContent}";
        }

        public string FormatInsert(ModSuggestionItem item, string searchTerm) => item.ModName;

        public int SuggestionPopupMinWidth => 560;
    }
}
