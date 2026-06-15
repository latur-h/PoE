using PoE.dlls.KeyBindings;
using PoE.dlls.Macros;
using PoE.dlls.Settings;
using PoE.dlls.Settings.Macros;
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

public class MacroKeyConflictCheckerTests
{
    [Fact]
    public void FindConflicts_reports_duplicate_macro_trigger_and_flask_key()
    {
        var settings = new Settings();
        MacroSettingsHelper.EnsureInitialized(settings.Macros);
        settings.Flasks["1"].Key = "F1";
        settings.Macros.GlobalProfile.Triggers.Add(new MacroTrigger
        {
            Active = true,
            TriggerKey = "F1",
            FireSequence = "Q Down\nQ Up",
            Behavior = MacroBehavior.Single,
        });

        var conflicts = MacroKeyConflictChecker.FindConflicts(settings);

        Assert.Contains(conflicts, c => c.Key == "F1");
    }

    [Fact]
    public void FindConflicts_ignores_repeat_without_toggle_key()
    {
        var settings = new Settings();
        MacroSettingsHelper.EnsureInitialized(settings.Macros);
        settings.Macros.GlobalProfile.Triggers.Add(new MacroTrigger
        {
            Active = true,
            Behavior = MacroBehavior.Repeat,
            FireSequence = "Q Down\nQ Up",
            ToggleKey = string.Empty,
        });

        var usages = MacroKeyConflictChecker.CollectUsages(settings);
        Assert.DoesNotContain(usages, u => u.Label.Contains("start/stop", StringComparison.Ordinal));
    }
}
