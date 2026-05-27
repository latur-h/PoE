# Alt gamble mode

**Source:** `dlls/Gamble/Modes/Alt.cs`  
**UI settings:** `UIAlt` (Item, Base = alteration orb)

Rolls **magic** (or appropriate) items with alteration orbs until modifier rules match.

---

## Part 1 — Rule set behaviour

### What the gambler does in game

1. Copy item → if already valid, stop.
2. Right-click alteration on item once.
3. Hold **Shift**, spam left-click alteration on item, copy after each click.
4. Release Shift on success or cancel.

### Coordinates

| Slot | Target |
|------|--------|
| Item | Item in grid |
| Base | Alteration orb |

### Priority meanings

| Priority | Role |
|----------|------|
| `≥ 1` | **Required** — each rule must be satisfied by at least one mod |
| `> 0` and `< 1` (e.g. `0.5`) | **Optional** — if any optional rules exist, at least one must match |

**Note:** Priority `0` is **not** treated as optional; only strictly fractional priorities between 0 and 1 count as optional.

### Rule matching (what you type in Content)

- **Content** is a .NET **regex** matched against the modifier **description text** (the line(s) after the `{ Prefix … }` header), not the crafted name in quotes.
- **Type** (Prefix / Suffix / Implicit / Any) filters which mods are eligible.
- **Tier** is a **maximum** allowed tier on the mod: mods with `mod.Tier > rule.Tier` are skipped (lower tier number = better).

Example required row:

| Priority | Type | Tier | Content |
|----------|------|------|---------|
| `1` | Prefix | `1` | `adds \d+ to \d+ physical damage` |

### Enchants

Lines containing `(enchant)` are parsed as extra **Implicit** mods named `Enchant` and participate in required/optional matching like other implicits.

### Success

- Every **required** rule has at least one matching mod (after type/tier filters).
- If optional rules exist, **at least one** optional rule must match; if there are no optional rules, this check is skipped.

---

## Part 2 — Technical

### `CheckItem()` flow

1. Clipboard + hash guard.
2. Parse `{ … }` modifier blocks → `Modifier` list (type, tier, name, content); strip `(min-max)` from content.
3. Append enchant lines via `.*?\(enchant\)` regex.
4. `required = rules.Where(Priority >= 1)`, `optional = rules.Where(Priority > 0 && Priority < 1)`.
5. Nested loops: for each **rule**, for each **mod**, apply type/tier filters, `Regex.IsMatch(mod.Content, rule.Content)` → increment `requiredCount` or `optionalCount` per hit (multiple mods can satisfy one rule; one mod can increment multiple rules).
6. Success when `required.Count == requiredCount` and (`optional` empty or `optionalCount > 0`).

### Differences vs Map mode

| | Alt | Map |
|---|-----|-----|
| Match field | `mod.Content` | `mod.Name` |
| Exclude priority | No | Yes (`≤ -1`) |
| Stat `q…r…ps…` | No | Yes |
| Enchants | Yes | No |
