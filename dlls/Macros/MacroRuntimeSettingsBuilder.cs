using PoE.dlls.Settings.Macros;

namespace PoE.dlls.Macros
{
    internal static class MacroRuntimeSettingsBuilder
    {
        public static MacroSettings Build(MacroSettings source)
        {
            bool buildActive = MacroSettingsHelper.IsAdditionalBuildProfileActive(source);
            string activeName = source.ActiveBuildProfileName;

            return new MacroSettings
            {
                EnableKey = source.EnableKey,
                FeatureEnabled = source.FeatureEnabled,
                ActiveBuildProfileName = source.ActiveBuildProfileName,
                RememberedColors = source.RememberedColors?.ToList() ?? [],
                GlobalProfile = new MacroProfile
                {
                    Name = MacroProfile.GlobalName,
                    Triggers = source.GlobalProfile.Triggers
                        .Select(MacroTriggerRuntimeHelper.ToRuntimeTrigger)
                        .Select(CloneTrigger)
                        .ToList(),
                },
                BuildProfiles = buildActive
                    ? source.BuildProfiles
                        .Where(profile => string.Equals(profile.Name, activeName, StringComparison.OrdinalIgnoreCase))
                        .Select(profile => new MacroProfile
                        {
                            Name = profile.Name,
                            Triggers = profile.Triggers
                                .Select(MacroTriggerRuntimeHelper.ToRuntimeTrigger)
                                .Select(CloneTrigger)
                                .ToList(),
                        })
                        .ToList()
                    : [],
            };
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
            PixelX = trigger.PixelX,
            PixelY = trigger.PixelY,
            ExpectedColor = trigger.ExpectedColor,
            LockMs = trigger.LockMs,
        };
    }
}
