namespace PoE.dlls.GameData
{
    internal static class OodleNativeBootstrap
    {
        internal static readonly string[] DllNames =
        [
            "oo2core_8_win64.dll",
            "oo2core_7_win64.dll",
            "oo2core_6_win64.dll",
            "oo2core.dll",
        ];

        /// <summary>
        /// Loads Oodle from the app directory (shipped beside PoE.exe).
        /// Falls back to the game install folder if the user already has a copy there.
        /// </summary>
        public static string? Resolve(string gameFolder) =>
            FindInFolder(AppContext.BaseDirectory) ?? FindInFolder(gameFolder);

        private static string? FindInFolder(string folder)
        {
            foreach (string name in DllNames)
            {
                string path = Path.Combine(folder, name);
                if (File.Exists(path))
                    return path;
            }

            return null;
        }
    }
}
