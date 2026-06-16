namespace PoE.dlls.GameData
{
    public sealed class ModSuggestionService
    {
        private readonly ModCacheDatabase _database;

        public ModSuggestionService(ModCacheDatabase database) => _database = database;

        public bool IsReady => _database.HasEntries;

        public IReadOnlyList<ModSuggestionItem> Search(
            string term,
            IModSuggestionStrategy strategy,
            int limit = 50,
            int offset = 0) =>
            strategy.Search(_database, term, limit, offset);
    }
}
