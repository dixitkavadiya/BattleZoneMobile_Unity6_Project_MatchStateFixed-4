# BattleZone Mobile - Test Checklist

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
