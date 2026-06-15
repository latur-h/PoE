namespace PoE.dlls.GameData
{
    public class GameDataSettings
    {
        public string GameFolderPath { get; set; } = string.Empty;
        public DateTime? ModCacheRefreshedUtc { get; set; }
        public int ModCacheEntryCount { get; set; }
    }
}
