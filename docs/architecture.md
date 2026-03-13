# Architecture

## Project Structure

```
IceSkates/
├── modinfo.json
├── docs/                              # Documentation
│   ├── README.md                      # Table of contents
│   ├── stats-and-progression.md
│   ├── crafting-guide.md
│   ├── sharpening.md
│   ├── configuration.md
│   ├── creative-tools.md
│   ├── architecture.md
│   └── development.md
├── assets/iceskates/
│   ├── itemtypes/
│   │   ├── skateblade.json            # 11 blade variants
│   │   ├── iceskates.json             # 33 assembled variants (11 × 3)
│   │   └── icerinkcreator.json        # Creative-mode rink tool
│   ├── blocktypes/
│   │   └── sharpenerjig.json          # Sharpening jig block
│   ├── recipes/
│   │   ├── smithing/skateblade.json   # Anvil recipe (10 metals)
│   │   └── grid/
│   │       ├── skateblade-bone.json
│   │       ├── iceskates-{strap}.json # 3 assembly recipes
│   │       └── sharpenerjig.json
│   ├── shapes/{item,block}/           # Placeholder models
│   └── lang/en.json                   # Item names + descriptions
└── src/
    ├── IceSkatesConfig.cs             # Grouped config (4 tier groups)
    ├── IceSkatesMod.cs                # Entry: config, registration, recipe pruning
    ├── ItemIceSkates.cs               # Lining + whetstone + tooltips
    ├── ItemIceRinkCreator.cs          # Creative-mode rink generator
    ├── EntityBehaviorIceSkating.cs    # Ice detection, speed, hunger, particles
    ├── TextureGenerator.cs            # Runtime blade/strap texture generation
    ├── HudIceSkatesDebug.cs           # Debug HUD overlay
    ├── BlockSharpenerJig.cs           # Jig block interaction
    └── BlockEntitySharpenerJig.cs     # Jig use tracking
```

## Lining as Tree Attributes (not variants)

The lining upgrade stores its type as a **tree attribute** on the itemstack, not as a variant code. This mirrors how vanilla lanterns store their metal lining internally.

```csharp
itemstack.Attributes.SetString("lining", "pelt")
```

Benefits:
- No variant explosion (33 base variants instead of 165)
- Upgrade is purely C# — no extra recipes needed
- Lining state persists through inventory operations
- Easy to extend with new lining types

## Interaction Flow

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

## GUI Icon Rendering (wearableAttachment)

Wearable items that use `stepParentName` bone attachments in their shape (e.g., `LowerFootR`, `LowerFootL`) require `"wearableAttachment": true` in their `attributes` object. Every vanilla VS wearable (all 28 item types in `assets/survival/itemtypes/wearable/`) includes this attribute.

**What it does:**
- Tells the VS renderer to resolve `stepParentName` bone references when building the GUI mesh
- Positions elements relative to their parent bones in non-entity contexts (inventory, ground, hand)
- Without it, bone-attached elements have no position anchor in GUI → **invisible icons**

**Vanilla reference** (`assets/survival/itemtypes/wearable/seraph/foot.json`):
```json
"attributes": {
    "clothescategory": "foot",
    "wearableAttachment": true,
    "displaycaseable": true,
    ...
}
```

**Symptom without it:** Skates render correctly on the player's feet (entity context resolves bones), but GUI icons in creative inventory / hotbar are blank. Blade standalone items render fine because their shape doesn't use `stepParentName`.

## Stat Application (EntityBehaviorIceSkating)

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
