using PoE.dlls.InteropServices;

namespace PoE.dlls.Settings.Mods
{
    public class GambleModeStore
    {
        public Coordinates Item { get; set; } = new(0, 0);
        public Coordinates Base { get; set; } = new(0, 0);
        public Coordinates Second { get; set; } = new(0, 0);
        public Coordinates Third { get; set; } = new(0, 0);

        public string ActivePresetName { get; set; } = GamblePreset.DefaultName;
        public List<GamblePreset> Presets { get; set; } = [new GamblePreset()];
    }
}
