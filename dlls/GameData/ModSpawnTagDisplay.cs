using System.Globalization;

namespace PoE.dlls.GameData
{
    internal static class ModSpawnTagDisplay
    {
        private static readonly Dictionary<string, string> DisplayNames = new(StringComparer.OrdinalIgnoreCase)
        {
            ["flask"] = "Flask",
            ["life_flask"] = "Life Flask",
            ["mana_flask"] = "Mana Flask",
            ["utility_flask"] = "Utility Flask",
            ["hybrid_flask"] = "Hybrid Flask",
            ["critical_utility_flask"] = "Critical Utility Flask",
            ["tincture"] = "Tincture",
            ["jewel"] = "Jewel",
            ["abyss_jewel"] = "Abyss Jewel",
            ["abyss_jewel_melee"] = "Murderous Eye Jewel",
            ["abyss_jewel_ranged"] = "Searching Eye Jewel",
            ["abyss_jewel_caster"] = "Hypnotic Eye Jewel",
            ["abyss_jewel_summoner"] = "Ghastly Eye Jewel",
            ["searing_eye_jewel"] = "Searing Eye Jewel",
            ["cluster"] = "Cluster Jewel",
            ["cluster_jewel"] = "Cluster Jewel",
            ["ring"] = "Ring",
            ["amulet"] = "Amulet",
            ["belt"] = "Belt",
            ["gloves"] = "Gloves",
            ["boots"] = "Boots",
            ["helmet"] = "Helmet",
            ["body_armour"] = "Body Armour",
            ["shield"] = "Shield",
            ["quiver"] = "Quiver",
            ["bow"] = "Bow",
            ["claw"] = "Claw",
            ["dagger"] = "Dagger",
            ["rune_dagger"] = "Rune Dagger",
            ["sword"] = "Sword",
            ["axe"] = "Axe",
            ["mace"] = "Mace",
            ["wand"] = "Wand",
            ["staff"] = "Staff",
            ["warstaff"] = "Warstaff",
            ["weapon"] = "Weapon",
            ["one_hand_weapon"] = "One Hand Weapon",
            ["two_hand_weapon"] = "Two Hand Weapon",
            ["onehand"] = "One Hand Weapon",
            ["twohand"] = "Two Hand Weapon",
            ["str_armour"] = "Armour (Strength)",
            ["dex_armour"] = "Armour (Dexterity)",
            ["int_armour"] = "Armour (Intelligence)",
            ["str_dex_armour"] = "Armour (Str/Dex)",
            ["str_int_armour"] = "Armour (Str/Int)",
            ["dex_int_armour"] = "Armour (Dex/Int)",
            ["str_dex_int_armour"] = "Armour (Str/Dex/Int)",
            ["expansion_jewel_small"] = "Small Cluster Jewel",
            ["expansion_jewel_medium"] = "Medium Cluster Jewel",
            ["expansion_jewel_large"] = "Large Cluster Jewel",
            ["affliction_minion_damage"] = "Minion Damage",
            ["affliction_minion_life"] = "Minion Life",
            ["affliction_fire_damage"] = "Fire Damage",
            ["affliction_cold_damage"] = "Cold Damage",
            ["affliction_lightning_damage"] = "Lightning Damage",
            ["affliction_chaos_damage"] = "Chaos Damage",
            ["affliction_physical_damage"] = "Physical Damage",
            ["affliction_elemental_damage"] = "Elemental Damage",
            ["affliction_bow_damage"] = "Bow Damage",
            ["affliction_critical_chance"] = "Critical Chance",
        };

        private static readonly Dictionary<string, string> DisplayToCanonical = BuildDisplayToCanonical();

        private static Dictionary<string, string> BuildDisplayToCanonical()
        {
            var reverse = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string> pair in DisplayNames)
                reverse.TryAdd(pair.Value, pair.Key);

            return reverse;
        }

        public static string GetDisplayName(string tagOrDisplay)
        {
            if (string.IsNullOrWhiteSpace(tagOrDisplay))
                return string.Empty;

            string trimmed = tagOrDisplay.Trim();
            if (TryGetCanonicalTag(trimmed, out string? canonical))
                trimmed = canonical;

            if (DisplayNames.TryGetValue(trimmed, out string? display))
                return display;

            return TitleCaseTag(trimmed);
        }

        public static string FormatListItem(string tag) => GetDisplayName(tag);

        public static bool TryGetCanonicalTag(string input, out string canonical)
        {
            canonical = string.Empty;
            if (string.IsNullOrWhiteSpace(input))
                return false;

            string trimmed = input.Trim();
            if (DisplayNames.ContainsKey(trimmed))
            {
                canonical = DisplayNames.Keys.First(k => string.Equals(k, trimmed, StringComparison.OrdinalIgnoreCase));
                return true;
            }

            if (DisplayToCanonical.TryGetValue(trimmed, out string? fromDisplay))
            {
                canonical = fromDisplay;
                return true;
            }

            string normalized = trimmed.Replace(' ', '_');
            if (DisplayNames.ContainsKey(normalized))
            {
                canonical = DisplayNames.Keys.First(k => string.Equals(k, normalized, StringComparison.OrdinalIgnoreCase));
                return true;
            }

            return false;
        }

        public static bool MatchesSearch(string tag, string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return true;

            string lower = term.Trim();
            if (tag.Contains(lower, StringComparison.OrdinalIgnoreCase))
                return true;

            return GetDisplayName(tag).Contains(lower, StringComparison.OrdinalIgnoreCase);
        }

        private static string TitleCaseTag(string tag) =>
            CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tag.Replace('_', ' '));
    }
}
