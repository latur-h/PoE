using PoE.dlls.Automation;
using PoE.dlls.Flasks.Base;
using PoE.dlls.InteropServices;
using PoE.dlls.Logger;
using PoE.dlls.Settings;

namespace PoE.dlls.Flasks
{
    public class FlaskManager
    {
        private readonly List<IFlask> flasks = [];
        private readonly Dictionary<string, IFlask> flasksBySlot = new(StringComparer.Ordinal);
        private readonly object _drinkLock = new();

        private readonly InputSimulatorHost _inputHost;

        public FlaskTiming Timing { get; } = new();

        private CancellationTokenSource? _drinkCts;

        public bool IsDrinking
        {
            get
            {
                lock (_drinkLock)
                    return _drinkCts is not null && !_drinkCts.Token.IsCancellationRequested;
            }
        }

        public event Action? DrinkingStateChanged;

        public FlaskManager(InputSimulatorHost inputHost)
        {
            _inputHost = inputHost;
        }

        public void ApplySettings(UIFlaskControls controls) => Timing.Apply(controls);

        public void Flush()
        {
            CancelDrinkLoopLocked();
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

        public Task DrinkFlasks()
        {
            lock (_drinkLock)
            {
                if (_drinkCts is not null && !_drinkCts.Token.IsCancellationRequested)
                    return Task.CompletedTask;

                _drinkCts = new CancellationTokenSource();
            }

            DrinkingStateChanged?.Invoke();
            _ = Task.Run(DrinkLoopAsync);
            return Task.CompletedTask;
        }

        public void Stop()
        {
            bool cancelled;
            lock (_drinkLock)
                cancelled = CancelDrinkLoopLocked();

            if (!cancelled)
                return;

            FlaskLog.StopRequested();
            DrinkingStateChanged?.Invoke();
        }

        private async Task DrinkLoopAsync()
        {
            CancellationToken cancellationToken;
            lock (_drinkLock)
                cancellationToken = _drinkCts!.Token;

            FlaskLog.DrinkStarted();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await ProcessTickAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch
                    {
                        // Keep polling even if a single tick fails.
                    }

                    try
                    {
                        await Task.Delay(Timing.PollDelay, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
            finally
            {
                FlaskLog.DrinkStopped();

                lock (_drinkLock)
                {
                    _drinkCts?.Dispose();
                    _drinkCts = null;
                }

                DrinkingStateChanged?.Invoke();
            }
        }

        private async Task ProcessTickAsync(CancellationToken cancellationToken)
        {
            if (!_inputHost.Simulator.IsActiveWindow())
                return;

            using var capture = new ScreenPixelCapture();

            foreach (IFlask flask in flasks)
                flask.UpdateReadiness(capture);

            List<Task> tasks = new(flasks.Count);
            foreach (IFlask flask in flasks)
                tasks.Add(flask.Drink(cancellationToken));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private bool CancelDrinkLoopLocked()
        {
            if (_drinkCts is null || _drinkCts.Token.IsCancellationRequested)
                return false;

            _drinkCts.Cancel();
            return true;
        }

        internal IReadOnlyList<IFlask> RegisteredFlasksForTests => flasks;
    }
}
