using PoE.dlls.Automation;
using PoE.dlls.KeyBindings;
using PoE.dlls.Settings.Macros;

namespace PoE.dlls.Macros
{
    /// <summary>
    /// Polls trigger keys and fires macro sequences.
    /// Loop/Repeat use a dedicated poll loop instead of <see cref="Poss.Win.Automation.GlobalHotKeys.GlobalHotKeyManager"/>
    /// for held-key repeat to avoid nested hook handlers, coalesced input, and deadlocks.
    /// </summary>
    public sealed class MacroEngine
    {
        private const int PollIntervalMs = 5;

        private readonly InputSimulatorHost _inputHost;
        private readonly object _stateLock = new();
        private MacroSettings _settings = new();
        private CancellationTokenSource? _pollCts;
        private readonly Dictionary<Guid, bool> _triggerWasDown = new();
        private readonly Dictionary<Guid, long> _lastCycleFireTicks = new();
        private readonly HashSet<Guid> _repeatRunning = new();

        public MacroEngine(InputSimulatorHost inputHost)
        {
            _inputHost = inputHost;
        }

        public event Action? SettingsChanged;

        public bool FeatureEnabled
        {
            get
            {
                lock (_stateLock)
                    return _settings.FeatureEnabled;
            }
        }

        public void ApplySettings(MacroSettings settings)
        {
            lock (_stateLock)
            {
                _settings = settings;
                if (!_settings.FeatureEnabled)
                    _repeatRunning.Clear();
            }
        }

        public void Start()
        {
            if (_pollCts is not null)
                return;

            _pollCts = new CancellationTokenSource();
            _ = Task.Run(() => PollLoopAsync(_pollCts.Token));
        }

        public void Stop()
        {
            _pollCts?.Cancel();
            _pollCts?.Dispose();
            _pollCts = null;
        }

        public void ToggleFeatureEnabled()
        {
            lock (_stateLock)
            {
                _settings.FeatureEnabled = !_settings.FeatureEnabled;
                if (!_settings.FeatureEnabled)
                    _repeatRunning.Clear();
            }

            SettingsChanged?.Invoke();
        }

        public void SetFeatureEnabled(bool enabled)
        {
            lock (_stateLock)
            {
                _settings.FeatureEnabled = enabled;
                if (!enabled)
                    _repeatRunning.Clear();
            }

            SettingsChanged?.Invoke();
        }

        public void ToggleRepeat(Guid triggerId)
        {
            lock (_stateLock)
            {
                if (!_settings.FeatureEnabled)
                    return;

                if (_repeatRunning.Contains(triggerId))
                    _repeatRunning.Remove(triggerId);
                else
                    _repeatRunning.Add(triggerId);
            }
        }

        public void ToggleTriggerActive(MacroTrigger trigger)
        {
            trigger.Active = !trigger.Active;

            lock (_stateLock)
            {
                _repeatRunning.Remove(trigger.Id);
            }

            SettingsChanged?.Invoke();
        }

        public void StopRepeat(Guid triggerId)
        {
            lock (_stateLock)
                _repeatRunning.Remove(triggerId);
        }

        public MacroTrigger? FindTrigger(Guid triggerId)
        {
            lock (_stateLock)
            {
                foreach (var trigger in _settings.GlobalProfile.Triggers)
                {
                    if (trigger.Id == triggerId)
                        return trigger;
                }

                var build = MacroSettingsHelper.GetActiveBuildProfile(_settings);
                if (build is null)
                    return null;

                return build.Triggers.FirstOrDefault(t => t.Id == triggerId);
            }
        }

