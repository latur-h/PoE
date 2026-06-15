using PoE.dlls.Gamble;

namespace PoE.dlls.Settings.Mods
{
    public static class GambleModeLayout
    {
        public const int MaxRules = 100;

        public static bool UsesSecond(GambleType type) =>
            type is GambleType.Alt_Aug or GambleType.Map;
    }
}
