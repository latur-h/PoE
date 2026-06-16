using PoE.dlls.InteropServices;

namespace PoE.dlls.Settings.Mods
{
    public sealed class GambleMapBulkSettings
    {
        public bool BulkInventory { get; set; }

        public bool CorruptOnSuccess { get; set; }

        public Coordinates GridStart { get; set; } = new(0, 0);

        public Coordinates GridEnd { get; set; } = new(0, 0);

        /// <summary>Screen center of the top-left map cell in the grid.</summary>
        public Coordinates CellAnchor { get; set; } = new(0, 0);

        /// <summary>Horizontal pixels between map cell centers.</summary>
        public int NextX { get; set; }

        /// <summary>Vertical pixels between map cell centers. Use 0 for a single row.</summary>
        public int NextY { get; set; }

        public bool HasGridArea =>
            GridStart.X != GridEnd.X || GridStart.Y != GridEnd.Y;

        public bool HasCellStep => NextX > 0;

        public bool IsConfigured =>
            HasGridArea && CellAnchor.X > 0 && CellAnchor.Y > 0 && HasCellStep;
    }
}
