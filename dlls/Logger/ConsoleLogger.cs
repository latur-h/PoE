using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Logger
{
    public class ConsoleLogger : TextWriter
    {
        private readonly TextBoxLogger _logger;
        public override Encoding Encoding => Encoding.UTF8;

        public ConsoleLogger(TextBoxLogger logger)
        {
            _logger = logger;
        }

        public override void WriteLine(string? value)
        {
            if (!string.IsNullOrEmpty(value))
                _logger.Info(value);
        }
    }
}
