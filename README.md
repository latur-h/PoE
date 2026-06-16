# PoE

Windows desktop helper for [Path of Exile](https://www.pathofexile.com/). Automates flask usage, currency gambling routines, stash coordinates, and configurable input macros while Path of Exile is the focused process.

**License:** [GNU AGPL v3](LICENSE)

## Features

### Main — Flask automation
- Up to five flasks with per-slot key, type (HP / MP / Utility / Tincture), and threshold
- Pixel-based flask bar detection with configurable poll and cooldown timings
- Hotkeys to register flasks, start drinking, and stop

### Gamble — Currency routines
Rule-driven automation for common crafting loops. Each mode uses coordinates from the **Orbs** tab and rules defined in the UI (priority, modifier type, tier, content).

| Mode | Summary |
|------|---------|
| Alteration | Spam alts until mod rules match |
| Alt + Aug | Alteration or Augmentation with name/content rules |
| Chromatic | Socket colour targeting |
| Chaos | Chaos orb spam on items |
| Essence | Essence crafting |
| Map / Map T17 / Map Exalt | Map rolling (alchemy, scouring, exalt variants) |
| Harvest | Harvest reforge UI |
| Eldritch | Eldritch orb on implicits |

**Bulk inventory** (Map, Map Exalt, Map T17): roll every map in a configured grid instead of a single item coordinate. Set grid area (Settings → Grid hotkey), first cell, and Next X / Next Y step on the Gamble tab. Each cycle batches scour/alch, exalt or chaos, optional Vaal corrupt, and stashes broken or already-corrupted maps that fail rules. See `dlls/Gamble/Modes/docs/` for mode details.

Presets and up to 100 rules per gamble type. Modifier **Content** fields support autocomplete from cached game data (GGPK).

### Orbs — Coordinates
Record mouse positions for item slots and currency positions used by gamble modes.

### Macros
- **Global** macros (always armed) and **build profiles** (one active at a time, up to 100 profiles)
- Behaviors: **Single** (press trigger), **Loop** (hold trigger), **Repeat** (toggle start/stop)
- Fire sequences use [Poss.Win.Automation](https://github.com/) key strings (`Ctrl Down`, `LButton Up`, …), one action per line or separated by `+`
- Per-macro active flag, optional toggle hotkeys, and a global feature enable hotkey

### Settings
- Global hotkeys for flasks and gamble start/stop
- Game data folder and mod cache refresh for gamble autocomplete
- Target process name for input (`PathOfExile.exe` by default)

### Logs
In-app log viewer for flask, gamble, and system messages.

## Requirements

- **Windows** x64
- **.NET 10** SDK (to build from source)
- Path of Exile installed (for game data cache and process-targeted input)

## Build and run

```bash
git clone <repository-url>
cd PoE
dotnet build PoE.csproj
dotnet run --project PoE.csproj
```

Settings are stored in `%AppData%\PoE\userSettings.json`.

## Publish

Release builds use the publish profile in `Properties/PublishProfiles/ReleaseProfile.pubxml` (self-contained single-file `win-x64`).

```bash
dotnet publish PoE.csproj -p:PublishProfile=ReleaseProfile
```

Output: `bin/Release/net10.0-windows/publish/win-x64/`

Pushing a version to `deploy/publish.txt` on `main` triggers the GitHub Actions release workflow (zip + GitHub Release).

## Third-party components

See [THIRD_PARTY_NOTICES.txt](THIRD_PARTY_NOTICES.txt). Notable dependencies:

- [Poss.Win.Automation](https://www.nuget.org/packages/Poss.Win.Automation.Input) — input simulation and global hotkeys
- LibDat2 / LibGGPK3 — reading PoE game archives for mod data
- Microsoft.Extensions.Hosting — DI and app host

Bundled native libraries live under `libs/`.

## Project layout

```
PoE/
├── Main*.cs              # WinForms UI and tab wiring
├── Core.cs               # Hotkeys, flask/gamble session entry points
├── dlls/
│   ├── Flasks/           # Flask manager and types
│   ├── Gamble/           # Gamble modes and UI
│   ├── Macros/           # Macro engine and UI
│   ├── GameData/         # GGPK mod cache and autocomplete
│   ├── Settings/         # Persisted user settings
│   └── ...
├── tests/PoE.Tests/      # Unit tests
└── deploy/publish.txt    # Release version (CI)
```

## Disclaimer

This tool sends keyboard and mouse input to Path of Exile and may violate Grinding Gear Games’ terms of service. Use at your own risk.
