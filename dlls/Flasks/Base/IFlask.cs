using InputSimulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Flasks.Base
{
    internal interface IFlask
    {
        public Flask Flask { get; set; }
        public Simulator Input { get; set; }

        public Task Drink();
    }
}
