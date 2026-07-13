# BattleZone Mobile - Regression Report

## Milestone 24C Gun Feel and Combat Feedback - 2026-07-13

Scope:

- Current project only: `BattleZoneMobile_Unity6_Project_MatchStateFixed-4`.
- Improve gun feel and feedback without changing the existing active weapon architecture or rebuilding movement, pickup, aircraft/drop, Animator, or match-flow systems.

Fix applied:

- Extended `WeaponDefinition` with per-weapon camera recoil, visual weapon kick, crosshair feedback, aim sway, scoped breathing sway, damage-number toggle, and optional audio override hooks.
- Updated `WeaponController` to apply separate camera recoil, local weapon visual kick, body-zone damage multipliers, surface-aware impact feedback, dry-fire feedback, headshot feedback, and kill confirmation.
- Updated `WeaponModelRig` so recoil is applied as local visual offset/rotation and smoothly returns to the base pose.
- Added `GunFeelFeedbackController` to continuously drive crosshair bloom from movement, stance, aim, and active weapon data.
- Added generated temporary audio hooks in `RuntimeAudioBank` for suppressed fire, dry fire, hit confirm, headshot confirm, kill confirm, and material impacts.
- Added `M24C Gun Feel Test Area` with close, medium, and long body-zone targets plus a recoil spray wall and material panels.

Regression audit results:

- PASS: `ReliablePlayerMovement` was not modified.
- PASS: `LootPickupInteractor` and pickup controls were not modified.
- PASS: `BattleRoyaleMatchFlow` was not modified.
- PASS: Animator placement remains on visual child; no Player-root Animator was added.
- PASS: Damage overlay scripts were not modified.
- PASS: Existing fire, reload, weapon switching, and inventory pickup paths remain on `WeaponController`/`PlayerInventory`.
- PASS: Runtime C# scripts compile with Unity 6.5 Roslyn references with zero errors and zero warnings.

Unity Editor verification:

- NOT RUN: Live Unity Play Mode remains blocked in this environment by Unity Hub/license activation.
- Required manual gun-feel verification is listed in `TEST_CHECKLIST.md`.

## Pickup Controls Hotfix - 2026-07-13

Scope:

- Current project only: `BattleZoneMobile_Unity6_Project_MatchStateFixed-4`.
- Fix item pickup controls without changing movement, camera, combat, Animator placement, aircraft/freefall/landing, match flow, red overlay, or weapon definitions.

Root issue:

- Pickup prompts appeared correctly, but the visible prompt was only text and the interactor relied on world-raycast mouse/touch input. In the Unity Editor, clicking the UI prompt did not clearly execute pickup, and there was no direct keyboard pickup path.

Fix applied:

- Added `E` and `F` pickup keys in `LootPickupInteractor`.
- Added a clickable prompt button on the existing pickup prompt text.
- Added a visible runtime HUD `PickupButton` labeled `PICKUP` and wired it to the same focused-pickup action.
- Updated prompt text to show `Press E to pick up` on desktop/editor and `Tap PICKUP` on mobile.
- Disabled picked-object colliders and deactivated the pickup immediately before inventory handoff to prevent duplicate pickup calls.

Regression audit results:

- PASS: `ReliablePlayerMovement` was not modified.
- PASS: `ThirdPersonMobileController` was not modified.
- PASS: `BattleRoyaleMatchFlow` was not modified.
- PASS: Animator placement and root-motion behavior were not modified.
- PASS: Damage overlay scripts were not modified.
- PASS: Weapon fire/reload logic was not modified.
- PASS: Runtime C# scripts compile with Unity 6.5 Roslyn references with zero errors and zero warnings.

Unity Editor verification:

- NOT RUN: Live Unity Play Mode remains blocked in this environment by Unity Hub/license activation.
- Required manual pickup verification is listed in `TEST_CHECKLIST.md`.

## Milestone 24B Live Weapon Test Area and Pickups - 2026-07-13

