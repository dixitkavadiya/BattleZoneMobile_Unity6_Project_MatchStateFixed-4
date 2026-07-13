# BattleZone Mobile Changelog

## Milestone 24D - Loot, Inventory and Attachments

- Added `InventoryItemData` and `WeaponAttachmentData` ScriptableObject data models for runtime loot and attachment references.
- Expanded ground loot to support weapons, ammo, tiered backpacks, tiered helmets, tiered vests, medkits, bandages, energy items, grenades, smoke grenades, and weapon attachments.
- Hardened pickup consumption so items are not duplicated and are not removed from the ground when inventory rejects them.
- Added backpack tiers with capacity limits and capacity-aware item pickup.
- Added timed medkit, bandage, and energy item use with progress HUD feedback, duplicate-use prevention, health caps, and configurable movement cancellation rules.
- Added helmet and vest tier replacement rules, helmet durability loss on headshots, and vest durability display through the existing armor value.
- Added data-driven attachment compatibility and modifiers for optic, muzzle, magazine, grip, stock, and laser slots.
- Added attachment application to the current compatible runtime weapon, including recoil, spread, magazine, reload, fire-rate, and suppressed-fire audio modifiers.
- Added desktop inventory controls: `I` opens inventory, `Escape` closes it, and right click uses/equips while inventory is open.
- Added mobile inventory actions: `USE`, `EQUIP`, and `DROP` buttons in the original inventory screen.
- Added `M24D Loot Inventory Test Area` with labeled pickups for all backpack tiers, armor tiers, healing items, ammo types, throwables, and attachment types.
- Preserved the stable movement, joystick, Animator placement, aircraft/freefall/landing, red overlay, pickup, recoil, reload, hit feedback, weapon switching, and backpack pickup paths.

## Milestone 24C - Gun Feel and Combat Feedback

- Added per-weapon camera recoil values for vertical kick, horizontal kick, and recovery while preserving the existing `WeaponController` fire/reload architecture.
- Added visual weapon recoil through `WeaponModelRig.ApplyWeaponKick`, keeping all recoil on the weapon visual rig and never moving the Player root.
- Added `GunFeelFeedbackController` to drive crosshair bloom from movement, firing, aiming, crouch, prone, and active weapon data.
- Added optional weapon data hooks for damage number visibility, fire/reload/dry-fire/hit/headshot/kill audio overrides, aim sway, scoped breathing sway, and crosshair tuning.
- Improved hit feedback with body-zone damage multipliers, headshot detection through `CombatHitbox`, headshot marker, kill confirmation feed, and optional damage numbers.
- Added surface-aware impact feedback for Metal, Wood, Stone, Glass, and ground-like surfaces with colored impact particles and generated temporary impact sounds.
- Added generated placeholder audio hooks for dry fire, hit confirm, headshot confirm, kill confirm, suppressed fire, and surface impacts.
- Added `M24C Gun Feel Test Area` in Editor/development builds with close, medium, and long hit-zone targets plus a recoil spray wall and surface panels.
- Preserved stable movement, joystick, pickup, fire, reload, weapon switching, inventory, aircraft/freefall/landing flow, Animator placement, and red overlay behavior.

## Pickup Controls Hotfix

- Added direct Editor/Desktop pickup controls: `E` picks up the highlighted item and `F` works as a fallback.
- Made the visible pickup prompt clickable with the left mouse button, so clicking `Press E to pick up` picks up the highlighted item.
- Added a visible `PICKUP` HUD button that appears whenever a pickup is in range and calls the same focused-pickup path for mobile and touch testing.
- Updated prompt copy to show `Press E to pick up` on desktop/editor and `Tap PICKUP` on mobile.
- Hardened pickup execution by disabling the picked object's colliders and GameObject immediately before inventory handoff, preventing duplicate pickup calls in the same frame.
- Preserved movement, fire, reload, camera, Animator placement, aircraft/freefall/landing flow, damage overlay fixes, and previous weapon test area behavior.

## Milestone 24B - Live Weapon Test Area and Pickups

