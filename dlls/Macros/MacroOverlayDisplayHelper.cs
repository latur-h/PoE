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

    internal enum OverlayRowState
    {
        Off,
        On,
        Warning,
    }

    internal readonly record struct OverlayRow(OverlayRowKind Kind, string Label, OverlayRowState State)
    {
        public bool IsOn => State == OverlayRowState.On;

        public bool IsWarning => State == OverlayRowState.Warning;

        public static OverlayRow Section(string label) => new(OverlayRowKind.Section, label, OverlayRowState.Off);

        public static OverlayRow Status(string label, OverlayRowState state) => new(OverlayRowKind.Status, label, state);
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

            IReadOnlyList<OverlayRow> flaskRows = BuildFlaskRows(settings.Flasks, flaskManager);
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
                rows.Add(OverlayRow.Status(FormatMacroLabel(profile, trigger), isOn ? OverlayRowState.On : OverlayRowState.Off));
            }

            return rows;
        }

        private static IReadOnlyList<OverlayRow> BuildFlaskRows(IReadOnlyDictionary<string, UIFlask> flasks, FlaskManager flaskManager)
        {
            var rows = new List<OverlayRow>();
            bool drinking = flaskManager.IsDrinking;

            foreach (string slot in new[] { "1", "2", "3", "4", "5" })
            {
                if (!flasks.TryGetValue(slot, out UIFlask? flask) || flask is null || !flask.Active)
                    continue;

                bool hasRuntime = flaskManager.TryGetSlotRuntime(slot, out FlaskSlotRuntime runtime);
                bool isReady = hasRuntime && runtime.IsReady;
                bool usesDualPixel = hasRuntime
                    ? runtime.UsesDualPixel
                    : FlaskRegistrationHelper.UsesDualPixelDetection(flask.FlaskType);

                OverlayRowState state;
                if (!flask.IsRegistered)
                {
                    state = OverlayRowState.Warning;
                }
                else if (!drinking)
                {
                    state = OverlayRowState.Off;
                }
                else if (usesDualPixel && !isReady)
                {
                    state = OverlayRowState.Off;
                }
                else
                {
                    state = OverlayRowState.On;
                }

                rows.Add(OverlayRow.Status(FormatFlaskLabel(slot, flask, drinking, isReady, usesDualPixel), state));
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

        private static string FormatFlaskLabel(
            string slot,
            UIFlask flask,
            bool drinking,
            bool isReady,
            bool usesDualPixel)
        {
            string keyLabel = "—";
            if (KeyBindingHelper.TryResolveStored(flask.Key, out _, out string displayKey))
                keyLabel = displayKey;
            else if (!string.IsNullOrWhiteSpace(flask.Key))
                keyLabel = flask.Key.Trim();

            string typeLabel = string.IsNullOrWhiteSpace(flask.FlaskType) ? "—" : flask.FlaskType;
            if (typeLabel is "HP" or "MP")
                typeLabel += $" {flask.Percent}%";

            string registration = FlaskRegistrationHelper.DescribeRegistration(flask);
            string runtime = FlaskRegistrationHelper.DescribeRuntimeState(flask, drinking, isReady);

            if (!flask.IsRegistered)
                return $"Flask {slot} · {typeLabel} · {keyLabel} · {registration}";

            if (usesDualPixel)
                return $"Flask {slot} · {typeLabel} · {keyLabel} · {registration} · {runtime}";

            return $"Flask {slot} · {typeLabel} · {keyLabel} · {registration} · {(drinking ? "Drinking" : "Idle")}";
        }
    }
}