        private async Task PollLoopAsync(CancellationToken cancellationToken)
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
                    await Task.Delay(PollIntervalMs, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private async Task ProcessTickAsync(CancellationToken cancellationToken)
        {
            List<MacroTrigger> triggers;
            bool enabled;

            lock (_stateLock)
            {
                enabled = _settings.FeatureEnabled;
                triggers = CollectRuntimeTriggersLocked().ToList();
            }

            if (!enabled || triggers.Count == 0)
                return;

            foreach (var trigger in triggers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch (trigger.Behavior)
                {
                    case MacroBehavior.Single:
                        await ProcessSingleAsync(trigger, cancellationToken).ConfigureAwait(false);
                        break;
                    case MacroBehavior.Loop:
                        await ProcessLoopAsync(trigger, cancellationToken).ConfigureAwait(false);
                        break;
                    case MacroBehavior.Repeat:
                        await ProcessRepeatAsync(trigger, cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
        }

        private IEnumerable<MacroTrigger> CollectRuntimeTriggersLocked()
        {
            foreach (var trigger in _settings.GlobalProfile.Triggers)
            {
                if (IsRuntimeCandidate(trigger))
                    yield return trigger;
            }

            var build = MacroSettingsHelper.GetActiveBuildProfile(_settings);
            if (build is null)
                yield break;

            foreach (var trigger in build.Triggers)
            {
                if (IsRuntimeCandidate(trigger))
                    yield return trigger;
            }
        }

        private static bool IsRuntimeCandidate(MacroTrigger trigger)
        {
            if (!trigger.Active || !MacroFireSequence.IsValid(trigger.FireSequence))
                return false;

            return trigger.Behavior switch
            {
                MacroBehavior.Repeat => KeyBindingHelper.TryResolveStored(trigger.ToggleKey, out _, out _),
                MacroBehavior.Single or MacroBehavior.Loop =>
                    KeyBindingHelper.TryResolveStored(trigger.TriggerKey, out _, out _),
                _ => false,
            };
        }

        private async Task ProcessSingleAsync(MacroTrigger trigger, CancellationToken cancellationToken)
        {
            if (!KeyBindingHelper.TryResolveStored(trigger.TriggerKey, out string sendKey, out _))
                return;

            bool isDown = _inputHost.Simulator.GetKeyState(sendKey);
            bool wasDown = _triggerWasDown.GetValueOrDefault(trigger.Id);
            _triggerWasDown[trigger.Id] = isDown;

            if (!isDown || wasDown)
                return;

            await MacroFireSequence.ExecuteAsync(
                _inputHost.Simulator,
                trigger.FireSequence,
                trigger.KeyDelayMs,
                cancellationToken).ConfigureAwait(false);
        }

        private async Task ProcessLoopAsync(MacroTrigger trigger, CancellationToken cancellationToken)
        {
            if (!KeyBindingHelper.TryResolveStored(trigger.TriggerKey, out string sendKey, out _))
                return;

            bool isDown = _inputHost.Simulator.GetKeyState(sendKey);
            _triggerWasDown[trigger.Id] = isDown;

            if (!isDown)
                return;

            if (!ShouldFireCycle(trigger.Id, trigger.CycleDelayMs))
                return;

            await MacroFireSequence.ExecuteAsync(
                _inputHost.Simulator,
                trigger.FireSequence,
                trigger.KeyDelayMs,
                cancellationToken).ConfigureAwait(false);
        }

        private async Task ProcessRepeatAsync(MacroTrigger trigger, CancellationToken cancellationToken)
        {
            bool isRunning;
            lock (_stateLock)
                isRunning = _repeatRunning.Contains(trigger.Id);

            if (!isRunning)
                return;

            if (!ShouldFireCycle(trigger.Id, trigger.CycleDelayMs))
                return;

            await MacroFireSequence.ExecuteAsync(
                _inputHost.Simulator,
                trigger.FireSequence,
                trigger.KeyDelayMs,
                cancellationToken).ConfigureAwait(false);
        }

        private static bool ShouldFireCycle(Guid triggerId, int cycleDelayMs)
        {
            long now = Environment.TickCount64;
            if (cycleDelayMs < 0)
                cycleDelayMs = 0;

            // Uses a static map keyed by trigger id; safe because poll loop is single-threaded.
            if (!CycleFireTicks.TryGetValue(triggerId, out long last))
            {
                CycleFireTicks[triggerId] = now;
                return true;
            }

            if (now - last < cycleDelayMs)
                return false;

            CycleFireTicks[triggerId] = now;
            return true;
        }

        private static readonly Dictionary<Guid, long> CycleFireTicks = new();
    }
}
