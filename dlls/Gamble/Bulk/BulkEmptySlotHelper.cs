using PoE.dlls.InteropServices;
using PoE.dlls.Macros;
using PoE.dlls.Settings.Mods;

namespace PoE.dlls.Gamble.Bulk
{
    internal static class BulkEmptySlotHelper
    {
        public static bool IsRegistered(GambleMapBulkSettings settings) =>
            settings.FastEmptyColorCheck && IsRegistrationValid(settings);

        public static bool IsRegistrationValid(GambleMapBulkSettings settings)
        {
            if (settings.EmptySlotSignatures.Count == 0)
                return false;

            IReadOnlyList<Coordinates> cells = GambleGridCalculator.BuildCellCenters(settings);
            if (cells.Count != settings.EmptySlotSignatures.Count)
                return false;

            for (int i = 0; i < cells.Count; i++)
            {
                BulkEmptySlotSignature signature = settings.EmptySlotSignatures[i];
                Coordinates cell = cells[i];
                if (signature.X != cell.X || signature.Y != cell.Y)
                    return false;

                if (!MacroColorHelper.TryParseHex(signature.Color, out _))
                    return false;
            }

            return true;
        }

        public static void ClearRegistrationIfStale(GambleMapBulkSettings settings)
        {
            if (settings.EmptySlotSignatures.Count == 0)
                return;

            if (!IsRegistrationValid(settings))
                settings.EmptySlotSignatures.Clear();
        }

        public static bool TryRegister(GambleMapBulkSettings settings, out string error)
        {
            if (!settings.IsConfigured)
            {
                error = "Configure the full grid before registering empty slots.";
                return false;
            }

            IReadOnlyList<Coordinates> cells = GambleGridCalculator.BuildCellCenters(settings);
            if (cells.Count == 0)
            {
                error = "Grid has no cells — check area, First cell, and Next X / Next Y.";
                return false;
            }

            settings.EmptySlotSignatures = cells
                .Select(cell =>
                {
                    Color sampled = InteropHelper.GetColorAt(cell.X, cell.Y);
                    return new BulkEmptySlotSignature
                    {
                        X = cell.X,
                        Y = cell.Y,
                        Color = MacroColorHelper.ToHex(sampled),
                    };
                })
                .ToList();

            error = string.Empty;
            return true;
        }

        public static int MarkMatchingSlotsEmpty(
            IReadOnlyList<BulkMapSlot> slots,
            IReadOnlyList<BulkEmptySlotSignature> signatures)
        {
            if (slots.Count != signatures.Count)
                return 0;

            int skipped = 0;

            for (int i = 0; i < slots.Count; i++)
            {
                BulkMapSlot slot = slots[i];
                BulkEmptySlotSignature signature = signatures[i];

                if (slot.IsEmpty)
                    continue;

                if (slot.Position.X != signature.X || slot.Position.Y != signature.Y)
                    continue;

                if (!MacroColorHelper.TryParseHex(signature.Color, out Color expected))
                    continue;

                Color sampled = InteropHelper.GetColorAt(slot.Position.X, slot.Position.Y);
                if (!MacroColorHelper.MatchesStrict(sampled, expected))
                    continue;

                slot.IsEmpty = true;
                skipped++;
            }

            return skipped;
        }
    }
}
