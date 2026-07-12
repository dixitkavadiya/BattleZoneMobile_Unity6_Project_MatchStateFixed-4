# BattleZone Mobile - Milestone 21 Changed Files

Milestone 21 continued from the current latest project and preserved existing gameplay systems.

## Runtime Systems

- `Assets/BattleZoneMobile/Scripts/Core/BattleZoneRuntimeBuilder.cs`
  - Added `BuildMilestone21PlayableIslandMap()` and the M21 Unity Terrain island foundation.
  - Added original M21 POIs: River Village, Warehouse District, Shipping Yard, Military Checkpoint, Coast Gas Station, Island School, Small Hospital, Pine Forest, and North Hills.
  - Added M21 asphalt/dirt roads, river segments, sand banks, perimeter water, and two river bridges.
  - Added generated village houses, warehouses, container yard, checkpoint, gas station, school, hospital, fences, props, windows, doors, interiors, cover points, and rooftop-access routes.
  - Added M21 terrain painting, terrain grass detail, terrain collider, URP Terrain/Lit material assignment, reflection probes, loot anchors, and loot cluster bias.
  - Added runtime NavMesh generation over the island collision bounds so existing bots can navigate the new playable island foundation.
- `Assets/BattleZoneMobile/Editor/BattleZoneRuntimeSmokeTest.cs`
  - Added validation for M21 terrain layers, required POIs, river bridges, loot anchors, and non-empty runtime NavMesh.

## Documentation

- `README.md`
- `CHANGELOG.md`
- `TEST_CHECKLIST.md`
- `PERFORMANCE_BUDGET.md`
- `PLACEHOLDERS_TO_REPLACE.md`
- `Assets/BattleZoneMobile/Docs/README.md`
- `Assets/BattleZoneMobile/Docs/TEST_CHECKLIST.md`
- `CHANGED_FILES_MILESTONE21.md`

## Generated Deliverable

- `BattleZoneMobile_Unity6_Project_Milestone21.zip`
  - Complete Unity project package with `Assets`, `Packages`, `ProjectSettings`, scene, scripts, docs, and runtime-generated systems.
