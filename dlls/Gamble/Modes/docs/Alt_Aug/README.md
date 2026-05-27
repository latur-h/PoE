# Alt_Aug gamble mode

**Source:** `dlls/Gamble/Modes/Alt_Aug.cs`  
**UI settings:** `UIAlt_Aug` (Item, Base = alt, Second = augmentation)

Automatically chooses **Alteration** vs **Augmentation** while rolling based on how many prefix/suffix mods are on the item.

---

## Part 1 — Rule set behaviour

### What the gambler does in game

1. Right-click alteration, move to item, hold **Shift** (setup in `FirstMove`).
2. Loop: copy → evaluate → either:
   - **Alt** — left-click item (reroll with alteration), or
   - **Aug** — Alt+left-click item (add mod with augmentation), or
   - **Success** — stop.

### Coordinates

| Slot | Target |
|------|--------|
| Item | Item |
| Base | Alteration orb |
| Second | Augmentation orb |

### Rule matching

Same intent as [Alt](Alt/README.md) for priorities:

| Priority | Role |
|----------|------|
| `≥ 1` | Required |
| `> 0` and `< 1` | Optional |

**Content** regex is tested against **`mod.Content` OR `mod.Name`** (either can satisfy).

**Type** and **Tier** filters apply like Alt.

### Success vs continue rolling

| State | Meaning |
|-------|---------|
| **Success** | `requiredCount >= required.Count` and optional rules satisfied (if any) |
| **Aug** | Rules not met and item has **one** prefix/suffix mod → add second mod |
| **Alt** | Rules not met and item has **two or more** prefix/suffix mods → reroll |

Special case: required met but optional rules exist and **none** matched → Aug if 1 mod, else Alt (keep rolling for optional).

### Success

Console: `[Gambler] [Success] Item matches the rules`.

---

## Part 2 — Technical

### `CheckItem()` return type

Returns `Response` enum: `Alt`, `Aug`, `Success`, `Failure` (clipboard/hash failure).

### Matching loop

- `modsCount` = count of mods where type is Prefix or Suffix.
- Required/optional loops match Alt_Aug pattern: `content.IsMatch(mod.Content) || content.IsMatch(mod.Name)`.
- Success condition: `required.Count <= requiredCount` (note **`<=`**, not `==` — extra matches allowed).

### Differences vs Alt

| | Alt_Aug | Alt |
|---|---------|-----|
| Name matching | Yes | No |
| Required count | `<=` | `==` |
| Orb selection | Automatic | Alt only |
| Enchants | Not parsed | Parsed |
