using PoE.dlls.Gamble;

namespace PoE.dlls.Gamble.UI
{
    public sealed record GambleModeHelpSection(string Heading, string Body);

    public sealed record GambleModeHelpContent(string Title, IReadOnlyList<GambleModeHelpSection> Sections);

    public static class GambleModeHelp
    {
        private const string ItemModNameOrContentMatch =
            "Matched against the mod description line or the crafted mod name in quotes — either can satisfy the rule.";

        private static GambleModeHelpSection ItemModContentSection(string? extraDetail = null) => Section("Content",
            (extraDetail is null ? ItemModNameOrContentMatch : extraDetail + " " + ItemModNameOrContentMatch) + "\r\n\r\n" +
            "Autocomplete: type in Content to search the mod list; picking a suggestion inserts a # template — add operators or exact numbers as needed.\r\n\r\n" +
            "Numbers and comparison:\r\n" +
            "• # — any value at that position\r\n" +
            "• 65 — exact match only (same as =65)\r\n" +
            "• >=60, <=-9, >5, <10 — minimum or maximum thresholds (useful for hitting a roll cap)\r\n" +
            "• Mix wildcards and thresholds: Adds # to >=15 Physical Damage\r\n\r\n" +
            "When your rule matches a known mod template in the cache (numbers and operators normalized to #), structured comparison is used. Otherwise rules with # fall back to regex (# matches digits).\r\n\r\n" +
            "Examples:\r\n" +
            "  #% increased Physical Damage\r\n" +
            "  65% increased Physical Damage\r\n" +
            "  >=60% increased Physical Damage\r\n" +
            "  Adds >=5 to >=15 Physical Damage\r\n" +
            "  <= -9% to all maximum Resistances");

        private static GambleModeHelpSection MoreStatRulesSection() => Section("More stat matching (format B)",
            "Use when the map header has lines like:\r\n" +
            "More Currency: +47% (augmented)\r\n\r\n" +
            "Rule Content — semicolon segments, short label + minimum only:\r\n" +
            "Label:minimum;\r\n\r\n" +
            "• Write the label after the word More on the map — do not prefix More in the rule.\r\n" +
            "  Currency:40;  →  checks More Currency: +NN% (augmented)\r\n" +
            "  Maps:35;      →  checks More Maps: +NN% (augmented)\r\n" +
            "  Scarabs:30;   →  checks More Scarabs: +NN% (augmented)\r\n\r\n" +
            "• Comparison: map % must be greater than or equal to your minimum (47 passes for Currency:40).\r\n" +
            "• Every segment in the row must pass. Multiple Stat rows must all pass.\r\n\r\n" +
            "Does not match:\r\n" +
            "• More Currency:40 in the rule (wrong — use Currency:40)\r\n" +
            "• Plain Item Quantity: +87% without More (use format A: q80r60ps25)\r\n" +
            "• Mod lines such as 44% more Monster Life in prefix/suffix blocks");

        private static GambleModeHelpSection OrbsTabCoordinatesSection(string modeUses) => Section("Coordinates",
            "Orbs tab — shared stash positions for all gamble modes.\r\n\r\n" +
            "Items: Default item (most modes), Harvest item, Essence item.\r\n" +
            "Orbs: Alt, Aug, Chaos, Chromatic, Essence, Alchemy, Scouring, Exalt, Vaal, Searing Exarch, Eater of Worlds, Craft.\r\n\r\n" +
            "This mode uses: " + modeUses);

