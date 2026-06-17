using PoE.dlls.Gamble.Bulk;
using PoE.dlls.Settings.Mods;
using Xunit;

namespace PoE.Tests;

public class BulkEmptySlotHelperTests
{
    [Fact]
    public void IsRegistrationValid_requires_matching_grid_and_signatures()
    {
        var bulk = new GambleMapBulkSettings
        {
            GridStart = new(100, 100),
            GridEnd = new(200, 200),
            CellAnchor = new(150, 150),
            NextX = 50,
            NextY = 0,
            EmptySlotSignatures =
            [
                new BulkEmptySlotSignature { X = 150, Y = 150, Color = "#112233" },
                new BulkEmptySlotSignature { X = 200, Y = 150, Color = "#445566" },
            ],
        };

        Assert.True(BulkEmptySlotHelper.IsRegistrationValid(bulk));
    }

    [Fact]
    public void IsRegistrationValid_false_when_cell_count_differs()
    {
        var bulk = new GambleMapBulkSettings
        {
            GridStart = new(140, 140),
            GridEnd = new(160, 160),
            CellAnchor = new(150, 150),
            NextX = 50,
            NextY = 0,
            EmptySlotSignatures =
            [
                new BulkEmptySlotSignature { X = 150, Y = 150, Color = "#112233" },
                new BulkEmptySlotSignature { X = 200, Y = 150, Color = "#445566" },
            ],
        };

        Assert.False(BulkEmptySlotHelper.IsRegistrationValid(bulk));
    }

    [Fact]
    public void IsRegistered_requires_checkbox_and_valid_signatures()
    {
        var bulk = new GambleMapBulkSettings
        {
            FastEmptyColorCheck = true,
            GridStart = new(140, 140),
            GridEnd = new(160, 160),
            CellAnchor = new(150, 150),
            NextX = 50,
            NextY = 0,
            EmptySlotSignatures =
            [
                new BulkEmptySlotSignature { X = 150, Y = 150, Color = "#112233" },
            ],
        };

        Assert.True(BulkEmptySlotHelper.IsRegistered(bulk));
    }

    [Fact]
    public void ClearRegistrationIfStale_removes_mismatched_signatures()
    {
        var bulk = new GambleMapBulkSettings
        {
            GridStart = new(140, 140),
            GridEnd = new(160, 160),
            CellAnchor = new(150, 150),
            NextX = 50,
            NextY = 0,
            EmptySlotSignatures =
            [
                new BulkEmptySlotSignature { X = 999, Y = 150, Color = "#112233" },
            ],
        };

        BulkEmptySlotHelper.ClearRegistrationIfStale(bulk);

        Assert.Empty(bulk.EmptySlotSignatures);
    }
}
