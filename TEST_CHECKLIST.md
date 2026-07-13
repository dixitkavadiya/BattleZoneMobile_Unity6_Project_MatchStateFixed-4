# BattleZone Mobile - Test Checklist

## Pickup Controls Hotfix

Automated/static checks run in this workspace:

- PASS: Runtime C# scripts compile with Unity 6.5 Roslyn references with zero errors and zero warnings.
- PASS: `LootPickupInteractor` has keyboard pickup through `E` and fallback `F`.
- PASS: `LootPickupInteractor` makes the pickup prompt clickable through a `Button` on the prompt `Text`.
- PASS: Runtime HUD creates `PickupButton` and passes it to `LootPickupInteractor`.
- PASS: Pickup prompt copy resolves to `Press E to pick up` on Editor/Desktop and `Tap PICKUP` on mobile.
- PASS: All pickup actions call the same `PickUpFocused()` path.
- PASS: Picked objects disable colliders and deactivate immediately before inventory handoff to prevent duplicate pickup calls.
- PASS: No movement, fire, reload, camera, Animator placement, aircraft/freefall/landing, red overlay, or weapon-test-area code paths were redesigned.

Required manual Unity Play Mode checks:

1. Open `Assets/BattleZoneMobile/Scenes/BZ_Main.unity`.
2. Press Play and confirm no red Console errors.
3. Start a match and land normally.
4. Walk to the `M24B Live Weapon Test Area`.
5. Approach `Breacher-12` or the existing shotgun pickup and confirm the prompt says `Press E to pick up`.
6. Press `E` and confirm the shotgun pickup works.
7. Repeat with another pickup and press `F`; confirm fallback pickup works.
8. Approach a backpack pickup and confirm the prompt appears.
9. Press `E` or tap/click `PICKUP`; confirm backpack pickup updates inventory/backpack capacity.
10. Approach ammo, helmet, vest, medkit, and bandage pickups; confirm each can be picked up with the same controls.
11. Confirm equipped weapon changes correctly after picking up a weapon.
12. Confirm inventory and pickup message update after each pickup.
13. Confirm a pickup cannot be collected twice by rapid-clicking or pressing `E` and `F` together.
14. Left-click directly on the pickup prompt and confirm it picks up the highlighted item.
15. In a mobile/touch test, confirm the visible `PICKUP` button appears in range and tapping it picks up the highlighted item.
16. Confirm WASD, mobile joystick, fire, reload, camera, sprint, jump, crouch, prone, Animator, aircraft/freefall/landing, and red overlay fixes still work.

## Milestone 24B - Live Weapon Test Area and Pickups

Automated/static checks run in this workspace:

- PASS: Runtime C# scripts compile with Unity 6.5 Roslyn references with zero errors and zero warnings.
- PASS: Editor C# scripts compile with Unity 6.5 Roslyn references with zero errors and zero warnings.
- PASS: `AdvancedWeaponPickup` supports visible weapon labels, direct pickup through `PlayerInventory`, and cleanup after pickup.
- PASS: `LootPickupInteractor` resolves both `AdvancedWeaponPickup` and existing `LootItem` pickups through the same prompt and pickup path.
- PASS: `PlayerInventory.AddAdvancedWeapon` equips Milestone 24B data into `ModularWeaponLoadout` and maps compatible weapons into the stable legacy `WeaponController` path for live fire/reload testing.
- PASS: `BattleZoneRuntimeBuilder` creates `M24B Live Weapon Test Area` in Editor/development builds when the area is missing.
- PASS: Editor menu command exists at `BattleZone Tools > Build Milestone 24B Weapon Test Area`.
- PASS: Test area builder orders all 10 weapons in two neat rows near the PlayerSpawn area.
- PASS: Matching ammo pickups are created beside every test weapon.
- PASS: Static test targets contain Head, Chest, Arm, and Leg `CombatHitbox` parts.
- PASS: The implementation did not modify `ReliablePlayerMovement`, `ThirdPersonMobileController`, `BattleRoyaleMatchFlow`, Animator placement, camera logic, aircraft/drop/landing ownership, or damage overlay logic.

Required manual Unity Play Mode checks:

