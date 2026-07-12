# BattleZone Mobile - Milestone 3 Stable Changed Files

## Core Runtime

- `Assets/BattleZoneMobile/Scripts/Core/BattleZoneRuntimeBuilder.cs`
  - Added armor HUD wiring, shoulder switch control, button-scale settings, developer panel binding, open interiors, street props, procedural skybox, camera culling, stable particle templates, weapon rarity/timing data, and runtime target wiring.
- `Assets/BattleZoneMobile/Scripts/Core/ThirdPersonMobileController.cs`
  - Added shoulder switching, separate ADS sensitivity behavior, smoother recoil recovery data, and camera shoulder offset initialization.
- `Assets/BattleZoneMobile/Scripts/Core/GameManager.cs`
  - Added armor UI event wiring.

## Combat

- `Assets/BattleZoneMobile/Scripts/Combat/Health.cs`
  - Added armor/health separation, armor absorption, armor reset, and armor restoration support.
- `Assets/BattleZoneMobile/Scripts/Combat/WeaponDefinition.cs`
  - Added rarity, attachment-ready metadata, ADS/hip spread multipliers, switch timing, and recoil recovery fields.
- `Assets/BattleZoneMobile/Scripts/Combat/WeaponController.cs`
  - Added pooled muzzle/impact effects, shell ejection, pooled damage numbers, headshot marker event, rarity label display, spread tuning, recoil recovery, and non-alloc raycasts.
- `Assets/BattleZoneMobile/Scripts/Combat/WorldDamageNumber.cs`
  - Added pooled damage-number lifecycle.
- `Assets/BattleZoneMobile/Scripts/Combat/PooledTracer.cs`
  - Existing tracer pool retained.
- `Assets/BattleZoneMobile/Scripts/Combat/RuntimeParticlePool.cs`
  - New pooled particle system helper for muzzle flashes and impacts.
- `Assets/BattleZoneMobile/Scripts/Combat/PooledShellCasing.cs`
  - New pooled shell ejection placeholder.

## AI

- `Assets/BattleZoneMobile/Scripts/AI/BotAI.cs`
  - Added easy/normal/hard difficulty presets, exposed stuck state, retained cover/reload/heal behavior, and replaced raycast allocations with non-alloc buffers.
- `Assets/BattleZoneMobile/Scripts/AI/BotManager.cs`
  - Added difficulty selection, active bot count, and stuck bot count.

## UI

- `Assets/BattleZoneMobile/Scripts/UI/UIManager.cs`
  - Added armor UI display.
- `Assets/BattleZoneMobile/Scripts/UI/HitMarkerUI.cs`
  - Added separate headshot feedback styling.
- `Assets/BattleZoneMobile/Scripts/UI/RuntimeHudTelemetry.cs`
  - Existing named-location minimap telemetry retained.
- `Assets/BattleZoneMobile/Scripts/UI/MobileButtonLayoutProfile.cs`
  - New customizable mobile button scale foundation.
- `Assets/BattleZoneMobile/Scripts/UI/RuntimeDeveloperPanel.cs`
  - New Editor-only FPS/player/bot/zone diagnostic panel.

## Zone And Tests

- `Assets/BattleZoneMobile/Scripts/Zone/SafeZoneController.cs`
  - Added zone phase reporting for the developer panel.
- `Assets/BattleZoneMobile/Editor/BattleZoneRuntimeSmokeTest.cs`
  - Removed deprecated Unity object-search APIs, added checks for stable HUD controls, missing components, developer panel, button layout profile, and material/shader validity.

## Documentation

- `README.md`
- `Assets/BattleZoneMobile/Docs/README.md`
- `Assets/BattleZoneMobile/Docs/TEST_CHECKLIST.md`
- `Assets/BattleZoneMobile/Docs/CHANGED_FILES_MILESTONE3_STABLE.md`

## Remaining Limitations

- This is a stable alpha foundation using original placeholder primitives and procedural audio, not final production content.
- Android device performance has not been tested in this pass; do not claim stable Android FPS until a real device build is profiled.
- Bot movement uses runtime steering and optional `NavMeshAgent` support if a NavMesh is added later; there is no baked production NavMesh in the generated scene.
- Custom mobile layout is currently a scalable-button foundation, not a full drag-and-drop layout editor.
- The procedural humanoid animations are polished placeholders, not authored production animation clips.
