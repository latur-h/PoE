using System.Reflection;
using System.Runtime.InteropServices;
using LibBundle3;
using LibBundle3.Records;
using LibBundledGGPK3;

namespace PoE.dlls.GameData
{
    internal sealed class GameArchiveSession : IDisposable
    {
        private static readonly object OodleLock = new();
        private static nint _oodleModule;
        private static bool _oodleResolverSet;

        // Item/map gamble autocomplete needs global stats, local weapon stats, map/atlas text, and cluster/passive lines.
        private static readonly string[] StatDescriptionFiles =
        [
            "metadata/statdescriptions/stat_descriptions.txt",
            "metadata/statdescriptions/weapon_stat_descriptions.txt",
            "metadata/statdescriptions/map_stat_descriptions.txt",
            "metadata/statdescriptions/atlas_stat_descriptions.txt",
            "metadata/statdescriptions/passive_skill_stat_descriptions.txt",
        ];

        private readonly string _gameFolder;
        private BundledGGPK? _ggpk;
        private LibBundle3.Index? _bundleIndex;
        private bool _disposed;

        [ThreadStatic]
        private static bool _oodleThreadInitialized;

        public GameArchiveSession(string gameFolder) => _gameFolder = gameFolder;

        public bool TryReadGameFile(string relativePath, out byte[] bytes, out string? source, out string? error)
        {
            bytes = [];
            source = null;
            error = null;

            string diskPath = Path.Combine(_gameFolder, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(diskPath))
            {
                try
                {
                    bytes = File.ReadAllBytes(diskPath);
                    source = diskPath;
                    return true;
                }
                catch (Exception ex)
                {
                    error = $"Failed to read file on disk '{diskPath}': {ex.GetType().Name}: {ex.Message}";
                    return false;
                }
            }

            try
            {
                EnsureArchiveOpen();
            }
            catch (Exception ex)
            {
                error = BuildArchiveOpenError(ex);
                return false;
            }

            LibBundle3.Index? index = _ggpk?.Index ?? _bundleIndex;
            if (index is null)
            {
                error = BuildMissingArchiveError(diskPath);
                return false;
            }

            string archiveLabel = _ggpk is not null
                ? Path.Combine(_gameFolder, "Content.ggpk")
                : Path.Combine(_gameFolder, "Bundles2", "_.index.bin");

            foreach (string path in GetPathVariants(relativePath))
            {
                if (!index.TryGetFile(path, out FileRecord? file))
                    continue;

                try
                {
                    ReadOnlyMemory<byte> content = file.Read();
                    bytes = content.ToArray();
                    source = $"{archiveLabel} → {path}";
                    return true;
                }
                catch (Exception ex)
                {
                    error = $"Failed to read '{path}' from '{archiveLabel}': {ex.GetType().Name}: {ex.Message}";
                    if (ex.InnerException is not null)
                        error += $" (inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message})";
                    return false;
                }
            }

            error = $"'{relativePath}' was not found on disk or in '{archiveLabel}'.";
            return false;
        }

        public bool TryReadRequiredDatFile(string relativePath, out byte[] bytes, out string? source, out string? error)
        {
            string? lastError = null;
            foreach (string path in ExpandDatPathCandidates(relativePath))
            {
                if (TryReadGameFile(path, out bytes, out source, out error) && bytes.Length > 0)
                    return true;

                lastError = error;
            }

            error = BuildCombinedReadError(relativePath, lastError);
            bytes = [];
            source = null;
            return false;
        }

        public IReadOnlyList<(string Path, byte[] Bytes)> ReadStatDescriptionFiles()
        {
            var files = new List<(string Path, byte[] Bytes)>();
            foreach (string path in StatDescriptionFiles)
            {
                if (TryReadGameFile(path, out byte[] bytes, out _, out string? error))
                {
                    if (bytes.Length > 0)
                        files.Add((path, bytes));
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(error))
                    GameDataLog.Error($"Could not read {path}: {error}");
            }

            return files;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _ggpk?.Dispose();
            _ggpk = null;
            _bundleIndex?.Dispose();
            _bundleIndex = null;
            _disposed = true;
        }

