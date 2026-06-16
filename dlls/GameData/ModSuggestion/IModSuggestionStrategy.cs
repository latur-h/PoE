namespace PoE.dlls.GameData
{
    public interface IModSuggestionStrategy
    {
        IReadOnlyList<ModSuggestionItem> Search(ModCacheDatabase database, string term, int limit, int offset);

        string FormatDisplay(ModSuggestionItem item, string searchTerm);

        string FormatInsert(ModSuggestionItem item, string searchTerm);

        int SuggestionPopupMinWidth { get; }
    }
}
