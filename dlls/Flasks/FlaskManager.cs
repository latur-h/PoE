using PoE.dlls.Automation;
using PoE.dlls.Flasks.Base;
using PoE.dlls.Logger;
using PoE.dlls.Settings;

namespace PoE.dlls.Flasks
{
    public class FlaskManager
    {
        private readonly List<IFlask> flasks = [];
        private readonly Dictionary<string, IFlask> flasksBySlot = new(StringComparer.Ordinal);

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
            flasksBySlot.Clear();
        }

        public void RegisterFlask(string slotKey, FlaskType type, int numer, string key, FlaskRegistration? saved = null)
        {
            FlaskLog.Registered(numer, type, key);

            IFlask flask = type switch
            {
                FlaskType.HP => new HP(_inputHost, key, numer, Timing, saved),
                FlaskType.MP => new MP(_inputHost, key, numer, Timing, saved),
                FlaskType.Utility => new Utility(_inputHost, key, numer, Timing, saved),
                FlaskType.Tincture => new Tincture(_inputHost, key, numer, Timing, saved),
                _ => throw new NotSupportedException("Unsupported flask type."),
            };

            flasks.Add(flask);
            flasksBySlot[slotKey] = flask;
        }

        public bool TryGetSlotRuntime(string slotKey, out FlaskSlotRuntime runtime)
        {
            if (flasksBySlot.TryGetValue(slotKey, out IFlask? flask))
            {
                runtime = new FlaskSlotRuntime
                {
                    Slot = slotKey,
                    IsReady = flask.IsReady,
                    UsesDualPixel = flask.Flask.Type is FlaskType.Utility or FlaskType.Tincture,
                };
                return true;
            }

            runtime = new FlaskSlotRuntime
            {
                Slot = slotKey,
                IsReady = false,
                UsesDualPixel = false,
            };
            return false;
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
