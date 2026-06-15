using PoE.dlls.Gamble;
using PoE.dlls.InteropServices;

namespace PoE.dlls.Settings.Mods
{
    public class GambleModeStore
    {
        public string ActivePresetName { get; set; } = GamblePreset.DefaultName;
        public List<GamblePreset> Presets { get; set; } = [new GamblePreset()];

        // Legacy — migrated to UIModifiers.Items / Orbs on load; not saved after migration.
        public Coordinates? Item { get; set; }
        public Coordinates? Base { get; set; }
        public Coordinates? Second { get; set; }
        public Coordinates? Third { get; set; }

        internal bool HasLegacyCoordinates() =>
            IsSet(Item) || IsSet(Base) || IsSet(Second) || IsSet(Third);

        private static bool IsSet(Coordinates? c) => c is { } value && (value.X != 0 || value.Y != 0);
    }
}
