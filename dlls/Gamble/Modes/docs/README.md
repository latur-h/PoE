# Gamble modes documentation

Each gamble type lives in its own module under `dlls/Gamble/Modes/` and implements `IGamba`. Rules are built from the UI (8 rows: **Priority**, **Type**, **Tier**, **Content**) in `Core.GamblerStart()` and passed into the mode constructor.

## Quick comparison

| Mode | Coordinates | Reroll action | Rule matching target | Priority model |
|------|-------------|---------------|----------------------|----------------|
| [Alt](Alt/README.md) | Item, Alteration | Shift+click alt on item | Mod **content** (+ enchants) | Required ≥1, optional fractional |
| [Alt_Aug](Alt_Aug/README.md) | Item, Alt, Aug | Alt or Aug (auto) | Mod **content** or **name** | Required ≥1 (≥ count), optional fractional |
| [Chaos](Chaos/README.md) | Item, Chaos | Shift+click chaos | Mod **content** | Required ≥1, optional fractional |
| [Chromatic](Chromatic/README.md) | Item, Chromatic | Shift+click chromatic | Socket colours | First non-empty Content |
| [Essence](Essence/README.md) | Item, Essence | Shift+click essence | Mod **content** | Required ≥1, optional fractional |
| [Map](Map/README.md) | Item, Alchemy, Scour | Alt+scour + alch cycle | Mod **name** (exclude-first) | Stats (compact + More), include ≥1, exclude ≤-1 |
| [MapT17](MapT17/README.md) | Item, Chaos | Shift+click chaos | Mod **name** + More% rules | Same stat model as Map |
| [MapExalt](MapExalt/README.md) | Item, Alchemy, Scour, Exalt | Scour+alch + Exalt to 6 mods | Mod **name** + More% rules | Same as Map; success at 6 mods |
| [Harvest](Harvest/README.md) | Item, Harvest button | Reforge UI click | Mod **content** | Required ≥1, optional fractional |
| [Eldritch](Eldritch/README.md) | Item, Eldritch orb | Shift+click orb | **Implicit** content only | Required ≥1, optional fractional |

## Shared technical notes

- **Clipboard**: `Ctrl+Alt+C` copy, read as plain text, then cleared.
- **Stuck detection**: if clipboard hash unchanged 3 times in a row → cancel (`maxAttempts = 3`).
- **Content field**: .NET regex, case-insensitive, unless noted otherwise.
- **Modifier parsing** (where applicable): blocks `{ Prefix … }` / `{ Suffix … }` plus following line(s); `(min-max)` rolled values stripped from content before matching.

See each mode folder for full behaviour and implementation detail.
