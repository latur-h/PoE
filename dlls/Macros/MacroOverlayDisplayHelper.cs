using PoE.dlls.Flasks;
using PoE.dlls.KeyBindings;
using AppSettings = PoE.dlls.Settings.Settings;
using PoE.dlls.Settings;
using PoE.dlls.Settings.Macros;

namespace PoE.dlls.Macros
{
    internal enum OverlayRowKind
    {
        Section,
        Status,
    }

    internal readonly record struct OverlayRow(OverlayRowKind Kind, string Label, bool IsOn)
    {
        public static OverlayRow Section(string label) => new(OverlayRowKind.Section, label, false);

        public static OverlayRow Status(string label, bool isOn) => new(OverlayRowKind.Status, label, isOn);
    }

    internal static class MacroOverlayDisplayHelper
    {
        public static IReadOnlyList<OverlayRow> BuildRows(AppSettings settings, MacroEngine engine, FlaskManager flaskManager)
        {
            var rows = new List<OverlayRow>();

            IReadOnlyList<OverlayRow> macroRows = BuildMacroRows(settings.Macros, engine);
            if (macroRows.Count > 0)
            {
                rows.Add(OverlayRow.Section("Macros"));
                rows.AddRange(macroRows);
            }

            IReadOnlyList<OverlayRow> flaskRows = BuildFlaskRows(settings.Flasks, flaskManager.IsDrinking);
            if (flaskRows.Count > 0)
            {
                rows.Add(OverlayRow.Section("Flasks"));
                rows.AddRange(flaskRows);
            }

            return rows;
        }

        private static IReadOnlyList<OverlayRow> BuildMacroRows(MacroSettings settings, MacroEngine engine)
        {
            var rows = new List<OverlayRow>();
            bool featureEnabled = engine.FeatureEnabled;

            foreach (var (profile, trigger) in MacroSettingsHelper.EnumerateTriggers(settings, includeGlobal: true, includeActiveBuild: true))
            {
                MacroTrigger? runtime = engine.FindTrigger(trigger.Id);
                bool isOn = featureEnabled && runtime?.Active == true;
                rows.Add(OverlayRow.Status(FormatMacroLabel(profile, trigger), isOn));
            }

            return rows;
        }

        private static IReadOnlyList<OverlayRow> BuildFlaskRows(IReadOnlyDictionary<string, UIFlask> flasks, bool drinking)
        {
            var rows = new List<OverlayRow>();

            foreach (string slot in new[] { "1", "2", "3", "4", "5" })
            {
                if (!flasks.TryGetValue(slot, out UIFlask? flask) || flask is null || !flask.Active)
                    continue;

                rows.Add(OverlayRow.Status(FormatFlaskLabel(slot, flask), drinking));
            }

            return rows;
        }

        private static string FormatMacroLabel(MacroProfile profile, MacroTrigger trigger)
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

        private static string FormatFlaskLabel(string slot, UIFlask flask)
        {
            string keyLabel = "—";
            if (KeyBindingHelper.TryResolveStored(flask.Key, out _, out string displayKey))
                keyLabel = displayKey;
            else if (!string.IsNullOrWhiteSpace(flask.Key))
                keyLabel = flask.Key.Trim();

            string typeLabel = string.IsNullOrWhiteSpace(flask.FlaskType) ? "—" : flask.FlaskType;
            if (typeLabel is "HP" or "MP")
                typeLabel += $" {flask.Percent}%";

            return $"Flask {slot} · {typeLabel} · {keyLabel}";
        }
    }
}