1. Open `Assets/BattleZoneMobile/Scenes/BZ_Main.unity`.
2. Optional scene bake: run `BattleZone Tools > Build Milestone 24B Weapon Test Area` and confirm the scene saves.
3. Press Play and confirm no red Console errors.
4. Confirm `M24B Live Weapon Test Area` exists near the PlayerSpawn area.
5. Confirm VXR-56, ARK-74, Sentinel AR, Pulse-9, Raptor-45, Longshot DMR, Falcon SR, Breacher-12, Sidearm P9, and Hammer .50 are visible on the ground in two rows.
6. Confirm every weapon has a label above it and does not fall through the ground pad/terrain.
7. Confirm matching ammo pickups sit beside each weapon.
8. Walk to a weapon and confirm the pickup prompt appears.
9. Pick up a weapon and confirm it equips without breaking movement.
10. Pick up the matching ammo and confirm ammo/reload values remain valid.
11. Fire, reload, and switch weapons.
12. Pick up another weapon for an occupied slot and confirm replacement is safe.
13. Shoot the test targets and confirm Head, Chest, Arm, and Leg hit zones can receive hits.
14. Confirm Metal, Wood, Stone, and Glass surface panels still exist for impact testing.
15. Confirm WASD, mobile joystick, sprint, jump, crouch, prone, camera, aircraft/freefall/landing, and red overlay fixes do not regress.

## Milestone 24B - Original Weapon Set

Automated/static checks run in this workspace:

- PASS: Runtime C# scripts compile with Unity 6.5 Roslyn references with zero errors and zero warnings.
- PASS: Weapon roster assets exist under `Assets/BattleZoneMobile/Resources/WeaponData`.
- PASS: Roster includes VXR-56, ARK-74, Sentinel AR, Pulse-9, Raptor-45, Longshot DMR, Falcon SR, Breacher-12, Sidearm P9, and Hammer .50.
- PASS: Every weapon asset uses `AdvancedWeaponData`.
- PASS: Every weapon asset stores damage, body multipliers, fire mode, fire rate, magazine size, reserve ammo, reload times, effective range, max range, bullet speed, recoil, spread, ADS spread, movement spread, hip spread, equip time, rarity, ammo class, and attachment slots.
- PASS: `ModularWeaponBase` supports Semi-auto, Full-auto, Burst, Bolt-action, and Pump-action behavior.
- PASS: `ModularWeaponLoadout` supports primary, secondary, and pistol slots, equip/unequip, weapon switching, empty-hands fallback, and pickup replacement rules.
- PASS: Modular ammo paths clamp magazine/reserve ammo and prevent negative ammo.
- PASS: `UIManager.SetAdvancedWeaponHud` can show weapon name, icon placeholder, ammo, reserve ammo, fire mode, reload state, and slot.
- PASS: Runtime builder creates the Milestone 24B weapon test scaffold with all 10 placeholder weapon views, ammo pickups, hit-zone targets, and Metal/Wood/Stone/Glass surface panels.
- PASS: Existing live-match fire/reload controls remain wired to the stable `WeaponController`, avoiding duplicate modular/legacy shots during this checkpoint.

Required manual Unity Play Mode checks:

1. Open `Assets/BattleZoneMobile/Scenes/BZ_Main.unity`.
2. Press Play and confirm no red Console errors.
3. Confirm `M24B Live Weapon Test Area` exists in the Hierarchy.
4. Confirm all 10 weapon placeholder views exist under the test area.
5. Confirm the test area includes Light, Medium, Heavy, and Shell ammo pickups.
6. Confirm hit-zone targets contain Head, Chest, Arm, and Leg `CombatHitbox` parts.
7. Confirm Metal, Wood, Stone, and Glass surface panels contain `CombatSurface`.
8. Press `START MATCH`.
9. Confirm aircraft/freefall/parachute/landing remain stable.
10. Confirm WASD movement works after landing.
11. Confirm mobile joystick movement works after landing.
12. Confirm camera, sprint, jump, crouch, prone, aim, fire, reload, and weapon switching still work.
13. Confirm reload does not permanently lock controls.
14. Confirm ammo values remain valid and never display negative values.
15. Confirm the permanent red overlay does not return.
16. Optional: enable `CombatDebugWindow.Show Combat Debug Window` and verify it reports legacy and modular loadout diagnostics.

## Milestone 24A - AAA Combat Foundation

Automated/static checks run in this workspace:

- PASS: Runtime C# scripts compile with Unity 6.5 Roslyn references with zero errors and zero warnings.
- PASS: New combat framework is additive and does not replace the active `WeaponController`.
- PASS: `ReliablePlayerMovement`, `ThirdPersonMobileController`, `BattleRoyaleMatchFlow`, and red overlay scripts were not modified.
- PASS: `AdvancedWeaponData` is a ScriptableObject data contract for weapon stats, recoil, spread, attachments, audio, animation hooks, body multipliers, and surface impacts.
- PASS: Assault Rifle, SMG, Sniper, DMR, Shotgun, Pistol, Melee, and Throwable components all inherit from `ModularWeaponBase`.
- PASS: Raycast, projectile, melee, and throwable delivery paths exist in `ModularWeaponBase`.
- PASS: `CombatProjectile` supports pooled projectile movement and raycast segment hit detection.
- PASS: `CombatHitbox` supports Head, Neck, Chest, Arm, and Leg metadata.
- PASS: `CombatSurface` supports Metal, Stone, Wood, Glass, Sand, Water, Grass, and fallback surface resolution.
- PASS: `CombatRecoilApplicator` separates camera, weapon, and crosshair recoil channels.
- PASS: `CombatAnimationEventBridge` provides future animation-event hooks without root motion.
- PASS: `CombatDebugWindow` defaults OFF.
- PASS: Runtime builder still creates the Animator on `LowPolyOriginalHumanoid`, and the Player root remains Animator-free.

