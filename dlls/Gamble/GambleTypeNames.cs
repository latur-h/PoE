namespace PoE.dlls.Gamble
{
    public static class GambleTypeNames
    {
        public static readonly GambleType[] All = Enum.GetValues<GambleType>();

        public static string DisplayName(GambleType type) => type switch
        {
            GambleType.Alt => "Alteration",
            GambleType.Alt_Aug => "Alt + Aug",
            GambleType.Chromatic => "Chromatic",
            GambleType.Chaos => "Chaos",
            GambleType.Essence => "Essence",
            GambleType.Map => "Map",
            GambleType.MapT17 => "Map T17",
            GambleType.MapExalt => "Map Exalt",
            GambleType.Harvest => "Harvest",
            GambleType.Eldritch => "Eldritch",
            _ => type.ToString(),
        };

        public static int IndexOf(GambleType type)
        {
            for (int i = 0; i < All.Length; i++)
            {
                if (All[i] == type)
                    return i;
            }

            return 0;
        }
    }
}
