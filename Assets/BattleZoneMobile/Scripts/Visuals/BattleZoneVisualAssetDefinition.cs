using System;
using UnityEngine;

namespace BattleZoneMobile
{
    public enum BattleZoneArtAssetCategory
    {
        Character,
        Weapon,
        Vehicle,
        Building,
        Vegetation,
        Loot,
        Prop,
        Terrain,
        UI,
        Audio
    }

    public enum BattleZoneArtProductionState
    {
        Placeholder,
        VisualSliceProxy,
        ArtistReadySlot,
        ProductionReady
    }

    [Serializable]
    public struct BattleZoneLODRequirement
    {
        public int lod0TriangleBudget;
        public int lod1TriangleBudget;
        public int lod2TriangleBudget;
        public float lod0ScreenRelativeHeight;
        public float lod1ScreenRelativeHeight;
        public float lod2ScreenRelativeHeight;
    }

    [CreateAssetMenu(fileName = "VA_NewVisualAsset", menuName = "BattleZone Mobile/Visual Asset Definition")]
    public sealed class BattleZoneVisualAssetDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string assetId;
        public string displayName;
        public BattleZoneArtAssetCategory category;
        public BattleZoneArtProductionState productionState = BattleZoneArtProductionState.ArtistReadySlot;
        [TextArea(2, 5)] public string originalityNotes;

        [Header("Replacement Prefabs")]
        public GameObject visualPrefab;
        public GameObject lod0Prefab;
        public GameObject lod1Prefab;
        public GameObject lod2Prefab;
        public RuntimeAnimatorController animatorController;

        [Header("Materials And Textures")]
        public Material[] materialPalette;
        public Vector2Int maxTextureResolution = new Vector2Int(1024, 1024);
        public string androidCompression = "ASTC 6x6 or ETC2 fallback";
        public bool usesURPCompatibleShaders = true;

        [Header("Budgets And Sockets")]
        public BattleZoneLODRequirement lodRequirement;
        public string[] requiredSockets;
        public bool gameplayReady;

        [Header("Notes")]
        [TextArea(3, 8)] public string replacementNotes;

        public bool IsReplaceable => visualPrefab != null && requiredSockets != null;
    }
}
