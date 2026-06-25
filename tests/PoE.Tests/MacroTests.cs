using PoE.dlls.KeyBindings;
using PoE.dlls.Macros;
using PoE.dlls.Settings;
using PoE.dlls.Settings.Macros;
using System.Drawing;
using Xunit;

namespace PoE.Tests;

public class MacroFireSequenceTests
{
    [Fact]
    public void ParseLines_splits_on_newlines_and_trims()
    {
        var lines = MacroFireSequence.ParseLines("Ctrl Down\n  A Down \r\nA Up");
        Assert.Equal(["Ctrl Down", "A Down", "A Up"], lines);
    }

    [Fact]
    public void ParseStrokes_splits_on_plus()
    {
        var strokes = MacroFireSequence.ParseStrokes("LButton Down + LButton Up");
        Assert.Equal(["LButton Down", "LButton Up"], strokes);
    }

    [Theory]
    [InlineData("LButton Down\nLButton Up", true)]
    [InlineData("LButton Down + LButton Up", true)]
    [InlineData("Ctrl Down\nnot a key", false)]
    [InlineData("", false)]
    public void IsValid_checks_each_stroke(string sequence, bool expected)
    {
        Assert.Equal(expected, MacroFireSequence.IsValid(sequence));
    }
}

public class MacroKeyInputTests
{
    [Theory]
    [InlineData("XButton1")]
    [InlineData("LButton")]
    [InlineData("F1")]
    [InlineData("1")]
    public void TryResolveStored_accepts_mouse_and_keyboard_names(string key)
    {
        Assert.True(KeyBindingHelper.TryResolveStored(key, out string sendKey, out _));
        Assert.False(string.IsNullOrWhiteSpace(sendKey));
    }
}

public class MacroSettingsHelperTests
{
    [Fact]
    public void IsAdditionalBuildProfileActive_false_when_global_selected()
    {
        var settings = new MacroSettings();
        MacroSettingsHelper.EnsureInitialized(settings);
        settings.ActiveBuildProfileName = MacroProfile.GlobalName;

        Assert.False(MacroSettingsHelper.IsAdditionalBuildProfileActive(settings));
        Assert.Null(MacroSettingsHelper.GetActiveBuildProfile(settings));
    }

    [Fact]
    public void IsAdditionalBuildProfileActive_true_for_named_build_profile()
    {
        var settings = new MacroSettings();
        MacroSettingsHelper.EnsureInitialized(settings);
        settings.BuildProfiles.Add(new MacroProfile { Name = "BV Minion", Triggers = [] });
        settings.ActiveBuildProfileName = "BV Minion";

        Assert.True(MacroSettingsHelper.IsAdditionalBuildProfileActive(settings));
        Assert.NotNull(MacroSettingsHelper.GetActiveBuildProfile(settings));
    }

    [Fact]
    public void EnsureInitialized_does_not_auto_create_default_build_profile()
    {
        var settings = new MacroSettings { BuildProfiles = [] };
        MacroSettingsHelper.EnsureInitialized(settings);

        Assert.Empty(settings.BuildProfiles);
        Assert.Equal(MacroProfile.GlobalName, settings.ActiveBuildProfileName);
    }

    [Fact]
    public void GetProfileByName_returns_global_profile()
    {
        var settings = new MacroSettings();
        MacroSettingsHelper.EnsureInitialized(settings);

        var profile = MacroSettingsHelper.GetProfileByName(settings, MacroProfile.GlobalName);

        Assert.Same(settings.GlobalProfile, profile);
    }
}

public class MacroRuntimeSettingsBuilderTests
{
    [Fact]
    public void Build_includes_only_active_build_profile_in_runtime()
    {
        var activeId = Guid.NewGuid();
        var inactiveId = Guid.NewGuid();
        var settings = new MacroSettings
        {
            ActiveBuildProfileName = "Active",
            GlobalProfile = new MacroProfile
            {
                Name = MacroProfile.GlobalName,
                Triggers = [CreateTrigger(active: false)],
            },
            BuildProfiles =
            [
                new MacroProfile
                {
                    Name = "Active",
                    Triggers = [CreateTrigger(active: true, id: activeId)],
                },
                new MacroProfile
                {
                    Name = "Inactive",
                    Triggers = [CreateTrigger(active: true, id: inactiveId)],
                },
            ],
        };

        MacroSettings runtime = MacroRuntimeSettingsBuilder.Build(settings);

        Assert.Single(runtime.BuildProfiles);
        Assert.Equal("Active", runtime.BuildProfiles[0].Name);
        Assert.Contains(runtime.BuildProfiles[0].Triggers, t => t.Id == activeId);
        Assert.DoesNotContain(runtime.BuildProfiles[0].Triggers, t => t.Id == inactiveId);
        Assert.Contains(settings.BuildProfiles.First(p => p.Name == "Inactive").Triggers, t => t.Id == inactiveId && t.Active);
    }

    [Fact]
    public void Build_global_only_excludes_all_build_profiles_from_runtime()
    {
        var settings = new MacroSettings
        {
            ActiveBuildProfileName = MacroProfile.GlobalName,
            GlobalProfile = new MacroProfile
            {
                Name = MacroProfile.GlobalName,
                Triggers = [CreateTrigger(active: true)],
            },
            BuildProfiles =
            [
                new MacroProfile
                {
                    Name = "Other",
                    Triggers = [CreateTrigger(active: true)],
                },
            ],
        };

        MacroSettings runtime = MacroRuntimeSettingsBuilder.Build(settings);

        Assert.Empty(runtime.BuildProfiles);
    }