        public static GambleModeHelpContent For(GambleType type) => type switch
        {
            GambleType.Alt => Alt,
            GambleType.Alt_Aug => AltAug,
            GambleType.Chromatic => Chromatic,
            GambleType.Chaos => Chaos,
            GambleType.Essence => Essence,
            GambleType.Map => Map,
            GambleType.MapT17 => MapT17,
            GambleType.MapExalt => MapExalt,
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
            OrbsTabCoordinatesSection("Default item, Alt orb"),
            Section("Role",
                "• Required — every such rule must match at least one mod\r\n" +
                "• Optional — if you use any optional rules, at least one must match\r\n" +
                "• Exclude — if any mod matches, roll is rejected (checked first)\r\n" +
                "• None — row inactive"),
            Section("Type & Tier",
                "• Type — Prefix, Suffix, Implicit, or Any\r\n" +
                "• Tier — maximum tier allowed (lower number = better mod)"),
            ItemModContentSection(),
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
            OrbsTabCoordinatesSection("Default item, Alt orb, Aug orb"),
            Section("When to Alt vs Aug",
                "• Aug — rules not met and the item has exactly one prefix or suffix mod (add second mod)\r\n" +
                "• Alt — rules not met and the item has two or more prefix/suffix mods (reroll)\r\n" +
                "• Success — required rules met and optional rules satisfied (if any)"),
            Section("Role",
                "Same as Alt: Required, Optional, Exclude (checked first), or None (inactive)."),
            Section("Type & Tier",
                "Same filters as Alt."),
            ItemModContentSection(),
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
            OrbsTabCoordinatesSection("Default item, Chaos orb"),
            Section("Role",
                "• Required — must match\r\n" +
                "• Optional — at least one optional must match when any optional rows exist\r\n" +
                "• Exclude — reject if any mod matches (checked first)\r\n" +
                "• None — row inactive"),
            Section("Type & Tier",
                "• Type — Prefix, Suffix, Implicit, or Any\r\n" +
                "• Tier — maximum tier allowed"),
            ItemModContentSection(),
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
            OrbsTabCoordinatesSection("Essence item, Essence orb"),
            Section("Role",
                "• Required — must match\r\n" +
                "• Optional — at least one optional must match when any optional rows exist\r\n" +
                "• Exclude — reject if any mod matches (checked first)\r\n" +
                "• None — row inactive"),
            Section("Type & Tier",
                "Type and Tier filters apply (same as Chaos)."),
            ItemModContentSection(),
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
            OrbsTabCoordinatesSection("Default item, Chromatic orb"),
            Section("Rules",
                "Only the first row with non-empty Content is used. Role, Type, and Tier are ignored."),
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
            OrbsTabCoordinatesSection("Default item, Alchemy orb, Scouring orb, Vaal orb (optional corrupt)"),
            Section("Role",
                "• Stat — map threshold in Content (see below)\r\n" +
                "• Include — mod name must match\r\n" +
                "• Exclude — if any mod name matches, roll is rejected\r\n" +
                "• None — row inactive\r\n\r\n" +
                "Type and Tier columns are ignored for map mod rules."),
            Section("Stat format A (compact)",
                "q80r60ps25 — minimum Item Quantity %, Item Rarity %, and Monster Pack Size % from lines like Item Quantity: +87% (augmented).\r\n" +
                "Missing lines count as 0. Separate stat rows allowed; all must pass."),
            MoreStatRulesSection(),
            Section("Exclude (main workflow)",
                "Content is regex on mod names (e.g. Splitting, of Toughness).\r\n" +
                "One row can block many mods: reflect|cannot regenerate|twinned\r\n" +
                "Do not list mods you want in exclude — a hit fails the roll.\r\n\r\n" +
                "Map mod rows use name regex only — comparison operators (>=, <=) and # templates are for item gamble modes, not Map/Map Exalt/Map T17."),
            Section("Include (optional)",
                "Regex on mod name when you need a specific prefix/suffix present. With no include rows, passing exclude + stats is enough."),
            Section("Success",
                "Stats pass, no exclude hit, and include logic passes (if used)."),
            GambleBulkHelp.DetailedBulkInventory(),
            GambleBulkHelp.DetailedGridArea(),
            GambleBulkHelp.DetailedFirstCell(),
            GambleBulkHelp.DetailedNextStep(),
            GambleBulkHelp.DetailedTiming(),
            GambleBulkHelp.DetailedCorruptOnSuccess(),
            GambleBulkHelp.DetailedCorruptedMaps(),
            GambleBulkHelp.DetailedBrokenMapHighlight(),
        ]);

        private static GambleModeHelpContent MapT17 => new("Map T17 — Elevated map rolling",
        [
            Section("Target",
                "Tier 17 / elevated maps rerolled with Chaos until mod and stat rules match."),
            Section("In game",
                "1. Copy the map and evaluate.\r\n" +
                "2. If not satisfied: right-click Chaos, hold Shift, spam Chaos on the map with copy between clicks.\r\n" +
                "No Scouring/Alchemy cycle (unlike standard Map mode)."),
            OrbsTabCoordinatesSection("Default item, Chaos orb, Vaal orb (optional corrupt)"),
            Section("Mod rules",
                "Same as Map mode: Exclude on mod names, optional Include, Stat rows for thresholds. Type and Tier are ignored."),
            Section("Stat format A (compact)",
                "q80r60ps25 — same quantity / rarity / pack size minimums as Map mode."),
            MoreStatRulesSection(),
            Section("Success",
                "All stat rules pass, mod include/exclude logic passes, and the map has modifier blocks present."),
            GambleBulkHelp.DetailedBulkInventory(),
            GambleBulkHelp.DetailedGridArea(),
            GambleBulkHelp.DetailedFirstCell(),
            GambleBulkHelp.DetailedNextStep(),
            GambleBulkHelp.DetailedTiming(),
            GambleBulkHelp.DetailedCorruptOnSuccess(),
            GambleBulkHelp.DetailedCorruptedMaps(),
            GambleBulkHelp.DetailedBrokenMapHighlight(),
        ]);

