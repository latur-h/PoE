namespace PoE.dlls.Logger
{
    public static class GamblerLog
    {
        public static void Info(string message) => AppLog.Gambler(LogSeverity.Info, message);
        public static void Success(string message = "Item matches the rules") => AppLog.Gambler(LogSeverity.Ok, message);
        public static void Warn(string message) => AppLog.Gambler(LogSeverity.Warn, message);
        public static void Error(string message) => AppLog.Gambler(LogSeverity.Error, message);
        public static void Debug(string message) => AppLog.Gambler(LogSeverity.Debug, message);

        public static void Cancelled() => Warn("Gambling was cancelled");
        public static void ClipboardEmptyWarning() =>
            Warn("Failed to get item content from clipboard. Try to increase the delay between actions if error is persist.");
        public static void MaxAttemptsReached() => Error("Maximum attempts reached. Cancelling.");
        public static void DebugSeparator() => Debug("—");

        public static void DebugMod(object type, int tier, string name, string content) =>
            Debug($"Type={type}, Tier={tier}, Name={name}, Content={content}");
    }
}