Scope:

- Current project only: `BattleZoneMobile_Unity6_Project_MatchStateFixed-4`.
- Add the missing live world pickup test area for the existing Milestone 24B weapon definitions.
- Do not modify stable movement, joystick, camera, Animator placement, aircraft/freefall/landing, startup gravity ownership, or red overlay fixes.

Root issue:

- Milestone 24B weapon definitions and fire/reload code existed, but `BZ_Main` had no reliable live ground-pickup area where the 10 weapon definitions could be seen, approached, picked up, equipped, and tested.

Fix applied:

- Added `AdvancedWeaponPickup` for physical 24B weapon pickups with name labels and `AdvancedWeaponData` references.
- Updated `LootPickupInteractor` to support both existing `LootItem` pickups and new advanced weapon pickups through the same prompt/tap/focused-pickup path.
- Updated `PlayerInventory` so advanced weapon pickups equip into `ModularWeaponLoadout` and also unlock the compatible stable legacy `WeaponController` slot for immediate fire/reload testing.
- Added `Milestone24BWeaponTestAreaBuilder` to create `M24B Live Weapon Test Area` near PlayerSpawn with two rows of all 10 weapons, adjacent ammo pickups, body-zone targets, and surface panels.
- Added editor menu command `BattleZone Tools > Build Milestone 24B Weapon Test Area` to rebuild/repair the area and save `BZ_Main`.
- Added Editor/development-build runtime fallback so `BZ_Main` creates the area once if it is missing at startup.

Regression audit results:

- PASS: `ReliablePlayerMovement` was not modified.
- PASS: `ThirdPersonMobileController` was not modified.
- PASS: `BattleRoyaleMatchFlow` was not modified.
- PASS: Animator placement and root-motion behavior were not modified.
- PASS: Damage overlay scripts were not modified.
- PASS: Existing camera, aircraft/drop/landing, weapon fire/reload behavior, vehicle, bot, zone, and match-flow code paths were not redesigned.
- PASS: Runtime C# scripts compile with Unity 6.5 Roslyn references with zero errors and zero warnings.
- PASS: Editor C# scripts compile with Unity 6.5 Roslyn references with zero errors and zero warnings.

Unity Editor verification:

- NOT RUN: Live Unity Play Mode remains blocked in this environment by Unity Hub/license activation.
- Required manual pickup verification is listed in `TEST_CHECKLIST.md`.

## Milestone 24B Original Weapon Set - 2026-07-12

Scope:

- Current project only: `BattleZoneMobile_Unity6_Project_MatchStateFixed-4`.
- Built on Milestone 24A combat foundation.
- Original weapon roster and modular weapon-loadout foundation only; no rewrite of movement, joystick, camera, Animator ownership, aircraft/freefall/landing, damage overlay, existing inventory, or existing live-match HUD flow.

Regression audit results:

- PASS: `ReliablePlayerMovement` was not modified and remains Combat ground movement owner.
- PASS: `BattleRoyaleMatchFlow` was not modified and remains aircraft/freefall/parachute/landing pose owner.
- PASS: Animator placement was not changed; Unity `Animator` remains on visual child `LowPolyOriginalHumanoid` with root motion disabled.
- PASS: Red damage overlay code was not modified.
- PASS: Existing `WeaponController` remains the active live-match input path, preventing duplicate shots from one button press.
- PASS: `ModularWeaponLoadout` is initialized passively and does not disable movement, camera, or the stable controller.
- PASS: Weapon values are stored in `AdvancedWeaponData` assets, not hardcoded inside the modular controller scripts.
- PASS: Weapon Test Area is added under a named root and does not alter match start, aircraft, drop, landing, bot, vehicle, zone, or inventory flow.
- PASS: Runtime C# scripts compile with Unity 6.5 Roslyn references with zero errors and zero warnings.

Architecture notes:

