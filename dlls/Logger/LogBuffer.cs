namespace PoE.dlls.Logger
{
    public sealed class LogBuffer
    {
        public const int MaxEntries = 5000;

        private readonly object _lock = new();
        private readonly List<LogEntry> _entries = [];

        public event Action? Changed;

        public int Count
        {
            get
            {
                lock (_lock)
                    return _entries.Count;
            }
        }

        public IReadOnlyList<LogEntry> Snapshot()
        {
            lock (_lock)
                return _entries.ToList();
        }

        public void Add(LogEntry entry)
        {
            lock (_lock)
            {
                _entries.Add(entry);
                if (_entries.Count > MaxEntries)
                    _entries.RemoveRange(0, _entries.Count - MaxEntries);
            }

            Changed?.Invoke();
        }

        public void Clear()
        {
            lock (_lock)
                _entries.Clear();

            Changed?.Invoke();
        }
    }
}
