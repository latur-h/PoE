namespace PoE.dlls.GameData
{
    public sealed class ModSuggestionService
    {
        private readonly ModCacheDatabase _database;

        public ModSuggestionService(ModCacheDatabase database) => _database = database;

        public bool IsReady => _database.HasEntries;

        public IReadOnlyList<ModSuggestionItem> Search(
            string term,
            ModSuggestionScope scope = ModSuggestionScope.All,
            int limit = 50,
            int offset = 0) =>
            scope == ModSuggestionScope.MapOnly
                ? _database.SearchMapGrouped(term, limit, offset)
                : _database.Search(term, limit, offset);
    }
}
