using Microsoft.Extensions.Logging;
using PoE.dlls.InteropServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Logger
{
    public class TextBoxLogger
    {
        private readonly TextBox _textBox;
        private readonly SynchronizationContext _syncContext;

        public TextBoxLogger(TextBox textBox)
        {
            _textBox = textBox;
            _syncContext = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
        }

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var prefix = $"{timestamp} | {level} | ";
            var full = prefix + message + Environment.NewLine;

            _syncContext.Post(_ =>
            {
                bool shouldScroll = IsAtBottom(_textBox);

                _textBox.AppendText(full);

                if (shouldScroll)
                    InteropHelper.ScrollToBottom(_textBox.Handle);
            }, null);
        }

        public void Info(string message) => Log(message, LogLevel.Info);
        public void Warn(string message) => Log(message, LogLevel.Warning);
        public void Error(string message) => Log(message, LogLevel.Error);

        private bool IsAtBottom(TextBox tb)
        {
            int visibleLines = tb.ClientSize.Height / tb.Font.Height;
            int totalLines = tb.GetLineFromCharIndex(tb.TextLength) + 1;
            int firstVisible = tb.GetLineFromCharIndex(tb.GetCharIndexFromPosition(new Point(0, 0)));
            return firstVisible + visibleLines >= totalLines - 1;
        }
    }
}
