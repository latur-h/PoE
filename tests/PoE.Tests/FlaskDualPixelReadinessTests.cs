using PoE.dlls.Flasks;
using System.Drawing;
using Xunit;

namespace PoE.Tests;

public class FlaskDualPixelReadinessTests
{
    [Fact]
    public void UtilityIsReady_when_full_and_effect_off()
    {
        Color top = Color.FromArgb(10, 20, 30);
        Color bottomReady = Color.FromArgb(40, 50, 60);

        Assert.True(FlaskDualPixelReadiness.UtilityIsReady(top, bottomReady, top));
    }

    [Fact]
    public void UtilityIsReady_false_when_effect_on()
    {
        Color top = Color.FromArgb(10, 20, 30);

        Assert.False(FlaskDualPixelReadiness.UtilityIsReady(
            top,
            FlaskDualPixelReadiness.UtilityEffectBottomColor,
            top));
    }

    [Fact]
    public void UtilityIsReady_false_when_not_full()
    {
        Color top = Color.FromArgb(10, 20, 30);
        Color bottomReady = Color.FromArgb(40, 50, 60);

        Assert.False(FlaskDualPixelReadiness.UtilityIsReady(top, bottomReady, Color.FromArgb(1, 2, 3)));
    }

    [Fact]
    public void TinctureIsReady_when_effect_off_and_not_on_cooldown()
    {
        Color top = Color.FromArgb(10, 20, 30);
        Color bottomReady = Color.FromArgb(40, 50, 60);

        Assert.True(FlaskDualPixelReadiness.TinctureIsReady(top, bottomReady, top));
    }

    [Fact]
    public void TinctureIsReady_false_when_on_cooldown()
    {
        Color top = Color.FromArgb(10, 20, 30);

        Assert.False(FlaskDualPixelReadiness.TinctureIsReady(
            top,
            FlaskDualPixelReadiness.TinctureCooldownBottomColor,
            top));
    }
}
