# Dynamic Texture Generation

## Overview

Ice Skates uses a **static-first** texture strategy: hand-crafted PNGs in `assets/iceskates/textures/item/` are used when present, and the runtime generator fills in any gaps. This means you can override any texture with a static file, while new metals only need a 5-color palette entry.

## Static Texture Priority

During `AssetsLoaded`, the generator checks each texture location with `api.Assets.TryGet()` before generating. If a static PNG already exists at the expected path, it is kept and generation is skipped for that texture.

**Naming convention for static overrides:**
- Blades: `assets/iceskates/textures/item/skateblade-{metal}.png` (e.g. `skateblade-iron.png`)
- Straps: `assets/iceskates/textures/item/skatestrap-{strap}.png` (e.g. `skatestrap-leather.png`)

Static textures should be 32x32 to match the generated resolution.

## How It Works

```
TextureGenerator.cs
├── 4 noise templates (16x16 byte arrays, values 0-4)
│   ├── Metal noise    — mottled, random hammered look
│   ├── Rawhide noise  — bumpy, irregular pattern
│   ├── Leather noise  — horizontal grain / striping
│   └── Fur noise      — fuzzy, dense dark clusters
├── 11 metal palettes (5 colors each)
├── 3 strap palettes (5 colors each)
└── GenerateAndInject(api)
    ├── For each metal: skip if static exists, else generate
    └── For each strap: skip if static exists, else generate
```

During `AssetsLoaded`, the generator:
1. Iterates each metal/strap
2. Checks if a static PNG already exists — if so, skips it
3. Maps the 16x16 noise template through the material's 5-color palette
4. Upscales to 32x32 via nearest-neighbor (each noise pixel becomes a 2x2 block)
5. Encodes the PNG via SkiaSharp and injects it via `api.Assets.Add()`

Item type JSON texture references (`skateblade-{blade}`, `skatestrap-{strap}`) resolve to either the static or injected assets — no changes needed in item/recipe definitions.

## Vanilla VS Art Style

The generated textures follow the vanilla Vintage Story aesthetic:

**Metals:** 3-5 discrete color values scattered randomly (mottled noise, no gradients). Mimics the hammered/forged look seen in `item/tool/material/` and `block/metal/ingot/` textures.

**Strap materials:**
- **Rawhide** — bumpy, irregular tan with lighter spots (like `block/cloth/rawhide.png`)
- **Leather** — smoother brown with horizontal grain/striping (like `block/leather/plain.png`)
- **Fur** — dark brown, fuzzy texture with dense clusters (like `block/cloth/fur/brown.png`)

## Metal Palettes

Each metal has 5 colors from darkest (C0) to brightest (C4):

| Metal          | C0 (deep)  | C1 (shadow) | C2 (base)  | C3 (light)  | C4 (bright) |
|----------------|-----------|-------------|-----------|------------|-------------|
| Bone           | `#8B7D60` | `#A89878`   | `#C8B898` | `#E0D0B0`  | `#F0E8D0`   |
| Copper         | `#5C2810` | `#8B4A18`   | `#B87333` | `#D89050`   | `#E8A868`   |
| Tin Bronze     | `#584018` | `#7A5C28`   | `#A88040` | `#C8A058`   | `#D8B870`   |
| Bismuth Bronze | `#483818` | `#685828`   | `#887840` | `#A89858`   | `#C0B070`   |
| Black Bronze   | `#181010` | `#302420`   | `#483830` | `#584840`   | `#685850`   |
| Iron           | `#383840` | `#505860`   | `#787880` | `#98A0A8`   | `#B0B8C0`   |
| Blister Steel  | `#303848` | `#485060`   | `#687888` | `#8898A8`   | `#A0B0C0`   |
| Meteoric Iron  | `#484030` | `#706848`   | `#988860` | `#B0A078`   | `#C8B890`   |
| Steel          | `#404048` | `#606068`   | `#888890` | `#A8A8B0`   | `#C8C8D0`   |
| Silver         | `#585860` | `#787880`   | `#A0A0A8` | `#C0C0C8`   | `#E0E0F0`   |
| Gold           | `#705008` | `#A07810`   | `#DAA520` | `#F0C830`   | `#FFE040`   |

## Strap Palettes

| Material | C0 (deep) | C1 (shadow) | C2 (base) | C3 (light) | C4 (bright) | Pattern |
|----------|----------|-------------|----------|-----------|-------------|---------|
| Rawhide  | `#5C4028` | `#7A5838` | `#988050` | `#B09868` | `#C8B080`   | Bumpy/irregular noise |
| Leather  | `#381C10` | `#583018` | `#784828` | `#906038` | `#A87848`   | Horizontal grain |
| Fur      | `#181008` | `#302010` | `#483820` | `#604830` | `#786040`   | Fuzzy noise with clusters |

## Shape Changes

The blade height was increased from 1px to 2px for visibility, and all boot elements raised by 1px to sit on top of the taller blade.

**`iceskates.json`:**

| Element   | Before (y range) | After (y range) | Change |
|-----------|-------------------|-----------------|--------|
| Blade     | 0-1 (1px)        | 0-2 (2px)       | Taller blade |
| BladeTip  | 0-1              | 0-2             | Taller tip |
| Sole      | 1-3              | 2-4             | Raised +1 |
| ToeBox    | 3-7              | 4-8             | Raised +1 |
| Heel      | 3-6              | 4-7             | Raised +1 |
| Ankle     | 3-6              | 4-7             | Raised +1 |
| Shaft     | 6-12             | 7-13            | Raised +1 |
| Cuff      | 12-13            | 13-14           | Raised +1 |
| Tongue    | 6-12             | 7-13            | Raised +1 |

**`skateblade.json`:**

| Element    | Before (y range) | After (y range) | Change |
|------------|-------------------|-----------------|--------|
| Blade      | 0-1              | 0-2             | Taller |
| BladeCurve | 1-2              | 2-3             | Raised +1 |

## Adding a New Metal

**Option A — Generated texture (easiest):** Add a palette entry to `MetalPalettes` in `TextureGenerator.cs`:

```csharp
["newmetal"] = new uint[] { 0xC0_dark, 0xC1_shadow, 0xC2_base, 0xC3_light, 0xC4_bright },
```

The noise template is shared across all metals — only the palette differs.

**Option B — Static texture override:** Place a 32x32 PNG at `assets/iceskates/textures/item/skateblade-newmetal.png`. The generator will detect it and skip generation for that metal. You still need a palette entry if you want the generator as a fallback.

## Future: Strip Crafting (Phase 2)

A future update will add a strip crafting mechanic:
- **Shears + hide/leather/pelt** in crafting grid produces strips
- Strips replace whole materials as the crafting ingredient for straps
- Strip textures will reuse the strap noise templates with the same palettes
