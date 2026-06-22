namespace PoE.dlls.Settings.Macros
{
    internal static class MacroProfileContentHelper
    {
        public static bool HasContent(MacroProfile profile) =>
            profile.Triggers.Any(HasTriggerContent);

        private static bool HasTriggerContent(MacroTrigger trigger) =>
            !string.IsNullOrWhiteSpace(trigger.FireSequence)
            || !string.IsNullOrWhiteSpace(trigger.TriggerKey)
            || trigger.PixelX != 0
            || trigger.PixelY != 0;
    }
}
