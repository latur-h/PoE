using PoE.dlls.Automation;

namespace PoE.dlls.Gamble
{
    internal static class GambleInputReleaseHelper
    {
        public static void ReleaseAll(InputSimulatorHost inputHost)
        {
            var simulator = inputHost.Simulator;
            simulator.Send("LButton Up");
            simulator.Send("RButton Up");
            simulator.Send("Shift Up");
            simulator.Send("Ctrl Up");
            simulator.Send("Alt Up");
        }
    }
}