- Added `AdvancedWeaponPickup` so Milestone 24B `AdvancedWeaponData` weapons can exist as live ground loot with visible labels, collider support, pickup prompts, inventory handoff, and safe slot replacement.
- Added `M24B Live Weapon Test Area`, positioned near the runtime PlayerSpawn area, with all 10 Milestone 24B weapons in two rows: VXR-56, ARK-74, Sentinel AR, Pulse-9, Raptor-45, Longshot DMR, Falcon SR, Breacher-12, Sidearm P9, and Hammer .50.
- Added matching ammo pickups beside every test weapon and added static hit-zone targets with Head, Chest, Arm, and Leg `CombatHitbox` parts.
- Added surface test panels for Metal, Wood, Stone, and Glass impact validation.
- Added editor repair command `BattleZone Tools > Build Milestone 24B Weapon Test Area`; it opens `BZ_Main`, rebuilds/repairs the test area without duplicates, and saves the scene.
- Added runtime fallback in Editor/development builds so `BZ_Main` creates the test area once if it is missing at startup.
- Updated pickup interaction to support both existing `LootItem` pickups and new `AdvancedWeaponPickup` world weapons without changing movement, camera, aircraft/drop/landing, Animator placement, or the damage overlay fixes.
- Runtime and Editor C# compile checks completed with Unity 6.5 Roslyn references with zero errors and zero warnings.

## Milestone 24B - Original Weapon Set

- Added 10 original weapon `AdvancedWeaponData` assets under `Assets/BattleZoneMobile/Resources/WeaponData`: VXR-56, ARK-74, Sentinel AR, Pulse-9, Raptor-45, Longshot DMR, Falcon SR, Breacher-12, Sidearm P9, and Hammer .50.
- Extended the Milestone 24A data contract with fire modes, loadout slots, rarity, effective range, max range, tactical/empty reload timings, equip time, bolt/pump cooldowns, VFX references, visual prefab references, icon placeholder data, and attachment slot compatibility.
- Added fire mode support for Semi-auto, Full-auto, Burst, Bolt-action, and Pump-action, including a `SwitchFireMode` hook.
- Added `ModularWeaponLoadout` for primary, secondary, and pistol slots with equip, unequip, weapon switching, empty-hands fallback, pickup replacement rules, ammo validation, reload guarding, and movement-safe control locking.
- Added `CombatWeaponRoster` resource loading for the original weapon data assets.
- Added `WeaponVisualPlaceholder` and `WeaponVisualPlaceholderFactory` for temporary original weapon views with muzzle, shell-ejection, hand-grip, optic, muzzle, magazine, grip, pickup collider, equipped-view, and world-pickup hooks.
- Added the first runtime `M24B Weapon Test Area` scaffold with all 10 weapon placeholder views, ammo pickups, static hit-zone targets, and Metal/Wood/Stone/Glass surface panels.
- Added `UIManager.SetAdvancedWeaponHud` so the modular loadout can display weapon name, icon placeholder text, ammo, reserve ammo, fire mode, reload state, and current slot without redesigning the existing HUD.
- Kept the stable live-match `WeaponController` as the active gameplay input path for this checkpoint to avoid duplicate shots or regressions in movement, joystick, camera, aircraft/drop/landing, inventory, and HUD.
- Added `WEAPON_BALANCE.md` with the full balance table, intended roles, fire modes, attachment slots, and placeholder limitations.
- Runtime C# compile check completed with Unity 6.5 Roslyn references with zero errors and zero warnings.

## Milestone 24A - AAA Combat Foundation

- Added a data-driven combat foundation through `AdvancedWeaponData` ScriptableObjects for damage, fire rate, reload time, magazine size, range, bullet speed, recoil, spread, attachments, modular audio, animation hooks, body-zone multipliers, and surface impact profiles.
- Added shared combat enums for weapon delivery, ammo class, hit zones, surface types, and direct combat damage sources.
- Added `ModularWeaponBase` plus concrete weapon components for Assault Rifle, SMG, Sniper, DMR, Shotgun, Pistol, Melee, and Throwable weapons.
- Added switch-ready raycast, projectile, melee, and throwable delivery support without replacing the stable active `WeaponController`.
- Added pooled `CombatProjectile` support with between-frame raycast collision, owner-collider filtering, hit damage, and surface impact routing.
- Added `CombatHitbox` body-zone metadata and tagged the current visual child body parts for Head, Chest, Arm, and Leg detection.
- Added `CombatSurface` and `CombatImpactUtility` for Metal, Stone, Wood, Glass, Sand, Water, Grass, and fallback impact resolution.
- Added `CombatRecoilApplicator` with separate camera recoil, weapon recoil, and crosshair recoil channels.
- Added `CombatAnimationEventBridge` for future animation events while preserving visual-child Animator ownership and root-motion-off behavior.
- Added a default-off `CombatDebugWindow` that can show legacy weapon, modular weapon, delivery, ammo, recoil bloom, and health diagnostics.
- Preserved current stable movement, joystick, camera, Animator placement, aircraft/freefall/parachute/landing ownership, damage overlay behavior, and active match flow.
- Runtime C# compile check completed with Unity 6.5 Roslyn references with zero errors and zero warnings.

