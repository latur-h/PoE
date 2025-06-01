using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Settings
{
    public class UIFlask
    {
        public bool Active { get; set; }
        public string Key { get; set; } = string.Empty;
        public string FlaskType { get; set; } = string.Empty;
        public int Percent { get; set; } = 0;
    }
}
