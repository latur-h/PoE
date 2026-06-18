using PoE.dlls.Gamble.Bulk;
using PoE.dlls.Gamble.Modes;
using PoE.dlls.InteropServices;
using PoE.dlls.Settings.Mods;
using Xunit;

namespace PoE.Tests;

public class BulkMapActionHelperTests
{
    [Fact]
    public void AssignScourOrAlchemyOnly_rare_uses_scour_alchemy()
    {
        var slot = new BulkMapSlot { Position = new Coordinates(1, 2) };
        var eval = new MapRulesResult(true, true, false, 1, 1, MapRuleFailure.None, false);

        BulkMapActionHelper.AssignScourOrAlchemyOnly(slot, eval);

        Assert.Equal(BulkMapAction.ScourAlchemy, slot.NextAction);
    }

    [Fact]
    public void AssignScourOrAlchemyOnly_normal_uses_alchemy_only()
    {
        var slot = new BulkMapSlot { Position = new Coordinates(1, 2), Content = "Rarity: Normal\nItem Class: Maps" };
        var eval = new MapRulesResult(true, false, false, 1, 1, MapRuleFailure.None, false);

        BulkMapActionHelper.AssignScourOrAlchemyOnly(slot, eval);

        Assert.Equal(BulkMapAction.AlchemyOnly, slot.NextAction);
    }

    [Fact]
    public void AssignScourOrAlchemyOnly_magic_uses_scour_alchemy()
    {
        var slot = new BulkMapSlot { Position = new Coordinates(1, 2), Content = "Rarity: Magic\nItem Class: Maps" };
        var eval = new MapRulesResult(true, false, false, 1, 1, MapRuleFailure.None, false);

        BulkMapActionHelper.AssignScourOrAlchemyOnly(slot, eval);

        Assert.Equal(BulkMapAction.ScourAlchemy, slot.NextAction);
    }

    [Fact]
    public void ResolveNonRarePrep_magic_uses_scour_alchemy()
    {
        Assert.Equal(
            BulkMapAction.ScourAlchemy,
            BulkMapActionHelper.ResolveNonRarePrep("Rarity: Magic\nItem Class: Maps"));
    }

    [Fact]
    public void ResolveRefreshDelay_uses_bulk_setting_when_positive()
    {
        var bulk = new GambleMapBulkSettings { RefreshDelayMs = 120 };

        TimeSpan delay = BulkMapActionHelper.ResolveRefreshDelay(bulk, TimeSpan.FromMilliseconds(10));

        Assert.Equal(TimeSpan.FromMilliseconds(120), delay);
    }

    [Fact]
    public void ResolveRefreshDelay_falls_back_to_action_delay_floor()
    {
        TimeSpan delay = BulkMapActionHelper.ResolveRefreshDelay(null, TimeSpan.FromMilliseconds(10));

        Assert.Equal(TimeSpan.FromMilliseconds(50), delay);
    }
}
