# Chaos gamble mode

**Source:** `dlls/Gamble/Modes/Chaos.cs`  
**UI settings:** `UIChaos` (Item, Base = chaos orb)

Rerolls **rare** item modifiers with chaos orbs until content-based rules match.

---

## Part 1 — Rule set behaviour

### What the gambler does in game

1. Copy → if valid, stop.
2. Right-click chaos on item, hold **Shift**, spam chaos clicks with copy between clicks.

### Coordinates

| Slot | Target |
|------|--------|
| Item | Rare item |
| Base | Chaos orb |

### Priority meanings

| Priority | Role |
|----------|------|
| `≥ 1` | **Required** — each rule must match at least one mod |
| `> 0` and `< 1` | **Optional** — if optional rows exist, at least one must match |

Priority `0` is **not** optional.

### Rule matching

- **Content**: regex on modifier **description text** (`mod.Content`), not the quoted prefix/suffix name.
- **Type**: Prefix / Suffix / Implicit / Any.
- **Tier**: rule tier is **maximum**; mods with higher tier number are ignored.

Use regex on the rolled line, e.g. `to maximum life`, `fire resistance`, `tier.*life`.

### Success

All required rules satisfied; if any optional rules configured, at least one optional hit.

---

## Part 2 — Technical

### `CheckItem()` flow

Identical rule engine to [Essence](Essence/README.md) and [Harvest](Harvest/README.md) (standard required/optional on `mod.Content`):

1. Clipboard + hash guard.
2. Parse `{ … }` blocks, strip `(d-d)` ranges.
3. `requiredCount` / `optionalCount` via nested rule×mod loops.
4. `required.Count == requiredCount` and optional guard.

### Differences vs Map

Chaos targets **rare** item mod **lines** and respects UI type/tier. Map targets **map mod names** with exclude-first logic and optional `q…r…ps…` stats.
