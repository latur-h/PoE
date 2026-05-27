# Map gamble mode

**Source:** `dlls/Gamble/Modes/Map.cs`  
**UI settings:** `UIMap` (Item, Base = alchemy, Second = scouring)

Map rolling is optimized for **rejecting bad prefixes/suffixes** via exclude regex, not for hunting many specific mods. Stat thresholds (`q…r…ps…`) are optional. **Include** rules are rare (e.g. forcing a specific mod like magic pack size for god-roll maps) because each include row increases matching work and tightens success logic.

---

## Part 1 — Rule set behaviour

### What the gambler does in game

1. Copy map → evaluate rules.
2. Right-click **alchemy** on map once (rare the map).
3. Loop until success or cancel:
   - **Alt+left-click** scouring on map (remove mods).
   - **Left-click** alchemy on map (reroll rare).
   - Copy → evaluate again.
4. **Shift** is held during the loop.

### Coordinates

| Slot | Orb / target |
|------|----------------|
| Item | Map in stash or craft grid |
| Base | Alchemy orb |
| Second | Scouring orb |

### Priority meanings (Map-specific)

| Priority | Role |
|----------|------|
| `0` (any value with `-1 < p < 1`, typically `0`) | Map stat threshold encoded as `q80r60ps25` |
| `≥ 1` | **Include** — mod **name** must match (regex); see counting below |
| `≤ -1` | **Exclude** — if **any** mod **name** matches, roll is rejected |

There is **no** optional/fractional mod tier on Map. UI **Type** and **Tier** columns are **ignored** for mod rules.

### Stat row: `q{quantity}r{rarity}ps{packsize}`

Example: `q80r60ps25`

| Letter | Meaning | Clipboard source |
|--------|---------|------------------|
| `q` | Minimum item quantity % | `Item Quantity: +NN%` |
| `r` | Minimum item rarity % | `Item Rarity: +NN%` |
| `ps` | Minimum monster pack size % | `Monster Pack Size: +NN%` |

All three are **minimums** (map value must be ≥ target). Console logs: `Q61vs80;R35vs60;PS23vs25`.

Works with `(augmented)` suffix on stats. If a line is missing, that stat is treated as **0**.

### Exclude (primary workflow)

**Content** is a regex tested against each modifier **name** (e.g. `Splitting`, `of Toughness`).

One row can block many mods:

```text
Priority: -1
Content:  reflect|cannot regenerate|twinned|beyond|abomination|no regen
```

If **any** prefix/suffix name on the map matches → keep rolling.

**Do not** list mods you want in exclude — matching a desired name fails the roll.

### Include (god-roll / rare)

Same regex on **name**, but success requires `includeCount == number of include rows`.

| Setup | Effect |
|-------|--------|
| **No include rows** | Pass mod check if exclude passes and stats pass |
| **One include** `magic\|pack` | Each map mod that matches increments counter; need **exactly as many hits as include rows** (see technical) |
| **Several include rows** | Each row is a separate pattern; counting is per mod×rule hit |

Typical god-roll: one include row for a specific prefix name you must have, plus a large exclude blacklist for everything you refuse.

### Item validation

Clipboard must contain `Item Class: Maps` or `Item Class: Expedition Logbooks`. Otherwise gambling cancels.

### Success

- Stats (if configured) meet mins.
- No exclude regex hit on any mod name.
- Include logic satisfied (often zero include rows).
- At least one `{ … }` modifier block parsed on the item.

---

## Part 2 — Technical

### `CheckItem()` flow

1. Read clipboard; empty → cancel token, `false`.
2. Hash guard (3 identical copies → cancel).
3. `item\sclass:\s(?>maps|expedition logbooks)` on full text.
4. **Percent rules**: `rules` where `-1 < Priority < 1` and Content matches `q\d+r\d+ps\d+`:
   - Parse thresholds from rule Content.
   - Extract map values via `quantity:`, `rarity:`, `pack size:` substrings with `+NN%`.
   - Compare with `<` (fail if map stat lower).
5. Parse modifiers with shared regex pipeline (`getModifiers`, `getType`, `getName`, `getTier`, `getContent`, strip `(d-d)`).
6. If `mods.Count == 0` → `false`.
7. For each parsed mod:
   - **Include** (`Priority >= 1`): `Regex.IsMatch(mod.Name, rule.Content)` → `includeCount++` per hit.
   - **Exclude** (`Priority <= -1`): any match → immediate `false`.
8. `return include.Count == includeCount`.

### Important implementation details

- Matching uses **`mod.Name` only**, not mod content text or tier from UI.
- **Include counting** is nested `foreach (mod) foreach (includeRule)` — multiple mods matching the same include rule increment multiple times. With **zero** include rules, `includeCount` and `include.Count` are both 0 → mod check passes (exclude-only setups).
- Stat regexes are substring-based (`quantity:` matches inside `Item Quantity:`).
- Fractured mods are still parsed; only console logging skips lines whose content contains `fractured`.

### Related types

- `GambleType.Map` → `Gambler` passes `item`, `baseXY`, `secondXY`.
- Stat struct `dlls/Gamble/Modifiers/Map.cs` exists but thresholds are driven by rule Content string, not that struct.
