using PoE.dlls.Settings.Macros;

namespace PoE.dlls.Macros
{
    internal static class MacroRuntimeSettingsBuilder
    {
        public static MacroSettings Build(
            MacroSettings source,
            IReadOnlyList<MacroTrigger> globalRuntimeTriggers,
            IReadOnlyList<MacroTrigger> buildRuntimeTriggers)
        {
            var runtime = new MacroSettings
            {
                EnableKey = source.EnableKey,
                FeatureEnabled = source.FeatureEnabled,
                ActiveBuildProfileName = source.ActiveBuildProfileName,
                GlobalProfile = new MacroProfile
                {
                    Name = MacroProfile.GlobalName,
                    Triggers = globalRuntimeTriggers.Select(CloneTrigger).ToList(),
                },
                BuildProfiles = source.BuildProfiles
                    .Select(profile => new MacroProfile
                    {
                        Name = profile.Name,
                        Triggers = string.Equals(profile.Name, source.ActiveBuildProfileName, StringComparison.OrdinalIgnoreCase)
                            ? buildRuntimeTriggers.Select(CloneTrigger).ToList()
                            : profile.Triggers.Select(CloneTrigger).ToList(),
                    })
                    .ToList(),
            };

            return runtime;
        }

        private static MacroTrigger CloneTrigger(MacroTrigger trigger) => new()
        {
            Id = trigger.Id,
            Active = trigger.Active,
            TriggerKey = trigger.TriggerKey,
            FireSequence = trigger.FireSequence,
            Behavior = trigger.Behavior,
            KeyDelayMs = trigger.KeyDelayMs,
            CycleDelayMs = trigger.CycleDelayMs,
            ToggleKey = trigger.ToggleKey,
        };
    }
}