## Milestone 23A - Character Animation Polish

- Kept the stable ownership architecture intact: Player root movement remains controlled by `ReliablePlayerMovement` during Combat and `BattleRoyaleMatchFlow` during aircraft/drop/landing phases.
- Kept Unity `Animator` on visual child `LowPolyOriginalHumanoid` only, with `Apply Root Motion` disabled.
- Expanded `AC_PlayerHumanoid` parameters with `VerticalVelocity`, `Sprinting`, `Prone`, `Aiming`, `Falling`, `Fire`, `Reload`, and `Landing`.
- Softened existing visual-child AnimatorController transition durations for smoother Idle/Walk/Run/Jump/Crouch state changes.
- Hardened `ThirdPersonMobileController` Animator writes through cached parameter checks so missing/changed Animator parameters cannot spam Console warnings.
- Added optional inspector toggle `Show Animator Debug`, default off, for visual-child Animator diagnostics.
- Routed fire, reload, weapon switch, and landing animation events through `ThirdPersonMobileController` so the Unity Animator parameters and procedural child poses stay synchronized.
- Improved child-only procedural pose blending for smoother idle, walk, sprint, jump, falling, landing, crouch, prone, aim, fire, and reload animation timing.
- Reduced visible foot sliding by tying gait cycle and limb swing to smoothed actual movement speed.

## Stable Post-Recovery Gameplay Checkpoint

- Created a stable checkpoint after the movement, Animator root override, red damage overlay, and startup drop gravity ownership recoveries.
- Kept the working movement system intact; no redesign or replacement of `ReliablePlayerMovement`, `ThirdPersonMobileController`, joystick, camera, Animator placement, or damage overlay logic.
- Disabled the large `ReliablePlayerMovement` debug overlay by default and exposed it through the inspector toggle `Show Debug Overlay`.
- Fixed the `BotManager.explicitSpawnPoints` compiler warning by initializing the serialized array.
- Added safe bot spawn-point discovery when explicit bot spawn points are empty, while retaining the existing NavMesh and ground-sampling fallback.
- Ran static regression audit for Player count, CharacterController ownership, movement/pose ownership, visual-child Animator placement, root motion, and direct-damage-only `DamageFlash`.
- Verified runtime scripts compile with Unity 6.5 Roslyn references with zero errors and zero warnings.
- Documented that live Unity Play Mode verification remains blocked in this environment by Unity licensing startup timeout.

## Match State Fixed

- Continued from the current MovementRecovered project without rebuilding from the broken Milestone 22 project.
- Fixed match-state defeat handling so the match cannot conclude as defeat while the local player `Health` component still reports alive.
- Added explicit local-player registration and validated alive-player counting.
- Hardened `StartMatch` and `RestartMatch` to restore `Time.timeScale`, enable `ThirdPersonMobileController`, enable `CharacterController`, clear external motion locks, unlock movement input, and reset player/bot/timer state.
- Guarded victory so invalid bot roster state or bot despawn cannot end the match unless expected bots were eliminated through the normal bot death notification path.
- Added an Editor-only match debug line showing match state, local alive state, alive count, controls enabled, and time scale.
- Updated README, ROOT_CAUSE, and TEST_CHECKLIST for the match-state fix.

## Milestone 21 - First Playable Original Production Island

