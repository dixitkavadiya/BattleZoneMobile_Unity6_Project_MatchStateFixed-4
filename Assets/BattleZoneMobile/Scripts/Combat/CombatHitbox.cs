using UnityEngine;

namespace BattleZoneMobile
{
    public class CombatHitbox : MonoBehaviour
    {
        [SerializeField] private CombatHitZone hitZone = CombatHitZone.Chest;
        [SerializeField] private Health damageOwner;
        [SerializeField, Min(0f)] private float multiplierOverride = 0f;

        public CombatHitZone HitZone => hitZone;
        public Health DamageOwner => damageOwner != null ? damageOwner : GetComponentInParent<Health>();
        public bool HasMultiplierOverride => multiplierOverride > 0f;
        public float MultiplierOverride => multiplierOverride;

        public void Configure(CombatHitZone zone, Health owner)
        {
            hitZone = zone;
            damageOwner = owner;
        }

        public float ResolveMultiplier(AdvancedWeaponData weaponData)
        {
            if (HasMultiplierOverride)
            {
                return multiplierOverride;
            }

            return weaponData != null && weaponData.BodyDamage != null ? weaponData.BodyDamage.GetMultiplier(hitZone) : 1f;
        }
    }
}
