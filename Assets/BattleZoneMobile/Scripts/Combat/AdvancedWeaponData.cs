using System;
using UnityEngine;

namespace BattleZoneMobile
{
    [Serializable]
    public class CombatRecoilProfile
    {
        [Header("Camera Recoil")]
        [Min(0f)] public float cameraPitchKick = 0.5f;
        [Min(0f)] public float cameraYawKick = 0.15f;
        [Min(0f)] public float cameraRecoverySpeed = 16f;

        [Header("Weapon Recoil")]
        [Min(0f)] public float weaponKickDistance = 0.035f;
        [Min(0f)] public float weaponPitchKick = 2f;
        [Min(0f)] public float weaponReturnSpeed = 18f;

        [Header("Crosshair Recoil")]
        [Min(0f)] public float crosshairBloomPerShot = 10f;
        [Min(0f)] public float crosshairRecoverySpeed = 20f;
    }

    [Serializable]
    public class CombatSpreadProfile
    {
        [Min(0f)] public float hipSpreadDegrees = 1.2f;
        [Min(0f)] public float adsSpreadDegrees = 0.45f;
        [Min(0f)] public float movingSpreadDegrees = 0.65f;
        [Range(0.1f, 2f)] public float crouchMultiplier = 0.78f;
        [Range(0.1f, 2f)] public float proneMultiplier = 0.58f;
        [Min(0f)] public float bloomPerShotDegrees = 0.12f;
        [Min(0f)] public float maxBloomDegrees = 2.4f;
        [Min(0f)] public float bloomRecoverySpeed = 3.5f;

        public float ResolveSpread(bool aiming, bool moving, bool crouching, bool prone, float currentBloom)
        {
            float spread = aiming ? adsSpreadDegrees : hipSpreadDegrees;
            if (moving)
            {
                spread += movingSpreadDegrees;
            }

            if (prone)
            {
                spread *= proneMultiplier;
            }
            else if (crouching)
            {
                spread *= crouchMultiplier;
            }

            return Mathf.Max(0f, spread + Mathf.Clamp(currentBloom, 0f, maxBloomDegrees));
        }
    }

    [Serializable]
    public class CombatBodyDamageProfile
    {
        [Min(0f)] public float headMultiplier = 2.2f;
        [Min(0f)] public float neckMultiplier = 1.55f;
        [Min(0f)] public float chestMultiplier = 1f;
        [Min(0f)] public float armMultiplier = 0.72f;
        [Min(0f)] public float legMultiplier = 0.64f;

        public float GetMultiplier(CombatHitZone zone)
        {
            switch (zone)
            {
                case CombatHitZone.Head:
                    return headMultiplier;
                case CombatHitZone.Neck:
                    return neckMultiplier;
                case CombatHitZone.Arm:
                    return armMultiplier;
                case CombatHitZone.Leg:
                    return legMultiplier;
                default:
                    return chestMultiplier;
            }
        }
    }

    [Serializable]
    public class CombatAttachmentSupport
    {
        public bool supportsOptic = true;
        public bool supportsMuzzle = true;
        public bool supportsMagazine = true;
        public bool supportsGrip = true;
        public bool supportsStock = true;
        public bool supportsUnderbarrel;
        public string[] compatibleAttachmentIds = new string[0];
    }

    [Serializable]
    public class CombatAudioProfile
    {
        public AudioClip shoot;
        public AudioClip reload;
        public AudioClip dryFire;
        public AudioClip equip;
        public AudioClip unequip;
        public AudioClip impact;
    }

    [Serializable]
    public class CombatAnimationHookProfile
    {
        public string equipTrigger = "Equip";
        public string unequipTrigger = "Unequip";
        public string fireTrigger = "Fire";
        public string reloadTrigger = "Reload";
        public string dryFireTrigger = "DryFire";
        [Range(0f, 1f)] public float reloadCommitNormalizedTime = 0.82f;
    }

    [Serializable]
    public class CombatSurfaceImpactEntry
    {
        public CombatSurfaceType surface = CombatSurfaceType.Default;
        public GameObject impactPrefab;
        public AudioClip impactClip;
        public Color debugColor = Color.white;
        [Min(0.05f)] public float effectLifetime = 1.25f;
    }

