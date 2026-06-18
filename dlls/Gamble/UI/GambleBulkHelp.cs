namespace PoE.dlls.Gamble.UI
{
    internal static class GambleBulkHelp
    {
        public static class Short
        {
            public const string BulkInventory =
                "Roll every map in the configured grid instead of a single item coordinate.";

            public const string CorruptOnSuccess =
                "After rules pass, Vaal corrupt each map. Bulk: broken maps are stashed after the orb is released; single: logs Broken map.";

            public const string CorruptRequireEightMods =
                "With corrupt enabled: after Vaal, re-check rules plus at least 8 affix mods. Maps below 8 affixes are stashed as broken.";

            public const string CorruptedMap =
                "Already-corrupted maps are evaluated only — no orb slams. Failed rules are queued for stash.";

            public const string GridArea =
                "Settings → Grid hotkey, then drag LMB from the first map slot corner to the last.";

            public const string FirstCell =
                "Center of the top-left map. Rec here, then F6 (Position hotkey).";

            public const string NextX =
                "Horizontal pixels between map centers (one column step).";

            public const string NextY =
                "Vertical pixels between map centers. Use 0 for a single row.";

            public const string NextCellRec =
                "Rec + F6 on the next map (right or below) to fill Next X / Next Y from the delta.";

            public const string RefreshDelay =
                "Map-only wait after slams and orb drops before copying tooltip text. Separate from Settings → Delay. " +
                "Without network lag, ~10 ms here with Delay ~20 ms feels fast; lower if copy stays reliable.";

            public const string FastEmptyColorCheck =
                "Before precheck, compare each cell to preregistered empty pixels and skip clipboard copy when they match.";

            public const string EmptySlotRegistration =
                "With an empty inventory, register once to save each grid cell's empty pixel. Re-register after changing the grid.";

            public const string EmptySlotRegister =
                "Sample every grid cell on the current screen and save as the empty reference for each slot.";
        }

        public static GambleModeHelpSection DetailedBulkInventory() => new(
            "Bulk inventory grid",
            "When enabled, gamble runs on every cell in the map grid instead of the Default item coordinate.\r\n\r\n" +
            "Applies to Map, Map Exalt, and Map T17 only.\r\n\r\n" +
            "Each cycle:\r\n" +
            "1. Copy and evaluate all non-empty slots (refresh assigns the next action per slot).\r\n" +
            "2. Batch Scour+Alchemy where needed (Alchemy orb picked up once: RMB + Shift, then Alt+LMB scour and LMB alch per slot).\r\n" +
            "3. Batch Exalt (Map Exalt) or Chaos (T17): move to each slot, refresh+assign, skip unless that action is still needed, one slam per slot per cycle.\r\n" +
            "4. Optional Vaal corrupt on maps that passed rules (Vaal orb picked up once per batch).\r\n" +
            "5. Stash broken maps: release Shift and all held keys/orbs, then Ctrl+LMB each queued map into stash.\r\n\r\n" +
            "Already-corrupted maps (clipboard ends with a Corrupted line) are never slammed — they are evaluated against rules only; pass = keep, fail = stash queue.\r\n\r\n" +
            "Empty slots (copy fails twice) are skipped for the rest of the session.");

        public static GambleModeHelpSection DetailedCorruptOnSuccess() => new(
            "Corrupt on success (Vaal)",
            "When rules pass, the map is corrupted before finishing.\r\n\r\n" +
            "• Vaal orb is picked up once per batch (RMB + Shift), then LMB on each map.\r\n" +
            "• Clipboard is refreshed and rules are evaluated again after each corrupt.\r\n" +
            "• Bulk: maps that break after Vaal are queued for stash — the stash step runs after the Vaal batch, releases the orb and Shift first, then Ctrl+LMB into stash.\r\n" +
            "• Single map: a broken result logs \"Broken map\" and stops.\r\n\r\n" +
            "Set the Vaal orb on the Orbs tab. If Vaal is not configured, corrupt is skipped.");

        public static GambleModeHelpSection DetailedCorruptedMaps() => new(
            "Already-corrupted maps",
            "If the copied map text ends with a Corrupted line (standard PoE clipboard format), no currency is used on that slot.\r\n\r\n" +
            "• Rules are evaluated normally (stats, exclude, include).\r\n" +
            "• Rules pass → slot is finished (map kept).\r\n" +
            "• Rules fail → queued for the stash batch at the end of the cycle (Ctrl+LMB after keys/orb are released).\r\n\r\n" +
            "Useful when the grid contains maps you already corrupted manually or from a previous session.");

        public static GambleModeHelpSection DetailedGridArea() => new(
            "Grid area",
            "Defines the rectangle that contains all map slots.\r\n\r\n" +
            "1. Assign the Grid hotkey in Settings → Gamble.\r\n" +
            "2. Press it, then drag LMB from the top-left corner of the first map slot to the bottom-right corner of the last map slot.\r\n" +
            "3. Direction of the drag does not matter.");

        public static GambleModeHelpSection DetailedFirstCell() => new(
            "First cell",
            "Screen position of the center of the top-left map in the grid.\r\n\r\n" +
            "Click Rec next to First, then press F6 (Position hotkey) while the cursor is on that map's center.\r\n" +
            "This is the origin for placing every other cell.");

        public static GambleModeHelpSection DetailedNextStep() => new(
            "Next X / Next Y",
            "Pixel distance between map centers when moving one column (Next X) or one row (Next Y).\r\n\r\n" +
            "• Type values manually, or use the second Rec + F6 on the next map to the right or below — deltas are filled automatically.\r\n" +
            "• Next X must be greater than 0.\r\n" +
            "• Next Y may be 0 when maps are in a single row.\r\n\r\n" +
            "Cell centers are: First + (column × Next X, row × Next Y), kept inside the grid area.");

        public static GambleModeHelpSection DetailedTiming() => new(
            "Delay vs Refresh ms",
            "Two different timings control map gambling speed:\r\n\r\n" +
            "Settings → Gamble → Delay\r\n" +
            "• Pause between every input step: click down/up, key down/up, and mouse move.\r\n" +
            "• Applies to all gamble modes.\r\n\r\n" +
            "Gamble tab → Bulk panel → Refresh ms\r\n" +
            "• Extra wait after currency slams, orb drops, and modifier release before copying the map.\r\n" +
            "• Lets the client refresh tooltip text. Used by Map, Map Exalt, and Map T17 — including single-map runs (not only bulk inventory).\r\n" +
            "• If Refresh ms is 0, the bot uses max(50, Delay) instead.\r\n\r\n" +
            "How they relate: Delay sets the base click/key rhythm; Refresh ms is added on top wherever the bot needs fresh clipboard text or the game to settle after currency. " +
            "Both stack — a low Delay with a high Refresh ms can still feel slow after each slam.\r\n\r\n" +
            "Tuning (no network / input lag):\r\n" +
            "• Start with Delay 20 ms and Refresh ms 10 ms — feels fast and works well for many setups.\r\n" +
            "• You can lower both further if copy and orb placement stay reliable.\r\n" +
            "• Raise Delay if clicks or orbs miss; raise Refresh ms if copied map text is empty or stale.");
    }
}
