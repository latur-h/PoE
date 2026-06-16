# MapT17 gamble mode

**Source:** `dlls/Gamble/Modes/MapT17.cs`  
**Shared rules:** `dlls/Gamble/Modes/MapRulesEvaluator.cs`  
**UI settings:** per-mode store (Item, Base = chaos orb)

Tier 17 / elevated map crafting using **chaos** rerolls. Mod rules mirror **Map** (name-based include/exclude). Adds a second stat rule format for `More … (augmented)` lines on the clipboard.

---

## Part 1 — Rule set behaviour

### What the gambler does in game

1. Copy map → evaluate.
2. If not satisfied:
   - Right-click **chaos** on map.
   - Hold **Shift**, spam left-click chaos on map, copy between clicks.
3. Release Shift when done.

No scouring/alchemy cycle (unlike standard Map mode).

### Coordinates

| Slot | Target |
|------|--------|
| Item | Map |
| Base | Chaos orb |

### Mod rules (same philosophy as Map)

| Priority | Role |
|----------|------|
| `-1 < p < 1` | Stat rules (`q…r…ps…` and/or T17 `More` rules) |
| `≥ 1` | Include — regex on mod **name** |
| `≤ -1` | Exclude — regex on mod **name**, any hit fails |

**Type** and **Tier** UI columns are ignored for mods.

Exclude blacklist + optional include for god rolls applies the same way as [Map](Map/README.md).

### Stat format A: `q80r60ps25`

Same as Map mode: minimum quantity, rarity, pack size from normal map stat lines (`Item Quantity`, etc.).

### Stat format B: `More … (augmented)` matching

Same rules as [Map](Map/README.md#stat-format-b-more--augmented-matching). Summary:

**Rule:** `Label:minimum;` segments — short label only (e.g. `Currency:40;`, `Maps:35;`), **not** `More Currency:40`.

**Map line:** `More {label}: +NN% (augmented)` in the header block.

**Compare:** map value **≥** minimum; every segment must pass; multiple stat rows all must pass.

**Not matched:** plain `Item Quantity:` lines (use `q80r60ps25`), or `% more Monster Life` in mod blocks.

### Success

- All configured stat rules pass.
- Mod include/exclude logic passes (see Map doc).
- At least one `{ … }` modifier block present.

---

## Part 2 — Technical

### `CheckItem()` flow

1. Clipboard + hash guard via `MapCheckHelper.TryEvaluateClipboard`.
2. **`Item Class: Maps`** check (via shared `MapRulesEvaluator`).
3. All stat rows with `-1 < Priority < 1` evaluated by `MapRulesEvaluator.CheckStats` (compact + More formats).
4. Modifier parse + include/exclude on **`mod.Name`** only.
5. `return result.RulesPassed`.

### Differences from Map

| Feature | Map | MapT17 |
|---------|-----|--------|
| Reroll | Scour + alch | Chaos |
| Item class check | Yes | Yes (shared evaluator) |
| `More … (augmented)` stats | Yes | Yes |
| Third coordinate | Scour | — |

---

## Part 3 — Bulk inventory grid

Same bulk engine as [Map](Map/README.md#part-3--bulk-inventory-grid) (`MapBulkGambler`). Differences for Map T17:

| Step | Behaviour |
|------|-----------|
| Precheck | Assign **Chaos**, **Vaal**, **StashBroken**, or **Done** per slot (no scour/alch) |
| Chaos batch | Chaos orb once + Shift; per slot: refresh+assign, skip unless Chaos action, one slam |
| Corrupted maps | Evaluate only — no orbs; fail rules → stash queue |

See Map doc for grid setup, Vaal corrupt, and deferred stash behaviour.