    [CreateAssetMenu(fileName = "WD_CombatWeapon", menuName = "BattleZone Mobile/Combat/Advanced Weapon Data")]
    public class AdvancedWeaponData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private CombatWeaponType weaponType = CombatWeaponType.Pistol;
        [SerializeField] private CombatWeaponDelivery delivery = CombatWeaponDelivery.Raycast;
        [SerializeField] private CombatAmmoClass ammoClass = CombatAmmoClass.Light;
        [SerializeField] private string displayName = "Combat Weapon";

        [Header("Core Stats")]
        [SerializeField, Min(0f)] private float damage = 20f;
        [SerializeField, Min(0.01f)] private float fireRate = 3f;
        [SerializeField, Min(0f)] private float reloadTime = 1.4f;
        [SerializeField, Min(1)] private int magazineSize = 12;
        [SerializeField, Min(0)] private int startingReserveAmmo = 36;
        [SerializeField, Min(0f)] private float range = 120f;
        [SerializeField, Min(0f)] private float bulletSpeed = 320f;
        [SerializeField, Min(1)] private int pelletCount = 1;
        [SerializeField] private bool automatic = false;
        [SerializeField] private bool usesAmmo = true;

        [Header("Configurable Systems")]
        [SerializeField] private CombatRecoilProfile recoil = new CombatRecoilProfile();
        [SerializeField] private CombatSpreadProfile spread = new CombatSpreadProfile();
        [SerializeField] private CombatBodyDamageProfile bodyDamage = new CombatBodyDamageProfile();
        [SerializeField] private CombatAttachmentSupport attachments = new CombatAttachmentSupport();
        [SerializeField] private CombatAudioProfile audio = new CombatAudioProfile();
        [SerializeField] private CombatAnimationHookProfile animationHooks = new CombatAnimationHookProfile();
        [SerializeField] private CombatSurfaceImpactEntry[] surfaceImpacts = new CombatSurfaceImpactEntry[0];

        public CombatWeaponType WeaponType => weaponType;
        public CombatWeaponDelivery Delivery => delivery;
        public CombatAmmoClass AmmoClass => ammoClass;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? weaponType.ToString() : displayName;
        public float Damage => Mathf.Max(0f, damage);
        public float FireRate => Mathf.Max(0.01f, fireRate);
        public float ReloadTime => Mathf.Max(0f, reloadTime);
        public int MagazineSize => Mathf.Max(1, magazineSize);
        public int StartingReserveAmmo => Mathf.Max(0, startingReserveAmmo);
        public float Range => Mathf.Max(0f, range);
        public float BulletSpeed => Mathf.Max(0f, bulletSpeed);
        public int PelletCount => Mathf.Max(1, pelletCount);
        public bool Automatic => automatic;
        public bool UsesAmmo => usesAmmo;
        public CombatRecoilProfile Recoil => recoil;
        public CombatSpreadProfile Spread => spread;
        public CombatBodyDamageProfile BodyDamage => bodyDamage;
        public CombatAttachmentSupport Attachments => attachments;
        public CombatAudioProfile Audio => audio;
        public CombatAnimationHookProfile AnimationHooks => animationHooks;

        public float ResolveDamage(CombatHitZone hitZone)
        {
            return Damage * (bodyDamage != null ? bodyDamage.GetMultiplier(hitZone) : 1f);
        }

        public CombatSurfaceImpactEntry ResolveSurfaceImpact(CombatSurfaceType surface)
        {
            if (surfaceImpacts == null || surfaceImpacts.Length == 0)
            {
                return null;
            }

            CombatSurfaceImpactEntry fallback = null;
            for (int i = 0; i < surfaceImpacts.Length; i++)
            {
                CombatSurfaceImpactEntry entry = surfaceImpacts[i];
                if (entry == null)
                {
                    continue;
                }

                if (entry.surface == surface)
                {
                    return entry;
                }

                if (entry.surface == CombatSurfaceType.Default)
                {
                    fallback = entry;
                }
            }

            return fallback;
        }

        private void OnValidate()
        {
            damage = Mathf.Max(0f, damage);
            fireRate = Mathf.Max(0.01f, fireRate);
            reloadTime = Mathf.Max(0f, reloadTime);
            magazineSize = Mathf.Max(1, magazineSize);
            startingReserveAmmo = Mathf.Max(0, startingReserveAmmo);
            range = Mathf.Max(0f, range);
            bulletSpeed = Mathf.Max(0f, bulletSpeed);
            pelletCount = Mathf.Max(1, pelletCount);
        }
    }
}
