using UnityEngine;

namespace BattleZoneMobile
{
    public class CombatSurface : MonoBehaviour
    {
        [SerializeField] private CombatSurfaceType surfaceType = CombatSurfaceType.Default;
        [SerializeField] private GameObject impactPrefabOverride = null;
        [SerializeField] private AudioClip impactAudioOverride = null;

        public CombatSurfaceType SurfaceType => surfaceType;
        public GameObject ImpactPrefabOverride => impactPrefabOverride;
        public AudioClip ImpactAudioOverride => impactAudioOverride;

        public void Configure(CombatSurfaceType type)
        {
            surfaceType = type;
        }

        public static CombatSurfaceType ResolveSurfaceType(Collider collider)
        {
            if (collider == null)
            {
                return CombatSurfaceType.Default;
            }

            CombatSurface surface = collider.GetComponentInParent<CombatSurface>();
            if (surface != null)
            {
                return surface.SurfaceType;
            }

            string source = collider.name;
            Renderer renderer = collider.GetComponentInParent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null)
            {
                source += " " + renderer.sharedMaterial.name;
            }

            source = source.ToLowerInvariant();
            if (source.Contains("metal") || source.Contains("container") || source.Contains("barrel"))
            {
                return CombatSurfaceType.Metal;
            }

            if (source.Contains("stone") || source.Contains("rock") || source.Contains("concrete") || source.Contains("road"))
            {
                return CombatSurfaceType.Stone;
            }

            if (source.Contains("wood") || source.Contains("crate") || source.Contains("fence") || source.Contains("bench"))
            {
                return CombatSurfaceType.Wood;
            }

            if (source.Contains("glass") || source.Contains("window"))
            {
                return CombatSurfaceType.Glass;
            }

            if (source.Contains("sand") || source.Contains("beach"))
            {
                return CombatSurfaceType.Sand;
            }

            if (source.Contains("water") || source.Contains("river") || source.Contains("lake"))
            {
                return CombatSurfaceType.Water;
            }

            if (source.Contains("grass") || source.Contains("tree") || source.Contains("bush") || source.Contains("forest"))
            {
                return CombatSurfaceType.Grass;
            }

            return CombatSurfaceType.Default;
        }
    }
}
