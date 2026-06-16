namespace PoE.dlls.GameData
{
    public sealed class ItemModSuggestionStrategy : IModSuggestionStrategy
    {
        public static ItemModSuggestionStrategy Instance { get; } = new();

        public IReadOnlyList<ModSuggestionItem> Search(ModCacheDatabase database, string term, int limit, int offset) =>
            database.SearchItemOnly(term, limit, offset);

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
