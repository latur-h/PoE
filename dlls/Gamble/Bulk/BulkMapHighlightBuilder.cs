using PoE.dlls.Gamble.Modes;
using PoE.dlls.Settings.Mods;

namespace PoE.dlls.Gamble.Bulk
{
    internal static class BulkMapHighlightBuilder
    {
        public static IReadOnlyList<BulkMapHighlightEntry> Build(
            IEnumerable<BulkMapSlot> slots,
            GambleMapBulkSettings grid)
        {
            var highlights = new List<BulkMapHighlightEntry>();

            foreach (BulkMapSlot slot in slots)
            {
                if (slot.IsEmpty || !slot.Evaluation.IsMap)
                    continue;

                BulkMapHighlightColor? color = ResolveColor(slot.Evaluation);
                if (color is null)
                    continue;

                highlights.Add(new BulkMapHighlightEntry(
                    GambleGridCellBounds.GetCellRectangle(grid, slot.Position),
                    color.Value));
            }

            return highlights;
        }

        public static BulkMapHighlightColor? ResolveColor(MapRulesResult eval)
        {
            if (!eval.IsMap)
                return null;

            if (!eval.RulesPassed)
                return BulkMapHighlightColor.Red;

            if (eval.AffixModCount >= 8)
                return BulkMapHighlightColor.Orange;

            return BulkMapHighlightColor.Green;
        }
    }
}
