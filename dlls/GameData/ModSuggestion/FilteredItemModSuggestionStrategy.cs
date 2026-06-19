namespace PoE.dlls.GameData
{
    public sealed class FilteredItemModSuggestionStrategy : IModSuggestionStrategy
    {
        private readonly string _spawnTagFilter;

        public FilteredItemModSuggestionStrategy(string spawnTagFilter) =>
            _spawnTagFilter = ModSpawnTagFilter.Normalize(spawnTagFilter) ?? string.Empty;

        public IReadOnlyList<ModSuggestionItem> Search(ModCacheDatabase database, string term, int limit, int offset) =>
            database.SearchItemOnly(term, _spawnTagFilter, limit, offset);

        public string FormatDisplay(ModSuggestionItem item, string searchTerm) =>
            item.GetDisplayText(searchTerm);

        public string FormatInsert(ModSuggestionItem item, string searchTerm)
        {
            string text = string.IsNullOrEmpty(item.ModContent) ? item.ModName : item.ModContent;
            return ModTemplateNormalizer.ToHashTemplate(text);
        }

        public int SuggestionPopupMinWidth => 360;
    }
}