- The Milestone 24B modular weapon stack now supports primary, secondary, and pistol slots, fire mode switching, burst timing, bolt/pump cooldowns, dry fire, reload guards, and ammo clamping.
- The live gameplay controller is intentionally left on the proven `WeaponController` for this checkpoint. Full migration of player input from legacy weapons to modular weapons remains a later integration task after Play Mode testing.
- Temporary original weapon visuals are generated at runtime by `WeaponVisualPlaceholderFactory` and are explicitly labeled as placeholders in `WEAPON_BALANCE.md`.

Unity Editor verification:

- NOT RUN: Unity batch mode remains blocked by Unity licensing startup timeout in this environment.
- Manual Play Mode validation is listed in `TEST_CHECKLIST.md`.

## Milestone 24A AAA Combat Foundation - 2026-07-12

Scope:

- Current project only: `BattleZoneMobile_Unity6_Project_MatchStateFixed-4`.
- Combat foundation only; no movement, joystick, Animator ownership, aircraft, landing, damage overlay, camera, vehicle, bot, or match-flow rewrites.
- The current active `WeaponController` remains the live gameplay weapon path for this checkpoint.

Regression audit results:

- PASS: `ReliablePlayerMovement` was not modified and remains Combat ground movement owner.
- PASS: `ThirdPersonMobileController` was not modified and remains enabled for camera, animation, drop, and UI support.
- PASS: `BattleRoyaleMatchFlow` was not modified and remains aircraft/freefall/parachute/landing pose owner.
- PASS: Player root Animator placement was not changed; Unity `Animator` remains on visual child `LowPolyOriginalHumanoid`.
- PASS: `Apply Root Motion` remains disabled on the visual child Animator.
- PASS: Red damage overlay code was not modified.
- PASS: New combat foundation components are passive by default and do not replace the current weapon controller.
- PASS: `CombatDebugWindow` is present but default OFF.
- PASS: Current visual humanoid body parts are tagged with `CombatHitbox` metadata without animating or moving the Player root.
- PASS: Runtime C# scripts compile with Unity 6.5 Roslyn references with zero errors and zero warnings.

Combat foundation summary:

- `AdvancedWeaponData` stores weapon stats, recoil, spread, attachments, audio clips, animation hook names, body-zone multipliers, and surface impact profiles as ScriptableObject data.
- `ModularWeaponBase` provides switch-ready raycast, projectile, melee, and throwable support.
- `AssaultRifleWeapon`, `SMGWeapon`, `SniperWeapon`, `DMRWeapon`, `ShotgunWeapon`, `PistolWeapon`, `MeleeWeapon`, and `ThrowableWeapon` inherit from the common base class.
- `CombatProjectile` uses pooling and per-frame raycast segments for projectile support.
- `CombatRecoilApplicator` separates camera recoil, weapon recoil, and crosshair recoil.
- `CombatSurface`, `CombatHitbox`, and `CombatImpactUtility` provide surface-aware and body-zone-aware hit detection for future weapon migration.

Unity Editor verification:

- NOT RUN: Unity batch mode remains blocked by Unity licensing startup timeout in this environment.
- Manual Play Mode validation is listed in `TEST_CHECKLIST.md`.

## Milestone 23A Character Animation Polish - 2026-07-12

Scope:

- Current project only: `BattleZoneMobile_Unity6_Project_MatchStateFixed-4`.
- Character animation polish only; no movement rewrite.
- Stable fixes preserved: WASD/mobile movement, visual-child Animator placement, direct-damage-only red overlay, and startup drop gravity ownership.

Regression audit results:

