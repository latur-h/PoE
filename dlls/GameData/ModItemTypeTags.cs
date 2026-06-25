namespace PoE.dlls.GameData
{
    internal static class ModItemTypeTags
    {
        private static readonly string[] OneHandWeaponShared =
        [
            "weapon", "one_hand_weapon", "onehand", "one_hand",
        ];

        private static readonly string[] TwoHandWeaponShared =
        [
            "weapon", "two_hand_weapon", "twohand", "two_hand",
        ];

        private static readonly HashSet<string> EldritchEligibleTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "gloves", "boots", "helmet", "body_armour", "amulet",
        };

        public static IReadOnlyList<string> EldritchEligibleItemTypes { get; } =
            EldritchEligibleTags.OrderBy(t => t, StringComparer.OrdinalIgnoreCase).ToArray();

        private static readonly string[] ArmourShared = ["default"];

        private static readonly string[] InfluenceSuffixes =
        [
            "shaper", "elder", "crusader", "hunter", "redeemer", "warlord",
        ];

        private static readonly HashSet<string> ConcreteWeaponTypeTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "bow", "claw", "dagger", "rune_dagger", "wand", "staff", "warstaff",
            "sword", "axe", "mace", "sceptre",
        };

        private static readonly Dictionary<string, string[]> Profiles = BuildProfiles();

        public static IReadOnlyList<string> GetMatchTags(string? normalizedFilter)
        {
            if (string.IsNullOrWhiteSpace(normalizedFilter))
                return [];

            if (string.Equals(normalizedFilter, "flask", StringComparison.OrdinalIgnoreCase))
            {
                return CombineTags(
                    "flask",
                    "life_flask",
                    "mana_flask",
                    "utility_flask",
                    "hybrid_flask",
                    "critical_utility_flask",
                    "tincture",
                    "default");
            }

            if (ModCatalogTagHelper.IsFlaskSpawnTag(normalizedFilter))
                return CombineTags(normalizedFilter, "flask", "default");

            if (Profiles.TryGetValue(normalizedFilter, out string[]? profile))
                return profile;

            if (ClusterJewelSizeTags.IsClusterSizeTag(normalizedFilter))
            {
                return Combine(
                    normalizedFilter,
                    ClusterJewelSizeTags.GetAfflictionTagsForSize(normalizedFilter));
            }

            if (ClusterJewelSizeTags.IsAfflictionClusterTag(normalizedFilter))
                return [normalizedFilter];

            IReadOnlyList<string> abyssTags = AbyssJewelSubtypeTags.GetMatchTagsForFilter(normalizedFilter);
            if (abyssTags.Count > 0)
                return abyssTags;

            if (ModSpawnTagFilter.TryResolveItemKind(normalizedFilter, out ModItemKind kind))
            {
                return kind switch
                {
                    ModItemKind.Flask => CombineTags(
                        "flask", "life_flask", "mana_flask", "utility_flask", "hybrid_flask", "critical_utility_flask", "tincture", "default"),
                    ModItemKind.Jewel => CombineTags("jewel", "default"),
                    ModItemKind.AbyssJewel => AbyssJewelSubtypeTags.GetAllMatchTags(),
                    ModItemKind.ClusterJewel => [],
                    _ => [normalizedFilter],
                };
            }

            return CombineTags(normalizedFilter, "default");
        }

        public static bool ModMatchesItemType(string spawnTagsCsv, string? normalizedFilter)
        {
            if (string.IsNullOrWhiteSpace(normalizedFilter))
                return true;

            if (string.IsNullOrWhiteSpace(spawnTagsCsv))
                return false;

            HashSet<string> modTags = ParseTags(spawnTagsCsv);
            if (modTags.Count == 0)
                return false;

            HashSet<string> itemTags = GetMatchTags(normalizedFilter).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (itemTags.Count == 0)
                return false;

            var concreteWeaponTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string modTag in modTags)
            {
                string? weaponBase = ResolveConcreteWeaponBaseTag(modTag);
                if (weaponBase is not null)
                    concreteWeaponTags.Add(weaponBase);
            }

            if (concreteWeaponTags.Count > 0)
                return concreteWeaponTags.Any(itemTags.Contains);

            return modTags.Any(modTag => TagMatchesItemTags(modTag, itemTags));
        }

        private static bool TagMatchesItemTags(string modTag, HashSet<string> itemTags)
        {
            if (itemTags.Contains(modTag))
                return true;

            foreach (string itemTag in itemTags)
            {
                if (modTag.StartsWith(itemTag + "_", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static string? ResolveConcreteWeaponBaseTag(string modTag)
        {
            if (ConcreteWeaponTypeTags.Contains(modTag))
                return modTag;

            foreach (string baseTag in ConcreteWeaponTypeTags)
            {
                if (modTag.StartsWith(baseTag + "_", StringComparison.OrdinalIgnoreCase))
                    return baseTag;
            }

            return null;
        }

        public static bool HasKnownProfile(string normalizedFilter) =>
            Profiles.ContainsKey(normalizedFilter)
            || ClusterJewelSizeTags.IsClusterSizeTag(normalizedFilter)
            || ClusterJewelSizeTags.IsAfflictionClusterTag(normalizedFilter)
            || AbyssJewelSubtypeTags.IsAbyssJewelTag(normalizedFilter)
            || ModSpawnTagFilter.TryResolveItemKind(normalizedFilter, out _)
            || ModCatalogTagHelper.IsFlaskSpawnTag(normalizedFilter);

        public static bool IsEldritchEligibleItemType(string? normalizedFilter)
        {
            if (string.IsNullOrWhiteSpace(normalizedFilter))
                return true;

            IReadOnlyList<string> matchTags = GetMatchTags(normalizedFilter);
            if (matchTags.Count == 0)
                return false;

            return matchTags.Any(EldritchEligibleTags.Contains);
        }

        private static HashSet<string> ParseTags(string spawnTagsCsv)
        {
            var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string part in spawnTagsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!string.IsNullOrWhiteSpace(part))
                    tags.Add(part);
            }

            return tags;
        }

        private static Dictionary<string, string[]> BuildProfiles()
        {
            var profiles = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

            AddOneHandWeapon(profiles, "claw");
            AddOneHandWeapon(profiles, "dagger");
            AddOneHandWeapon(profiles, "rune_dagger");
            AddOneHandWeapon(profiles, "sword");
            AddOneHandWeapon(profiles, "axe");
            AddOneHandWeapon(profiles, "mace");
            AddOneHandWeapon(profiles, "wand");

            profiles["sceptre"] = Combine("sceptre", OneHandWeaponShared);
            profiles["focus"] = Combine("focus", OneHandWeaponShared);

            AddTwoHandWeapon(profiles, "bow");
            AddTwoHandWeapon(profiles, "staff");
            AddTwoHandWeapon(profiles, "warstaff");

            profiles["weapon"] = Combine(
                OneHandWeaponShared.Concat(TwoHandWeaponShared).Concat(["claw", "dagger", "sword", "axe", "mace", "wand", "sceptre", "bow", "staff", "warstaff", "rune_dagger"]));

            profiles["ring"] = Combine("ring", ArmourShared, InfluenceVariants("ring"));
            profiles["amulet"] = Combine("amulet", ArmourShared, InfluenceVariants("amulet"));
            profiles["belt"] = Combine("belt", ArmourShared, InfluenceVariants("belt"));
            profiles["gloves"] = Combine("gloves", ArmourShared, InfluenceVariants("gloves"));
            profiles["boots"] = Combine("boots", ArmourShared, InfluenceVariants("boots"));
            profiles["helmet"] = Combine("helmet", ArmourShared, InfluenceVariants("helmet"));
            profiles["body_armour"] = Combine("body_armour", ArmourShared, InfluenceVariants("body_armour"));
            profiles["shield"] = Combine("shield", ArmourShared, InfluenceVariants("shield"));
            profiles["quiver"] = Combine("quiver", ArmourShared);

            profiles["str_armour"] = Combine("str_armour", "body_armour", "helmet", "gloves", "boots", ArmourShared);
            profiles["dex_armour"] = Combine("dex_armour", "body_armour", "helmet", "gloves", "boots", ArmourShared);
            profiles["int_armour"] = Combine("int_armour", "body_armour", "helmet", "gloves", "boots", ArmourShared);
            profiles["str_dex_armour"] = Combine("str_dex_armour", "body_armour", "helmet", "gloves", "boots", ArmourShared);
            profiles["str_int_armour"] = Combine("str_int_armour", "body_armour", "helmet", "gloves", "boots", ArmourShared);
            profiles["dex_int_armour"] = Combine("dex_int_armour", "body_armour", "helmet", "gloves", "boots", ArmourShared);
            profiles["str_dex_int_armour"] = Combine("str_dex_int_armour", "body_armour", "helmet", "gloves", "boots", ArmourShared);

            profiles["one_hand_weapon"] = OneHandWeaponShared;
            profiles["two_hand_weapon"] = TwoHandWeaponShared;
            profiles["onehand"] = OneHandWeaponShared;
            profiles["twohand"] = TwoHandWeaponShared;

            return profiles;
        }

        private static void AddOneHandWeapon(Dictionary<string, string[]> profiles, string specific) =>
            profiles[specific] = Combine(specific, OneHandWeaponShared, InfluenceVariants(specific));

        private static void AddTwoHandWeapon(Dictionary<string, string[]> profiles, string specific) =>
            profiles[specific] = Combine(specific, TwoHandWeaponShared, InfluenceVariants(specific));

        private static string[] InfluenceVariants(string baseTag)
        {
            var variants = new List<string>(InfluenceSuffixes.Length);
            foreach (string suffix in InfluenceSuffixes)
                variants.Add($"{baseTag}_{suffix}");

            return variants.ToArray();
        }

        private static string[] Combine(params object[] parts)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (object part in parts)
            {
                switch (part)
                {
                    case string tag:
                        set.Add(tag);
                        break;
                    case string[] tags:
                        foreach (string tag in tags)
                            set.Add(tag);
                        break;
                    case IEnumerable<string> tags:
                        foreach (string tag in tags)
                            set.Add(tag);
                        break;
                }
            }

            return set.OrderBy(t => t, StringComparer.OrdinalIgnoreCase).ToArray();
        }

        private static string[] CombineTags(params string[] tags) =>
            Combine((object[])tags);
    }
}
