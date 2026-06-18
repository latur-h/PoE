using PoE.dlls.Automation;
using PoE.dlls.InteropServices;
using PoE.dlls.KeyBindings;
using PoE.dlls.Settings.Macros;

namespace PoE.dlls.Macros
{
    /// <summary>
    /// Polls trigger keys and fires macro sequences.
    /// Loop/Repeat/JE/JNE use a dedicated poll loop instead of <see cref="Poss.Win.Automation.GlobalHotKeys.GlobalHotKeyManager"/>
    /// for held-key repeat to avoid nested hook handlers, coalesced input, and deadlocks.
    /// </summary>
    public sealed class MacroEngine
    {
        private const int PollIntervalMs = 5;
        private const int ToggleDebounceMs = 300;
        /// <summary>Reject a disarm that lands immediately after arm from the same hotkey bounce.</summary>
        private const int ArmDisarmPairRejectMs = 120;

        private readonly InputSimulatorHost _inputHost;
        private readonly object _stateLock = new();
        private MacroSettings _settings = new();
        private CancellationTokenSource? _pollCts;
        private readonly Dictionary<Guid, bool> _triggerWasDown = new();
        private readonly Dictionary<Guid, long> _lastCycleFireTicks = new();
        private readonly Dictionary<Guid, long> _lastToggleTicks = new();
        private readonly Dictionary<Guid, long> _lastArmTicks = new();
        private readonly Dictionary<Guid, long> _lockUntilTicks = new();
        private readonly HashSet<Guid> _cycleInProgress = new();

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
                _settings = settings;
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
                _settings.FeatureEnabled = !_settings.FeatureEnabled;

            SettingsChanged?.Invoke();
        }

        public void SetFeatureEnabled(bool enabled)
        {
            lock (_stateLock)
                _settings.FeatureEnabled = enabled;

            SettingsChanged?.Invoke();
        }

        public bool IsCycleInProgress(Guid triggerId) => _cycleInProgress.Contains(triggerId);

        public void ToggleTriggerActive(MacroTrigger trigger)
        {
            long now = Environment.TickCount64;
            bool changed;

            lock (_stateLock)
            {
                changed = TryToggleTriggerActiveLocked(trigger, now);
            }

            if (changed)
                SettingsChanged?.Invoke();
        }

        private bool TryToggleTriggerActiveLocked(MacroTrigger trigger, long now)
        {
            if (trigger.Active)
            {
                if (WasRecentlyArmed(trigger.Id, now))
                    return false;

                trigger.Active = false;
                _lastToggleTicks[trigger.Id] = now;
                return true;
            }

            if (WasRecentlyToggled(trigger.Id, now, ToggleDebounceMs))
                return false;

            trigger.Active = true;
            _lastArmTicks[trigger.Id] = now;
            _lastToggleTicks[trigger.Id] = now;
            return true;
        }

        private bool WasRecentlyArmed(Guid triggerId, long now) =>
            _lastArmTicks.TryGetValue(triggerId, out long lastArm)
            && now - lastArm < ArmDisarmPairRejectMs;

        private bool WasRecentlyToggled(Guid triggerId, long now, int toggleDebounceMs) =>
            _lastToggleTicks.TryGetValue(triggerId, out long lastToggle)
            && now - lastToggle < toggleDebounceMs;

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

            if (!_inputHost.Simulator.IsActiveWindow())
                return;

            foreach (var trigger in triggers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_cycleInProgress.Contains(trigger.Id))
                    continue;

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
                    case MacroBehavior.JE:
                    case MacroBehavior.JNE:
                        await ProcessPixelJumpAsync(trigger, cancellationToken).ConfigureAwait(false);
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
                MacroBehavior.Repeat => true,
                MacroBehavior.JE or MacroBehavior.JNE => IsPixelTriggerValid(trigger),
                MacroBehavior.Single or MacroBehavior.Loop =>
                    KeyBindingHelper.TryResolveStored(trigger.TriggerKey, out _, out _),
                _ => false,
            };
        }

        private static bool IsPixelTriggerValid(MacroTrigger trigger)
        {
            if (trigger.PixelX < 0 || trigger.PixelY < 0)
                return false;

            return MacroColorHelper.TryParseHex(trigger.ExpectedColor, out _);
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

            await RunFireCycleAsync(trigger, cancellationToken).ConfigureAwait(false);
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

            await RunFireCycleAsync(trigger, cancellationToken).ConfigureAwait(false);
        }

        private async Task ProcessRepeatAsync(MacroTrigger trigger, CancellationToken cancellationToken)
        {
            if (!ShouldFireCycle(trigger.Id, trigger.CycleDelayMs))
                return;

            await RunFireCycleAsync(trigger, cancellationToken).ConfigureAwait(false);
        }

        private async Task ProcessPixelJumpAsync(MacroTrigger trigger, CancellationToken cancellationToken)
        {
            if (!MacroColorHelper.TryParseHex(trigger.ExpectedColor, out Color expected))
                return;

            Color sampled = InteropHelper.GetColorAt(trigger.PixelX, trigger.PixelY);
            bool matches = MacroColorHelper.MatchesStrict(sampled, expected);
            bool condition = trigger.Behavior == MacroBehavior.JE ? matches : !matches;

            if (!condition)
                return;

            if (IsLocked(trigger.Id))
                return;

            if (!ShouldFireCycle(trigger.Id, trigger.CycleDelayMs))
                return;

            await RunFireCycleAsync(trigger, cancellationToken, trigger.LockMs).ConfigureAwait(false);
        }

        private async Task RunFireCycleAsync(
            MacroTrigger trigger,
            CancellationToken cancellationToken,
            int lockMs = 0)
        {
            _cycleInProgress.Add(trigger.Id);
            try
            {
                await MacroFireSequence.ExecuteAsync(
                    _inputHost.Simulator,
                    trigger.FireSequence,
                    trigger.KeyDelayMs,
                    cancellationToken).ConfigureAwait(false);

                if (lockMs > 0)
                    _lockUntilTicks[trigger.Id] = Environment.TickCount64 + lockMs;
            }
            finally
            {
                _cycleInProgress.Remove(trigger.Id);
            }
        }

        private bool IsLocked(Guid triggerId)
        {
            if (!_lockUntilTicks.TryGetValue(triggerId, out long until))
                return false;

            if (Environment.TickCount64 >= until)
            {
                _lockUntilTicks.Remove(triggerId);
                return false;
            }

            return true;
        }

        private bool ShouldFireCycle(Guid triggerId, int cycleDelayMs)
        {
            long now = Environment.TickCount64;
            if (cycleDelayMs < 0)
                cycleDelayMs = 0;

            if (!_lastCycleFireTicks.TryGetValue(triggerId, out long last))
            {
                _lastCycleFireTicks[triggerId] = now;
                return true;
            }

            if (now - last < cycleDelayMs)
                return false;

            _lastCycleFireTicks[triggerId] = now;
            return true;
        }
    }
}
