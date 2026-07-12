using UnityEngine;

namespace BattleZoneMobile
{
    public static class CombatImpactUtility
    {
        public static CombatHitZone ResolveHitZone(Collider collider)
        {
            CombatHitbox hitbox = collider != null ? collider.GetComponentInParent<CombatHitbox>() : null;
            if (hitbox != null)
            {
                return hitbox.HitZone;
            }

            string name = collider != null ? collider.name.ToLowerInvariant() : string.Empty;
            if (name.Contains("head") || name.Contains("helmet"))
            {
                return CombatHitZone.Head;
            }

            if (name.Contains("neck"))
            {
                return CombatHitZone.Neck;
            }

            if (name.Contains("arm") || name.Contains("hand") || name.Contains("shoulder"))
            {
                return CombatHitZone.Arm;
            }

            if (name.Contains("leg") || name.Contains("boot") || name.Contains("knee"))
            {
                return CombatHitZone.Leg;
            }

            return CombatHitZone.Chest;
        }

        public static float ResolveDamage(AdvancedWeaponData weaponData, Collider collider)
        {
            if (weaponData == null)
            {
                return 0f;
            }

            CombatHitbox hitbox = collider != null ? collider.GetComponentInParent<CombatHitbox>() : null;
            if (hitbox != null)
            {
                return weaponData.Damage * hitbox.ResolveMultiplier(weaponData);
            }

            return weaponData.ResolveDamage(ResolveHitZone(collider));
        }

        public static void ApplyDamage(AdvancedWeaponData weaponData, Collider collider, Vector3 hitPoint, Vector3 hitNormal, GameObject source)
        {
            IDamageable damageable = collider != null ? collider.GetComponentInParent<IDamageable>() : null;
            if (damageable == null || !damageable.IsAlive)
            {
                return;
            }

            float damage = ResolveDamage(weaponData, collider);
            if (damage > 0f)
            {
                damageable.TakeDamage(damage, hitPoint, hitNormal, source);
            }
        }

        public static void PlaySurfaceImpact(AdvancedWeaponData weaponData, RaycastHit hit, GameObject fallbackPrefab)
        {
            if (hit.collider == null)
            {
                return;
            }

            CombatSurface surface = hit.collider.GetComponentInParent<CombatSurface>();
            CombatSurfaceType surfaceType = surface != null ? surface.SurfaceType : CombatSurface.ResolveSurfaceType(hit.collider);
            CombatSurfaceImpactEntry entry = weaponData != null ? weaponData.ResolveSurfaceImpact(surfaceType) : null;
            GameObject effectPrefab = surface != null && surface.ImpactPrefabOverride != null ? surface.ImpactPrefabOverride : entry != null ? entry.impactPrefab : fallbackPrefab;
            AudioClip impactClip = surface != null && surface.ImpactAudioOverride != null ? surface.ImpactAudioOverride : entry != null ? entry.impactClip : weaponData != null && weaponData.Audio != null ? weaponData.Audio.impact : null;

            if (effectPrefab != null)
            {
                Quaternion rotation = hit.normal.sqrMagnitude > 0.001f ? Quaternion.LookRotation(hit.normal) : Quaternion.identity;
                GameObject effect = Object.Instantiate(effectPrefab, hit.point + hit.normal * 0.02f, rotation);
                float lifetime = entry != null ? entry.effectLifetime : 1.25f;
                Object.Destroy(effect, Mathf.Max(0.05f, lifetime));
            }

            if (impactClip != null)
            {
                AudioSource.PlayClipAtPoint(impactClip, hit.point);
            }
        }
    }
}
