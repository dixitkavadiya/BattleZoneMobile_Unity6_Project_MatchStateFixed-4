# BattleZone Mobile - Regression Report

## Stable Post-Recovery Gameplay Checkpoint - 2026-07-12

Checkpoint commit target:

- `Create stable post-recovery gameplay checkpoint`

Scope:

- Current project only: `BattleZoneMobile_Unity6_Project_MatchStateFixed-4`.
- No redesign or replacement of the working movement system.
- No changes to the recovered Animator placement, red overlay behavior, or startup drop gravity ownership.

Regression audit results:

- PASS: Static audit found one runtime `Player` creation path in `BattleZoneRuntimeBuilder.BuildPlayer`.
- PASS: Static audit found one `CharacterController` added to the Player root.
- PASS: Static audit found `ReliablePlayerMovement` and `ThirdPersonMobileController` both attached to the Player root, with `ThirdPersonMobileController` ground locomotion bypassed by `ReliablePlayerMovement`.
- PASS: `ReliablePlayerMovement` owns Combat ground movement.
- PASS: `BattleRoyaleMatchFlow.MatchFlowOwnsLocalPlayerPose` owns `Lobby`, `Waiting`, `Aircraft`, `Freefall`, `Parachute`, and `Landing` pose control until Combat.
- PASS: Static audit found Unity `Animator` creation only on `LowPolyOriginalHumanoid`, the visual child.
- PASS: Visual child `Animator.applyRootMotion` is explicitly false.
- PASS: `DamageFlash` remains inactive by default and direct-damage-only; safe-zone damage does not trigger the full-screen flash.
- PASS: Large `ReliablePlayerMovement` debug overlay is disabled by default and is available through the inspector toggle `Show Debug Overlay`.
- PASS: `BotManager.explicitSpawnPoints` warning is fixed by initializing the serialized array and adding safe runtime spawn-point discovery when it is empty.
- PASS: C# runtime script compile check completed with zero errors and zero warnings.

Unity Editor verification:

- NOT RUN: Unity batch mode timed out while launching the Unity licensing client in this environment.
- Manual Play Mode verification is still required in Unity Hub after the license is active.

Remaining known workspace note:

- `ProjectSettings/GraphicsSettings.asset` was already modified before this checkpoint pass and was left untouched.

## Last Known Working Version

`outputs/BattleZoneMobile_Unity6_Project 21`

Reason:

- It is the newest earlier project folder before M22.
- Its `README.md` and `TEST_CHECKLIST.md` record a successful Unity `6000.5.3f1` runtime smoke test.
- Its movement-critical scripts are unchanged from M20:
  - `ThirdPersonMobileController.cs`
  - `FloatingJoystick.cs`
  - `MobileLookArea.cs`
  - `BattleRoyaleMatchFlow.cs`
  - `GameManager.cs`
  - `VehicleInteractor.cs`

## Broken Version Compared

`outputs/BattleZoneMobile_Unity6_Project 22`

There is no git commit history for these generated project folders, so the regression was identified by folder diff rather than commit hash.

## Exact Broken File/Change

Regression window: M21 -> M22.

The only changed runtime scripts in that window are:

- `Assets/BattleZoneMobile/Scripts/Core/BattleZoneRuntimeBuilder.cs`
- `Assets/BattleZoneMobile/Scripts/Vehicles/VehicleController.cs`
- `Assets/BattleZoneMobile/Scripts/AI/BotAI.cs`
- `Assets/BattleZoneMobile/Scripts/MobileControls/MobileSteeringWheel.cs` added

Movement-critical scripts themselves did not change between M21 and M22. That means the regression was caused by runtime wiring/state introduced around the movement system, not by a deliberate rewrite of locomotion.

The unsafe M22 movement-adjacent changes were:

- `BattleZoneRuntimeBuilder.cs` added always-active M22 vehicle steering controls to the in-game HUD.
- `BattleZoneRuntimeBuilder.cs` wired M22 vehicles to shared player mobile input references.
- `VehicleController.cs` expanded vehicle/bot driving control paths using the same joystick/keyboard input environment.
- The project still lacked a movement-chain smoke test that verified joystick output reached `ThirdPersonMobileController`.
- The existing match opening sequence could set `SetExternalMotionLock(true)` and `ControlsEnabled = false` before the player reached combat, making a state/input wiring issue look like a permanently frozen player.

## Exact Fix Applied

Base recovery:

- Rebuilt the delivered project from M21, not M22.
- Did not carry forward the unsafe M22 steering-wheel/expanded vehicle-input changes.
- Kept M21 world, terrain, buildings, roads, bridges, interiors, loot anchors, runtime NavMesh, vehicles, bots, weapons, inventory, armor, match flow, aircraft/drop/parachute, HUD, URP, and Android settings.

Movement fixes:

- `ThirdPersonMobileController.cs`
  - Added one shared movement input path for keyboard and joystick.
  - Added direct WASD/arrow-key fallback for Editor testing.
  - Kept legacy axis fallback safely wrapped.
  - Ensured `Move()` always reads the shared final movement Vector2.
  - Ensured `CharacterController.Move()` remains the applied movement path.
  - Added runtime repair for missing joystick/look/camera references.
  - Re-enabled a disabled `CharacterController` when not in vehicle mode.
  - Clamped move speeds and gravity values so bad serialized data cannot create a zero-speed player.
  - Added Editor movement debug data.

- `FloatingJoystick.cs`
  - Added visual-reference validation.
  - Added Editor-only test injection so smoke tests can verify non-zero joystick output.

- `MobileLookArea.cs`
  - Added a raycast filter so right-side camera drag does not accept left-side joystick touches.

- `BattleZoneRuntimeBuilder.cs`
  - Ensured runtime EventSystem uses `InputSystemUIInputModule`.
  - Assigned Input System default UI actions when available.
  - Disabled transparent HUD background raycast blocking.
  - Connected `RuntimeDeveloperPanel` to the actual runtime player controller.

- `BattleRoyaleMatchFlow.cs`
  - Allows movement during waiting lobby while combat remains locked.
  - Keeps aircraft/drop/parachute external movement behavior.
  - Restores normal controls after landing.

- `RuntimeDeveloperPanel.cs`
  - Shows keyboard input, joystick input, final movement input, move speed, grounded state, controls state, and time scale in Editor only.

- `BattleZoneRuntimeSmokeTest.cs`
  - Added movement-chain validation for runtime player count, enabled movement script, enabled `CharacterController`, joystick output, controller final input, move speed, EventSystem, and nonzero `Time.timeScale`.

## Safe M21/M22 Reapply Decision

Safe retained changes:

- Milestone 21 island/world systems and all earlier working gameplay systems.
- Existing pre-M22 vehicle enter/exit support.
- Movement diagnostics and input hardening.

Unsafe skipped changes:

- M22 steering-wheel UI.
- M22 expanded vehicle-input wiring.
- M22 bot-driving additions.
- M22 additional vehicle fleet.

Those M22 changes are not included in this recovery package because they are inside the exact regression window and are not required to restore movement.

## Remaining Limitation

Unity Editor smoke testing could not be completed in this environment because Unity licensing timed out before project import. Once Unity Hub/license is active, run `TEST_CHECKLIST.md` in the Editor.
