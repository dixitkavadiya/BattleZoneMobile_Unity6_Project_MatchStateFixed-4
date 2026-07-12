# BattleZone Mobile - Milestone 4 Changed Files

## Core Runtime

- `Assets/BattleZoneMobile/Scripts/Core/BattleZoneRuntimeBuilder.cs`
  - Expanded the runtime-generated world, added Milestone 4 named locations, roads, coastal/industrial/military/forest/town areas, vehicle spawn points, vehicle prefabs, plane visual, parachute visual, airdrop flow, new HUD controls, and match-flow wiring.
- `Assets/BattleZoneMobile/Scripts/Core/ThirdPersonMobileController.cs`
  - Added prone movement/state, vehicle enter/exit mode, vehicle-seat pose handling, and keyboard fallback for prone.
- `Assets/BattleZoneMobile/Scripts/Core/HumanoidPlaceholderAnimator.cs`
  - Added prone-friendly placeholder pose blending.
- `Assets/BattleZoneMobile/Scripts/Core/GameManager.cs`
  - Added coroutine-based match start flow, opening drop-sequence hookup, match announcements, and bot gunshot broadcast wiring.

## Vehicles

- `Assets/BattleZoneMobile/Scripts/Vehicles/VehicleController.cs`
  - New original arcade vehicle controller for jeep, buggy, motorcycle, and coastal jeep with enter/exit, mobile gas/brake/steer input, fuel, health, damage, status HUD, bot-drive hook, and lightweight ground snapping.
- `Assets/BattleZoneMobile/Scripts/Vehicles/VehicleInteractor.cs`
  - New player-side vehicle prompt and DRIVE/EXIT interaction.

## Battle Royale Flow

- `Assets/BattleZoneMobile/Scripts/Match/BattleRoyaleMatchFlow.cs`
  - New randomized plane route, countdown, parachute drop, player drop placement, airdrop crate, and airdrop loot spawning foundation.
- `Assets/BattleZoneMobile/Scripts/Zone/SafeZoneController.cs`
  - Added runtime zone scaling and blue-zone damage phases.

## Player, Inventory, And Combat

- `Assets/BattleZoneMobile/Scripts/Player/PlayerEquipment.cs`
  - New armor-tier, helmet-tier, armor restoration, and headshot mitigation system.
- `Assets/BattleZoneMobile/Scripts/Player/KnockdownReviveState.cs`
  - New local knockdown/revive foundation for future squad or multiplayer work.
- `Assets/BattleZoneMobile/Scripts/Combat/Health.cs`
  - Added revive-with-values support.
- `Assets/BattleZoneMobile/Scripts/Combat/WeaponController.cs`
  - Added controls-enable gate for driving and helmet-aware headshot mitigation.
- `Assets/BattleZoneMobile/Scripts/Loot/LootItem.cs`
  - Added armor plate, armor vest, and helmet loot kinds.
- `Assets/BattleZoneMobile/Scripts/Loot/PlayerInventory.cs`
  - Added equipment summary, armor plate pickup/restoration, armor vest pickup, and helmet pickup behavior.

## AI

- `Assets/BattleZoneMobile/Scripts/AI/BotAI.cs`
  - Added gunshot reaction, nearby loot pickup, simple vehicle-use behavior, and revive-foundation scanning.
- `Assets/BattleZoneMobile/Scripts/AI/BotManager.cs`
  - Added gunshot broadcast to active bots.

## UI And Tests

- `Assets/BattleZoneMobile/Scripts/UI/UIManager.cs`
  - Added match announcement display.
- `Assets/BattleZoneMobile/Editor/BattleZoneRuntimeSmokeTest.cs`
  - Added Milestone 4 smoke checks for vehicle controls, vehicle systems, player equipment, match flow, expanded map landmarks, and inactive runtime visuals.

## Documentation

- `README.md`
- `Assets/BattleZoneMobile/Docs/README.md`
- `Assets/BattleZoneMobile/Docs/TEST_CHECKLIST.md`
- `Assets/BattleZoneMobile/Docs/CHANGED_FILES_MILESTONE4.md`

## Remaining Limitations

- This is a single-player playable alpha foundation using original low-poly placeholder primitives and procedural audio, not final shipped content.
- Vehicles use lightweight arcade transform movement, not production wheel physics, suspension, or network prediction.
- Plane, parachute, and airdrop are functional foundations with placeholder visuals, not a full cinematic drop system.
- Knockdown/revive and bot teammate revival are local foundations/hooks. There is no full squad, multiplayer, or networking implementation yet.
- Bots can react to gunshots, loot nearby items, and use a simple vehicle behavior, but they do not yet have a production behavior tree, baked NavMesh, or advanced squad tactics.
- Runtime steering is used with optional `NavMeshAgent` support if a NavMesh is later baked; the generated scene does not include a production baked NavMesh.
- Occlusion culling is documented as an optimization target; this runtime-generated scene uses camera culling distances and LOD groups, not baked occlusion data.
- Android compatibility settings are preserved, but Android device performance has not been tested in this pass. Do not claim stable FPS or thermal performance until a real device build is profiled.
