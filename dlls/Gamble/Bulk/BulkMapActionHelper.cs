using PoE.dlls.Gamble.Modes;
using PoE.dlls.Settings.Mods;

namespace PoE.dlls.Gamble.Bulk
{
    internal static class BulkMapActionHelper
    {
        public static void AssignScourOrAlchemyOnly(BulkMapSlot slot, MapRulesResult eval)
        {
            slot.NextAction = eval.IsRare ? BulkMapAction.ScourAlchemy : BulkMapAction.AlchemyOnly;
        }

        public static TimeSpan ResolveRefreshDelay(GambleMapBulkSettings? bulkGrid, TimeSpan actionDelay)
        {
            if (bulkGrid is { RefreshDelayMs: > 0 })
                return TimeSpan.FromMilliseconds(bulkGrid.RefreshDelayMs);

            return TimeSpan.FromMilliseconds(Math.Max(50, (int)actionDelay.TotalMilliseconds));
        }
    }
}
