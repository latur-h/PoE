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
                MacroBehavior behavior = trigger.Behavior;

                _hotkeys.Register(id, () =>
                {
                    var resolved = _engine.FindTrigger(triggerId);
                    if (resolved is null || !resolved.Active)
                        return Task.CompletedTask;

                    if (behavior == MacroBehavior.Repeat)
                        _engine.ToggleRepeat(triggerId);
                    else
                        _engine.ToggleTriggerActive(resolved);

                    return Task.CompletedTask;
                }, trigger.ToggleKey);

                _registeredToggleIds.Add(id);
            }
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
