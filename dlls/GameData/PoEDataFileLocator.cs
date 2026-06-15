namespace PoE.dlls.GameData
{
    internal static class PoEDataFileLocator
    {
        public static bool TryReadGameFile(GameArchiveSession session, string relativePath, out byte[] bytes, out string? source, out string? error) =>
            session.TryReadGameFile(relativePath, out bytes, out source, out error);

        public static bool TryReadGameFile(GameArchiveSession session, string relativePath, out byte[] bytes, out string? source) =>
            TryReadGameFile(session, relativePath, out bytes, out source, out _);

        public static bool TryReadRequiredDatFile(GameArchiveSession session, string relativePath, out byte[] bytes, out string? source, out string? error) =>
            session.TryReadRequiredDatFile(relativePath, out bytes, out source, out error);

        public static bool TryReadRequiredDatFile(GameArchiveSession session, string relativePath, out byte[] bytes, out string? source) =>
            TryReadRequiredDatFile(session, relativePath, out bytes, out source, out _);

        public static IReadOnlyList<(string Path, byte[] Bytes)> ReadStatDescriptionFiles(GameArchiveSession session) =>
            session.ReadStatDescriptionFiles();
    }
}