Required manual Unity Play Mode checks:

1. Open `Assets/BattleZoneMobile/Scenes/BZ_Main.unity`.
2. Press Play and start a match.
3. Confirm the aircraft, freefall, parachute, and landing sequence still works.
4. Confirm WASD and mobile joystick movement work after landing.
5. Confirm sprint, jump, crouch, prone, aim, fire, reload, and weapon switching still work.
6. Confirm the camera follows and rotates normally.
7. Confirm the visual child `LowPolyOriginalHumanoid` still has the Animator and `Apply Root Motion` is off.
8. Confirm `CombatDebugWindow` remains hidden during normal gameplay.
9. Optional: enable `CombatDebugWindow.Show Combat Debug Window` in the inspector and confirm it displays weapon diagnostics.
10. Confirm firing the current legacy weapons still produces muzzle flash, tracer, recoil, hit marker, and damage number behavior.
11. Confirm no permanent red overlay appears during normal gameplay.
12. Confirm no red Console errors and no repeated BattleZone script warnings.

## Milestone 23A - Character Animation Polish

Automated/static checks run in this workspace:

- PASS: Runtime C# scripts compile with Unity 6.5 Roslyn references with zero errors and zero warnings.
- PASS: Static audit confirms the Player root still has no Unity `Animator`.
- PASS: Static audit confirms the Unity `Animator` is created on visual child `LowPolyOriginalHumanoid`.
- PASS: Static audit confirms visual child `Animator.applyRootMotion = false`.
- PASS: Static audit confirms Player-root movement ownership still routes through `ReliablePlayerMovement` for Combat and `BattleRoyaleMatchFlow` for aircraft/freefall/parachute/landing pose.
- PASS: `AC_PlayerHumanoid` contains the Milestone 23A parameters: `Speed`, `VerticalVelocity`, `Grounded`, `Sprinting`, `Crouching`, `Prone`, `Aiming`, `Falling`, `Fire`, `Reload`, and `Landing`.
- PASS: Animator diagnostics are optional through inspector toggle `Show Animator Debug`, default off.
- PASS: Fire, reload, weapon switch, and landing animation events are routed through `ThirdPersonMobileController` without locking movement.

Required manual Unity Play Mode checks:

1. Start `BZ_Main` and press `START MATCH`.
2. Confirm no Player-root Animator appears at runtime.
3. Confirm `LowPolyOriginalHumanoid` has Animator and root motion disabled.
4. Confirm idle-to-walk and walk-to-sprint transitions are smooth.
5. Confirm normal walk/run does not visibly slide feet more than the low-poly placeholder limitation.
6. Confirm jump, falling, and landing poses match movement timing.
7. Confirm crouch and prone transitions blend smoothly.
8. Confirm aiming works while standing, walking, and crouching.
9. Confirm fire and reload play visual reactions and never permanently lock movement.
10. Confirm WASD, mobile joystick, camera, sprint, jump, crouch, prone, aim, fire, reload, and vehicle enter/exit still work.
11. Confirm no permanent red overlay returns.
12. Confirm no red Console errors.

## Stable Post-Recovery Gameplay Checkpoint - 2026-07-12

Automated/static checks run in this workspace:

- PASS: `git diff --check` completed with no whitespace or patch-format issues.
- PASS: Runtime C# scripts compiled with Unity 6.5 Roslyn references with zero errors and zero warnings.
- PASS: Static audit confirms one runtime Player creation path in `BattleZoneRuntimeBuilder.BuildPlayer`.
- PASS: Static audit confirms one Player-root `CharacterController`.
- PASS: Static audit confirms Combat ground locomotion remains owned by `ReliablePlayerMovement`.
- PASS: Static audit confirms Aircraft/Freefall/Parachute/Landing pose control remains owned by `BattleRoyaleMatchFlow`.
- PASS: Static audit confirms Unity `Animator` is created on visual child `LowPolyOriginalHumanoid`, not on Player root.
- PASS: Static audit confirms visual child `Animator.applyRootMotion = false`.
- PASS: Static audit confirms `DamageFlash` is inactive at rest and safe-zone damage does not call `FlashDamage`.
- PASS: `ReliablePlayerMovement` debug overlay defaults OFF through inspector toggle `Show Debug Overlay`.
- PASS: `BotManager.explicitSpawnPoints` compile warning is fixed, with empty-list spawn discovery fallback.

