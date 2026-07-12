# BattleZone Mobile - Milestone 21 Test Checklist

Use the root `TEST_CHECKLIST.md` as the primary checklist. This copy exists so the checklist is visible inside Unity's Project window.

## Required Checks

- `BZ_Main` opens and runs immediately in Unity 6.5.
- No red project Console errors appear.
- No repeated BattleZone script warnings appear.
- `BattleZoneMobile_URP` is assigned in Graphics and Quality settings.
- Low, Medium, High, and Ultra quality presets exist.
- `Milestone21 Playable Original Island Root`, `Milestone21 Unity Terrain - First Playable Island`, `M21 Island Water Perimeter`, `M21 River Segment 1`, `M21 River North Bridge Deck`, and `M21 River South Bridge Deck` are created.
- M21 terrain has TerrainData, at least five terrain layers, terrain collision, and no bright yellow, magenta, pink, or untextured surfaces.
- M21 POIs are created: `M21 Village House 1 Root`, `M21 Warehouse District Warehouse A Root`, `M21 Container Yard Stack 1`, `M21 Military Checkpoint Gatehouse Root`, `M21 Gas Station Canopy`, `M21 School Main Building Root`, and `M21 Hospital Main Building Root`.
- M21 POIs include connected roads, collision, simple interiors, windows/doors, cover, rooftop access where intended, loot anchors, reflection probes, and runtime NavMesh coverage.
- Existing bots can navigate the M21 island foundation after the runtime NavMesh bake.
- `Milestone20 Realistic Original World Root`, `Milestone20 Unity Terrain - Original Battle Royale World`, `M20 Main Asphalt Route`, `M20 River Water Segment 1`, `M20 River Bridge North Deck`, `M20 Riverbend Village House 1 Root`, `M20 Aster Town Shop Row Building Root`, `M20 Military Base Command Building Root`, `M20 Warehouse Main Building Root`, `M20 Container Yard Stack 1`, and `M20 Gas Station Canopy` are created.
- `M20 Light Probe Grid`, `M20 Rain System`, `M20 Dynamic Cloud Root`, and `Milestone20 Dynamic Weather Controller` are created.
- The M20 Terrain object has TerrainData, grass/dirt/road/rock/sand terrain layers, and no bright yellow, magenta, pink, or untextured surfaces.
- Day/night, fog, moving clouds, and rain can run without Console red errors.
- `Milestone19 Drop Experience Terrain Root`, `M19 Large Drop Grassland`, `M19 Village Road`, `M19 Drop River Segment 1`, `M19 Drop Bridge Deck`, `M19 Drop Forest Cluster 1`, `M19 Container Yard Stack 1`, and `M19 Military Camp Command Tent` are created.
- Jumping from the aircraft enters skydiving, allows steering and camera look, auto-deploys parachute, supports manual parachute request, lands safely, and returns to normal movement.
- Bots descend by parachute, hand off to navigation cleanly after landing, and continue loot/combat/zone behavior.
- `Milestone18 Complete Battle Royale Match Flow`, `Milestone18 Randomized Flight Path`, `Milestone18 Flight Start Marker`, `Milestone18 Flight End Marker`, `MiniMapFlightPath`, `MatchPhaseText`, and `FlightPathText` are created.
- Start Match enters waiting lobby, shows countdown, runs aircraft route, allows jump, supports parachute landing, starts combat, and ends with victory/defeat screens.
- Bots drop at varied flight-path positions and loot/zone/combat systems remain active after landing.
- `M17_VerticalSlice_200m_Root`, `M17_Apartment_Root`, `M17_Warehouse_Root`, `M17_Shop_Root`, `M17_GuardTower_Root`, `M17_TacticalCharacter_PrefabSlot`, and `M17_VerticalSlice_ReflectionProbe` are created.
- The M17 district includes grass, dirt, sand, asphalt road, sidewalk, river edge, apartment, warehouse, shop, guard tower, trees, bushes, rocks, street lights, electric poles, fences, barrels, crates, and benches.
- M17 buildings and prefab slot have replacement slots and LOD groups.
- `Assets/BattleZoneMobile/ArtPipeline` contains generated prefab slots and visual definitions.
- `VS_StreetBlock_Root`, `VS_Enterable_House_Root`, `VS_Warehouse_Root`, `VS_Tactical_Humanoid_Showcase`, `VS_Rook17_AssaultRifle_Showcase`, and `VS_Sable9_Pistol_Showcase` are created.
- Visual-slice showcase objects have replacement slots and LOD groups.
- Ground/terrain does not render as solid bright yellow, magenta, pink, or untextured.
- Existing gameplay systems still work: movement, combat, bots, loot, inventory, vehicles, zone, aircraft/parachute flow, match flow, HUD, pause/settings, victory, defeat, and local stats.
