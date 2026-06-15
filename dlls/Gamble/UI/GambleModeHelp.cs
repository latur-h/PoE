using PoE.dlls.Gamble;

namespace PoE.dlls.Gamble.UI
{
    public sealed record GambleModeHelpSection(string Heading, string Body);

    public sealed record GambleModeHelpContent(string Title, IReadOnlyList<GambleModeHelpSection> Sections);

    public static class GambleModeHelp
    {
        public static GambleModeHelpContent For(GambleType type) => type switch
        {
            GambleType.Alt => Alt,
            GambleType.Alt_Aug => AltAug,
            GambleType.Chromatic => Chromatic,
            GambleType.Chaos => Chaos,
            GambleType.Essence => Essence,
            GambleType.Map => Map,
            GambleType.MapT17 => MapT17,
            GambleType.Harvest => Harvest,
            GambleType.Eldritch => Eldritch,
            _ => Alt,
        };

        private static GambleModeHelpContent Alt => new("Alt — Alteration rolling",
        [
            Section("Target",
                "Magic (blue) items rolled with Alteration orbs until your modifier rules match."),
            Section("In game",
                "1. Copy the item — if it already matches, stop.\r\n" +
                "2. Right-click Alteration on the item once.\r\n" +
                "3. Hold Shift and spam left-click Alteration on the item; copy after each click.\r\n" +
                "4. Release Shift when done or cancelled."),
            Section("Coordinates",
                "• Item — item in the grid\r\n" +
                "• Base — Alteration orb"),
            Section("Priority",
                "• 1 or higher — Required: every such rule must match at least one mod\r\n" +
                "• Between 0 and 1 (e.g. 0.5) — Optional: if you use any optional rules, at least one must match\r\n" +
                "• 0 is not optional"),
            Section("Type & Tier",
                "• Type — Prefix, Suffix, Implicit, or Any\r\n" +
                "• Tier — maximum tier allowed (lower number = better mod)"),
            Section("Content",
                "Regex matched against the mod description line (text under the { Prefix … } header), not the crafted name in quotes.\r\n\r\n" +
                "Example: Priority 1, Prefix, Tier 1, Content: adds \\d+ to \\d+ physical damage"),
            Section("Enchants",
                "Lines marked (enchant) count as Implicit mods and follow the same rules."),
            Section("Success",
                "All required rules match, and any optional rules you defined are satisfied."),
        ]);

        private static GambleModeHelpContent AltAug => new("Alt + Aug — Smart alteration rolling",
        [
            Section("Target",
                "Magic items rolled using Alteration or Augmentation automatically, depending on how many prefix/suffix mods are on the item."),
            Section("In game",
                "1. Right-click Alteration and move to the item; hold Shift.\r\n" +
                "2. Each loop: copy → evaluate → Alteration click (reroll), Augmentation click (add a mod), or stop on success."),
            Section("Coordinates",
                "• Item — item in the grid\r\n" +
                "• Base — Alteration orb\r\n" +
                "• Second — Augmentation orb"),
            Section("When to Alt vs Aug",
                "• Aug — rules not met and the item has exactly one prefix or suffix mod (add second mod)\r\n" +
                "• Alt — rules not met and the item has two or more prefix/suffix mods (reroll)\r\n" +
                "• Success — required rules met and optional rules satisfied (if any)"),
            Section("Priority",
                "Same as Alt: 1+ required, fractional optional (not 0)."),
            Section("Type & Tier",
                "Same filters as Alt."),
            Section("Content",
                "Regex tested against the mod description or the mod name — either can satisfy the rule."),
            Section("Success",
                "Required rules satisfied; optional rules satisfied when present."),
        ]);

        private static GambleModeHelpContent Chaos => new("Chaos — Rare rerolling",
        [
            Section("Target",
                "Rare items rerolled with Chaos orbs until modifier rules match."),
            Section("In game",
                "1. Copy the item — if it matches, stop.\r\n" +
                "2. Right-click Chaos on the item, hold Shift, spam Chaos clicks with copy between clicks."),
            Section("Coordinates",
                "• Item — rare item\r\n" +
                "• Base — Chaos orb"),
            Section("Priority",
                "• 1 or higher — Required\r\n" +
                "• Between 0 and 1 — Optional (if any optional rows exist, at least one must match)\r\n" +
                "• 0 is not optional"),
            Section("Type & Tier",
                "• Type — Prefix, Suffix, Implicit, or Any\r\n" +
                "• Tier — maximum tier allowed"),
            Section("Content",
                "Regex on the mod description line (e.g. to maximum life, fire resistance)."),
            Section("Success",
                "All required rules match; optional rules satisfied when configured."),
        ]);

        private static GambleModeHelpContent Essence => new("Essence — Essence crafting",
        [
            Section("Target",
                "Items crafted or rerolled with Essences until modifier rules match."),
            Section("In game",
                "1. Copy the item — if it matches, stop.\r\n" +
                "2. Right-click Essence on the item, hold Shift, click with copy between clicks."),
            Section("Coordinates",
                "• Item — item\r\n" +
                "• Base — Essence"),
            Section("Priority",
                "• 1 or higher — Required\r\n" +
                "• Between 0 and 1 — Optional\r\n" +
                "• 0 is not optional"),
            Section("Type & Tier",
                "Type and Tier filters apply (same as Chaos)."),
            Section("Content",
                "Regex on mod description (e.g. to strength on a suffix row)."),
            Section("Success",
                "All required rules match; optional rules satisfied when configured."),
        ]);

