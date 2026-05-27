# MapT17 gamble mode

**Source:** `dlls/Gamble/Modes/MapT17.cs`  
**UI settings:** `UIMapT17` (Item, Base = chaos orb)

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

### Stat format B: T17 `More` thresholds

**Content** example:

```text
Item Quantity:80;Item Rarity:60;Monster Pack Size:25;
```

Each segment is `Label:MinimumNumber;` matched against clipboard lines like:

```text
More Item Quantity: +80% (augmented)
```

Every segment in the rule must find a matching `More {label}:` line on the map with map value **≥** rule minimum. Console prints `80vs82` style comparisons per matched pair.

### Success

- All configured stat rules pass.
- Mod include/exclude logic passes (see Map doc).
- At least one `{ … }` modifier block present.

---

## Part 2 — Technical

### `CheckItem()` flow

1. Clipboard + hash guard (same as other modes).
2. **No** `Item Class: Maps` check (unlike Map).
3. Percent bucket: rules with `-1 < Priority < 1`:
   - **`q\d+r\d+ps\d+`**: identical parsing/comparison to `Map.cs`.
   - **`.*?:\d+(;|$)`**: `moreRuleRegex` parses rule Content; `moreMapRegex` = `more\s(?'type'.*?):\s\+(?'number'\d+)%\s\(augmented\)` on clipboard. For each rule segment, find map line with same `type` (case-insensitive); success only if `moreRuleMatches.Count == matchCount` where `matchCount` increments when `ruleMin < mapValue` (strictly less — map must meet or exceed rule threshold via that comparison branch).
4. Modifier parse + include/exclude on **`mod.Name`** only (content match is commented out in source).
5. `include.Count == includeCount`; `mods.Count == 0` → fail.

### Differences from Map

| Feature | Map | MapT17 |
|---------|-----|--------|
| Reroll | Scour + alch | Chaos |
| Item class check | Yes | No |
| `More … (augmented)` stats | No | Yes |
| Third coordinate | Scour | — |
