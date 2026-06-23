using System.Diagnostics;
using System.IO.Compression;

namespace PoE.Updater;

internal static class Program
{
    private const int MaxWaitSeconds = 120;

    static int Main(string[] args)
    {
        try
        {
            UpdateOptions options = UpdateOptions.Parse(args);
            RunUpdate(options);
            return 0;
        }
        catch (Exception ex)
        {
            string logPath = Path.Combine(Path.GetTempPath(), "PoE-updater-error.txt");
            File.WriteAllText(logPath, ex.ToString());
            return 1;
        }
    }

    private static void RunUpdate(UpdateOptions options)
    {
        WaitForProcessExit(options.WaitProcessId);
        Thread.Sleep(1000);

        if (!File.Exists(options.ZipPath))
            throw new FileNotFoundException("Update package was not found.", options.ZipPath);

        Directory.CreateDirectory(options.InstallDirectory);
        ZipFile.ExtractToDirectory(options.ZipPath, options.InstallDirectory, overwriteFiles: true);

        Process.Start(new ProcessStartInfo
        {
            FileName = options.ExecutablePath,
            WorkingDirectory = options.InstallDirectory,
            UseShellExecute = true,
        });

        TryDelete(options.ZipPath);
        TryDeleteSelfIfTemporary();
    }

    private static void WaitForProcessExit(int processId)
    {
        for (int attempt = 0; attempt < MaxWaitSeconds; attempt++)
        {
            try
            {
                using Process process = Process.GetProcessById(processId);
                if (process.HasExited)
                    return;

                if (!process.WaitForExit(1000))
                    continue;

                return;
            }
            catch (ArgumentException)
            {
                return;
            }
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static void TryDeleteSelfIfTemporary()
    {
        string? processPath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(processPath))
            return;

        string tempRoot = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string fullPath = Path.GetFullPath(processPath);
        if (!fullPath.StartsWith(tempRoot, StringComparison.OrdinalIgnoreCase))
            return;

        TryDelete(fullPath);
    }
}

internal sealed class UpdateOptions
{
    public required int WaitProcessId { get; init; }

    public required string ZipPath { get; init; }

    public required string InstallDirectory { get; init; }

    public required string ExecutablePath { get; init; }

    public static UpdateOptions Parse(string[] args)
    {
        int waitProcessId = 0;
        string? zipPath = null;
        string? installDirectory = null;
        string? executablePath = null;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            switch (arg)
            {
                case "--wait-pid" when i + 1 < args.Length && int.TryParse(args[++i], out waitProcessId):
                    break;
                case "--zip" when i + 1 < args.Length:
                    zipPath = args[++i];
                    break;
                case "--install" when i + 1 < args.Length:
                    installDirectory = args[++i];
                    break;
                case "--exe" when i + 1 < args.Length:
                    executablePath = args[++i];
                    break;
                default:
                    throw new ArgumentException($"Unknown or incomplete argument: {arg}");
            }
        }

        if (waitProcessId <= 0)
            throw new ArgumentException("Missing or invalid --wait-pid.");

        if (string.IsNullOrWhiteSpace(zipPath))
            throw new ArgumentException("Missing --zip.");

        if (string.IsNullOrWhiteSpace(installDirectory))
            throw new ArgumentException("Missing --install.");

        if (string.IsNullOrWhiteSpace(executablePath))
            throw new ArgumentException("Missing --exe.");

        return new UpdateOptions
        {
            WaitProcessId = waitProcessId,
            ZipPath = Path.GetFullPath(zipPath),
            InstallDirectory = Path.GetFullPath(installDirectory),
            ExecutablePath = Path.GetFullPath(executablePath),
        };
    }
}
