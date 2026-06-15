namespace PoE.dlls.GameData
{
    public sealed class ModSuggestionService
    {
        private readonly ModCacheDatabase _database;

        public ModSuggestionService(ModCacheDatabase database) => _database = database;

        public bool IsReady => _database.HasEntries;

        public IReadOnlyList<string> Search(string prefix, int limit = 20) => _database.Search(prefix, limit);
    }
}