- Continued from the current latest project without rebuilding from an older version or removing working gameplay systems.
- Added `Milestone21 Unity Terrain - First Playable Island` with generated height shaping, grass/dirt/asphalt/rock/sand terrain layers, splat maps, terrain grass detail, URP Terrain/Lit material assignment, and terrain collision.
- Added original island locations: River Village, Warehouse District, Shipping Yard, Military Checkpoint, Coast Gas Station, Island School, Small Hospital, Pine Forest, and North Hills.
- Added original playable environment pieces: detailed village houses, warehouse district, shipping container yard, military checkpoint, gas station, school, hospital, river, sand banks, two bridges, roads, fences, watch towers, windows, doors, simple interiors, rooftop-access routes, cover points, rocks, trees, bushes, and grass patches.
- Added M21 loot anchors and increased loot cluster bias around the new village, warehouse, container yard, military, gas station, school, and hospital POIs.
- Added runtime NavMesh generation for the new island collision layer so existing bots can navigate the first playable island foundation.
- Added M21 reflection probe coverage while preserving URP lighting, fog, shadows, weather, post-processing, quality presets, mobile controls, HUD, inventory, weapons, vehicles, bots, zone, aircraft/drop flow, minimap, and match flow.
- Expanded editor smoke-test validation for M21 terrain layers, required POIs, bridges, loot anchors, and non-empty runtime NavMesh.
- Documented that Milestone 21 remains original procedural proxy content and still needs professional authored art, baked lighting, baked occlusion, and real Android profiling.

## Milestone 20 - Realistic Original World Foundation

- Continued from the current latest project without rebuilding from an older version or removing working gameplay systems.
- Added a Unity Terrain System world foundation with generated heightmap, terrain layers, splat maps, terrain grass detail, and URP Terrain/Lit material assignment.
- Added original major world areas: Riverbend Village, Aster Town, North Military Base, Rail Warehouse, Shipping Yard, Hillside Gas Station, Pine River Forest, and Ridge Hills.
- Added original runtime environment assets: detailed village houses, town blocks, military command/barracks, warehouse/yard, shipping containers, gas station, roads, bridges, river water, sand banks, trees, grass patches, and rocks.
- Improved the URP visual foundation with HDR-style procedural sky, mixed sun light, post-processing, reflection probes, light-probe grid, soft shadows, and mobile-friendly material settings.
- Added `RuntimeDynamicWeatherController` for day/night sun movement, dynamic fog, moving cloud root, and rain particles.
- Added mobile optimization hooks: terrain instancing, GPU-instanced materials, LOD groups, static occlusion-friendly cell markers, constrained detail density, and disabled realtime reflection probes.
- Preserved match flow, aircraft/drop/parachute, bots, loot, vehicles, weapons, armor, inventory, zone, minimap, HUD, audio, controls, and VisualSlice replacement architecture.
- Documented that Milestone 20 is a stronger original visual foundation, not final AAA production art or baked-lightmap/occlusion output.

## Milestone 19 - Full Drop Experience

- Continued from the current latest project without rebuilding from an older version or removing working gameplay systems.
- Upgraded the post-aircraft drop with velocity-based free-fall, steering acceleration, wind drift, terminal speed limits, manual parachute request, automatic parachute deployment, parachute damping, and landing recovery.
- Added drop-specific player camera modes for free-fall and parachute phases while preserving right-side camera look input during externally controlled drop motion.
- Extended the procedural humanoid animation layer with skydiving, parachute, and landing poses.
- Expanded the original runtime world with `Milestone19 Drop Experience Terrain Root`, including village, forest, river, hills, roads, bridge, container yard, and military camp landmarks.
- Added Milestone 19 loot cluster bias around the new village, forest, container yard, and military camp while keeping full-map random loot spawning.
- Improved bot parachute descent, landing NavMesh handoff, and stuck recovery behavior.
- Enhanced the original parachute proxy visual with lightweight canopy panels and stabilizer cords.
- Preserved waiting lobby, match countdown, randomized flight direction, combat flow, bots, loot, vehicles, inventory, armor, weapons, UI, minimap, zone, audio, and Android touch controls.

## Milestone 18 - Complete Battle Royale Match Flow

- Continued from the latest stable project without rebuilding or removing working gameplay systems.
- Added a `Milestone18 Complete Battle Royale Match Flow` runtime marker and smoke-test validation.
- Improved the match opening sequence with clearer waiting lobby state, countdown phase telemetry, randomized cross-map aircraft route generation, jump prompt, auto-jump window, free-fall phase, parachute phase, and landing recovery.
- Added minimap flight-path visualization through `MiniMapFlightPath`, plus persistent `MatchPhaseText` and `FlightPathText` HUD labels.
- Preserved authored route fallbacks while making most matches use procedural flight directions within the existing large map bounds.
- Added per-match safe-zone preparation with randomized mobile-friendly center placement while preserving multi-phase shrinking and escalating outside-zone damage.
- Improved loot spawning with full-map distribution plus cluster bias around major original map locations and the M17 vertical-slice area.
- Increased local bot count to 8 and improved bot parachute drop spacing so bots distribute along and beside the flight path.
- Added short weapon fire input buffering to make mobile tap firing feel more responsive.
- Added landing recovery feedback through the existing player controller and humanoid placeholder animation path.
- Preserved player movement, camera, bots, vehicles, weapons, loot, armor, inventory, zone, UI, audio, VisualSlice architecture, and M17 vertical-slice district.

