using PoE.dlls.Macros;
using PoE.dlls.Settings.Macros;
using PoE.dlls.Automation;
using PoE.dlls.InteropServices;
using Xunit;

namespace PoE.Tests
{
    public class MacroOverlayDisplayHelperTests
    {
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

            var settings = new MacroSettings
            {
                FeatureEnabled = true,
                GlobalProfile = new MacroProfile
                {
                    Name = MacroProfile.GlobalName,
                    Triggers = [triggerOn, triggerOff],
                },
            };

            var inputHost = new InputSimulatorHost();
            var engine = new MacroEngine(inputHost);
            engine.ApplySettings(MacroRuntimeSettingsBuilder.Build(settings));

            IReadOnlyList<MacroOverlayDisplayHelper.MacroOverlayRow> rows =
                MacroOverlayDisplayHelper.BuildRows(settings, engine);

            Assert.Equal(2, rows.Count);
            Assert.True(rows[0].IsOn);
            Assert.False(rows[1].IsOn);
            Assert.Contains("Loop", rows[0].Label);
            Assert.Contains("Single", rows[1].Label);
        }

        [Fact]
        public void BuildRows_turns_all_off_when_feature_disabled()
        {
            var trigger = new MacroTrigger
            {
                Id = Guid.NewGuid(),
                Active = true,
                TriggerKey = "F1",
                Behavior = MacroBehavior.Repeat,
                FireSequence = "LButton Down",
            };

            var settings = new MacroSettings
            {
                FeatureEnabled = false,
                GlobalProfile = new MacroProfile
                {
                    Name = MacroProfile.GlobalName,
                    Triggers = [trigger],
                },
            };

            var inputHost = new InputSimulatorHost();
            var engine = new MacroEngine(inputHost);
            engine.ApplySettings(MacroRuntimeSettingsBuilder.Build(settings));

            MacroOverlayDisplayHelper.MacroOverlayRow row =
                Assert.Single(MacroOverlayDisplayHelper.BuildRows(settings, engine));

            Assert.False(row.IsOn);
        }
    }
}
