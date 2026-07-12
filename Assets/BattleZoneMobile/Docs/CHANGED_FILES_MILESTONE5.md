# BattleZone Mobile - Milestone 5 Changed Files

## Core Runtime

- `Assets/BattleZoneMobile/Scripts/Core/BattleZoneRuntimeBuilder.cs`
  - Added Milestone 5 map polish props, original low-poly weapon models, weapon model rig wiring, improved mobile joystick setup, static world block flags, vehicle visual wiring, and Milestone 5 menu text.
- `Assets/BattleZoneMobile/Scripts/Core/ThirdPersonMobileController.cs`
  - Added coyote time, jump buffering, variable jump cut, air acceleration, and more forgiving mobile jump handling.
- `Assets/BattleZoneMobile/Scripts/Core/AndroidPerformanceSetup.cs`
  - Added conservative runtime quality defaults and low-memory cleanup hook for Android compatibility.

## Combat

- `Assets/BattleZoneMobile/Scripts/Combat/WeaponController.cs`
  - Added touch-friendly aim assist, recoil heat/recovery, sustained-fire spread bloom, helmet-aware headshot mitigation retained, and runtime weapon model switching.
- `Assets/BattleZoneMobile/Scripts/Combat/WeaponModelRig.cs`
  - New original weapon model rig that toggles low-poly Pistol, Assault Rifle, SMG, Sniper, and Shotgun models.

## Mobile Controls

- `Assets/BattleZoneMobile/Scripts/MobileControls/FloatingJoystick.cs`
  - Added dead zone and output curve support for smoother mobile movement input.

## Vehicles

- `Assets/BattleZoneMobile/Scripts/Vehicles/VehicleController.cs`
  - Added smoother steering, visible wheel spin, body lean, and visual configuration hooks.

## AI

- `Assets/BattleZoneMobile/Scripts/AI/BotAI.cs`
  - Added combat memory, last-known-position investigation, improved gunshot reaction, prone-aware aiming, and cover scoring that prefers positions safer from player line-of-sight.

## Tests And Docs

- `Assets/BattleZoneMobile/Editor/BattleZoneRuntimeSmokeTest.cs`
  - Added Milestone 5 checks for original weapon models, weapon rig, joystick, and new environment props.
- `README.md`
- `Assets/BattleZoneMobile/Docs/README.md`
- `Assets/BattleZoneMobile/Docs/TEST_CHECKLIST.md`
- `Assets/BattleZoneMobile/Docs/CHANGED_FILES_MILESTONE5.md`

## Remaining Limitations

- This is still a single-player playable alpha foundation using original low-poly placeholder primitives and procedural audio, not final shipped content.
- Vehicles are improved but still use lightweight arcade transform movement, not production wheel physics, suspension, or network prediction.
- Aim assist is intentionally subtle and local; it is not a final competitive tuning pass.
- Bot intelligence has better memory and cover scoring, but it is not a production behavior tree or squad tactics system.
- Knockdown/revive remains a local foundation/hook without full squad or multiplayer implementation.
- Runtime steering is used with optional `NavMeshAgent` support if a NavMesh is later baked; the generated scene does not include a production baked NavMesh.
- Runtime-generated static flags, culling distances, LOD groups, and pooling help, but Android device performance has not been tested. Do not claim stable FPS or thermal behavior until a real device build is profiled.
