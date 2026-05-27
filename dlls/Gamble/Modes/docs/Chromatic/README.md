# Chromatic gamble mode

**Source:** `dlls/Gamble/Modes/Chromatic.cs`  
**UI settings:** `UIChromatic` (Item, Base = chromatic orb)

Recolours item sockets until the socket colour counts match a pattern. **Does not parse item modifiers.**

---

## Part 1 — Rule set behaviour

### What the gambler does in game

1. Copy item → compare sockets.
2. If mismatch: right-click chromatic, hold **Shift**, click item until colours match.

### Coordinates

| Slot | Target |
|------|--------|
| Item | Item with sockets |
| Base | Chromatic orb |

### Rule format (Content only)

Uses the **first** rule row with non-empty **Content**. **Priority**, **Type**, and **Tier** are **not used**.

Content is an uppercase colour pattern using:

| Char | Colour |
|------|--------|
| `R` | Red |
| `G` | Green |
| `B` | Blue |
| `W` | White |

Example: `RRGG` — need exactly 2 red and 2 green linked sockets in the clipboard socket string.

The pattern length is the **total** socket count required; count of each letter must equal counted sockets on the item.

### Success

`R`, `G`, `B`, `W` counts on item equal counts in the rule string.

---

## Part 2 — Technical

### `CheckItem()` flow

1. Clipboard + hash guard.
2. Regex `([rgbw](\s|-)?){4,6}` finds socket substring in full item text.
3. Count `R`, `G`, `B`, `W` in matched socket string.
4. `rules.FirstOrDefault(x => x.Content.Length > 0).Content.ToUpperInvariant()` → count required letters.
5. Exact equality on all four counts.

### Limitations

- Only first populated rule row matters.
- Socket regex expects 4–6 socket groups in PoE clipboard format; unusual layouts may not match.
- No modifier parsing pipeline.