        private static GambleModeHelpContent MapExalt => new("Map Exalt — Six-mod map rolling",
        [
            Section("Target",
                "Rare maps rolled to six mods using Exalts, with the same exclude/include/stat rules as Map mode."),
            Section("In game",
                "1. Copy the map and evaluate.\r\n" +
                "2. If not rare: Scouring + Alchemy (same as Map mode).\r\n" +
                "3. If rare and fewer than 6 mods: Exalt slam on the map, then copy and re-check.\r\n" +
                "4. If an exclude rule matches or the map has 6 mods but rules fail: Scouring + Alchemy and restart.\r\n" +
                "5. Stop when the map is rare, has 6 mods, and all rules pass."),
            OrbsTabCoordinatesSection("Default item, Alchemy orb, Scouring orb, Exalt orb, Vaal orb (optional corrupt)"),
            Section("Rules",
                "Same as Map mode: Stat rows, Exclude, optional Include. Type and Tier are ignored for map mod rows."),
            Section("Stat format A (compact)",
                "q80r60ps25 — minimum Item Quantity / Rarity / Pack Size from normal map stat lines."),
            MoreStatRulesSection(),
            Section("Success",
                "Map is rare, has exactly 6 mods, stats pass, no exclude hit, and include logic passes (if used)."),
            GambleBulkHelp.DetailedBulkInventory(),
            GambleBulkHelp.DetailedGridArea(),
            GambleBulkHelp.DetailedFirstCell(),
            GambleBulkHelp.DetailedNextStep(),
            GambleBulkHelp.DetailedTiming(),
            GambleBulkHelp.DetailedCorruptOnSuccess(),
            GambleBulkHelp.DetailedCorruptedMaps(),
            GambleBulkHelp.DetailedBrokenMapHighlight(),
        ]);

        private static GambleModeHelpContent Harvest => new("Harvest — Harvest crafting",
        [
            Section("Target",
                "Items reforged via the Harvest craft UI until modifier rules match."),
            Section("In game",
                "1. Copy the item.\r\n" +
                "2. If not valid: click the Harvest craft button, then the item, then copy again.\r\n" +
                "3. Repeat until match or cancel. No Shift+orb spam."),
            OrbsTabCoordinatesSection("Harvest item, Craft button"),
            Section("Role",
                "• Required — must match\r\n" +
                "• Optional — at least one optional must match when any optional rows exist\r\n" +
                "• Exclude — reject if any mod matches (checked first)\r\n" +
                "• None — row inactive"),
            Section("Type & Tier",
                "Both apply — same as Chaos/Essence."),
            ItemModContentSection("Typical reforge outcome lines."),
            Section("Success",
                "All required rules match; optional rules satisfied when configured."),
        ]);

        private static GambleModeHelpContent Eldritch => new("Eldritch — Implicit rerolling",
        [
            Section("Target",
                "Eldritch implicit lines on maps or other items — one Searing Exarch line and one Eater of Worlds line."),
            Section("In game",
                "1. Move to the item → copy → evaluate. Stop if rules already match.\r\n" +
                "2. Pick up the Searing Exarch orb, move back to the item, hold Shift.\r\n" +
                "3. Loop: copy → evaluate → slam (switch to Eater orb when Exarch line is satisfied).\r\n" +
                "4. Release Shift when done or cancelled."),
            OrbsTabCoordinatesSection("Default item, Searing Exarch orb, Eater of Worlds orb"),
            Section("Role",
                "• Required on the matching implicit line\r\n" +
                "• Optional\r\n" +
                "• Exclude — if any mod on the item matches, roll is rejected (checked first)\r\n" +
                "• None — row inactive"),
            Section("Rules",
                "Each rule targets one influence (Searing Exarch or Eater of Worlds). Required/optional apply only to the matching implicit line.\r\n" +
                "Reject rules apply to all mods on the item. Tier and Type columns are hidden for implicit matching."),
            ItemModContentSection("Implicit lines for the row's influence."),
            Section("Success",
                "Both influence groups pass their required rules; optional rules satisfied when configured."),
        ]);

        private static GambleModeHelpSection Section(string heading, string body) => new(heading, body);
    }
}
