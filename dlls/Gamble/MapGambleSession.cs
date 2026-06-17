using PoE.dlls.Gamble;
using PoE.dlls.InteropServices;
using PoE.dlls.Settings.Mods;

namespace PoE.dlls.Gamba
{
    public sealed record MapGambleSession(
        bool CorruptOnSuccess,
        Coordinates Vaal,
        Coordinates Exalt,
        Coordinates Chaos,
        IReadOnlyList<Coordinates>? BulkCells,
        GambleMapBulkSettings? BulkGrid)
    {
        public bool RequiresEightModsAfterCorrupt =>
            CorruptOnSuccess && BulkGrid?.CorruptRequireEightMods == true;
    }
}
