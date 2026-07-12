using UnityEngine;

namespace BattleZoneMobile
{
    public sealed class BattleZoneArtReplacementSlot : MonoBehaviour
    {
        [SerializeField] private string assetId;
        [SerializeField] private BattleZoneArtAssetCategory category;
        [SerializeField] private BattleZoneArtProductionState productionState;
        [SerializeField] private BattleZoneVisualAssetDefinition definition;
        [SerializeField] private Transform lod0Slot;
        [SerializeField] private Transform lod1Slot;
        [SerializeField] private Transform lod2Slot;
        [SerializeField] private Transform[] requiredSockets;
        [SerializeField] private bool gameplayBindingStable = true;

        public string AssetId => assetId;
        public BattleZoneArtAssetCategory Category => category;
        public BattleZoneArtProductionState ProductionState => productionState;
        public BattleZoneVisualAssetDefinition Definition => definition;
        public Transform LOD0Slot => lod0Slot;
        public Transform LOD1Slot => lod1Slot;
        public Transform LOD2Slot => lod2Slot;
        public Transform[] RequiredSockets => requiredSockets;
        public bool GameplayBindingStable => gameplayBindingStable;

        public void ConfigureRuntime(
            string slotAssetId,
            BattleZoneArtAssetCategory slotCategory,
            BattleZoneArtProductionState slotProductionState,
            BattleZoneVisualAssetDefinition visualDefinition = null,
            Transform lod0 = null,
            Transform lod1 = null,
            Transform lod2 = null,
            Transform[] sockets = null,
            bool stableBinding = true)
        {
            assetId = slotAssetId;
            category = slotCategory;
            productionState = slotProductionState;
            definition = visualDefinition;
            lod0Slot = lod0;
            lod1Slot = lod1;
            lod2Slot = lod2;
            requiredSockets = sockets;
            gameplayBindingStable = stableBinding;
        }
    }
}
