namespace PoE.dlls.GameData
{
    internal static class ClusterJewelSizeTags
    {
        public static bool IsClusterSizeTag(string tag) =>
            tag.Equals("expansion_jewel_large", StringComparison.OrdinalIgnoreCase)
            || tag.Equals("expansion_jewel_medium", StringComparison.OrdinalIgnoreCase)
            || tag.Equals("expansion_jewel_small", StringComparison.OrdinalIgnoreCase);

        public static bool IsAfflictionClusterTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return false;

            if (!tag.StartsWith("affliction_", StringComparison.OrdinalIgnoreCase))
                return false;

            return !tag.StartsWith("old_do_not_use_", StringComparison.OrdinalIgnoreCase);
        }

        private static readonly HashSet<string> MediumAfflictionTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "affliction_armour",
            "affliction_evasion",
            "affliction_maximum_life",
            "affliction_maximum_mana",
            "affliction_maximum_energy_shield",
            "affliction_fire_resistance",
            "affliction_cold_resistance",
            "affliction_lightning_resistance",
            "affliction_chaos_resistance",
            "affliction_chance_to_block",
            "affliction_chance_to_dodge_attacks",
            "affliction_strength",
            "affliction_dexterity",
            "affliction_intelligence",
            "affliction_flask_duration",
            "affliction_life_and_mana_recovery_from_flasks",
            "affliction_minion_life",
            "affliction_effect_of_non-damaging_ailments",
        };

        public static IReadOnlyList<string> GetAfflictionTagsForSize(string sizeTag) =>
            sizeTag.ToLowerInvariant() switch
            {
                "expansion_jewel_small" => AllAfflictionTags
                    .Where(t => t.EndsWith("_small", StringComparison.OrdinalIgnoreCase))
                    .ToArray(),
                "expansion_jewel_medium" => MediumAfflictionTags.OrderBy(t => t, StringComparer.OrdinalIgnoreCase).ToArray(),
                "expansion_jewel_large" => AllAfflictionTags
                    .Where(t => !MediumAfflictionTags.Contains(t)
                                && !t.EndsWith("_small", StringComparison.OrdinalIgnoreCase))
                    .ToArray(),
                _ => [],
            };

        private static readonly string[] AllAfflictionTags =
        [
            "affliction_area_damage",
            "affliction_armour",
            "affliction_attack_damage_",
            "affliction_attack_damage_while_dual_wielding_",
            "affliction_attack_damage_while_holding_a_shield",
            "affliction_axe_and_sword_damage",
            "affliction_bow_damage",
            "affliction_brand_damage",
            "affliction_chance_to_block",
            "affliction_chance_to_dodge_attacks",
            "affliction_channelling_skill_damage",
            "affliction_chaos_damage",
            "affliction_chaos_damage_over_time_multiplier",
            "affliction_chaos_resistance",
            "affliction_cold_damage",
            "affliction_cold_damage_over_time_multiplier",
            "affliction_cold_resistance",
            "affliction_critical_chance",
            "affliction_curse_effect_small",
            "affliction_damage_over_time_multiplier",
            "affliction_damage_while_you_have_a_herald",
            "affliction_damage_with_two_handed_melee_weapons",
            "affliction_dagger_and_claw_damage",
            "affliction_dexterity",
            "affliction_effect_of_non-damaging_ailments",
            "affliction_elemental_damage",
            "affliction_evasion",
            "affliction_fire_damage",
            "affliction_fire_damage_over_time_multiplier",
            "affliction_fire_resistance",
            "affliction_flask_duration",
            "affliction_intelligence",
            "affliction_life_and_mana_recovery_from_flasks",
            "affliction_lightning_damage",
            "affliction_lightning_resistance",
            "affliction_mace_and_staff_damage",
            "affliction_maximum_energy_shield",
            "affliction_maximum_life",
            "affliction_maximum_mana",
            "affliction_minion_damage",
            "affliction_minion_damage_while_you_have_a_herald",
            "affliction_minion_life",
            "affliction_physical_damage",
            "affliction_physical_damage_over_time_multiplier",
            "affliction_projectile_damage",
            "affliction_reservation_efficiency_small",
            "affliction_strength",
            "affliction_totem_damage",
            "affliction_trap_and_mine_damage",
            "affliction_warcry_buff_effect",
        ];
    }
}
