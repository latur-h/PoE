# Eldritch gamble mode

**Source:** `dlls/Gamble/Modes/Eldritch.cs`  
**UI settings:** `UIEldritch` (Item, Base = eldritch orb)

Rerolls **eldritch implicit** lines on maps or other items using eldritch currency. Only **implicit** mods are considered for rules.

---

## Part 1 — Rule set behaviour

### What the gambler does in game

Same orb loop as Chaos: right-click orb, Shift+spam on item, copy between clicks.

### Coordinates

| Slot | Target |
|------|--------|
| Item | Item with eldritch implicits |
| Base | Eldritch chaos/remnant/etc. orb position |

### Priority meanings

| Priority | Role |
|----------|------|
| `≥ 1` | Required |
| `> 0` and `< 1` | Optional |

### Rule matching

- Only mods with `Type == Implicit` are checked.
- **Content** regex runs on **`mod.Content`** (the implicit line text).
- UI **Type** column is effectively overridden — prefix/suffix rules never see non-implicit mods.
- **Tier** column is **not** applied in code (no tier filter in loop).

Example implicit text to match: `of the Conqueror`, `Eater of Worlds`, damage pen lines, etc.

### Success

`required.Count == requiredCount`; optional rules need at least one hit if any optional rows exist.

---

## Part 2 — Technical

### `CheckItem()` flow

1. Clipboard + hash guard.
2. Full modifier parse (prefix/suffix blocks still parsed into list).
3. Required/optional loops **skip** any mod where `mod.Type != Implicit`.
4. `Regex.IsMatch(mod.Content, rule.Content)` only.

### Differences vs Chaos

| | Eldritch | Chaos |
|---|----------|-------|
| Mods considered | Implicit only | All types (filtered by rule.Type) |
| Tier filter | Not used | Used |

Prefix/suffix lines on the clipboard are parsed but never matched against rules.
