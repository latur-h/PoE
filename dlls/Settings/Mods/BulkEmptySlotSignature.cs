namespace PoE.dlls.Settings.Mods
{
    public sealed class BulkEmptySlotSignature
    {
        public int X { get; set; }

        public int Y { get; set; }

        /// <summary>Center pixel of an empty cell at registration (#RRGGBB).</summary>
        public string Color { get; set; } = string.Empty;
    }
}
