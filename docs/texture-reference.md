# Texture Reference

## Texture Breakdown

All ice skate textures are generated at runtime by `TextureGenerator.cs` — no manual PNGs needed.

### Blade Textures (11 total) — `#blade` channel

All use the same `MetalNoise` template (mottled hammered-metal look), each with a 5-color palette:

| Texture name | Style |
|---|---|
| `skateblade-bone` | Creamy tan/beige |
| `skateblade-copper` | Orange-brown copper |
| `skateblade-tinbronze` | Warm golden bronze |
| `skateblade-bismuthbronze` | Muted olive bronze |
| `skateblade-blackbronze` | Very dark near-black |
| `skateblade-iron` | Blue-grey steel |
| `skateblade-blistersteel` | Cooler blue steel |
| `skateblade-meteoriciron` | Warm grey-green |
| `skateblade-steel` | Neutral silver-grey |
| `skateblade-silver` | Bright silver |
| `skateblade-gold` | Rich yellow gold |

### Strap Textures (3 total) — `#strap` channel

Each has its own noise template for a distinct material feel:

| Texture name | Noise pattern | Style |
|---|---|---|
| `skatestrap-rawhide` | `RawhideNoise` (bumpy/irregular) | Rough tan-brown |
| `skatestrap-leather` | `LeatherNoise` (horizontal grain bands) | Dark reddish-brown |
| `skatestrap-fur` | `FurNoise` (dark dense clusters) | Very dark brown/black |

### How It Works

All 14 textures are **16x16 PNGs** generated at startup via `TextureGenerator.GenerateAndInject()` and injected into the asset system before the texture atlas is built. The old static PNGs were deleted. The only remaining static texture is `icerinkcreator.png`.

### Shared by Both Items

The `skateblade-{metal}` textures are used by both the standalone `skateblade` item (as `#metal`) and the assembled `iceskates` item (as `#blade`) — same texture, different channel name.

---

## Comparison: Ice Skates vs. Vanilla Boots

### Texture Resolution

| Item | Texture Size | Approach |
|---|---|---|
| Seraph boots (player) | 32x32 or 32x56 | Static hand-painted PNGs |
| Trader boots (NPC) | 48x48 to 128x128 | Static PNGs |
| **Ice skates** | **16x16** | **Runtime-generated from noise templates** |

The skate textures are lower resolution than vanilla boots. Vanilla seraph boots use 32x32 (or 32x56 for tall boots like knee-high-fur-boots), while the skates generate 16x16 textures. However, the skate shape's `textureWidth`/`textureHeight` is declared as **32x32**, which means the 16x16 generated texture gets stretched across that UV space.

### Visual Style

**Vanilla boots** — hand-painted with intentional pixel art detail, shading, and material definition. Each variant is a unique PNG with carefully placed highlights and shadows.

**Ice skates** — algorithmically mottled via noise templates. The look depends on the noise pattern:
- **Metal blades**: random hammered-metal noise (same template for all 11 metals, just recolored)
- **Rawhide**: bumpy/irregular clusters
- **Leather**: horizontal grain bands
- **Fur**: dark, sparse clusters

This gives a serviceable procedural look but lacks the hand-crafted detail of vanilla textures — no deliberate highlights, stitching, buckle details, or edge shading that vanilla artists place.

### Structure Difference

Vanilla boots typically use a **single texture** channel for the whole shoe. The skates split into **two channels** (`#blade` and `#strap`), which lets the blade and boot portions have distinct materials. But both channels sample from the same flat procedural fill with no painted detail.

### Shape Complexity

Vanilla seraph boots have 2-4 elements, sometimes with rotated sub-elements for organic curves (fur collars, flaps). The skates have 8 children per foot (blade, blade tip, toe box, heel, ankle, shaft, cuff, tongue) — more geometric parts but all axis-aligned boxes with no rotation. This gives a blockier, more mechanical look which suits ice skates well.

### Key Differences Summary

| Feature | Vanilla Boots | Ice Skates |
|---------|---------------|------------|
| **Texture approach** | Static hand-painted PNGs (32-128px) | Runtime 16x16 noise templates |
| **Texture file count** | One per variant (dozens of PNG files) | Zero — generated at mod load |
| **Blade/specialty parts** | None (clothing only) | Blade element attached below foot |
| **Shape geometry** | Organic curves, foot-shaped | Stylized boot with blade |
| **Element count** | 2-4 elements | 2 main + 7 children (skate parts) |
| **Texture refs** | Single `#material` per shoe | Two refs: `#blade` + `#strap` |
| **Variant system** | Clothing variants (shoe names) | Blade metal x strap material (33 combos) |
| **Attachment bone** | `LowerFootL/R` | `LowerFootL/R` (same) |
| **File footprint** | ~20 MB textures + JSONs | ~10 KB textures (generated) + JSONs |

### Bottom Line

The skates look **flatter and more uniform** than vanilla boots because procedural noise lacks the intentional detail of hand-painted textures. To blend better with vanilla, options include bumping the generated resolution to 32x32 (matching seraph boots) with more sophisticated noise patterns, or replacing the generator with hand-painted PNGs for each variant.