        private void EnsureArchiveOpen()
        {
            if (_ggpk is not null || _bundleIndex is not null)
                return;

            EnsureOodleThread();

            string ggpkPath = Path.Combine(_gameFolder, "Content.ggpk");
            string onDiskIndexPath = Path.Combine(_gameFolder, "Bundles2", "_.index.bin");

            if (File.Exists(ggpkPath))
            {
                GameDataLog.Info("Opening Content.ggpk and parsing bundle index…");
                _ggpk = new BundledGGPK(ggpkPath, parsePathsInIndex: false);
                int failedPaths = _ggpk.Index.ParsePaths();
                if (failedPaths > 0)
                    GameDataLog.Info($"Bundle index loaded with {failedPaths} unresolved path(s).");
                else
                    GameDataLog.Info("Bundle index loaded.");
                return;
            }

            if (File.Exists(onDiskIndexPath))
            {
                GameDataLog.Info("Reading Bundles2/_.index.bin from disk…");
                _bundleIndex = new LibBundle3.Index(onDiskIndexPath, parsePaths: true);
                GameDataLog.Info("Bundle index loaded.");
                return;
            }

            throw new FileNotFoundException(
                $"Neither '{ggpkPath}' nor '{onDiskIndexPath}' exists in the selected game folder.");
        }

        private void EnsureOodleThread()
        {
            EnsureOodleNative(_gameFolder);
            if (_oodleThreadInitialized)
                return;

            Oodle.Initialize(new Oodle.Settings { EnableCompressing = false });
            _oodleThreadInitialized = true;
        }

        private static void EnsureOodleNative(string gameFolder)
        {
            lock (OodleLock)
            {
                if (_oodleResolverSet)
                    return;

                string? dllPath = OodleNativeBootstrap.Resolve(gameFolder);
                if (dllPath is null)
                {
                    throw new DllNotFoundException(
                        "Could not find an Oodle library (oo2core.dll) in the game folder or bundled with this app.");
                }

                _oodleModule = NativeLibrary.Load(dllPath);
                NativeLibrary.SetDllImportResolver(typeof(Oodle).Assembly, ResolveOodleImport);
                _oodleResolverSet = true;
                GameDataLog.Info($"Loaded Oodle decompressor from {dllPath}.");
            }
        }

        private static nint ResolveOodleImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName == "oo2core")
                return _oodleModule;

            return NativeLibrary.Load(libraryName, assembly, searchPath);
        }

        private static IEnumerable<string> GetPathVariants(string relativePath)
        {
            string normalized = relativePath.Replace('\\', '/').TrimStart('/');
            yield return normalized;

            string lower = normalized.ToLowerInvariant();
            if (!string.Equals(lower, normalized, StringComparison.Ordinal))
                yield return lower;
        }

        private static IEnumerable<string> ExpandDatPathCandidates(string relativePath)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string variant in GetPathVariants(relativePath))
            {
                if (seen.Add(variant))
                    yield return variant;
            }

            string lower = relativePath.Replace('\\', '/').TrimStart('/').ToLowerInvariant();
            foreach (string extension in GetDatExtensionFallbacks(lower))
            {
                string candidate = ReplaceExtension(lower, extension);
                if (seen.Add(candidate))
                    yield return candidate;
            }
        }

        private static IEnumerable<string> GetDatExtensionFallbacks(string lowerPath)
        {
            if (lowerPath.EndsWith(".datc64", StringComparison.Ordinal))
            {
                yield return ".dat64";
                yield return ".dat";
            }
            else if (lowerPath.EndsWith(".dat64", StringComparison.Ordinal))
            {
                yield return ".datc64";
                yield return ".dat";
            }
            else if (lowerPath.EndsWith(".dat", StringComparison.Ordinal))
            {
                yield return ".datc64";
                yield return ".dat64";
            }
        }

        private static string ReplaceExtension(string path, string extension)
        {
            int lastSlash = path.LastIndexOf('/');
            int lastDot = path.LastIndexOf('.');
            if (lastDot <= lastSlash)
                return path + extension;

            return path[..lastDot] + extension;
        }

        private static string BuildMissingArchiveError(string diskPath)
        {
            string ggpkPath = Path.Combine(Path.GetDirectoryName(diskPath)!, "..", "Content.ggpk");
            ggpkPath = Path.GetFullPath(ggpkPath);
            return $"Neither '{diskPath}' nor a readable game archive ('{ggpkPath}') is available.";
        }

        private static string BuildArchiveOpenError(Exception ex)
        {
            string message = $"Failed to open the game archive: {ex.GetType().Name}: {ex.Message}";
            if (ex.InnerException is not null)
                message += $" (inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message})";
            return message;
        }

        private static string BuildCombinedReadError(string primaryPath, string? lastError)
        {
            var parts = new List<string> { $"Could not read '{primaryPath}'." };
            if (!string.IsNullOrWhiteSpace(lastError))
                parts.Add($"  {lastError}");
            parts.Add("  Tried .datc64, .dat64, and .dat variants (bundled installs use lowercase paths like data/mods.datc64).");
            return string.Join(Environment.NewLine, parts);
        }
    }
}
