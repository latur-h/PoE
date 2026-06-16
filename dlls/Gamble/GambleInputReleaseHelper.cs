using PoE.dlls.Automation;

namespace PoE.dlls.Gamble
{
    internal static class GambleInputReleaseHelper
    {
        /// <summary>
        /// Releases keyboard modifiers only. Safe after a normal stop while the cursor is over the item
        /// (avoids synthetic LMB/RMB up acting as a click with an orb on the cursor).
        /// </summary>
        public static void ReleaseModifiers(InputSimulatorHost inputHost)
        {
            var simulator = inputHost.Simulator;
            simulator.Send("Shift Up");
            simulator.Send("Ctrl Up");
            simulator.Send("Alt Up");
        }

        /// <summary>
        /// Releases modifiers and mouse buttons. Use when gambling is interrupted mid-input.
        /// </summary>
        public static void ReleaseAll(InputSimulatorHost inputHost)
        {
            var simulator = inputHost.Simulator;
            simulator.Send("LButton Up");
            simulator.Send("RButton Up");
            ReleaseModifiers(inputHost);
        }
    }
}
