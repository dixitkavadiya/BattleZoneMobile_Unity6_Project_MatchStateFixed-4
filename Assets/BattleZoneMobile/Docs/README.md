# BattleZone Mobile - Milestone 21 Playable Original Island

Milestone 21 continues from the latest stable project, keeps existing gameplay systems intact, and adds the first playable original island map foundation with Unity Terrain, named POIs, connected roads, river crossings, enterable structures, loot anchors, runtime NavMesh coverage, URP lighting/probes, procedural PBR-style materials, dynamic weather, and mobile optimization hooks while preserving match flow, drop systems, bots, loot, vehicles, UI, and VisualSlice replacement architecture.

## Summary

- Added `Milestone21 Unity Terrain - First Playable Island` with generated height shaping, grass/dirt/asphalt/rock/sand layers, splat maps, terrain grass detail, TerrainCollider, and URP Terrain/Lit material assignment.
- Added M21 River Village, Warehouse District, Shipping Yard, Military Checkpoint, Coast Gas Station, Island School, Small Hospital, Pine Forest, and North Hills named locations.
- Added M21 asphalt roads, dirt roads, river segments, sand banks, perimeter water, and two bridge crossings.
- Added enterable M21 village houses, warehouse district, shipping container yard, military checkpoint, gas station, school, hospital, watch towers, windows, doors, simple interiors, cover, fences, and rooftop-access routes.
- Added M21 loot anchors and loot-cluster bias around the new POIs.
- Added runtime NavMesh generation over the island collision layer for existing bot navigation.
- Added M21 reflection probes while preserving URP lighting, fog, shadows, weather, post-processing, match flow, bots, loot, vehicles, weapons, inventory, zone, minimap, and Android controls.
- Documents M21 as original runtime-generated proxy content, not final authored AAA art.
- Added `Milestone20 Unity Terrain - Original Battle Royale World` with generated heightmap, grass/dirt/road/rock/sand layers, splat maps, and terrain grass detail.
- Added M20 Riverbend Village, Aster Town, North Military Base, Rail Warehouse, Shipping Yard, Hillside Gas Station, Pine River Forest, and Ridge Hills.
- Added M20 roads, bridges, river water, sand banks, houses with interiors, town blocks, warehouse, container yard, gas station, military structures, trees, rocks, and grass patches.
- Added HDR-style procedural sky, mixed sun light, reflection probes, light-probe grid, rain, moving clouds, day/night movement, and dynamic fog.
- Added mobile optimization hooks for terrain instancing, GPU-instanced materials, LOD groups, static occlusion-friendly cells, and constrained detail density.
- Added velocity-based skydiving, steering, wind drift, terminal speed limits, automatic parachute deployment, manual parachute request, parachute glide damping, and landing recovery.
- Added drop-specific camera framing and procedural skydiving/parachute/landing poses for the current original low-poly gameplay character.
- Added `Milestone19 Drop Experience Terrain Root` with village, forest, river, hills, roads, bridge, container yard, and military camp landmarks.
- Added M19 loot cluster bias and improved bot parachute landing/NavMesh handoff.
- Added complete BR director improvements: waiting lobby, countdown telemetry, randomized flight direction, jump prompt, auto-jump, free-fall, parachute, landing recovery, combat unlock, and winner/defeat flow preservation.
- Added `MiniMapFlightPath`, `MatchPhaseText`, and `FlightPathText` for clearer match-state and route readability.
- Added per-match safe-zone preparation and improved loot distribution clusters.
- Improved bot drop spacing and mobile fire input buffering.
- Added `M17_VerticalSlice_200m_Root` with grass, dirt, sand, road, sidewalk, river edge, street props, vegetation, lighting props, and a reflection probe.
- Added enterable M17 apartment, warehouse, shop, and guard tower roots with interiors, replacement slots, art sockets, cover points, and LOD groups.
- Added `M17_TacticalCharacter_PrefabSlot` for future professional character replacement without changing the current gameplay character.
- Added smoke-test validation for M17 roots, slots, LODs, materials, reflection probe, and generated non-primitive meshes.
- Added `Assets/BattleZoneMobile/ArtPipeline` with material palette, prefab slots, visual definitions, and catalog.
- Added replacement scripts under `Assets/BattleZoneMobile/Scripts/Visuals`.
- Added a labeled visual-slice area in `BZ_Main` with street block, enterable house, warehouse, character showcase, and two weapon showcases.
- Added Low, Medium, High, and Ultra URP quality presets.
- Clearly documents placeholder/proxy content separately from production-ready art targets.
- Existing match flow, bots, loot, zone, vehicles, armor, inventory, weapons, UI, audio, and Android touch controls were preserved.

See the project root `README.md`, `ART_SPEC.md`, `ASSET_REPLACEMENT_GUIDE.md`, `PERFORMANCE_BUDGET.md`, `PLACEHOLDERS_TO_REPLACE.md`, and `TEST_CHECKLIST.md` for details.
