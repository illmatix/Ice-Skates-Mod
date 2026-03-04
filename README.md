# Ice Skates Mod for Vintage Story

Forge skate blades, strap them with hide, leather, or fur, then line them for warmth — and glide across frozen lakes!

## Three-Layer Stat System

Every pair of ice skates is defined by three independent choices — **blade**, **strap**, and **lining** — each affecting different gameplay stats:

| Layer    | Set During    | Affects                        |
|----------|---------------|--------------------------------|
| **Blade** | Crafting     | Ice speed bonus, durability    |
| **Strap** | Crafting     | Off-ice penalty (control)      |
| **Lining** | Post-craft upgrade | Warmth, hunger reduction  |

### Blade → Speed & Durability (from smithing/crafting)

| Material        | Ice Speed Bonus | Durability |
|-----------------|----------------|------------|
| Bone            | +20%           | 120        |
| Iron            | +40%           | 400        |
| Meteoric Iron   | +55%           | 550        |
| Steel           | +65%           | 700        |

### Strap → Control (from assembly recipe)

| Material | Off-Ice Penalty | Base Hunger While Skating |
|----------|----------------|---------------------------|
| Rawhide  | -20% speed     | +30% hunger drain         |
| Leather  | -15% speed     | Normal                    |
| Fur/Pelt | -10% speed     | -20% hunger drain         |

### Lining → Warmth & Hunger Reduction (post-craft upgrade)

Applied after crafting, like lantern lining. Hold skates in main hand, lining material in offhand, and right-click.

| Material        | Item Code Pattern    | Warmth  | Hunger Reduction |
|-----------------|----------------------|---------|------------------|
| Linen           | `game:linen-*`       | +1.0°C  | -5% hunger       |
| Fur (Pelt)      | `game:pelt-*`        | +2.5°C  | -15% hunger      |
| Sturdy Leather  | `game:*sturdyleather*` | +2.0°C | -20% hunger      |
| Wool Cloth      | Wool mod compat      | +3.0°C  | -10% hunger      |

Lining hunger reduction **stacks** with the strap's base hunger rate. For example, fur strap (-20%) + sturdy leather lining (-20%) = -40% net hunger drain.

Once lined, skates cannot be re-lined — choose wisely!

---

## Crafting Flow

### Step 1: Make the Blades

**Bone** (grid recipe, earliest tier):
```
[Bone ×2] [Knife]  →  2× Bone Skate Blade
```

**Iron / Meteoric Iron / Steel** (smithed on anvil):
Heat ingot → place on anvil → select "Ice Skate Blade" → smith → 2× blades per ingot

### Step 2: Assemble the Skates

Grid recipe — combine blade + strap material + sticks:
```
[Strap] [Stick] [Strap]
[     ] [Blade] [     ]
```

Where `[Strap]` is one of:
- **Raw Hide** (`game:hide-*`)
- **Leather** (`game:leather-*`)
- **Pelt** (`game:pelt-*`)

### Step 3: Line the Skates (optional upgrade)

1. Hold the skates in your **main hand**
2. Place lining material in your **offhand** (linen, pelt, sturdy leather, or wool cloth)
3. **Right-click** to apply

You'll hear a cloth rustling sound and see a chat confirmation. The tooltip updates to show the new lining stats.

---

## Example Builds

| Build Name         | Blade | Strap   | Lining         | Character                      |
|--------------------|-------|---------|----------------|--------------------------------|
| Survivor Special   | Bone  | Rawhide | None           | Cheap, disposable, early game  |
| Winter Workhorse   | Iron  | Leather | Linen          | Balanced mid-game daily driver |
| Speed Demon        | Steel | Rawhide | None           | Max speed, rough ride, hungry  |
| Comfort Cruiser    | Iron  | Fur     | Pelt           | Low hunger, warm, decent speed |
| Endgame Luxury     | Steel | Fur     | Sturdy Leather | Fast, warm, barely any hunger  |
| Wool Warmth        | M.Iron| Leather | Wool           | Warmest possible, great speed  |

