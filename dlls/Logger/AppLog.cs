namespace PoE.dlls.Logger
{
    public static class AppLog
    {
        public static LogBuffer Buffer { get; } = new();

        public static void Write(LogCategory category, LogSeverity severity, string message) =>
            Buffer.Add(new LogEntry(DateTime.Now, category, severity, message));

        public static void Gambler(LogSeverity severity, string message) => Write(LogCategory.Gambler, severity, message);
        public static void Flask(LogSeverity severity, string message) => Write(LogCategory.Flask, severity, message);
        public static void System(LogSeverity severity, string message) => Write(LogCategory.System, severity, message);
    }
}
