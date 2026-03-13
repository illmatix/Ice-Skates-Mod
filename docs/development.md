# Development

## Setup

1. .NET 8.0 SDK + `VINTAGE_STORY` env var pointing to game install
2. `dotnet new install VintageStory.Mod.Templates`
3. Create project, replace `src/` with this mod's files
4. Reference `VintagestoryAPI.dll` + `VSSurvivalMod.dll`
5. Build → copy output + `assets/` + `modinfo.json` to `Mods/`

Quick test: Drop entire folder with `.cs` files into `Mods/` — VS compiles at runtime.

## Textures Needed

11 blade textures + 3 strap textures in `textures/item/`:
- `skateblade-{bone,copper,tinbronze,bismuthbronze,blackbronze,iron,blistersteel,meteoriciron,steel,silver,gold}.png`
- `skatestrap-{rawhide,leather,fur}.png`

Tip: Match vanilla tool head color palettes. Silver and gold should be recognizably shiny/precious.

## VS Wearable Gotchas

- **`wearableAttachment: true`** — Required in `attributes` for any wearable whose shape uses `stepParentName` bone attachments. Without it, GUI icons are invisible (bones can't be resolved outside entity context). See [Architecture > GUI Icon Rendering](architecture.md#gui-icon-rendering-wearableattachment).
- **`storageFlags: 128`** — Required for equippable items.
- **`clothescategory`** — Goes inside `attributes` (lowercase), not top-level.

## Future Ideas

- [ ] 3D models + wearable shapes on player feet
- [ ] Skating sound effects
- [ ] Momentum/slide mechanics
- [ ] Hook into 1.22 buff system (sharpening → crit chance on ice?)
- [ ] Visual model change for lined vs unlined
- [ ] Allow re-lining with shears
- [ ] Handbook illustrations
