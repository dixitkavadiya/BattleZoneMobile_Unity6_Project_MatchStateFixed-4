# BattleZone Mobile - Milestone 14 Changed Files

## Scripts

- `Assets/BattleZoneMobile/Scripts/Core/BattleZoneRuntimeBuilder.cs`
  - Updated runtime material creation to prefer Unity 6.5 URP-compatible shaders.
  - Tuned terrain, dirt, sand, rock, road, water, and crop colors to avoid solid yellow diagnostic rendering.
  - Added stricter opaque/transparent URP material setup.
  - Updated particle effect materials to prefer URP particle shaders.

- `Assets/BattleZoneMobile/Editor/BattleZoneRuntimeSmokeTest.cs`
  - Added terrain/road/water material validation.
  - Added checks for non-URP terrain shaders and bright-yellow diagnostic terrain colors.
  - Preserved missing material, missing script, event system, font, shader, and Milestone 13 runtime checks.

- `Assets/BattleZoneMobile/Scripts/Match/BattleRoyaleMatchFlow.cs`
  - Updated flight-path line material helper to prefer a URP unlit shader.

- `Assets/BattleZoneMobile/Scripts/Combat/WeaponController.cs`
  - Updated pooled bullet tracer line material helper to prefer a URP unlit shader.

- `Assets/BattleZoneMobile/Scripts/Combat/PooledShellCasing.cs`
  - Updated shell casing material helper to prefer URP lit/simple-lit shaders.

## Documentation

- `README.md`
  - Updated for Milestone 14 open/build instructions, hotfix notes, verification, and limitations.

- `CHANGELOG.md`
  - Added Milestone 14 terrain/URP stability hotfix entry.

- `TEST_CHECKLIST.md`
  - Added actual Unity 6.5 smoke-test results and terrain rendering validation checks.

- `Assets/BattleZoneMobile/Docs/README.md`
  - Updated the Unity Project-window README copy from Milestone 12 to Milestone 14.

- `Assets/BattleZoneMobile/Docs/TEST_CHECKLIST.md`
  - Updated the Unity Project-window checklist copy with terrain/URP hotfix checks.

## Verification

- Unity `6000.5.3f1` batch smoke test passed using `Assets/BattleZoneMobile/Scenes/BZ_Main.unity`.
- Smoke log: `outputs/unity_m14_final_smoke.log`.
- No red BattleZone project Console errors were reported by the smoke test.
