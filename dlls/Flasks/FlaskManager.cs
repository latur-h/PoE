using PoE.dlls.Flasks.Base;
using PoE.dlls.Settings;
using Poss.Win.Automation.Input;

namespace PoE.dlls.Flasks
{
    public class FlaskManager
    {
        private readonly List<IFlask> flasks = [];

        private readonly InputSimulator simulator;

        public FlaskTiming Timing { get; } = new();

        private CancellationTokenSource? cts;
        private CancellationToken token;

        public FlaskManager(InputSimulator _simulator)
        {
            simulator = _simulator;

            cts = new CancellationTokenSource();
            token = cts.Token;
        }

        public void ApplySettings(UIFlaskControls controls) => Timing.Apply(controls);

        public void Flush()
        {
            if (cts is not null && !token.IsCancellationRequested)
                cts?.Cancel();

            flasks.Clear();
        }

        public void RegisterFlask(FlaskType type, int numer, string key)
        {
            Console.WriteLine($"Registering flask | Number: {numer}; Type: {type}; Key: {key}");

            switch (type)
            {
                case FlaskType.HP: flasks.Add(new HP(simulator, key, numer, Timing)); break;
                case FlaskType.MP: flasks.Add(new MP(simulator, key, numer, Timing)); break;
                case FlaskType.Utility: flasks.Add(new Utility(simulator, key, numer, Timing)); break;
                case FlaskType.Tincture: flasks.Add(new Tincture(simulator, key, numer, Timing)); break;
                default: throw new NotSupportedException("Unsupported flask type.");
            }
        }

        public async Task DrinkFlasks()
        {
            if (cts is not null && !token.IsCancellationRequested) return;

            Console.WriteLine("Starting drinking...");

            cts = new CancellationTokenSource();
            token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(Timing.PollDelay);

                if (!simulator.IsActiveWindow()) continue;

                List<Task> tasks = [];

                foreach (var flask in flasks)
                    tasks.Add(flask.Drink());

                await Task.WhenAll(tasks);
            }

            Console.WriteLine("Drinking stopped.");
            cts.Dispose();
            cts = null;
        }

        public void Stop()
        {
            if (cts is null || token.IsCancellationRequested) return;

            Console.WriteLine("Requested stop drinking...");
            cts.Cancel();
        }
    }
}