        private static GambleModeHelpContent Chromatic => new("Chromatic — Socket colours",
        [
            Section("Target",
                "Item socket colours — does not read item modifiers."),
            Section("In game",
                "1. Copy the item and compare socket colours.\r\n" +
                "2. If they do not match: right-click Chromatic, hold Shift, click the item until colours match."),
            Section("Coordinates",
                "• Item — item with sockets\r\n" +
                "• Base — Chromatic orb"),
            Section("Rules",
                "Only the first row with non-empty Content is used. Priority, Type, and Tier are ignored."),
            Section("Content",
                "Uppercase colour pattern:\r\n" +
                "• R — Red\r\n" +
                "• G — Green\r\n" +
                "• B — Blue\r\n" +
                "• W — White\r\n\r\n" +
                "Example: RRGG means exactly 2 red and 2 green sockets. Pattern length is total socket count; each letter count must match the item."),
            Section("Success",
                "R, G, B, W counts on the item equal counts in your pattern."),
        ]);

        private static GambleModeHelpContent Map => new("Map — Map rolling (exclude-first)",
        [
            Section("Target",
                "Rare maps rolled to reject bad mods and optionally hit stat thresholds. Designed around exclude lists, not hunting many specific mods."),
            Section("In game",
                "1. Copy the map and evaluate rules.\r\n" +
                "2. Right-click Alchemy on the map once (make it rare).\r\n" +
                "3. Loop with Shift held: Alt+click Scouring (remove mods) → click Alchemy (reroll) → copy → evaluate."),
            Section("Coordinates",
                "• Item — map\r\n" +
                "• Base — Alchemy orb\r\n" +
                "• Second — Scouring orb"),
            Section("Priority",
                "• 0 (or any value between -1 and 1) — Map stat threshold in Content (see below)\r\n" +
                "• 1 or higher — Include: mod name must match\r\n" +
                "-1 or lower — Exclude: if any mod name matches, roll is rejected\r\n\r\n" +
                "Type and Tier columns are ignored for map mod rules."),
            Section("Stat row (Content)",
                "Format: q80r60ps25 — minimum Item Quantity %, Item Rarity %, and Monster Pack Size %.\r\n" +
                "Map values must be equal or higher. Missing stat lines count as 0."),
            Section("Exclude (main workflow)",
                "Content is regex on mod names (e.g. Splitting, of Toughness).\r\n" +
                "One row can block many mods: reflect|cannot regenerate|twinned\r\n" +
                "Do not list mods you want in exclude — a hit fails the roll."),
            Section("Include (optional)",
                "Regex on mod name when you need a specific prefix/suffix present. With no include rows, passing exclude + stats is enough."),
            Section("Success",
                "Stats pass, no exclude hit, and include logic passes (if used)."),
        ]);

        private static GambleModeHelpContent MapT17 => new("Map T17 — Elevated map rolling",
        [
            Section("Target",
                "Tier 17 / elevated maps rerolled with Chaos until mod and stat rules match."),
            Section("In game",
                "1. Copy the map and evaluate.\r\n" +
                "2. If not satisfied: right-click Chaos, hold Shift, spam Chaos on the map with copy between clicks.\r\n" +
                "No Scouring/Alchemy cycle (unlike standard Map mode)."),
            Section("Coordinates",
                "• Item — map\r\n" +
                "• Base — Chaos orb"),
            Section("Mod rules",
                "Same as Map mode: exclude on mod names (priority ≤ -1), optional include (priority ≥ 1). Type and Tier are ignored."),
            Section("Stat format A",
                "q80r60ps25 — same minimum quantity / rarity / pack size as Map mode."),
            Section("Stat format B (T17 More lines)",
                "Content example:\r\n" +
                "Item Quantity:80;Item Rarity:60;Monster Pack Size:25;\r\n\r\n" +
                "Each segment is matched against More … (augmented) lines on the map. Map value must meet or exceed each minimum."),
            Section("Success",
                "All stat rules pass, mod include/exclude logic passes, and the map has modifier blocks present."),
        ]);

        private static GambleModeHelpContent Harvest => new("Harvest — Harvest crafting",
        [
            Section("Target",
                "Items reforged via the Harvest craft UI until modifier rules match."),
            Section("In game",
                "1. Copy the item.\r\n" +
                "2. If not valid: click the Harvest craft button, then the item, then copy again.\r\n" +
                "3. Repeat until match or cancel. No Shift+orb spam."),
            Section("Coordinates",
                "• Item — item in the Harvest craft window\r\n" +
                "• Base — Reforge / craft button"),
            Section("Priority",
                "• 1 or higher — Required\r\n" +
                "• Between 0 and 1 — Optional"),
            Section("Type & Tier",
                "Both apply — same as Chaos/Essence."),
            Section("Content",
                "Regex on mod description (typical reforge outcome lines)."),
            Section("Success",
                "All required rules match; optional rules satisfied when configured."),
        ]);

        private static GambleModeHelpContent Eldritch => new("Eldritch — Implicit rerolling",
        [
            Section("Target",
                "Eldritch implicit lines on maps or other items, rerolled with Eldritch currency."),
            Section("In game",
                "Same as Chaos: right-click orb, hold Shift, spam on item with copy between clicks."),
            Section("Coordinates",
                "• Item — item with Eldritch implicits\r\n" +
                "• Base — Eldritch orb position"),
            Section("Priority",
                "• 1 or higher — Required\r\n" +
                "• Between 0 and 1 — Optional"),
            Section("Rules",
                "Only Implicit mods are checked. Prefix/suffix rows never match.\r\n" +
                "Tier is not used. Content regex runs on the implicit line text."),
            Section("Content",
                "Examples: of the Conqueror, Eater of Worlds, damage penetration lines."),
            Section("Success",
                "All required rules match; optional rules satisfied when configured."),
        ]);

        private static GambleModeHelpSection Section(string heading, string body) => new(heading, body);
    }
}
