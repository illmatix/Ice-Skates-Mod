# Ice Skates Mod for Vintage Story

Forge skate blades, strap them up, line them for warmth, sharpen them at your workshop — then glide across frozen lakes!

## Full Metal Progression

The mod ships with 4 always-on blade metals and 7 configurable bridging/novelty metals, organized by game tier:

```
Gold (+15%, 60 dura)          ← Novelty (config)
Silver (+18%, 80 dura)        ← Novelty (config)
Bone (+20%, 120 dura)         ← Always on
  Copper (+25%, 180 dura)     ← Early-game bridge (config)
    Tin Bronze (+30%, 250)    ← Mid-game bridge (config)
    Bismuth Bronze (+30%, 250)← Mid-game bridge (config)
    Black Bronze (+32%, 280)  ← Mid-game bridge (config)
      Iron (+40%, 400)        ← Always on
        Blister Steel (+50%, 480) ← Late-game bridge (config)
          Meteoric Iron (+55%, 550) ← Always on
            Steel (+65%, 700)       ← Always on
```

Gold and silver are intentionally *worse* than bone — they're soft metals, historically useless for blades, included purely for the vanity of skating on precious metal.

---

## Configuration

Config file: `VintagestoryData/ModConfig/IceSkatesConfig.json`
Compatible with ConfigLib for in-game GUI editing.

```json
{
  "EnableEarlyGameBridges": false,
  "EnableCopper": true,

  "EnableMidGameBridges": false,
  "EnableTinBronze": true,
  "EnableBismuthBronze": false,
  "EnableBlackBronze": false,

  "EnableLateGameBridges": false,
  "EnableBlisterSteel": true,

  "EnableNoveltyMetals": false,
  "EnableSilver": true,
  "EnableGold": true,

  "WhetstoneRepairPercent": 12,
  "SharpenerJigRepairPercent": 45,
  "SharpenerJigMaxUses": 15
}
```

Each tier has a **master toggle** (e.g. `EnableMidGameBridges`) plus individual switches for each metal. Both must be `true` for the metal to appear. Disabled metals are fully pruned — recipes removed, items hidden from creative inventory.

**Example configs:**

| Playstyle | Early | Mid | Late | Novelty |
|-----------|-------|-----|------|---------|
| Purist (bone→iron→steel only) | off | off | off | off |
| Smooth progression | copper | tin bronze | blister steel | off |
| Kitchen sink | copper | all 3 bronzes | blister steel | gold + silver |
| Roleplay server | off | off | off | gold + silver |

---

## Stat System (Four Layers)

| Layer       | Set During          | Affects                     |
|-------------|---------------------|-----------------------------|
| **Blade**   | Crafting            | Ice speed, durability       |
| **Strap**   | Crafting            | Off-ice penalty, base hunger|
| **Lining**  | Post-craft upgrade  | Warmth, hunger reduction    |
| **Sharpening** | Maintenance      | Durability restoration      |

### Blade Stats (complete table)

| Material        | Speed  | Durability | Tier Group  |
|-----------------|--------|------------|-------------|
| Gold            | +15%   | 60         | Novelty     |
| Silver          | +18%   | 80         | Novelty     |
| Bone            | +20%   | 120        | Always on   |
| Copper          | +25%   | 180        | Early-game  |
| Tin Bronze      | +30%   | 250        | Mid-game    |
| Bismuth Bronze  | +30%   | 250        | Mid-game    |
| Black Bronze    | +32%   | 280        | Mid-game    |
| Iron            | +40%   | 400        | Always on   |
| Blister Steel   | +50%   | 480        | Late-game   |
| Meteoric Iron   | +55%   | 550        | Always on   |
| Steel           | +65%   | 700        | Always on   |

### Strap Stats

| Strap   | Off-Ice Penalty | Base Hunger |
|---------|----------------|-------------|
| Rawhide | -20% speed     | +30% drain  |
| Leather | -15% speed     | Normal      |
| Fur     | -10% speed     | -20% drain  |

### Lining Stats (post-craft upgrade)

Hold skates in main hand, lining in offhand, right-click. Permanent, non-replaceable.

