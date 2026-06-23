using System.Drawing;
using PoE.dlls.Gamble.Bulk;
using PoE.dlls.Gamble.Modes;
using PoE.dlls.InteropServices;
using PoE.dlls.Settings.Mods;
using Xunit;

namespace PoE.Tests;

public class BulkMapHighlightBuilderTests
{
    [Fact]
    public void ResolveColor_FailedRules_ReturnsRed()
    {
        var eval = new MapRulesResult(true, true, false, 6, 6, MapRuleFailure.Exclude, false);

        Assert.Equal(BulkMapHighlightColor.Red, BulkMapHighlightBuilder.ResolveColor(eval));
    }

    [Fact]
    public void ResolveColor_PassedWithEightAffixes_ReturnsOrange()
    {
        var eval = new MapRulesResult(true, true, false, 8, 8, MapRuleFailure.None, true);

        Assert.Equal(BulkMapHighlightColor.Orange, BulkMapHighlightBuilder.ResolveColor(eval));
    }

    [Fact]
    public void ResolveColor_PassedWithSixAffixes_ReturnsGreen()
    {
        var eval = new MapRulesResult(true, true, false, 6, 6, MapRuleFailure.None, true);

        Assert.Equal(BulkMapHighlightColor.Green, BulkMapHighlightBuilder.ResolveColor(eval));
    }

    [Fact]
    public void GetCellRectangle_UsesGridStep()
    {
        var grid = new GambleMapBulkSettings
        {
            GridStart = new Coordinates(100, 100),
            GridEnd = new Coordinates(500, 500),
            CellAnchor = new Coordinates(150, 150),
            NextX = 60,
            NextY = 60,
        };

        Rectangle rect = GambleGridCellBounds.GetCellRectangle(grid, new Coordinates(210, 150));

        Assert.Equal(new Rectangle(182, 122, 56, 56), rect);
    }
}
