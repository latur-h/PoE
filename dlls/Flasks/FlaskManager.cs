using PoE.dlls.Automation;
using PoE.dlls.Flasks.Base;
using PoE.dlls.Logger;
using PoE.dlls.Settings;
using Poss.Win.Automation.Input;

namespace PoE.dlls.Flasks
{
    public class FlaskManager
    {
        private readonly List<IFlask> flasks = [];

        private readonly InputSimulatorHost _inputHost;

        public FlaskTiming Timing { get; } = new();

        private CancellationTokenSource? cts;
        private CancellationToken token;

        public bool IsDrinking => cts is not null && !token.IsCancellationRequested;

        public event Action? DrinkingStateChanged;

        public FlaskManager(InputSimulatorHost inputHost)
        {
            _inputHost = inputHost;
        }

        public void ApplySettings(UIFlaskControls controls) => Timing.Apply(controls);

        public void Flush()
        {
            if (cts is not null && !cts.Token.IsCancellationRequested)
                cts.Cancel();

            flasks.Clear();
        }

        public void RegisterFlask(FlaskType type, int numer, string key, FlaskRegistration? saved = null)
        {
            FlaskLog.Registered(numer, type, key);

            switch (type)
            {
                case FlaskType.HP: flasks.Add(new HP(_inputHost, key, numer, Timing, saved)); break;
                case FlaskType.MP: flasks.Add(new MP(_inputHost, key, numer, Timing, saved)); break;
                case FlaskType.Utility: flasks.Add(new Utility(_inputHost, key, numer, Timing, saved)); break;
                case FlaskType.Tincture: flasks.Add(new Tincture(_inputHost, key, numer, Timing, saved)); break;
                default: throw new NotSupportedException("Unsupported flask type.");
            }
        }

        public async Task DrinkFlasks()
        {
            if (cts is not null && !token.IsCancellationRequested) return;

            FlaskLog.DrinkStarted();

            cts = new CancellationTokenSource();
            token = cts.Token;
            DrinkingStateChanged?.Invoke();

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(Timing.PollDelay);

                if (!_inputHost.Simulator.IsActiveWindow()) continue;

                List<Task> tasks = [];

                foreach (var flask in flasks)
                    tasks.Add(flask.Drink());

                await Task.WhenAll(tasks);
            }

            FlaskLog.DrinkStopped();
            cts.Dispose();
            cts = null;
            DrinkingStateChanged?.Invoke();
        }

        public void Stop()
        {
            if (cts is null || token.IsCancellationRequested) return;

            FlaskLog.StopRequested();
            cts.Cancel();
            DrinkingStateChanged?.Invoke();
        }

        internal IReadOnlyList<IFlask> RegisteredFlasksForTests => flasks;
    }
}