| Lining          | Warmth  | Hunger Reduction |
|-----------------|---------|------------------|
| Linen           | +1.0°C  | -5%              |
| Fur (Pelt)      | +2.5°C  | -15%             |
| Sturdy Leather  | +2.0°C  | -20%             |
| Wool Cloth      | +3.0°C  | -10%             |

Lining hunger reduction stacks with strap base rate.

---

## Sharpening

### Whetstone (portable, ~12% repair)

Main hand: skates | Offhand: whetstone | Right-click.
Damages whetstone by 1 durability. Matches any item with "whetstone" in code — compatible with vanilla 1.22+, RepairMe, Toolsmith, etc.

### Sharpening Jig (workshop, ~45% repair)

Place on surface, right-click while holding skates. 15 uses before the jig breaks.
Crafted from: `[Plank] [Stone] [Plank] / [Nail] [    ] [Nail]`
Spawns stone dust particles when sharpening.

---

## Crafting Flow

### Step 1: Make the Blades

**Bone** (grid recipe, earliest tier):
```
[Bone ×2] [Knife]  →  2× Bone Skate Blade
```

**Metals** (smithed on anvil):
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

### Step 4: Maintain (sharpening)

- **Whetstone** (field): Main hand skates, offhand whetstone, right-click (~12% durability restored)
- **Sharpening Jig** (workshop): Place jig block, right-click while holding skates (~45% durability restored, 15 uses per jig)

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
│   │   ├── skateblade.json            # 11 blade variants
│   │   └── iceskates.json             # 33 assembled variants (11 × 3)
│   ├── blocktypes/
│   │   └── sharpenerjig.json          # Sharpening jig block
│   ├── recipes/
│   │   ├── smithing/skateblade.json   # Anvil recipe (10 metals)
│   │   └── grid/
│   │       ├── skateblade-bone.json
│   │       ├── iceskates-{strap}.json # 3 assembly recipes
│   │       └── sharpenerjig.json
│   ├── shapes/{item,block}/           # Placeholder models
│   └── lang/en.json                   # 44 item names + descriptions
└── src/
    ├── IceSkatesConfig.cs             # Grouped config (4 tier groups)
    ├── IceSkatesMod.cs                # Entry: config, registration, recipe pruning
    ├── ItemIceSkates.cs               # Lining + whetstone + tooltips
    ├── EntityBehaviorIceSkating.cs    # Ice detection, speed, hunger, particles
    ├── BlockSharpenerJig.cs           # Jig block interaction
    └── BlockEntitySharpenerJig.cs     # Jig use tracking
```

## Architecture

### Lining as Tree Attributes (not variants)

The lining upgrade stores its type as a **tree attribute** on the itemstack, not as a variant code. This mirrors how vanilla lanterns store their metal lining internally.

```
itemstack.Attributes.SetString("lining", "pelt")
```

Benefits:
- No variant explosion (33 base variants instead of 165)
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

11 blade textures + 3 strap textures in `textures/item/`:
- `skateblade-{bone,copper,tinbronze,bismuthbronze,blackbronze,iron,blistersteel,meteoriciron,steel,silver,gold}.png`
- `skatestrap-{rawhide,leather,fur}.png`

Tip: Match vanilla tool head color palettes. Silver and gold should be recognizably shiny/precious.

## Development Setup

1. .NET 8.0 SDK + `VINTAGE_STORY` env var pointing to game install
2. `dotnet new install VintageStory.Mod.Templates`
3. Create project, replace `src/` with this mod's files
4. Reference `VintagestoryAPI.dll` + `VSSurvivalMod.dll`
5. Build → copy output + `assets/` + `modinfo.json` to `Mods/`

Quick test: Drop entire folder with `.cs` files into `Mods/` — VS compiles at runtime.

## Future Ideas

- [ ] 3D models + wearable shapes on player feet
- [ ] Skating sound effects
- [ ] Momentum/slide mechanics
- [ ] Hook into 1.22 buff system (sharpening → crit chance on ice?)
- [ ] Visual model change for lined vs unlined
- [ ] Allow re-lining with shears
- [ ] Handbook illustrations
