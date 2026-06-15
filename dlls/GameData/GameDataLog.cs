using PoE.dlls.Logger;

namespace PoE.dlls.GameData
{
    internal static class GameDataLog
    {
        public static void Info(string message) => AppLog.System(LogSeverity.Info, $"[Game data] {message}");

        public static void Error(string message) => AppLog.System(LogSeverity.Error, $"[Game data] {message}");

        public static void Error(string message, Exception ex)
        {
            Error($"{message} ({ex.GetType().Name}: {ex.Message})");
            if (ex.InnerException is not null)
                Error($"  Caused by: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
        }
    }
}
