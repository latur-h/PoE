using PoE.dlls.InteropServices;

namespace PoE.dlls.Settings.Mods
{
    public class GambleItemCoordinates
    {
        public Coordinates Default { get; set; } = new(0, 0);
        public Coordinates Harvest { get; set; } = new(0, 0);
        public Coordinates Essence { get; set; } = new(0, 0);
    }
}
