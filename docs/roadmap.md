# Roadmap

## v1.0 — MVP (current)

Core skating loop: forge, assemble, equip, skate.

- [x] 11 blade metals across 4 tiers (bone → steel)
- [x] 3 strap types (rawhide, leather, fur)
- [x] Lining upgrade system (linen, pelt, sturdy leather, wool)
- [x] Ice detection + speed boost + off-ice penalty
- [x] Hunger rate modifier (strap + lining stacking)
- [x] Durability drain while skating
- [x] Whetstone sharpening (portable)
- [x] Sharpening Jig block (workshop)
- [x] Configurable metal toggles with recipe pruning
- [x] ConfigLib compatibility
- [x] Smithing recipes (10 metals) + bone grid recipe
- [x] Assembly grid recipes (3 strap variants)
- [x] Ice Rink Creator (creative tool)
- [x] Placeholder textures and shapes
- [x] Debug HUD overlay

## v1.1 — Polish

Visual and audio improvements to make skating feel good.

- [ ] Custom blade and strap textures (replace placeholders)
- [ ] Handbook illustrations and descriptions
- [ ] Skating sound effects (blade on ice, stop/start)
- [ ] Ice particle effects tuning
- [ ] Visual model change for lined vs unlined skates

## v1.2 — Gameplay Depth

New mechanics that add strategic choices.

- [ ] Strip crafting mechanic (see below)
- [ ] Momentum/slide mechanics (inertia on ice)
- [ ] Strap-based on-ice control (rawhide = wider turns, fur = tighter carving)
- [ ] Allow re-lining with shears (replace existing lining)
- [ ] Durability warning (chat message or visual cue when low)
- [ ] Skating on rivers vs lakes (different friction?)

### Strip Crafting Mechanic

Shears + hide/leather/pelt in the crafting grid produce strips. Strips replace whole materials as the crafting ingredient for skate straps.

**Strip types:**

| Strip Type | Source Material | Grid Recipe |
|------------|----------------|-------------|
| Rawhide strip | Raw/prepared hide | Shears + hide |
| Leather strip | Leather (plain) | Shears + leather |
| Fur strip | Pelt | Shears + pelt |

**Yield by hide size:**

| Hide Size | Strip Yield |
|-----------|------------|
| Small     | 2 strips   |
| Medium    | 4 strips   |
| Large     | 6 strips   |
| Huge      | 8 strips   |

Leather and pelt don't have sizes — fixed yield (4 strips each).

**Required work:**
- New item type: `skatestrap-strip` with variants (rawhide, leather, fur)
- New grid recipes: shears + material → strips
- Modify existing skate assembly recipes to use strips instead of whole hides
- Strip texture generation (narrow texture variant) or reuse strap textures
- New shape for strip item
- Lang entries for strip items

## v2.0 — 3D Models & Wearables

Major visual upgrade — skates visible on the player.

- [ ] 3D skate blade models per metal
- [ ] Wearable shapes rendered on player feet
- [ ] Strap visual differences on model
- [ ] Lining visual indicator on model

## Future / Stretch

Ideas to explore if there's demand.

- [ ] Hook into 1.22 buff system (sharpening → crit chance on ice?)
- [ ] Multiplayer skating races (timed course with checkpoints)
- [ ] Ice trail visual effect behind skater
- [ ] Integration with seasons mods (ice availability)
- [ ] Skating trick system (jumps, spins — cosmetic)
