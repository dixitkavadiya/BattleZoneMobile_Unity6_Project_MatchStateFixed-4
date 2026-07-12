using UnityEngine;

namespace BattleZoneMobile
{
    public enum RuntimeVisualModuleKind
    {
        Character,
        Weapon,
        Building,
        Road,
        Terrain,
        Vegetation,
        Prop,
        Lighting,
        Vehicle,
        Loot
    }

    public class RuntimeVisualModule : MonoBehaviour
    {
        [SerializeField] private RuntimeVisualModuleKind kind;
        [SerializeField] private string moduleId;
        [SerializeField] private bool mobileOptimized = true;

        public RuntimeVisualModuleKind Kind => kind;
        public string ModuleId => moduleId;
        public bool MobileOptimized => mobileOptimized;

        public void Configure(RuntimeVisualModuleKind moduleKind, string id, bool optimized = true)
        {
            kind = moduleKind;
            moduleId = id;
            mobileOptimized = optimized;
        }
    }
}
