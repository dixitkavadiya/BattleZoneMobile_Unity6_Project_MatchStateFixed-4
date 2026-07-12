# BattleZone Mobile - VisualSlice Changed Files

## Added

- `ART_SPEC.md`
- `ASSET_REPLACEMENT_GUIDE.md`
- `PERFORMANCE_BUDGET.md`
- `PLACEHOLDERS_TO_REPLACE.md`
- `Assets/BattleZoneMobile/Scripts/Visuals/BattleZoneVisualAssetDefinition.cs`
- `Assets/BattleZoneMobile/Scripts/Visuals/BattleZoneVisualCatalog.cs`
- `Assets/BattleZoneMobile/Scripts/Visuals/BattleZoneArtReplacementSlot.cs`
- `Assets/BattleZoneMobile/Scripts/Visuals/BattleZoneArtSocket.cs`
- `Assets/BattleZoneMobile/Editor/BattleZoneVisualSliceAssetGenerator.cs`
- `Assets/BattleZoneMobile/ArtPipeline/Materials/*`
- `Assets/BattleZoneMobile/ArtPipeline/PrefabSlots/*`
- `Assets/BattleZoneMobile/ArtPipeline/VisualDefinitions/*`

## Modified

- `Assets/BattleZoneMobile/Scripts/Core/BattleZoneRuntimeBuilder.cs`
  - Added a labeled visual-slice street block, enterable house, warehouse, character showcase, and two weapon showcase objects.
  - Added runtime custom mesh helpers for the visual-slice proxy area.

- `Assets/BattleZoneMobile/Editor/BattleZoneRuntimeSmokeTest.cs`
  - Added checks for visual-slice roots, replacement slots, LOD groups, generated catalog assets, quality preset names, and non-primitive visual-slice meshes.

- `ProjectSettings/QualitySettings.asset`
  - Added named `Low`, `Medium`, `High`, and `Ultra` URP quality presets.

- `README.md`
- `CHANGELOG.md`
- `TEST_CHECKLIST.md`

## Verification

- Generated art-pipeline assets through Unity `6000.5.3f1`.
- Ran `BZ_Main` smoke test successfully with `outputs/unity_visualslice_final_smoke.log`.
- Packaged and archive-tested `BattleZoneMobile_Unity6_Project_VisualSlice.zip`.
