using PoE.dlls.InteropServices;
using Poss.Win.Automation.Input;

namespace PoE.dlls.Flasks.Base
{
    internal interface IFlask
    {
        Flask Flask { get; set; }

        InputSimulator Input { get; }

        /// <summary>Last readiness from the drink-loop poll; does not read the screen.</summary>
        bool IsReady { get; }

        void UpdateReadiness(ScreenPixelCapture capture);

        Task Drink(CancellationToken cancellationToken);
    }
}
