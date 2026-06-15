using Poss.Win.Automation.Input;

namespace PoE.dlls.Automation
{
    /// <summary>
    /// Models how gamble/flask code resolves <see cref="InputSimulator"/> during a session.
    /// <paramref name="captureAtConstruction"/> mirrors the old bug (captured reference goes stale after <see cref="InputSimulatorHost.Configure"/>).
    /// </summary>
    internal sealed class SessionSimulatorBinding
    {
        private readonly InputSimulatorHost _host;
        private readonly InputSimulator? _captured;

        public SessionSimulatorBinding(InputSimulatorHost host, bool captureAtConstruction)
        {
            _host = host;
            if (captureAtConstruction)
                _captured = host.Simulator;
        }

        public InputSimulator ForActiveWindowCheck => _host.Simulator;

        public InputSimulator ForInput => _captured ?? _host.Simulator;
    }
}
