using System.Text;

namespace PoE.dlls.Logger
{
    public sealed class ConsoleLogger : TextWriter
    {
        private readonly LogBuffer _buffer;

        public override Encoding Encoding => Encoding.UTF8;

        public ConsoleLogger(LogBuffer buffer) => _buffer = buffer;

        public override void WriteLine(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            var entry = ConsoleLogParser.Parse(value);
            if (entry is not null)
                _buffer.Add(entry);
        }
    }
}