- PASS: Unity `Animator` remains on `LowPolyOriginalHumanoid`, not Player root.
- PASS: `Animator.applyRootMotion` remains false in runtime builder and is reinforced by `ThirdPersonMobileController.SetUnityAnimator`.
- PASS: Player root transform remains controlled only by `ReliablePlayerMovement` during Combat and `BattleRoyaleMatchFlow` during aircraft/freefall/parachute/landing phases.
- PASS: `ThirdPersonMobileController` writes Animator parameters through cached existence checks, preventing missing-parameter Console warnings.
- PASS: `HumanoidPlaceholderAnimator` animates only child body part local transforms under the visual child.
- PASS: Fire, reload, weapon switch, and landing animation events are synchronized through `ThirdPersonMobileController`.
- PASS: `AC_PlayerHumanoid` includes the Milestone 23A parameter set.
- PASS: Runtime C# compile check completed with zero errors and zero warnings.

Transition summary:

- Idle, walk, and sprint blend from smoothed actual CharacterController horizontal speed.
- Existing visual-child AnimatorController transitions were softened to `0.12s`.
- Jump and falling pose blends use `VerticalVelocity`, `Grounded`, and `Falling`.
- Landing is triggered when falling returns to grounded and when match-flow landing recovery runs.
- Crouch and prone use separate smoothed blend channels so stance changes do not snap.
- Aim blends over standing, walking, and crouching poses.
- Fire and reload are short additive child-pose reactions and do not disable movement.

Unity Editor verification:

- NOT RUN: Unity batch mode remains blocked by Unity licensing startup timeout in this environment.
- Manual Play Mode validation is listed in `TEST_CHECKLIST.md`.

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
# Milestone 24D Loot, Inventory and Attachments - 2026-07-13

Base:

- Continued from `BattleZoneMobile_Unity6_Project_MatchStateFixed-4`.
- Did not rebuild the project.
- Did not redesign the stable movement, joystick, Animator placement, aircraft/freefall/landing, red overlay, weapon pickup, recoil, reload, hit feedback, weapon switching, or backpack pickup paths.

Changes applied:

- Added data-driven `InventoryItemData` and `WeaponAttachmentData` ScriptableObjects.
- Expanded `LootItem` to carry inventory data references, item tier, attachment data, rarity, quantity, and backpack cost.
- Changed pickup consumption so rejected items remain on the ground instead of being disabled before inventory acceptance.
- Replaced the old instant-use `PlayerInventory` internals with capacity-aware backpack tiers, timed healing, throwables, stored attachments, and armor/helmet replacement support.
- Added helmet durability loss on headshot mitigation and vest durability reporting through current armor.
- Added attachment application to `WeaponController` using `WeaponAttachmentProfile`, preserving existing fire/reload/recoil/hit code paths.
- Added inventory desktop/mobile controls through `InventoryInputBridge`, inventory action buttons, and HUD use-progress feedback.
- Added `M24D Loot Inventory Test Area` in Editor/development builds for all requested loot and attachment pickup types.
- Fixed a pre-existing `CombatProjectile` layer-mask conditional that Roslyn flags by using `context.hitMask.value` explicitly.

Regression protections:

- `ReliablePlayerMovement` remains the only Combat ground movement owner.
- `BattleRoyaleMatchFlow` remains the authoritative pose owner for Aircraft/Freefall/Parachute/Landing.
- Animator remains on `LowPolyOriginalHumanoid`; Player root remains non-animated.
- Damage flash behavior was not modified.
- Fire/reload/recoil/hit feedback continue through the existing `WeaponController`.
- Pickup with `E`, `F`, prompt click, and mobile `PICKUP` continue through the same focused pickup path.

Verification performed:

- PASS: Runtime C# scripts compiled with Unity `6000.5.3f1` managed references and zero compiler errors.
- PASS: Static code audit confirmed no movement ownership rewrite.
- PASS: Static code audit confirmed new test area is additive and guarded by Editor/development build symbols.
- NOT RUN: Unity Play Mode could not be launched here because this machine still requires Unity Hub/license activation.

Remaining risk:

- Manual Unity Play Mode verification is required for live pickup/equip/use/drop flows, because Editor execution is blocked by licensing in this environment.
- Attachment icons, loot meshes, healing sounds, and throwable gameplay are still original runtime placeholders rather than final authored assets.

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
