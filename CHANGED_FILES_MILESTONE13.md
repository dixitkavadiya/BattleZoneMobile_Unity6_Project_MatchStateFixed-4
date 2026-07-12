# BattleZone Mobile - Milestone 13 Changed Files

## Core Gameplay

- `Assets/BattleZoneMobile/Scripts/Match/BattleRoyaleMatchFlow.cs`
- `Assets/BattleZoneMobile/Scripts/Match/BattleZoneLocalMatchHistory.cs`
- `Assets/BattleZoneMobile/Scripts/Core/GameManager.cs`
- `Assets/BattleZoneMobile/Scripts/Core/ThirdPersonMobileController.cs`
- `Assets/BattleZoneMobile/Scripts/Core/BattleZoneRuntimeBuilder.cs`

## Bots, Zone, Loot, UI

- `Assets/BattleZoneMobile/Scripts/AI/BotAI.cs`
- `Assets/BattleZoneMobile/Scripts/AI/BotManager.cs`
- `Assets/BattleZoneMobile/Scripts/Zone/SafeZoneController.cs`
- `Assets/BattleZoneMobile/Scripts/Loot/LootItem.cs`
- `Assets/BattleZoneMobile/Scripts/Loot/LootSpawner.cs`
- `Assets/BattleZoneMobile/Scripts/Loot/LootPickupInteractor.cs`
- `Assets/BattleZoneMobile/Scripts/Loot/PlayerInventory.cs`
- `Assets/BattleZoneMobile/Scripts/UI/UIManager.cs`
- `Assets/BattleZoneMobile/Scripts/UI/RuntimeHudTelemetry.cs`
- `Assets/BattleZoneMobile/Editor/BattleZoneRuntimeSmokeTest.cs`

## Documentation

- `README.md`
- `CHANGELOG.md`
- `TEST_CHECKLIST.md`

## Remaining Limitations

- Match flow is local single-player only; no multiplayer, squads, matchmaking, or online accounts are included.
- Aircraft, parachute, bot drop, and loot rarity use original runtime placeholder geometry and procedural logic.
- Bot pathfinding still depends on runtime fallback movement and available NavMesh sampling; final production AI would need baked navigation and deeper tactical tuning.
- Android compatibility is preserved, but no physical Android device performance test was run for this package.
