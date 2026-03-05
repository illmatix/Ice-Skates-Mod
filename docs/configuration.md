# Configuration

Config file: `VintagestoryData/ModConfig/IceSkatesConfig.json`

Compatible with ConfigLib for in-game GUI editing.

## Default Config

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

## How Toggles Work

Each tier has a **master toggle** (e.g. `EnableMidGameBridges`) plus individual switches for each metal. Both must be `true` for the metal to appear. Disabled metals are fully pruned — recipes removed, items hidden from creative inventory.

## Example Playstyle Configs

| Playstyle                        | Early  | Mid            | Late          | Novelty        |
|----------------------------------|--------|----------------|---------------|----------------|
| Purist (bone→iron→steel only)    | off    | off            | off           | off            |
| Smooth progression               | copper | tin bronze     | blister steel | off            |
| Kitchen sink                     | copper | all 3 bronzes  | blister steel | gold + silver  |
| Roleplay server                  | off    | off            | off           | gold + silver  |
