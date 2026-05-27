# Harvest gamble mode

**Source:** `dlls/Gamble/Modes/Harvest.cs`  
**UI settings:** `UIHarvest` (Item, Base = harvest craft button)

Uses Harvest crafting UI (click craft button, then item) instead of orbs. Rule logic matches Chaos/Essence.

---

## Part 1 — Rule set behaviour

### What the gambler does in game

1. Copy item.
2. If not valid: move to **harvest craft button**, left-click, move to item, copy again.
3. Repeat until match or cancel.

**No Shift+orb spam** — coordinates are item + UI button.

### Coordinates

| Slot | Target |
|------|--------|
| Item | Item in harvest craft window |
| Base | Reforge / craft button position |

### Priority meanings

| Priority | Role |
|----------|------|
| `≥ 1` | Required |
| `> 0` and `< 1` | Optional |

### Rule matching

- Regex on **`mod.Content`**.
- **Type** and **Tier** filters active.

Typical use: match reforge outcome lines like `increased fire damage` or essence-style suffix text.

### Success

Same as Chaos: all required rules hit; optional satisfied if configured.

---

## Part 2 — Technical

`CheckItem()` is structurally identical to `Chaos.cs` / `Essence.cs` (required/optional on parsed modifier content).

`Gamble()` loop clicks `button` then re-copies `item` — no orb right-click or Shift hold.
