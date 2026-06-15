using PoE.dlls.KeyBindings;
using PoE.dlls.Settings.Macros;

namespace PoE.dlls.Macros
{
    internal static class MacroTriggerRuntimeHelper
    {
        public static MacroTrigger ToRuntimeTrigger(MacroTrigger trigger)
        {
            bool keysReady = KeysAllowRuntime(trigger);
            return new MacroTrigger
            {
                Id = trigger.Id,
                Active = trigger.Active && keysReady,
                TriggerKey = AllowsTriggerKey(trigger) ? trigger.TriggerKey : string.Empty,
                FireSequence = trigger.FireSequence,
                Behavior = trigger.Behavior,
                KeyDelayMs = trigger.KeyDelayMs,
                CycleDelayMs = trigger.CycleDelayMs,
                ToggleKey = AllowsToggleKey(trigger) ? trigger.ToggleKey : string.Empty,
                PixelX = trigger.PixelX,
                PixelY = trigger.PixelY,
                ExpectedColor = trigger.ExpectedColor,
                LockMs = trigger.LockMs,
            };
        }

        public static bool KeysAllowRuntime(MacroTrigger trigger) => trigger.Behavior switch
        {
            MacroBehavior.Repeat => MacroFireSequence.IsValid(trigger.FireSequence),
            MacroBehavior.JE or MacroBehavior.JNE => IsPixelConfigValid(trigger),
            _ => MacroFireSequence.IsValid(trigger.FireSequence)
                && KeyBindingHelper.TryResolveStored(trigger.TriggerKey, out _, out _),
        };

        private static bool AllowsTriggerKey(MacroTrigger trigger) =>
            trigger.Behavior is MacroBehavior.Single or MacroBehavior.Loop
            && KeyBindingHelper.TryResolveStored(trigger.TriggerKey, out _, out _);

        private static bool AllowsToggleKey(MacroTrigger trigger) =>
            !string.IsNullOrWhiteSpace(trigger.ToggleKey)
            && KeyBindingHelper.TryResolveStored(trigger.ToggleKey, out _, out _);

        private static bool IsPixelConfigValid(MacroTrigger trigger)
        {
            if (trigger.PixelX < 0 || trigger.PixelY < 0)
                return false;

            return MacroColorHelper.TryParseHex(trigger.ExpectedColor, out _);
        }
    }
}
