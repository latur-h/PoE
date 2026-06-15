# Map gamble mode

**Source:** `dlls/Gamble/Modes/Map.cs`  
**Shared rules:** `dlls/Gamble/Modes/MapRulesEvaluator.cs`  
**UI settings:** per-mode store (Item, Base = alchemy, Second = scouring)

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

### Stat format B: `More … (augmented)` matching

Use when the map **header** (above the first `--------`) contains lines like:

```text
More Maps: +35% (augmented)
More Currency: +47% (augmented)
```

#### Rule Content syntax

One stat row (priority between `-1` and `1`, usually `0`). Content is semicolon-separated segments:

```text
Label:minimum;
```

| Rule segment | Matches map line | Notes |
|--------------|------------------|-------|
| `Maps:35;` | `More Maps: +35% (augmented)` | Label = `Maps` |
| `Currency:40;` | `More Currency: +47% (augmented)` | Label = `Currency` |
| `Scarabs:30;` | `More Scarabs: +35% (augmented)` | Label = `Scarabs` |

**Important:** In the rule, use only the **short label** — the text **after** the word `More ` on the map. **Do not** write `More` in the rule.

| Rule | Result |
|------|--------|
| `Currency:40;` | Correct |
| `More Currency:40;` | **Wrong** — label becomes `More Currency`, map label is `Currency` |

#### Comparison

For each segment: **map value ≥ rule minimum** (greater than or equal).  
Example: `Currency:40` passes when the map shows `+47%`; fails at `+39%`.  
Debug log: `40vs47`.

**Every segment** in the Content string must find a matching `More {label}:` line and pass.  
You may use **multiple stat rows**; **all** rows must pass.

#### What is not matched by format B

| Clipboard text | Use instead |
|----------------|-------------|
| `Item Quantity: +87% (augmented)` (no `More`) | Format A: `q80r60ps25` |
| `44(40-49)% more Monster Life` in `{ Prefix … }` blocks | Mod exclude/include rules, not stat rows |
| Lines without `(augmented)` in the More pattern | Not read as More stats |

#### Example preset row

```text
Priority: 0
Content:  Maps:35;Currency:40;
```

Passes a map with `More Maps: +35%` and `More Currency: +47%`. Fails if the map has `More Scarabs` but no `More Currency` line when `Currency:40` is required.

You can combine format A and B in **separate rows**; both must pass.

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
4. **Percent rules**: all rules where `-1 < Priority < 1` and non-empty Content — **each row** is evaluated:
   - **`q\d+r\d+ps\d+`**: compact quantity / rarity / pack size minimums (same as before).
   - **`Label:NN;` segments**: More augmented lines (`more\s{type}:\s+NN% (augmented)` on clipboard).
5. Parse modifiers via `MapRulesEvaluator.ParseModifiers` (internal pipeline).
6. Include/exclude on **`mod.Name`** — same as before.
7. `return result.RulesPassed`.

### Important implementation details

- Matching uses **`mod.Name` only**, not mod content text or tier from UI.
- **Include counting** is nested `foreach (mod) foreach (includeRule)` — multiple mods matching the same include rule increment multiple times. With **zero** include rules, `includeCount` and `include.Count` are both 0 → mod check passes (exclude-only setups).
- Stat regexes are substring-based (`quantity:` matches inside `Item Quantity:`).
- Fractured mods are still parsed; only console logging skips lines whose content contains `fractured`.

### Related types

- `GambleType.Map` → `Gambler` passes `item`, `baseXY`, `secondXY`.
- Stat struct `dlls/Gamble/Modifiers/Map.cs` exists but thresholds are driven by rule Content string, not that struct.
