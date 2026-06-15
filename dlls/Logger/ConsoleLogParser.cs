namespace PoE.dlls.Logger
{
    internal static class ConsoleLogParser
    {
        public static LogEntry? Parse(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            string text = line.Trim();

            if (IsSeparator(text))
                return new LogEntry(DateTime.Now, LogCategory.Gambler, LogSeverity.Debug, "—");

            if (text.StartsWith("[Gambler]", StringComparison.Ordinal))
                return ParseGambler(text);

            if (IsGamblerDebugLine(text))
                return new LogEntry(DateTime.Now, LogCategory.Gambler, LogSeverity.Debug, text);

            if (IsFlaskLine(text))
                return new LogEntry(DateTime.Now, LogCategory.Flask, LogSeverity.Info, text);

            if (IsGamblerStatLine(text))
                return new LogEntry(DateTime.Now, LogCategory.Gambler, LogSeverity.Info, text);

            if (IsGamblerTraceLine(text))
                return new LogEntry(DateTime.Now, LogCategory.Gambler, LogSeverity.Debug, text);

            if (text.Contains("Error", StringComparison.OrdinalIgnoreCase))
                return new LogEntry(DateTime.Now, LogCategory.System, LogSeverity.Error, text);

            return new LogEntry(DateTime.Now, LogCategory.System, LogSeverity.Info, text);
        }

        private static LogEntry ParseGambler(string text)
        {
            string body = text["[Gambler]".Length..].Trim();

            if (body.StartsWith("[Success]", StringComparison.Ordinal))
                return Entry(LogCategory.Gambler, LogSeverity.Ok, body["[Success]".Length..].Trim());

            if (body.StartsWith("[Cancelled]", StringComparison.Ordinal))
                return Entry(LogCategory.Gambler, LogSeverity.Warn, body["[Cancelled]".Length..].Trim());

            if (body.StartsWith("[Warning]", StringComparison.Ordinal))
                return Entry(LogCategory.Gambler, LogSeverity.Warn, body["[Warning]".Length..].Trim());

            if (body.StartsWith("[Failed]", StringComparison.Ordinal))
                return Entry(LogCategory.Gambler, LogSeverity.Error, body["[Failed]".Length..].Trim());

            LogSeverity severity = body.Contains("Failed", StringComparison.OrdinalIgnoreCase) ||
                                   body.Contains("Maximum attempts", StringComparison.OrdinalIgnoreCase)
                ? LogSeverity.Error
                : LogSeverity.Info;

            return Entry(LogCategory.Gambler, severity, body);
        }

        private static bool IsGamblerDebugLine(string text) =>
            text.StartsWith("Type=", StringComparison.Ordinal) ||
            text.StartsWith("Enchants count", StringComparison.OrdinalIgnoreCase);

        private static bool IsFlaskLine(string text) =>
            text.Contains("flask", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("drinking", StringComparison.OrdinalIgnoreCase);

        private static bool IsGamblerStatLine(string text) =>
            text.Contains("vs", StringComparison.Ordinal) &&
            (text.StartsWith('Q') || text.Contains(";R", StringComparison.Ordinal) || text.Contains("PS", StringComparison.Ordinal));

        private static bool IsGamblerTraceLine(string text) =>
            int.TryParse(text, out _) ||
            (text.Contains("vs", StringComparison.Ordinal) && text.All(c => char.IsDigit(c) || c is 'v' or 's'));

        private static bool IsSeparator(string text) =>
            text.Length >= 4 && text.All(c => c == '-');

        private static LogEntry Entry(LogCategory category, LogSeverity severity, string message) =>
            new(DateTime.Now, category, severity, string.IsNullOrWhiteSpace(message) ? "—" : message);
    }
}