    [Fact]
    public void Build_runtime_engine_does_not_resolve_inactive_profile_triggers()
    {
        var inactiveId = Guid.NewGuid();
        var settings = new MacroSettings
        {
            ActiveBuildProfileName = "Active",
            BuildProfiles =
            [
                new MacroProfile
                {
                    Name = "Active",
                    Triggers = [CreateTrigger(active: true)],
                },
                new MacroProfile
                {
                    Name = "Inactive",
                    Triggers = [CreateTrigger(active: true, id: inactiveId)],
                },
            ],
        };

        var engine = new MacroEngine(new PoE.dlls.Automation.InputSimulatorHost());
        engine.ApplySettings(MacroRuntimeSettingsBuilder.Build(settings));

        Assert.Null(engine.FindTrigger(inactiveId));
    }

    private static MacroTrigger CreateTrigger(bool active, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Active = active,
        TriggerKey = "F1",
        FireSequence = "LButton Down\nLButton Up",
        Behavior = MacroBehavior.Single,
    };
}

public class MacroEngineToggleTests
{
    [Fact]
    public void ToggleTriggerActive_ignores_duplicate_arm_within_debounce_window()
    {
        var engine = new MacroEngine(new PoE.dlls.Automation.InputSimulatorHost());
        var trigger = new MacroTrigger { Id = Guid.NewGuid(), Active = false };
        var settings = new MacroSettings
        {
            GlobalProfile = new MacroProfile
            {
                Name = MacroProfile.GlobalName,
                Triggers = [trigger],
            },
        };

        engine.ApplySettings(settings);
        MacroTrigger? runtime = engine.FindTrigger(trigger.Id);
        Assert.NotNull(runtime);

        engine.ToggleTriggerActive(runtime!);
        engine.ToggleTriggerActive(runtime!);

        Assert.True(runtime!.Active);
    }

    [Fact]
    public void ToggleTriggerActive_disarm_is_not_blocked_by_arm_debounce()
    {
        var engine = new MacroEngine(new PoE.dlls.Automation.InputSimulatorHost());
        var trigger = new MacroTrigger { Id = Guid.NewGuid(), Active = false };
        var settings = new MacroSettings
        {
            GlobalProfile = new MacroProfile
            {
                Name = MacroProfile.GlobalName,
                Triggers = [trigger],
            },
        };

        engine.ApplySettings(settings);
        MacroTrigger? runtime = engine.FindTrigger(trigger.Id);
        Assert.NotNull(runtime);

        engine.ToggleTriggerActive(runtime!);
        Thread.Sleep(150);
        engine.ToggleTriggerActive(runtime!);

        Assert.False(runtime!.Active);
    }

    [Fact]
    public void ToggleTriggerActive_allows_second_invocation_after_debounce_window()
    {
        var engine = new MacroEngine(new PoE.dlls.Automation.InputSimulatorHost());
        var trigger = new MacroTrigger { Id = Guid.NewGuid(), Active = false };
        var settings = new MacroSettings
        {
            GlobalProfile = new MacroProfile
            {
                Name = MacroProfile.GlobalName,
                Triggers = [trigger],
            },
        };

        engine.ApplySettings(settings);
        MacroTrigger? runtime = engine.FindTrigger(trigger.Id);
        Assert.NotNull(runtime);

        engine.ToggleTriggerActive(runtime!);
        Thread.Sleep(350);
        engine.ToggleTriggerActive(runtime!);

        Assert.False(runtime!.Active);
    }
}

public class MacroColorHelperTests
{
    [Theory]
    [InlineData("#AABBCC", true, 0xAA, 0xBB, 0xCC)]
    [InlineData("AABBCC", true, 0xAA, 0xBB, 0xCC)]
    [InlineData("#abc", false, 0, 0, 0)]
    [InlineData("not-a-color", false, 0, 0, 0)]
    public void TryParseHex_parses_strict_rrggbb(string raw, bool expectedValid, int r, int g, int b)
    {
        bool valid = MacroColorHelper.TryParseHex(raw, out Color color);
        Assert.Equal(expectedValid, valid);

        if (expectedValid)
        {
            Assert.Equal(r, color.R);
            Assert.Equal(g, color.G);
            Assert.Equal(b, color.B);
        }
    }

    [Fact]
    public void MatchesStrict_compares_rgb_only()
    {
        Color expected = Color.FromArgb(255, 10, 20, 30);
        Color match = Color.FromArgb(0, 10, 20, 30);
        Color mismatch = Color.FromArgb(255, 10, 20, 31);

        Assert.True(MacroColorHelper.MatchesStrict(match, expected));
        Assert.False(MacroColorHelper.MatchesStrict(mismatch, expected));
    }

    [Fact]
    public void RememberColor_deduplicates_and_caps_list()
    {
        var settings = new MacroSettings();
        MacroColorHelper.RememberColor(settings, "#112233");
        MacroColorHelper.RememberColor(settings, "#AABBCC");
        MacroColorHelper.RememberColor(settings, "#112233");

        Assert.Equal(2, settings.RememberedColors.Count);
        Assert.Equal("#112233", settings.RememberedColors[0], StringComparer.OrdinalIgnoreCase);
    }
}
