using Poss.Win.Automation.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoE.dlls.Flasks.Base
{
    internal interface IFlask
    {
        Flask Flask { get; set; }

        InputSimulator Input { get; }

        bool IsReady { get; }

        Task Drink();
    }
}