## Milestone 17 - Vertical Slice

- Continued from the latest stable project without rebuilding from an older version or removing working gameplay systems.
- Added a focused original `M17 Vertical Slice District` around 200m x 200m inside `BZ_Main`.
- Added URP-compatible runtime materials for M17 grass, dirt, sand, road, sidewalk, river water, apartment, shop, warehouse, street lamp, and barrel surfaces.
- Added an original playable street block with roads, sidewalks, lane marks, river edge, grass, dirt yard, sand bank, fences, street lights, electric poles, trees, bushes, rocks, barrels, crates, and benches.
- Added enterable original apartment, warehouse, shop, and guard tower structures with simple interiors, cover points, loot/art sockets, replacement slots, and LOD groups.
- Added an artist-ready tactical character prefab slot with sockets for future professional character replacement while preserving the current gameplay character.
- Added a Milestone 17 reflection probe and named location for minimap/location readability.
- Expanded smoke-test validation to check Milestone 17 required objects, replacement slots, LOD groups, reflection probe, assigned materials, and non-primitive generated meshes.
- Preserved existing match flow, bots, loot, vehicles, armor, inventory, weapons, zone, UI, audio, visual-slice art pipeline, and Android touch controls.

## VisualSlice - Art Pipeline and Visual Quality Plan

- Continued from the latest stable project without removing working gameplay systems or adding major gameplay features.
- Added `ART_SPEC.md`, `ASSET_REPLACEMENT_GUIDE.md`, `PERFORMANCE_BUDGET.md`, and `PLACEHOLDERS_TO_REPLACE.md`.
- Added ScriptableObject-driven visual replacement architecture: `BattleZoneVisualAssetDefinition`, `BattleZoneVisualCatalog`, `BattleZoneArtReplacementSlot`, and `BattleZoneArtSocket`.
- Generated art-pipeline material palette, prefab replacement slots, visual definitions, and catalog under `Assets/BattleZoneMobile/ArtPipeline`.
- Added a labeled original visual-slice area in `BZ_Main` with street block, enterable house, warehouse, terrain/road/sidewalk/grass/tree/rock/prop details, tactical humanoid showcase, and two original weapon showcases: Rook-17 assault rifle and Sable-9 pistol.
- Added `Low`, `Medium`, `High`, and `Ultra` URP quality presets.
- Expanded smoke-test validation for visual-slice roots, replacement slots, LOD groups, visual catalog definitions, quality preset names, and non-primitive visual-slice meshes.
- Clearly documented remaining placeholder content and did not claim production-ready or AAA visuals.

## Milestone 14 - Terrain/URP Stability Hotfix

- Continued from the current latest Milestone 13 project without rebuilding from an older version or removing working systems.
- Fixed the Unity 6.5 bright-yellow ground/terrain issue by hardening runtime material creation to prefer URP-compatible shaders.
- Rebalanced generated grass, dirt, sand, rock, road, crop, coast, river, and lake colors to avoid diagnostic yellow, magenta, pink, and washed-out surfaces.
- Improved opaque and transparent runtime material setup for URP, including water, zone, cloud, particle, tracer, flight-path, and shell-casing materials.
- Confirmed the project uses the `BattleZoneMobile_URP` pipeline asset through Graphics and Quality settings.
- Expanded the editor smoke test to validate terrain/road/water surfaces, reject non-URP terrain shaders, reject bright-yellow terrain diagnostic colors, and keep existing missing-material/missing-script/error-shader checks.
- Preserved Milestone 13 match flow, vehicles, bots, loot, zone, armor, inventory, weapons, UI, audio, and Android touch controls.

## Milestone 13 - Complete Battle Royale Match Flow

