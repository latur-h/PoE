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

        public bool SpawnTagExists(string tag) => _database.SpawnTagExists(tag);

        public bool SpawnTagExists(string tag, bool eldritchArmourOnly) =>
            eldritchArmourOnly
                ? _database.SpawnTagExistsForEldritchArmour(tag)
                : _database.SpawnTagExists(tag);

        public IReadOnlyList<string> SearchSpawnTags(string term, int limit = 50) =>
            _database.SearchSpawnTags(term, limit);

        public IReadOnlyList<string> SearchSpawnTags(string term, int limit, bool eldritchArmourOnly) =>
            _database.SearchSpawnTags(term, limit, eldritchArmourOnly);
    }
}
