namespace PoE.dlls.Logger
{
    public sealed class LogEntry(DateTime timestamp, LogCategory category, LogSeverity severity, string message)
    {
        public DateTime Timestamp { get; } = timestamp;
        public LogCategory Category { get; } = category;
        public LogSeverity Severity { get; } = severity;
        public string Message { get; } = message;

        public string TimeText => Timestamp.ToString("HH:mm:ss");

        public string CategoryCode => Category switch
        {
            LogCategory.Gambler => "GMB",
            LogCategory.Flask => "FLK",
            LogCategory.System => "SYS",
            _ => "???",
        };

        public string SeverityCode => Severity switch
        {
            LogSeverity.Debug => "DBG",
            LogSeverity.Info => "INF",
            LogSeverity.Ok => "OK ",
            LogSeverity.Warn => "WRN",
            LogSeverity.Error => "ERR",
            _ => "???",
        };

        public string FormatLine() => $"{TimeText}  {CategoryCode}  {SeverityCode}  {Message}";

        public bool MatchesSearch(string search) =>
            string.IsNullOrEmpty(search) ||
            Message.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            CategoryCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            SeverityCode.Contains(search, StringComparison.OrdinalIgnoreCase);
    }
}
