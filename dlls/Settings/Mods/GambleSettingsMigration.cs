using PoE.dlls.Gamble;
using PoE.dlls.InteropServices;

namespace PoE.dlls.Settings.Mods
{
    public static class GambleSettingsMigration
    {
        public static void EnsureMigrated(UIModifiers modifiers)
        {
            modifiers.ModeStores ??= [];

            if (modifiers.ModeStores.Count > 0)
            {
                EnsureAllModesPresent(modifiers.ModeStores);
            }
            else
            {
                modifiers.ModeStores = CreateEmptyModeStores();

                MigrateLegacy(modifiers, GambleType.Alt, modifiers._uialt);
                MigrateLegacy(modifiers, GambleType.Alt_Aug, modifiers._uialt_aug);
                MigrateLegacy(modifiers, GambleType.Chaos, modifiers._uichaos);
                MigrateLegacy(modifiers, GambleType.Chromatic, modifiers._uichromatic);
                MigrateLegacy(modifiers, GambleType.Eldritch, modifiers._uieldritch);
                MigrateLegacy(modifiers, GambleType.Essence, modifiers._uiesscence);
                MigrateLegacy(modifiers, GambleType.Harvest, modifiers._uiharvest);
                MigrateLegacy(modifiers, GambleType.Map, modifiers._uimap);
                MigrateLegacy(modifiers, GambleType.MapT17, modifiers._uimapT17);
            }

            MigrateCoordinates(modifiers);
        }

        private static void MigrateCoordinates(UIModifiers modifiers)
        {
            modifiers.Items ??= new();
            modifiers.Orbs ??= new();
            modifiers.Orbs.MigrateLegacyEldritchOrb();

            foreach (GambleType type in Enum.GetValues<GambleType>())
            {
                if (!modifiers.ModeStores.TryGetValue(type, out var store))
                    continue;

                if (store.Item is { } item && IsSet(item))
                    SetItemIfEmpty(modifiers, type, item);

                if (store.Base is { } primary && IsSet(primary))
                {
                    var orb = GambleCoordinateResolver.PrimaryOrb(type);
                    if (orb is not null)
                        SetOrbIfEmpty(modifiers.Orbs, orb.Value, primary);
                }

                if (store.Second is { } second && IsSet(second))
                {
                    var orb = GambleCoordinateResolver.SecondaryOrb(type);
                    if (orb is not null)
                        SetOrbIfEmpty(modifiers.Orbs, orb.Value, second);
                }

                if (store.Third is { } third && IsSet(third))
                {
                    var orb = GambleCoordinateResolver.TertiaryOrb(type);
                    if (orb is not null)
                        SetOrbIfEmpty(modifiers.Orbs, orb.Value, third);
                }

                store.Item = null;
                store.Base = null;
                store.Second = null;
                store.Third = null;
            }
        }

        private static void SetItemIfEmpty(UIModifiers modifiers, GambleType type, Coordinates value)
        {
            switch (type)
            {
                case GambleType.Harvest:
                    if (!IsSet(modifiers.Items.Harvest))
                        modifiers.Items.Harvest = value;
                    break;
                case GambleType.Essence:
                    if (!IsSet(modifiers.Items.Essence))
                        modifiers.Items.Essence = value;
                    break;
                default:
                    if (!IsSet(modifiers.Items.Default))
                        modifiers.Items.Default = value;
                    break;
            }
        }

        private static void SetOrbIfEmpty(GambleOrbCoordinates orbs, GambleOrbType type, Coordinates value)
        {
            if (IsSet(orbs.Get(type)))
                return;

            orbs.Set(type, value);
        }

        private static bool IsSet(Coordinates c) => c.X != 0 || c.Y != 0;

        private static void MigrateLegacy(UIModifiers modifiers, GambleType type, IUIMods legacy)
        {
            var store = modifiers.ModeStores[type];
            store.Item = legacy.Item;
            store.Base = legacy.Base;
            store.Second = legacy.Second;
            store.ActivePresetName = GamblePreset.DefaultName;
            store.Presets =
            [
                new GamblePreset
                {
                    Name = GamblePreset.DefaultName,
                    Rules = ExtractRules(legacy)
                }
            ];
        }

        private static List<GambleRuleRow> ExtractRules(IUIMods legacy)
        {
            var rows = new List<GambleRuleRow>();

            AddIfPresent(rows, legacy.Priority1, legacy.modifierType1, legacy.Tier1, legacy.Content1);
            AddIfPresent(rows, legacy.Priority2, legacy.modifierType2, legacy.Tier2, legacy.Content2);
            AddIfPresent(rows, legacy.Priority3, legacy.modifierType3, legacy.Tier3, legacy.Content3);
            AddIfPresent(rows, legacy.Priority4, legacy.modifierType4, legacy.Tier4, legacy.Content4);
            AddIfPresent(rows, legacy.Priority5, legacy.modifierType5, legacy.Tier5, legacy.Content5);
            AddIfPresent(rows, legacy.Priority6, legacy.modifierType6, legacy.Tier6, legacy.Content6);
            AddIfPresent(rows, legacy.Priority7, legacy.modifierType7, legacy.Tier7, legacy.Content7);
            AddIfPresent(rows, legacy.Priority8, legacy.modifierType8, legacy.Tier8, legacy.Content8);

            return rows;
        }

        private static void AddIfPresent(
            List<GambleRuleRow> rows,
            decimal priority,
            Gamble.Modifiers.ModifierType type,
            int tier,
            string content)
        {
            if (string.IsNullOrEmpty(content))
                return;

            rows.Add(new GambleRuleRow
            {
                Priority = priority,
                ModifierType = type,
                Tier = tier,
                Content = content
            });
        }

        private static Dictionary<GambleType, GambleModeStore> CreateEmptyModeStores()
        {
            var stores = new Dictionary<GambleType, GambleModeStore>();
            foreach (GambleType type in Enum.GetValues<GambleType>())
                stores[type] = new GambleModeStore();

            return stores;
        }

        private static void EnsureAllModesPresent(Dictionary<GambleType, GambleModeStore> stores)
        {
            foreach (GambleType type in Enum.GetValues<GambleType>())
            {
                if (!stores.ContainsKey(type))
                    stores[type] = new GambleModeStore();

                var store = stores[type];
                GamblePresetHelper.NormalizeModeStore(store);
            }
        }
    }
}
