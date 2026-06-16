# MapExalt gamble mode

**Source:** `dlls/Gamble/Modes/MapExalt.cs`  
**Shared rules:** `dlls/Gamble/Modes/MapRulesEvaluator.cs`  
**UI settings:** per-mode store (Item, Alchemy, Scouring, Exalt)

Map Exalt rolls a **rare map to six mods** using Exalted orbs, while applying the same **exclude / include / stat** rules as [Map](Map/README.md) and [MapT17](MapT17/README.md). Designed for finishing maps that already pass your blacklist but need more affixes.

---

## Part 1 — Rule set behaviour

### What the gambler does in game

1. Copy map → evaluate rules and mod count.
2. If **not Rare** → **Scouring + Alchemy** (same Alt+click / click cycle as Map mode on the item).
3. If **Rare** and **exclude** hits → Scouring + Alchemy, restart.
4. If **Rare**, fewer than **6 mods**, and no exclude → **Exalt** slam (right-click Exalt, Shift+click map), copy, re-check.
5. If **6 mods** and all rules pass → **success**.
6. If **6 mods** but stats/include fail → Scouring + Alchemy, restart.

**Shift** is held during Scouring/Alchemy and Exalt actions (after Alchemy is primed with right-click, same as Map).

### Coordinates

| Slot | Orb / target |
|------|----------------|
| Item | Map in stash or craft grid |
| Alchemy | Alchemy orb (right-click once to prime) |
| Scouring | Scouring orb slot (Alt+click on item uses scour) |
| Exalt | Exalted orb |

### Priority meanings (same as Map)

| Priority | Role |
|----------|------|
| `0` (any value with `-1 < p < 1`, typically `0`) | Map **stat** threshold — see stat formats below |
| `≥ 1` | **Include** — mod **name** must match (regex) |
| `≤ -1` | **Exclude** — if **any** mod **name** matches, roll is rejected |

**Type** and **Tier** UI columns are **ignored** for map mod rules.

You may use **multiple stat rows** (each with priority between -1 and 1). **Every** stat row must pass.

### Stat format A: `q{quantity}r{rarity}ps{packsize}`

Example: `q80r60ps25`

| Letter | Meaning | Clipboard source |
|--------|---------|------------------|
| `q` | Minimum item quantity % | `Item Quantity: +NN%` |
| `r` | Minimum item rarity % | `Item Rarity: +NN%` |
| `ps` | Minimum monster pack size % | `Monster Pack Size: +NN%` |

All three are **minimums** (map value must be ≥ target). Missing lines count as **0**.

### Stat format B: `More … (augmented)` matching

Same rules as [Map](Map/README.md#stat-format-b-more--augmented-matching). Summary:

**Rule:** `Label:minimum;` — e.g. `Maps:35;Currency:40;Scarabs:30;` (no `More` prefix in the rule).

**Map line:** `More Currency: +47% (augmented)` → rule label `Currency`, compare **≥** minimum.

**Every segment** in the row must pass. Mod text like `% more Monster Life` is **not** a More stat line.

See the Map doc for full tables, wrong vs right examples, and what format B does not match.

### Exclude (primary workflow)

Regex on mod **names** — identical to Map mode. Any hit while evaluating triggers a Scouring + Alchemy reroll.

### Include (optional)

Regex on mod **name**. With **zero** include rows, passing exclude + stats is enough. With include rows, each include pattern must match at least once (same counting as Map).

### Success

- Map is **Rare**.
- Exactly **6** parsed modifier blocks (`{ Prefix … }` / `{ Suffix … }` / `{ Implicit … }`).
- All stat rows pass (compact and/or More format).
- No exclude hit.
- Include logic satisfied (if configured).

---

## Part 2 — Technical

### State machine (`Gamble()`)

1. `Copy()` → `MapRulesEvaluator.Evaluate()`.
2. `!IsMap` → cancel.
3. `!IsRare` → `EnsureAlchemyPrimed()` + `ScouringAlchemyOnItem()`.
4. `ExcludeHit` → Scouring + Alchemy.
5. `ModCount >= 6 && RulesPassed` → success.
6. `ModCount >= 6 && !RulesPassed` → Scouring + Alchemy.
7. Else → `ExaltSlam()`.

### Shared evaluator

All stat and mod checks run through `MapRulesEvaluator` (also used by Map, MapT17, and MapExalt). Stat evaluation iterates **every** rule with `-1 < Priority < 1`:

- Content matching `q\d+r\d+ps\d+` → compact stat check.
- Content matching `Label:NN;` segments → More augmented check.

Mod matching uses **`mod.Name` only** for include/exclude. **`IsCorrupted`** is set when the clipboard ends with a `Corrupted` line.

### Related types

- `GambleType.MapExalt` → `Gambler` passes `item`, `base` (Alchemy), `second` (Scouring), `third` (Exalt).

---

## Part 3 — Bulk inventory grid

Same bulk engine as [Map](Map/README.md#part-3--bulk-inventory-grid) (`MapBulkGambler`). Differences for Map Exalt:

| Step | Behaviour |
|------|-----------|
| Precheck | Assign **ScourAlchemy**, **Exalt**, **Vaal**, **StashBroken**, or **Done** per slot |
| Exalt batch | One exalt per map per cycle; refresh+assign before each slam so maps at **6 affixes** are never exalted |
| 6 mods + rules fail | Scour + Alchemy (not exalt) |
| Corrupted maps | Evaluate only — no orbs; fail rules → stash queue |

See Map doc for grid setup, Vaal corrupt, and deferred stash behaviour.
