# BattleZone Mobile - Match State Fixed Test Checklist

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
