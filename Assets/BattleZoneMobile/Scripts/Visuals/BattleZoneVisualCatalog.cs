using UnityEngine;

namespace BattleZoneMobile
{
    [CreateAssetMenu(fileName = "BattleZoneVisualCatalog", menuName = "BattleZone Mobile/Visual Catalog")]
    public sealed class BattleZoneVisualCatalog : ScriptableObject
    {
        public BattleZoneVisualAssetDefinition[] definitions;

        public bool TryGetDefinition(string assetId, out BattleZoneVisualAssetDefinition definition)
        {
            if (definitions != null)
            {
                for (int i = 0; i < definitions.Length; i++)
                {
                    BattleZoneVisualAssetDefinition candidate = definitions[i];
                    if (candidate != null && candidate.assetId == assetId)
                    {
                        definition = candidate;
                        return true;
                    }
                }
            }

            definition = null;
            return false;
        }
    }
}
