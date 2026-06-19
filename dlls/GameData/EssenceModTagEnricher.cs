namespace PoE.dlls.GameData
{
    internal static class EssenceModTagEnricher
    {
        private static readonly (string Column, string Tag)[] TypeColumns =
        [
            ("Claw_ModsKey", "claw"),
            ("Dagger_ModsKey", "dagger"),
            ("Wand_ModsKey", "wand"),
            ("OneHandSword_ModsKey", "sword"),
            ("OneHandThrustingSword_ModsKey", "sword"),
            ("OneHandAxe_ModsKey", "axe"),
            ("OneHandMace_ModsKey", "mace"),
            ("Sceptre_ModsKey", "sceptre"),
            ("Bow_ModsKey", "bow"),
            ("Staff_ModsKey", "staff"),
            ("TwoHandSword_ModsKey", "sword"),
            ("TwoHandAxe_ModsKey", "axe"),
            ("TwoHandMace_ModsKey", "mace"),
            ("Ring_ModsKey", "ring"),
            ("Amulet_ModsKey", "amulet"),
            ("Belt_ModsKey", "belt"),
            ("Gloves_ModsKey", "gloves"),
            ("Boots_ModsKey", "boots"),
            ("Helmet_ModsKey", "helmet"),
            ("BodyArmour_ModsKey", "body_armour"),
            ("Shield_ModsKey", "shield"),
        ];

        public static Dictionary<int, HashSet<string>> BuildModRowTags(LibDat2DatTable essences)
        {
            var map = new Dictionary<int, HashSet<string>>();

            for (int row = 0; row < essences.RowCount; row++)
            {
                foreach ((string column, string tag) in TypeColumns)
                {
                    int? modKey = essences.GetForeignKey(row, column);
                    if (modKey is null || (uint)modKey.Value >= int.MaxValue)
                        continue;

                    if (!map.TryGetValue(modKey.Value, out HashSet<string>? set))
                    {
                        set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        map[modKey.Value] = set;
                    }

                    set.Add(tag);
                }
            }

            return map;
        }
    }
}
