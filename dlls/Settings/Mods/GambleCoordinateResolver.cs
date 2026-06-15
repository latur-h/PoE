using PoE.dlls.Gamble;
using PoE.dlls.InteropServices;

namespace PoE.dlls.Settings.Mods
{
    public readonly record struct ResolvedGambleCoordinates(
        Coordinates Item,
        Coordinates Base,
        Coordinates Second,
        Coordinates Third);

    public static class GambleCoordinateResolver
    {
        public static Coordinates GetItem(GambleType type, GambleItemCoordinates items) => type switch
        {
            GambleType.Harvest => items.Harvest,
            GambleType.Essence => items.Essence,
            _ => items.Default,
        };

        public static void SetItem(GambleType type, GambleItemCoordinates items, Coordinates value)
        {
            switch (type)
            {
                case GambleType.Harvest: items.Harvest = value; break;
                case GambleType.Essence: items.Essence = value; break;
                default: items.Default = value; break;
            }
        }

        public static string ItemSlotLabel(GambleType type) => type switch
        {
            GambleType.Harvest => "Harvest item",
            GambleType.Essence => "Essence item",
            _ => "Default item",
        };

        public static string OrbLabel(GambleOrbType type) => type switch
        {
            GambleOrbType.Alt => "Alt",
            GambleOrbType.Aug => "Aug",
            GambleOrbType.Chaos => "Chaos",
            GambleOrbType.Chromatic => "Chromatic",
            GambleOrbType.Essence => "Essence",
            GambleOrbType.Alchemy => "Alchemy",
            GambleOrbType.Scouring => "Scouring",
            GambleOrbType.Exalt => "Exalt",
            GambleOrbType.Eldritch => "Eldritch",
            GambleOrbType.Craft => "Craft",
            _ => type.ToString(),
        };

        public static ResolvedGambleCoordinates Resolve(
            GambleType type,
            GambleItemCoordinates items,
            GambleOrbCoordinates orbs)
        {
            var item = GetItem(type, items);
            var primary = PrimaryOrb(type);
            var second = SecondaryOrb(type);
            var third = TertiaryOrb(type);

            return new ResolvedGambleCoordinates(
                item,
                primary is null ? new Coordinates(0, 0) : orbs.Get(primary.Value),
                second is null ? new Coordinates(0, 0) : orbs.Get(second.Value),
                third is null ? new Coordinates(0, 0) : orbs.Get(third.Value));
        }

        public static GambleOrbType? PrimaryOrb(GambleType type) => type switch
        {
            GambleType.Alt or GambleType.Alt_Aug => GambleOrbType.Alt,
            GambleType.Chaos or GambleType.MapT17 => GambleOrbType.Chaos,
            GambleType.Chromatic => GambleOrbType.Chromatic,
            GambleType.Essence => GambleOrbType.Essence,
            GambleType.Map or GambleType.MapExalt => GambleOrbType.Alchemy,
            GambleType.Harvest => GambleOrbType.Craft,
            GambleType.Eldritch => GambleOrbType.Eldritch,
            _ => null,
        };

        public static GambleOrbType? SecondaryOrb(GambleType type) => type switch
        {
            GambleType.Alt_Aug => GambleOrbType.Aug,
            GambleType.Map or GambleType.MapExalt => GambleOrbType.Scouring,
            _ => null,
        };

        public static GambleOrbType? TertiaryOrb(GambleType type) => type switch
        {
            GambleType.MapExalt => GambleOrbType.Exalt,
            _ => null,
        };

        public static IEnumerable<GambleOrbType> AllOrbTypes() => Enum.GetValues<GambleOrbType>();
    }
}
