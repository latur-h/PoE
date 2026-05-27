# Essence gamble mode

**Source:** `dlls/Gamble/Modes/Essence.cs`  
**UI settings:** `UIEssence` (Item, Base = essence)

Applies essences (or spams reroll via essence workflow) until modifier **content** rules match. Same rule engine as Chaos/Harvest.

---

## Part 1 — Rule set behaviour

### What the gambler does in game

1. Copy → if valid, stop.
2. Right-click essence on item, hold **Shift**, click item with copy between clicks.

### Coordinates

| Slot | Target |
|------|--------|
| Item | Item |
| Base | Essence |

### Priority meanings

| Priority | Role |
|----------|------|
| `≥ 1` | Required |
| `> 0` and `< 1` | Optional (fractional only; not `0`) |

### Rule matching

- Regex on **`mod.Content`** (description under `{ … }` header).
- **Type** and **Tier** from UI apply (tier = max allowed tier on mod).

Example:

| Priority | Type | Tier | Content |
|----------|------|------|---------|
| `1` | Suffix | `1` | `to strength` |

### Success

`required.Count == requiredCount` and optional rules satisfied if present.

---

## Part 2 — Technical

Same `CheckItem()` structure as `Chaos.cs`:

- Modifier block parsing with `(d-d)` stripped.
- No enchant extraction (unlike Alt).
- No exclude/include split (unlike Map).
- No name-only matching (unlike Map).

See [Chaos](Chaos/README.md) technical section for loop semantics (rule×mod increments).
