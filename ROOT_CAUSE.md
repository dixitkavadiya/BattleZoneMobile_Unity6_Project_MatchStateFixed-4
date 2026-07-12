# BattleZone Mobile - Match State Root Cause

## Reported Symptom

The match could end automatically after roughly 48 seconds and open the Match Summary as `DEFEAT / Placement #9 of 9`. Once that happened, `GameManager` disabled player controls, so WASD and the mobile joystick no longer moved the character.

## Root Cause

The defeat screen is produced only by `GameManager.OnPlayerDied`. In the recovered project, that method accepted any `Health.onDied` event while `matchActive` was true and immediately:

- set `matchActive = false`
- set `matchConcluded = true`
- disabled `ThirdPersonMobileController`
- disabled weapon controls
- stopped the zone
- opened Game Over and Match Summary

The match manager did not explicitly register the local player as a valid alive participant at match reset, and it did not re-check `playerHealth.IsAlive` before concluding defeat. Alive-player display was also derived from bot count plus an implicit local player, so invalid bot setup and local-player state were not separated clearly.

That made the runtime fragile: a stale or invalid death/alive-state event could put the game into Game Over state and lock controls even when the local player should still be controllable.

## Exact Fix Applied

- Added explicit local-player registration after every match reset.
- Added `IsLocalPlayerAlive()` and `GetAliveParticipantCount()` so the local player is counted separately from bots.
- Guarded `OnPlayerDied` so it refuses to conclude defeat if `playerHealth` is missing or still reports alive.
- Hardened `StartMatch` and `RestartMatch` so they:
  - set `Time.timeScale = 1`
  - enable `ThirdPersonMobileController`
  - enable `CharacterController`
  - clear external motion locks
  - unlock movement input
  - reset player health, weapons, inventory, bots, timers, and match state
- Guarded victory so it requires:
  - local player alive
  - combat phase started
  - valid opponent roster
  - all expected bots eliminated through the normal bot death notification path
- Added a `CloseMatchSummary` handler that hides the summary and returns to the main menu after a concluded match.
- Added the requested Editor-only debug panel values:
  - Match State
  - Local Player Alive
  - Alive Count
  - Controls Enabled
  - Time Scale

## Changed Files

- `Assets/BattleZoneMobile/Scripts/Core/GameManager.cs`
- `Assets/BattleZoneMobile/Scripts/Core/BattleZoneRuntimeBuilder.cs`
- `Assets/BattleZoneMobile/Scripts/UI/RuntimeDeveloperPanel.cs`
- `README.md`
- `TEST_CHECKLIST.md`
- `ROOT_CAUSE.md`

## Remaining Limitation

If the local player legitimately reaches zero health from bot damage or zone damage, defeat is still valid and controls are intentionally disabled. The new debug line makes that visible: false defeats should not happen while `LOCAL True`.
