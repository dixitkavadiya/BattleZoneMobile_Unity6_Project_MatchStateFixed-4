# BattleZone Mobile - Milestone 20 Changed Files

Milestone 20 continued from the current latest project and preserved existing gameplay systems.

## Runtime Systems

- `Assets/BattleZoneMobile/Scripts/Core/BattleZoneRuntimeBuilder.cs`
  - Added Unity Terrain System generation with procedural heightmap, terrain layers, splat maps, terrain grass detail, and URP Terrain/Lit material assignment.
  - Added original M20 world areas: Riverbend Village, Aster Town, North Military Base, Rail Warehouse, Shipping Yard, Hillside Gas Station, Pine River Forest, and Ridge Hills.
  - Added original runtime environment assets for village houses, town blocks, military base, warehouse, shipping containers, gas station, roads, bridges, river water, sand banks, rocks, trees, and grass patches.
  - Added M20 reflection probes, light-probe grid, mixed sun-light setup, HDR-style procedural sky, terrain instancing, GPU-instanced materials, LOD groups, and occlusion-friendly markers.
  - Moved the legacy prototype ground below the M20 terrain so the flat prototype plane is no longer the visible world base.
- `Assets/BattleZoneMobile/Scripts/Core/RuntimeDynamicWeatherController.cs`
  - Added dynamic day/night sun movement, fog changes, moving clouds, and runtime rain-particle emission.
- `Assets/BattleZoneMobile/Editor/BattleZoneRuntimeSmokeTest.cs`
  - Added validation for M20 terrain, terrain layers, landmarks, probes, weather objects, and dynamic weather controller.

## Documentation

- `README.md`
- `CHANGELOG.md`
- `TEST_CHECKLIST.md`
- `PLACEHOLDERS_TO_REPLACE.md`
- `PERFORMANCE_BUDGET.md`
- `Assets/BattleZoneMobile/Docs/README.md`
- `Assets/BattleZoneMobile/Docs/TEST_CHECKLIST.md`
- `CHANGED_FILES_MILESTONE20.md`

## Generated Deliverable

- `BattleZoneMobile_Unity6_Project_Milestone20.zip`
  - Complete Unity project package with `Assets`, `Packages`, `ProjectSettings`, scene, scripts, docs, and runtime-generated systems.
