using PoE.dlls.KeyBindings;
using PoE.dlls.Settings;
using PoE.dlls.Settings.Macros;
using Poss.Win.Automation.GlobalHotKeys;

namespace PoE.dlls.Macros
{
    public sealed class MacroHotkeyBinder
    {
        private const string EnableHotkeyId = "Macros enable";
        private const string ToggleHotkeyPrefix = "Macro toggle ";

        private readonly GlobalHotKeyManager _hotkeys;
        private readonly MacroEngine _engine;
        private readonly List<string> _registeredToggleIds = [];

        public MacroHotkeyBinder(GlobalHotKeyManager hotkeys, MacroEngine engine)
        {
            _hotkeys = hotkeys;
            _engine = engine;
        }

        public void BindAll(PoE.dlls.Settings.Settings settings)
        {
            MacroSettings macros = settings.Macros;
            MacroSettingsHelper.EnsureInitialized(macros);

            if (KeyBindingHelper.TryResolveStored(macros.EnableKey, out _, out _))
                _hotkeys.Change(EnableHotkeyId, macros.EnableKey);

            foreach (string id in _registeredToggleIds)
                _hotkeys.Unregister(id);

            _registeredToggleIds.Clear();

            foreach (var (profile, trigger) in MacroSettingsHelper.EnumerateTriggers(macros, true, true))
            {
                if (string.IsNullOrWhiteSpace(trigger.ToggleKey))
                    continue;

                if (!KeyBindingHelper.TryResolveStored(trigger.ToggleKey, out _, out _))
                    continue;

                string id = ToggleHotkeyPrefix + trigger.Id;
                Guid triggerId = trigger.Id;

                string toggleBinding = ToToggleHotkeyBinding(trigger.ToggleKey);

                _hotkeys.Register(id, () =>
                {
                    if (_engine.IsCycleInProgress(triggerId))
                        return Task.CompletedTask;

                    var resolved = _engine.FindTrigger(triggerId);
                    if (resolved is null)
                        return Task.CompletedTask;

                    _engine.ToggleTriggerActive(resolved);
                    return Task.CompletedTask;
                }, toggleBinding);

                _registeredToggleIds.Add(id);
            }
        }

        private static string ToToggleHotkeyBinding(string toggleKey)
        {
            if (string.IsNullOrWhiteSpace(toggleKey))
                return toggleKey;

            toggleKey = toggleKey.Trim();
            if (toggleKey.EndsWith(" Down", StringComparison.OrdinalIgnoreCase)
                || toggleKey.EndsWith(" Up", StringComparison.OrdinalIgnoreCase)
                || toggleKey.EndsWith(" Press", StringComparison.OrdinalIgnoreCase))
            {
                return toggleKey;
            }

            return $"{toggleKey} Down";
        }

        public static void RegisterEnableHotkey(GlobalHotKeyManager hotkeys, MacroEngine engine, string enableKey)
        {
            string key = KeyBindingHelper.TryResolveStored(enableKey, out string sendKey, out _)
                ? sendKey
                : "F9";

            hotkeys.Register(EnableHotkeyId, () =>
            {
                engine.ToggleFeatureEnabled();
                return Task.CompletedTask;
            }, key);
        }
    }
}