Unity Editor status in this environment:

- BLOCKED: Unity batch mode timed out while launching the Unity licensing client.
- NOT RUN HERE: Live Play Mode verification for Start Match, aircraft, freefall, parachute, landing, movement, combat, vehicles, Match Summary, and restart.

Required manual Unity Play Mode verification:

1. Open `Assets/BattleZoneMobile/Scenes/BZ_Main.unity`.
2. Press Play.
3. Press `START MATCH`.
4. Confirm there is exactly one active Player in the Hierarchy.
5. Confirm the Player root has exactly one active `CharacterController`.
6. Confirm Player root has no `Animator`.
7. Confirm `LowPolyOriginalHumanoid` has the Animator and `Apply Root Motion` is off.
8. Confirm the large Reliable Player Movement overlay is hidden by default.
9. Enable `Show Debug Overlay` only when debugging and confirm it reports phase, pose owner, vertical velocity, and gravity suppression.
10. Confirm aircraft sequence remains stable.
11. Confirm freefall/parachute path remains stable.
12. Confirm landing is smooth with no huge negative CharacterController Y velocity.
13. Confirm WASD moves after landing.
14. Confirm mobile joystick moves after landing.
15. Confirm sprint, jump, crouch, and prone work.
16. Confirm aim, fire, reload, and weapon switching work.
17. Confirm vehicle enter/exit works.
18. Confirm Match Summary appears only for valid victory/defeat.
19. Confirm restarting a match resets player, bots, timers, controls, and red overlay.
20. Confirm no permanent red overlay appears during normal gameplay.
21. Confirm no red Console errors.

## Package Checks

- PASS: Project was copied from the current `BattleZoneMobile_Unity6_Project_MovementRecovered 2` base.
- PASS: `Assets`, `Packages`, and `ProjectSettings` are included.
- PASS: `README.md`, `ROOT_CAUSE.md`, and `TEST_CHECKLIST.md` are included.
- PASS: Active Input Handling remains configured in project settings.
- PASS: The archive excludes generated Unity folders: `Library`, `Temp`, `Logs`, `obj`, and `UserSettings`.
- PASS: Zip integrity test passed.
- NOT RUN HERE: Unity Play Mode smoke test, because no usable Unity Editor CLI executable was available in this environment.

## Required Editor Checks

1. Open `Assets/BattleZoneMobile/Scenes/BZ_Main.unity`.
2. Press Play.
3. Press `START MATCH`.
4. Confirm `DeveloperTestPanel` shows:
   - `MOVE | KEY`
   - `JOY`
   - `FINAL`
   - `SPEED`
   - `GND`
   - `CTRL`
   - `TS`
   - `MATCH | STATE`
   - `LOCAL`
   - `ALIVE`
5. Confirm `MATCH | STATE` enters `Lobby`, then `Combat`.
6. Confirm `LOCAL True` while the local player health is above zero.
7. Confirm `ALIVE` is local player plus alive bots.
8. Confirm `CTRL True` after match start and after landing.
9. Confirm `TS 1.00` after pressing `START MATCH`.

## Movement Verification

- `W` moves the player forward.
- `S` moves the player backward.
- `A` and `D` strafe or rotate correctly according to the current controller behavior.
- Diagonal movement works.
- The left on-screen joystick moves the player.
- `JOY` and `FINAL` become non-zero when dragging the joystick.
- Sprint still works.
- Jump still works.
- Crouch still works.
- Prone still works.
- Aim, fire, reload, and weapon switching still work.
- Vehicle enter/exit still works.
- Movement returns after aircraft, freefall, parachute, and landing.

## Match-State Verification

- Match Summary does not open automatically while `LOCAL True`.
- Defeat appears only after local player health reaches zero.
- Victory appears only after the local player is alive and all valid bots were eliminated through the normal bot death path.
- Bot spawn failure, bot initialization failure, or invalid bot alive count does not mark the player defeated.
- Closing Match Summary after a completed match returns to the main menu.
- Starting a new match fully resets:
  - player health
  - local alive status
  - match state
  - controls
  - bot roster
  - match timer
  - `Time.timeScale`

## Console Checks

- Confirm no red Console errors from BattleZone scripts.
- Confirm no repeated warnings from match-state scripts.
- Confirm no legacy `Submit` input error.
- Confirm no invalid built-in `Arial.ttf` font error.
- Confirm no particle duration warning loop.

## Notes

- Android device testing was not performed here.
- If the player legitimately reaches zero health from bots or the zone, the defeat screen is expected.
- If a false defeat appears while `LOCAL True`, capture the `DeveloperTestPanel` line and Console log for the next pass.
