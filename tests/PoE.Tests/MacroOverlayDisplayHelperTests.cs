using PoE.dlls.Flasks;
using PoE.dlls.Macros;
using AppSettings = PoE.dlls.Settings.Settings;
using PoE.dlls.Settings.Macros;
using PoE.dlls.Automation;
using Xunit;

namespace PoE.Tests
{
    public class MacroOverlayDisplayHelperTests
    {
        private static (MacroEngine Engine, FlaskManager FlaskManager) CreateRuntime()
        {
            var inputHost = new InputSimulatorHost();
            return (new MacroEngine(inputHost), new FlaskManager(inputHost));
        }

        [Fact]
        public void BuildRows_marks_active_triggers_green_state()
        {
            var triggerOn = new MacroTrigger
            {
                Id = Guid.NewGuid(),
                Active = true,
                TriggerKey = "F1",
                Behavior = MacroBehavior.Loop,
                FireSequence = "LButton Down",
            };
            var triggerOff = new MacroTrigger
            {
                Id = Guid.NewGuid(),
                Active = false,
                TriggerKey = "F2",
                Behavior = MacroBehavior.Single,
                FireSequence = "LButton Up",
            };

            var settings = new AppSettings
            {
                Macros = new MacroSettings
                {
                    FeatureEnabled = true,
                    GlobalProfile = new MacroProfile
                    {
                        Name = MacroProfile.GlobalName,
                        Triggers = [triggerOn, triggerOff],
                    },
                },
            };

            var (engine, flaskManager) = CreateRuntime();
            engine.ApplySettings(MacroRuntimeSettingsBuilder.Build(settings.Macros));

            IReadOnlyList<OverlayRow> rows = MacroOverlayDisplayHelper.BuildRows(settings, engine, flaskManager);

            Assert.Contains(rows, r => r.Kind == OverlayRowKind.Section && r.Label == "Macros");
            OverlayRow macroOn = Assert.Single(rows, r => r.Label.Contains("Loop"));
            OverlayRow macroOff = Assert.Single(rows, r => r.Label.Contains("Single"));
            Assert.Equal(OverlayRowState.On, macroOn.State);
            Assert.Equal(OverlayRowState.Off, macroOff.State);
        }

        [Fact]
        public void BuildRows_turns_all_macros_off_when_feature_disabled()
        {
            var trigger = new MacroTrigger
            {
                Id = Guid.NewGuid(),
                Active = true,
                TriggerKey = "F1",
                Behavior = MacroBehavior.Repeat,
                FireSequence = "LButton Down",
            };

            var settings = new AppSettings
            {
                Macros = new MacroSettings
                {
                    FeatureEnabled = false,
                    GlobalProfile = new MacroProfile
                    {
                        Name = MacroProfile.GlobalName,
                        Triggers = [trigger],
                    },
                },
            };

            var (engine, flaskManager) = CreateRuntime();
            engine.ApplySettings(MacroRuntimeSettingsBuilder.Build(settings.Macros));

            OverlayRow macroRow = MacroOverlayDisplayHelper.BuildRows(settings, engine, flaskManager)
                .First(r => r.Kind == OverlayRowKind.Status && r.Label.Contains("F1"));

            Assert.Equal(OverlayRowState.Off, macroRow.State);
        }

        [Fact]
        public void BuildRows_marks_unregistered_flask_as_warning()
        {
            var settings = new AppSettings
            {
                Macros = new MacroSettings
                {
                    GlobalProfile = new MacroProfile
                    {
                        Name = MacroProfile.GlobalName,
                        Triggers = [],
                    },
                },
            };
            settings.Flasks["1"].Active = true;
            settings.Flasks["1"].FlaskType = "Utility";
            settings.Flasks["1"].Key = "1";
            settings.Flasks["1"].IsRegistered = false;

            var (engine, flaskManager) = CreateRuntime();
            engine.ApplySettings(MacroRuntimeSettingsBuilder.Build(settings.Macros));

            OverlayRow flaskRow = MacroOverlayDisplayHelper.BuildRows(settings, engine, flaskManager)
                .Single(r => r.Label.StartsWith("Flask 1", StringComparison.Ordinal));

            Assert.Equal(OverlayRowState.Warning, flaskRow.State);
            Assert.Contains("Not reg", flaskRow.Label);
        }

        [Fact]
        public void BuildRows_shows_only_enabled_flasks_colored_by_drink_state()
        {
            var settings = new AppSettings
            {
                Macros = new MacroSettings
                {
                    GlobalProfile = new MacroProfile
                    {
                        Name = MacroProfile.GlobalName,
                        Triggers = [],
                    },
                },
            };
            settings.Flasks["1"].Active = true;
            settings.Flasks["1"].FlaskType = "HP";
            settings.Flasks["1"].Percent = 55;
            settings.Flasks["1"].Key = "1";
            settings.Flasks["1"].IsRegistered = true;
            settings.Flasks["2"].Active = false;
            settings.Flasks["2"].FlaskType = "Utility";
            settings.Flasks["2"].Key = "2";

            var (engine, flaskManager) = CreateRuntime();
            engine.ApplySettings(MacroRuntimeSettingsBuilder.Build(settings.Macros));

            IReadOnlyList<OverlayRow> rowsStopped = MacroOverlayDisplayHelper.BuildRows(settings, engine, flaskManager);
            Assert.Contains(rowsStopped, r => r.Kind == OverlayRowKind.Section && r.Label == "Flasks");
            OverlayRow flask1Stopped = Assert.Single(rowsStopped, r => r.Label.StartsWith("Flask 1", StringComparison.Ordinal));
            Assert.DoesNotContain(rowsStopped, r => r.Label.StartsWith("Flask 2", StringComparison.Ordinal));
            Assert.Equal(OverlayRowState.Off, flask1Stopped.State);
            Assert.Contains("Reg", flask1Stopped.Label);
            Assert.Contains("HP 55%", flask1Stopped.Label);

            Assert.False(flaskManager.IsDrinking);
        }
    }
}
