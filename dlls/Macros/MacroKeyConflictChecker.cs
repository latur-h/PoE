using PoE.dlls.KeyBindings;
using PoE.dlls.Settings;
using PoE.dlls.Settings.Macros;

namespace PoE.dlls.Macros
{
    public sealed class MacroKeyUsage
    {
        public required string Key { get; init; }
        public required string Label { get; init; }
        public MacroProfile? MacroProfile { get; init; }
        public MacroTrigger? MacroTrigger { get; init; }
    }

    public sealed class MacroKeyConflict
    {
        public required string Key { get; init; }
        public required IReadOnlyList<string> Labels { get; init; }
        public required IReadOnlyList<MacroTrigger> MacroTriggers { get; init; }
    }

    public static class MacroKeyConflictChecker
    {
        public static IReadOnlyList<MacroKeyConflict> FindConflicts(PoE.dlls.Settings.Settings settings)
        {
            var usages = CollectUsages(settings);
            return usages
                .GroupBy(u => u.Key, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => new MacroKeyConflict
                {
                    Key = g.Key,
                    Labels = g.Select(u => u.Label).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                    MacroTriggers = g.Where(u => u.MacroTrigger is not null)
                        .Select(u => u.MacroTrigger!)
                        .Distinct()
                        .ToList(),
                })
                .OrderBy(c => c.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static IReadOnlyList<MacroKeyUsage> CollectUsages(PoE.dlls.Settings.Settings settings)
        {
            var usages = new List<MacroKeyUsage>();
            settings.Macros ??= new MacroSettings();
            MacroSettingsHelper.EnsureInitialized(settings.Macros);

            AddKey(usages, settings.Macros.EnableKey, "Macros: feature enable");

            AddKey(usages, settings.FlaskControls.RegisterKey, "Flasks: register");
            AddKey(usages, settings.FlaskControls.DrinkKey, "Flasks: drink");
            AddKey(usages, settings.FlaskControls.StopKey, "Flasks: stop");

            AddKey(usages, settings.Modifiers.GetCoorinatesKey, "Gamble: get coordinates");
            AddKey(usages, settings.Modifiers.GamblerStart, "Gamble: start");
            AddKey(usages, settings.Modifiers.GamblerStop, "Gamble: stop");

            foreach (var (slot, flask) in settings.Flasks.OrderBy(f => f.Key, StringComparer.Ordinal))
                AddKey(usages, flask.Key, $"Flask slot {slot}");

            foreach (var (profile, trigger) in MacroSettingsHelper.EnumerateTriggers(
                         settings.Macros, includeGlobal: true, includeActiveBuild: true))
            {
                string scope = string.Equals(profile.Name, MacroProfile.GlobalName, StringComparison.OrdinalIgnoreCase)
                    ? "Global"
                    : $"Build profile \"{profile.Name}\"";

                if (trigger.Behavior is MacroBehavior.Single or MacroBehavior.Loop)
                    AddMacroKey(usages, trigger.TriggerKey, $"{scope}: trigger", profile, trigger);

                if (!string.IsNullOrWhiteSpace(trigger.ToggleKey))
                    AddMacroKey(usages, trigger.ToggleKey, $"{scope}: toggle active", profile, trigger);
            }

            return usages;
        }

        private static void AddKey(List<MacroKeyUsage> usages, string? raw, string label)
        {
            if (!KeyBindingHelper.TryResolveStored(raw ?? string.Empty, out string sendKey, out _))
                return;

            usages.Add(new MacroKeyUsage
            {
                Key = sendKey,
                Label = label,
            });
        }

        private static void AddMacroKey(
            List<MacroKeyUsage> usages,
            string? raw,
            string label,
            MacroProfile profile,
            MacroTrigger trigger)
        {
            if (!KeyBindingHelper.TryResolveStored(raw ?? string.Empty, out string sendKey, out _))
                return;

            usages.Add(new MacroKeyUsage
            {
                Key = sendKey,
                Label = label,
                MacroProfile = profile,
                MacroTrigger = trigger,
            });
        }
    }
}
