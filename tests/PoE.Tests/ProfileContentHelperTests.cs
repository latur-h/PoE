using PoE.dlls.Settings.Macros;
using PoE.dlls.Settings.Mods;
using Xunit;

namespace PoE.Tests;

public class ProfileContentHelperTests
{
    [Fact]
    public void GamblePreset_HasContent_when_any_rule_has_content()
    {
        var preset = new GamblePreset
        {
            Rules =
            [
                new GambleRuleRow { Content = string.Empty },
                new GambleRuleRow { Content = "life" },
            ],
        };

        Assert.True(GamblePresetContentHelper.HasContent(preset));
    }

    [Fact]
    public void GamblePreset_HasContent_false_when_all_rules_empty()
    {
        var preset = new GamblePreset
        {
            Rules =
            [
                new GambleRuleRow { Content = string.Empty },
                new GambleRuleRow { Content = "   " },
            ],
        };

        Assert.False(GamblePresetContentHelper.HasContent(preset));
    }

    [Fact]
    public void MacroProfile_HasContent_when_trigger_configured()
    {
        var profile = new MacroProfile
        {
            Triggers =
            [
                new MacroTrigger { FireSequence = "LButton Down" },
            ],
        };

        Assert.True(MacroProfileContentHelper.HasContent(profile));
    }

    [Fact]
    public void MacroProfile_HasContent_false_when_triggers_blank()
    {
        var profile = new MacroProfile
        {
            Triggers = [new MacroTrigger()],
        };

        Assert.False(MacroProfileContentHelper.HasContent(profile));
    }
}
