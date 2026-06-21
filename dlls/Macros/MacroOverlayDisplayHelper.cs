using PoE.dlls.KeyBindings;
using PoE.dlls.Settings.Macros;

namespace PoE.dlls.Macros
{
    internal static class MacroOverlayDisplayHelper
    {
        internal readonly record struct MacroOverlayRow(string Label, bool IsOn);

        public static IReadOnlyList<MacroOverlayRow> BuildRows(MacroSettings settings, MacroEngine engine)
        {
            var rows = new List<MacroOverlayRow>();
            bool featureEnabled = engine.FeatureEnabled;

            foreach (var (profile, trigger) in MacroSettingsHelper.EnumerateTriggers(settings, includeGlobal: true, includeActiveBuild: true))
            {
                MacroTrigger? runtime = engine.FindTrigger(trigger.Id);
                bool isOn = featureEnabled && runtime?.Active == true;
                rows.Add(new MacroOverlayRow(FormatLabel(profile, trigger), isOn));
            }

            return rows;
        }

        private static string FormatLabel(MacroProfile profile, MacroTrigger trigger)
        {
            string keyLabel = "—";
            if (KeyBindingHelper.TryResolveStored(trigger.TriggerKey, out _, out string displayKey))
                keyLabel = displayKey;
            else if (!string.IsNullOrWhiteSpace(trigger.TriggerKey))
                keyLabel = trigger.TriggerKey.Trim();

            string profilePrefix = string.Equals(profile.Name, MacroProfile.GlobalName, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : $"[{profile.Name}] ";

            return $"{profilePrefix}{keyLabel} · {trigger.Behavior}";
        }
    }
}
