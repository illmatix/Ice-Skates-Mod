# Sharpening

Ice skates lose durability as you skate on ice. Two sharpening methods let you restore them.

## Whetstone (portable, ~12% repair)

**How to use:** Main hand: skates | Offhand: whetstone | Right-click.

Damages the whetstone by 1 durability per use. Matches any item with "whetstone" in its code — compatible with vanilla 1.22+, RepairMe, Toolsmith, and other mods that add whetstones.

Best for quick field repairs when you're out exploring.

## Sharpening Jig (workshop, ~45% repair)

**How to use:** Place the jig on a surface, then right-click it while holding skates. 15 uses before the jig breaks.

**Crafting recipe:**
```
[Plank] [Stone] [Plank]
[Nail ] [     ] [Nail ]
```

Spawns stone dust particles when sharpening.

Best for bulk maintenance at your base — one jig handles many pairs of skates before wearing out.

## Repair Amounts

Both repair percentages are configurable. See [Configuration](configuration.md) for details:
- `WhetstoneRepairPercent` (default: 12)
- `SharpenerJigRepairPercent` (default: 45)
- `SharpenerJigMaxUses` (default: 15)
