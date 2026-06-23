using System.Diagnostics;
using System.Reflection;

namespace PoE.dlls.Updates
{
    internal static class AppUpdater
    {
        public const string UpdaterFileName = "PoE.Updater.exe";
        private const string EmbeddedUpdaterResourceName = "PoE.Updater.exe";

        public static string GetInstallDirectory()
        {
            string? processPath = Environment.ProcessPath;
            if (!string.IsNullOrWhiteSpace(processPath))
            {
                string? directory = Path.GetDirectoryName(processPath);
                if (!string.IsNullOrWhiteSpace(directory))
                    return directory;
            }

            return AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        public static string GetExecutablePath()
        {
            if (!string.IsNullOrWhiteSpace(Environment.ProcessPath))
                return Environment.ProcessPath;

            return Path.Combine(GetInstallDirectory(), "PoE.exe");
        }

        public static async Task DownloadReleaseAsync(
            GitHubReleaseInfo release,
            string destinationFile,
            IProgress<long>? progress,
            CancellationToken cancellationToken = default)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);

            using var client = GitHubReleaseClient.CreateClient();
            using HttpResponseMessage response = await client
                .GetAsync(release.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength;
            await using Stream source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using FileStream destination = new(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None);

            byte[] buffer = new byte[81920];
            long downloaded = 0;
            int read;
            while ((read = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false)) > 0)
            {
                await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                downloaded += read;
                progress?.Report(totalBytes is > 0 ? downloaded * 100 / totalBytes.Value : downloaded);
            }
        }

        public static void ScheduleApplyAndRestart(string zipPath, string installDirectory, string executablePath, int processId)
        {
            string updaterPath = PrepareUpdaterExecutable();

            var startInfo = new ProcessStartInfo
            {
                FileName = updaterPath,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            startInfo.ArgumentList.Add("--wait-pid");
            startInfo.ArgumentList.Add(processId.ToString());
            startInfo.ArgumentList.Add("--zip");
            startInfo.ArgumentList.Add(Path.GetFullPath(zipPath));
            startInfo.ArgumentList.Add("--install");
            startInfo.ArgumentList.Add(Path.GetFullPath(installDirectory));
            startInfo.ArgumentList.Add("--exe");
            startInfo.ArgumentList.Add(Path.GetFullPath(executablePath));

            Process? process = Process.Start(startInfo);
            if (process is null)
                throw new InvalidOperationException("Failed to start PoE.Updater.exe.");
        }

        public static string PrepareUpdaterExecutable()
        {
            string sidecarPath = Path.Combine(GetInstallDirectory(), UpdaterFileName);
            if (File.Exists(sidecarPath))
                return CopyUpdaterToTemp(sidecarPath);

            return ExtractEmbeddedUpdaterToTemp();
        }

        private static string CopyUpdaterToTemp(string sourcePath)
        {
            string destinationPath = CreateTempUpdaterPath();
            File.Copy(sourcePath, destinationPath, overwrite: true);
            return destinationPath;
        }

        private static string ExtractEmbeddedUpdaterToTemp()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream? stream = assembly.GetManifestResourceStream(EmbeddedUpdaterResourceName);
            if (stream is null)
            {
                throw new InvalidOperationException(
                    $"{UpdaterFileName} was not found next to the app and is not embedded in this build.");
            }

            string destinationPath = CreateTempUpdaterPath();
            using FileStream destination = File.Create(destinationPath);
            stream.CopyTo(destination);
            return destinationPath;
        }

        private static string CreateTempUpdaterPath() =>
            Path.Combine(Path.GetTempPath(), $"PoE.Updater-{Guid.NewGuid():N}.exe");
    }
}
