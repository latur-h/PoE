using PoE.dlls.InteropServices;

namespace PoE.dlls.Settings.Mods
{
    public class GambleOrbCoordinates
    {
        public Coordinates Alt { get; set; } = new(0, 0);
        public Coordinates Aug { get; set; } = new(0, 0);
        public Coordinates Chaos { get; set; } = new(0, 0);
        public Coordinates Chromatic { get; set; } = new(0, 0);
        public Coordinates Essence { get; set; } = new(0, 0);
        public Coordinates Alchemy { get; set; } = new(0, 0);
        public Coordinates Scouring { get; set; } = new(0, 0);
        public Coordinates Exalt { get; set; } = new(0, 0);
        public Coordinates Vaal { get; set; } = new(0, 0);
        public Coordinates EldritchExarch { get; set; } = new(0, 0);
        public Coordinates EldritchEater { get; set; } = new(0, 0);
        public Coordinates Craft { get; set; } = new(0, 0);

        // Legacy JSON field — migrated to EldritchExarch / EldritchEater on load.
        public Coordinates Eldritch { get; set; } = new(0, 0);

        public void MigrateLegacyEldritchOrb()
        {
            if (!IsSet(Eldritch))
                return;

            if (!IsSet(EldritchExarch))
                EldritchExarch = Eldritch;
            if (!IsSet(EldritchEater))
                EldritchEater = Eldritch;

            Eldritch = new Coordinates(0, 0);
        }

        private static bool IsSet(Coordinates c) => c.X != 0 || c.Y != 0;

        public Coordinates Get(GambleOrbType type) => type switch
        {
            GambleOrbType.Alt => Alt,
            GambleOrbType.Aug => Aug,
            GambleOrbType.Chaos => Chaos,
            GambleOrbType.Chromatic => Chromatic,
            GambleOrbType.Essence => Essence,
            GambleOrbType.Alchemy => Alchemy,
            GambleOrbType.Scouring => Scouring,
            GambleOrbType.Exalt => Exalt,
            GambleOrbType.Vaal => Vaal,
            GambleOrbType.EldritchExarch => EldritchExarch,
            GambleOrbType.EldritchEater => EldritchEater,
            GambleOrbType.Craft => Craft,
            _ => new Coordinates(0, 0),
        };

        public void Set(GambleOrbType type, Coordinates value)
        {
            switch (type)
            {
                case GambleOrbType.Alt: Alt = value; break;
                case GambleOrbType.Aug: Aug = value; break;
                case GambleOrbType.Chaos: Chaos = value; break;
                case GambleOrbType.Chromatic: Chromatic = value; break;
                case GambleOrbType.Essence: Essence = value; break;
                case GambleOrbType.Alchemy: Alchemy = value; break;
                case GambleOrbType.Scouring: Scouring = value; break;
                case GambleOrbType.Exalt: Exalt = value; break;
                case GambleOrbType.Vaal: Vaal = value; break;
                case GambleOrbType.EldritchExarch: EldritchExarch = value; break;
                case GambleOrbType.EldritchEater: EldritchEater = value; break;
                case GambleOrbType.Craft: Craft = value; break;
            }
        }
    }
}
