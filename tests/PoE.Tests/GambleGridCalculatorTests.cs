using PoE.dlls.Gamble.Bulk;
using PoE.dlls.InteropServices;
using PoE.dlls.Settings.Mods;
using Xunit;

namespace PoE.Tests;

public class GambleGridCalculatorTests
{
    [Fact]
    public void BuildCellCenters_returns_empty_when_grid_area_missing()
    {
        var grid = new GambleMapBulkSettings
        {
            GridStart = new Coordinates(100, 100),
            GridEnd = new Coordinates(100, 100),
            CellAnchor = new Coordinates(120, 130),
        };

        Assert.Empty(GambleGridCalculator.BuildCellCenters(grid));
    }

    [Fact]
    public void BuildCellCenters_builds_regular_grid_from_explicit_next_step()
    {
        var grid = new GambleMapBulkSettings
        {
            GridStart = new Coordinates(100, 100),
            GridEnd = new Coordinates(179, 179),
            CellAnchor = new Coordinates(120, 120),
            NextX = 40,
            NextY = 40,
        };

        var cells = GambleGridCalculator.BuildCellCenters(grid);

        Assert.Equal(4, cells.Count);
        Assert.Contains(new Coordinates(120, 120), cells);
        Assert.Contains(new Coordinates(160, 120), cells);
        Assert.Contains(new Coordinates(120, 160), cells);
        Assert.Contains(new Coordinates(160, 160), cells);
    }

    [Fact]
    public void BuildCellCenters_supports_single_row_when_next_y_is_zero()
    {
        var grid = new GambleMapBulkSettings
        {
            GridStart = new Coordinates(100, 100),
            GridEnd = new Coordinates(219, 139),
            CellAnchor = new Coordinates(120, 120),
            NextX = 40,
            NextY = 0,
        };

        var cells = GambleGridCalculator.BuildCellCenters(grid);

        Assert.Equal(3, cells.Count);
        Assert.Contains(new Coordinates(120, 120), cells);
        Assert.Contains(new Coordinates(160, 120), cells);
        Assert.Contains(new Coordinates(200, 120), cells);
    }

    [Fact]
    public void BuildCellCenters_falls_back_to_anchor_insets_when_next_step_missing()
    {
        var grid = new GambleMapBulkSettings
        {
            GridStart = new Coordinates(100, 100),
            GridEnd = new Coordinates(179, 179),
            CellAnchor = new Coordinates(120, 120),
        };

        var cells = GambleGridCalculator.BuildCellCenters(grid);

        Assert.Equal(4, cells.Count);
        Assert.Contains(new Coordinates(160, 160), cells);
    }
}
