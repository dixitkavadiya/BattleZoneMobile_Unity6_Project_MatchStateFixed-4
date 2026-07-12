# BattleZone Mobile - Asset Replacement Guide

Milestone VisualSlice adds an art replacement layer without changing existing gameplay systems. Gameplay code can keep using player, weapon, loot, vehicle, and world systems while artists replace visual slots with authored FBX prefabs.

## Key Folders

- `Assets/BattleZoneMobile/ArtPipeline/Materials`
- `Assets/BattleZoneMobile/ArtPipeline/PrefabSlots`
- `Assets/BattleZoneMobile/ArtPipeline/VisualDefinitions`
- `Assets/BattleZoneMobile/Scripts/Visuals`

## Core Scripts

- `BattleZoneVisualAssetDefinition`: ScriptableObject describing one visual asset, budgets, material palette, texture target, LOD targets, sockets, and replacement notes.
- `BattleZoneVisualCatalog`: ScriptableObject index of all current replacement definitions.
- `BattleZoneArtReplacementSlot`: MonoBehaviour placed on prefab roots and runtime visual-slice showcase roots.
- `BattleZoneArtSocket`: Named transform marker for muzzle, magazine, hand, loot, doorway, and other stable gameplay/FX anchors.

## Generated Prefab Slots

- `PF_ArtSlot_CHR_TacticalOperator`
- `PF_ArtSlot_WPN_Rook17_AssaultRifle`
- `PF_ArtSlot_WPN_Sable9_Pistol`
- `PF_ArtSlot_BLD_EnterableHouse`
- `PF_ArtSlot_BLD_Warehouse`
- `PF_ArtSlot_ENV_StreetBlock`
- `PF_ArtSlot_VEG_StreetTree`
- `PF_ArtSlot_LOOT_FieldCrate`

These prefabs are clean slot roots. They intentionally do not pretend to be finished art. Add authored LOD mesh children under `LOD0_ART_MESH_SLOT`, `LOD1_ART_MESH_SLOT`, and `LOD2_ART_MESH_SLOT`, and preserve `SOCKETS_DO_NOT_RENAME`.

## Replacement Steps

1. Import the authored FBX under a future production art folder.
2. Configure import settings from `ART_SPEC.md`.
3. Create or assign URP mobile materials.
4. Drag LOD meshes into the matching prefab slot under the LOD slot roots.
5. Preserve required socket names exactly.
6. Assign the final prefab to the matching `BattleZoneVisualAssetDefinition`.
7. Mark `gameplayReady` only after sockets, scale, colliders, animator, and LODs are verified.
8. Run `BattleZone Mobile/Generate Visual Slice Art Pipeline Assets` only if slot assets need to be restored.
9. Run the editor smoke test before delivery.

## Runtime Binding Rules

- Do not rename gameplay anchors such as `Muzzle`, `ShellEject`, `Magazine`, `RightHand`, `Doorway`, or `PickupAnchor`.
- Keep collider replacements separate from render meshes for buildings, vehicles, and loot.
- Character visual swaps must preserve the player root, `CharacterController`, camera pivot, and weapon controller components.
- Weapon mesh swaps must preserve muzzle and shell sockets so fire, tracer, muzzle flash, and shell ejection stay aligned.
- Loot visual swaps must preserve pickup collider behavior and prompt anchor placement.

## Current Visual Slice

`BZ_Main` now builds a labeled visual-slice area with:

- `VS_StreetBlock_Root`
- `VS_Enterable_House_Root`
- `VS_Warehouse_Root`
- `VS_Tactical_Humanoid_Showcase`
- `VS_Rook17_AssaultRifle_Showcase`
- `VS_Sable9_Pistol_Showcase`

This area is an original art-direction proxy and replacement architecture demo. It is not final production art.