---

## Project Structure

```
IceSkates/
├── modinfo.json
├── assets/iceskates/
│   ├── itemtypes/
│   │   ├── skateblade.json           # Blade component (4 metal variants)
│   │   └── iceskates.json            # Assembled skates (4 blades × 3 straps = 12 variants)
│   ├── recipes/
│   │   ├── smithing/skateblade.json  # Anvil recipe for metal blades
│   │   └── grid/
│   │       ├── skateblade-bone.json  # Bone + knife → blade
│   │       ├── iceskates-rawhide.json
│   │       ├── iceskates-leather.json
│   │       └── iceskates-fur.json
│   ├── shapes/item/                  # Placeholder models
│   ├── textures/item/                # (You create these)
│   └── lang/en.json
└── src/
    ├── IceSkatesMod.cs               # Entry point
    ├── ItemIceSkates.cs              # Item class: lining upgrade, stats, tooltip
    └── EntityBehaviorIceSkating.cs   # Behavior: ice detection, speed, hunger, particles
```

## Architecture

### Lining as Tree Attributes (not variants)

The lining upgrade stores its type as a **tree attribute** on the itemstack, not as a variant code. This mirrors how vanilla lanterns store their metal lining internally.

```
itemstack.Attributes.SetString("lining", "pelt")
```

Benefits:
- No variant explosion (12 base variants instead of 60)
- Upgrade is purely C# — no extra recipes needed
- Lining state persists through inventory operations
- Easy to extend with new lining types

### Interaction Flow

```
ItemIceSkates.OnHeldInteractStart():
├── Check offhand for valid lining material
├── IdentifyLiningMaterial() matches item code patterns:
│   ├── game:linen-*         → "linen"
│   ├── game:pelt-*          → "pelt"
│   ├── game:*sturdyleather* → "sturdyleather"
│   └── *wool*cloth*         → "wool"
├── Check current lining attribute ("none" = unlined)
├── If unlined: set attribute, consume offhand, play sound
└── If already lined: show error message
```

### Stat Application (EntityBehaviorIceSkating)

```
Read foot slot → is ItemIceSkates?
├── Get blade variant → speedBonus
├── Get strap variant → offIcePenalty, baseHungerMod
├── Get lining attribute → liningHungerReduction
├── netHungerMod = baseHungerMod + liningHungerReduction
│
├── ON ICE:
│   ├── walkspeed += speedBonus
│   ├── hungerrate += netHungerMod (while moving)
│   ├── Tick durability (server)
│   └── Spawn particles (client)
└── OFF ICE:
    ├── walkspeed += offIcePenalty
    └── Remove hunger modifier
```

---

## Textures Needed

**Blades** (16×16 or 32×32 PNG in `textures/item/`):
- `skateblade-bone.png`, `skateblade-iron.png`, `skateblade-meteoriciron.png`, `skateblade-steel.png`

**Straps**:
- `skatestrap-rawhide.png`, `skatestrap-leather.png`, `skatestrap-fur.png`

Tip: reference vanilla textures in `game:textures/item/` for consistent palette.

## Development Setup

1. .NET 8.0 SDK + `VINTAGE_STORY` env var pointing to game install
2. `dotnet new install VintageStory.Mod.Templates`
3. Create project, replace `src/` with this mod's files
4. Reference `VintagestoryAPI.dll` + `VSSurvivalMod.dll`
5. Build → copy output + `assets/` + `modinfo.json` to `Mods/`

Quick test: Drop entire folder with `.cs` files into `Mods/` — VS compiles at runtime.

## Future Ideas

- [ ] Proper 3D models + wearable shape on player feet
- [ ] Skating sound effects (blade scraping on ice)
- [ ] Momentum/slide mechanic (reduced friction on ice)
- [ ] Allow re-lining by removing old lining with shears (returns scrap)
- [ ] Visual indicator on model for lined vs unlined skates
- [ ] Handbook illustrations
- [ ] Integration with modded ice blocks