- Continued from the current latest stable project without rebuilding from an older version or removing working systems.
- Added a complete local match phase loop: waiting area, countdown, aircraft route, manual/automatic jump, free-fall, parachute, landing, combat unlock, zone play, victory/defeat, and match summary.
- Added a visible flight-path preview and original runtime transport aircraft/parachute flow.
- Added bot route distribution and varied bot parachute drops, with combat locked until match start.
- Added match stat tracking for kills, damage, survival time, placement, shots, hits, and accuracy.
- Added local PlayerPrefs match history for total matches, wins, total kills, best placement, and recent result summary.
- Added backpack loot, loot rarity labels, and safer full-map loot placement retries.
- Added next-zone radius, minimap next-zone ring, zone phase display, and safe-zone helper points for bots.
- Preserved vehicles, weapons, armor, inventory, AI combat behavior, HUD, minimap, settings, and Android touch controls.
- Expanded smoke-test validation for Milestone 13 waiting area, flight path, next-zone, backpack loot, and match-flow objects.

## Milestone 12 - Gameplay Polish

- Continued from the current project without rebuilding or removing working gameplay systems.
- Tuned player movement responsiveness with joystick dead-zone shaping, smoother acceleration/deceleration, sprint acceleration, jump forward boost, improved landing retention, and smoothed animation speed to reduce visible foot sliding.
- Preserved camera-relative movement, shoulder switching, camera collision, ADS zoom, and separate camera/ADS/scope sensitivity while improving collision safety and minimap/compass yaw smoothing.
- Improved weapon feel with tighter ADS recoil, faster recoil heat recovery, switch fire lockout, earlier reload commit timing, brighter pooled bullet tracers, varied shell ejection, and lightweight weapon rig switch/sway animation.
- Improved bot behavior with reaction delays, player velocity prediction, cover-break decisions, longer heal-cover timing, NavMesh destination refresh cadence, and stuck detection for agent movement.
- Improved vehicles with distinct braking/reverse behavior, braking turn assist, coast drag, bump response on collision, reduced collision damage spikes, and safer bot vehicle steering.
- Improved HUD feedback with low/empty ammo color, danger-zone color, cleaner backpack capacity header, and per-frame compass allocation cleanup.

## Milestone 12 - Professional Visual and Gameplay Upgrade

- Continued from the current Milestone 11 project without removing existing gameplay systems.
- Reworked the player into a more professional original low-poly tactical humanoid with improved proportions, armor/vest detail, helmet rails, guards, pouches, backpack bedroll, gloves, and boots.
- Improved procedural animation blending for idle, walk, run, sprint, jump, crouch, prone, aim, fire, reload, hit reaction, weapon switching, and death.
- Added player hit-reaction animation through the existing `Health.onDamageTaken` event.
- Added separate scope sensitivity alongside camera and ADS sensitivity.
- Preserved camera collision, ADS zoom, shoulder switching, smooth follow, recoil response, mobile controls, and keyboard fallback.
- Added surface-aware footsteps for grass, road/dirt/bridge, building/interior, and water.
- Added generated UI click and zone warning audio.
- Added a town ambient loop while keeping wind, river, forest, and base ambient loops.
- Added Milestone 12 terrain/world polish: grass/dirt/rock/sand material blends, river foam, bridge cable rails, road cones, utility poles, road signs, crates, building detail props, roof details, and layered cloud props.
- Improved original low-poly weapons with sights, rails, ejection ports, vents, scope lenses, sling loops, pistol slide/trigger guard, sniper shade, and shotgun tube details.
- Improved HUD readability with tactical status and weapon panels, segmented crosshair marks, cleaner settings layout, and updated match summary label.
- Expanded smoke-test validation for Milestone 12 objects and controls.

## Verification

- Unity `6000.5.3f1` batch smoke test passed with `outputs/unity_m21_final_smoke.log`.
- No red BattleZone project Console errors were reported by the smoke run.
- Known prior issues remain fixed: no legacy `StandaloneInputModule`, no `Arial.ttf` built-in font reference, no old `Submit` input error, no particle duration error, and no missing/error shaders detected by smoke test.

## Placeholder Assets Still Remaining

- Runtime-generated low-poly primitive art remains a placeholder for final authored meshes.
- Procedural transform animation remains a placeholder for final authored humanoid animation clips.
- Procedural temporary audio remains a placeholder for final recorded or designed audio assets.
- Generated terrain/world layout remains a prototype foundation rather than final terrain art.
