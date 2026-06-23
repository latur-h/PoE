using PoE.dlls.InteropServices;

namespace PoE.dlls.Settings.Mods
{
    public sealed class GambleMapBulkSettings
    {
        public bool BulkInventory { get; set; }

        public bool CorruptOnSuccess { get; set; }

        /// <summary>After Vaal corrupt, require at least 8 affix mods before keeping the map.</summary>
        public bool CorruptRequireEightMods { get; set; }

        /// <summary>When rules fail on a corrupted/broken map: stash into inventory or highlight on screen.</summary>
        public BulkMapBrokenDisposition BrokenMapDisposition { get; set; } = BulkMapBrokenDisposition.Stash;

        public Coordinates GridStart { get; set; } = new(0, 0);

        public Coordinates GridEnd { get; set; } = new(0, 0);

        /// <summary>Screen center of the top-left map cell in the grid.</summary>
        public Coordinates CellAnchor { get; set; } = new(0, 0);

        /// <summary>Horizontal pixels between map cell centers.</summary>
        public int NextX { get; set; }

        /// <summary>Vertical pixels between map cell centers. Use 0 for a single row.</summary>
        public int NextY { get; set; }

        /// <summary>Map-only wait after slams/orb drops before copying tooltip (ms). Separate from Settings → Delay.</summary>
        public int RefreshDelayMs { get; set; } = 80;

        /// <summary>Skip slots whose center pixel matches preregistered empty signatures before clipboard precheck.</summary>
        public bool FastEmptyColorCheck { get; set; }

        /// <summary>Per-cell empty reference captured on a fully empty inventory grid.</summary>
        public List<BulkEmptySlotSignature> EmptySlotSignatures { get; set; } = [];

        public bool HasGridArea =>
            GridStart.X != GridEnd.X || GridStart.Y != GridEnd.Y;

        public bool HasCellStep => NextX > 0;

        public bool IsConfigured =>
            HasGridArea && CellAnchor.X > 0 && CellAnchor.Y > 0 && HasCellStep;
    }
}
