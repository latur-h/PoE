using PoE.dlls.Gamble;

namespace PoE.dlls.Settings.Mods
{
    public static class GambleModeLayout
    {
        public const int MaxRules = 100;

        public static bool UsesSecond(GambleType type) =>
            type is GambleType.Alt_Aug or GambleType.Map or GambleType.MapExalt;

        public static bool UsesThird(GambleType type) =>
            type is GambleType.MapExalt;

        public static string BaseCoordinateLabel(GambleType type) => type switch
        {
            GambleType.Alt => "Alt",
            GambleType.Alt_Aug => "Alt",
            GambleType.Chromatic => "Chromatic",
            GambleType.Chaos => "Chaos",
            GambleType.Essence => "Essence",
            GambleType.Map => "Alchemy",
            GambleType.MapT17 => "Chaos",
            GambleType.MapExalt => "Alchemy",
            GambleType.Harvest => "Craft",
            GambleType.Eldritch => "Eldritch",
            _ => "Base",
        };

        public static string SecondCoordinateLabel(GambleType type) => type switch
        {
            GambleType.Alt_Aug => "Aug",
            GambleType.Map => "Scouring",
            GambleType.MapExalt => "Scouring",
            _ => "Second",
        };

        public static string ThirdCoordinateLabel(GambleType type) => type switch
        {
            GambleType.MapExalt => "Exalt",
            _ => "Third",
        };
    }
}
