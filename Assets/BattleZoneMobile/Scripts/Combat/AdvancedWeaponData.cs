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
        public AudioClip suppressedShoot;
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
        public string idleBool = "WeaponIdle";
        public string fireTrigger = "Fire";
        public string reloadTrigger = "Reload";
        public string tacticalReloadTrigger = "TacticalReload";
        public string emptyReloadTrigger = "EmptyReload";
        public string dryFireTrigger = "DryFire";
        public string boltCycleTrigger = "BoltCycle";
        public string pumpCycleTrigger = "PumpCycle";
        public string weaponSwitchTrigger = "WeaponSwitch";
        [Range(0f, 1f)] public float reloadCommitNormalizedTime = 0.82f;
    }

    [Serializable]
    public class CombatVfxProfile
    {
        public ParticleSystem muzzleFlashPrefab;
        public GameObject shellEjectionPrefab;
        public GameObject tracerPrefab;
        public GameObject impactPrefab;
    }

    [Serializable]
    public class CombatWeaponVisualProfile
    {
        public GameObject equippedPrefab;
        public GameObject worldPickupPrefab;
        public Sprite icon;
        public Color iconColor = Color.white;
        public string placeholderLabel = "Temporary original placeholder";
        public Vector3 equippedScale = Vector3.one;
        public Vector3 worldScale = Vector3.one;
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
        [SerializeField] private string weaponId = "weapon_id";
        [SerializeField] private CombatWeaponType weaponType = CombatWeaponType.Pistol;
        [SerializeField] private CombatWeaponEquipSlot equipSlot = CombatWeaponEquipSlot.Pistol;
        [SerializeField] private CombatWeaponDelivery delivery = CombatWeaponDelivery.Raycast;
        [SerializeField] private CombatAmmoClass ammoClass = CombatAmmoClass.Light;
        [SerializeField] private WeaponRarity rarity = WeaponRarity.Common;
        [SerializeField] private string displayName = "Combat Weapon";

        [Header("Fire Mode")]
        [SerializeField] private CombatFireMode primaryFireMode = CombatFireMode.SemiAuto;
        [SerializeField] private CombatFireMode[] supportedFireModes = { CombatFireMode.SemiAuto };
        [SerializeField, Min(1)] private int burstShotCount = 3;
        [SerializeField, Min(0.01f)] private float burstShotInterval = 0.075f;
        [SerializeField, Min(0f)] private float boltActionCooldown = 0.8f;
        [SerializeField, Min(0f)] private float pumpActionCooldown = 0.55f;

        [Header("Core Stats")]
        [SerializeField, Min(0f)] private float damage = 20f;
        [SerializeField, Min(0.01f)] private float fireRate = 3f;
        [SerializeField, Min(0f)] private float reloadTime = 1.4f;
        [SerializeField, Min(0f)] private float tacticalReloadTime = 1.15f;
        [SerializeField, Min(0f)] private float emptyReloadTime = 1.65f;
        [SerializeField, Min(0f)] private float equipTime = 0.22f;
        [SerializeField, Min(1)] private int magazineSize = 12;
        [SerializeField, Min(0)] private int startingReserveAmmo = 36;
        [SerializeField, Min(0f)] private float effectiveRange = 60f;
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
        [SerializeField] private CombatVfxProfile vfx = new CombatVfxProfile();
        [SerializeField] private CombatWeaponVisualProfile visual = new CombatWeaponVisualProfile();
        [SerializeField] private CombatAnimationHookProfile animationHooks = new CombatAnimationHookProfile();
        [SerializeField] private CombatSurfaceImpactEntry[] surfaceImpacts = new CombatSurfaceImpactEntry[0];

        public string WeaponId => string.IsNullOrWhiteSpace(weaponId) ? DisplayName.Replace(" ", "_").ToLowerInvariant() : weaponId;
        public CombatWeaponType WeaponType => weaponType;
        public CombatWeaponEquipSlot EquipSlot => equipSlot;
        public CombatWeaponDelivery Delivery => delivery;
        public CombatAmmoClass AmmoClass => ammoClass;
        public WeaponRarity Rarity => rarity;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? weaponType.ToString() : displayName;
        public CombatFireMode PrimaryFireMode => primaryFireMode;
        public CombatFireMode[] SupportedFireModes => supportedFireModes;
        public int BurstShotCount => Mathf.Max(1, burstShotCount);
        public float BurstShotInterval => Mathf.Max(0.01f, burstShotInterval);
        public float BoltActionCooldown => Mathf.Max(0f, boltActionCooldown);
        public float PumpActionCooldown => Mathf.Max(0f, pumpActionCooldown);
        public float Damage => Mathf.Max(0f, damage);
        public float FireRate => Mathf.Max(0.01f, fireRate);
        public float ReloadTime => Mathf.Max(0f, reloadTime);
        public float TacticalReloadTime => Mathf.Max(0f, tacticalReloadTime);
        public float EmptyReloadTime => Mathf.Max(0f, emptyReloadTime);
        public float EquipTime => Mathf.Max(0f, equipTime);
        public int MagazineSize => Mathf.Max(1, magazineSize);
        public int StartingReserveAmmo => Mathf.Max(0, startingReserveAmmo);
        public float EffectiveRange => Mathf.Max(0f, effectiveRange);
        public float Range => Mathf.Max(0f, range);
        public float MaxRange => Range;
        public float BulletSpeed => Mathf.Max(0f, bulletSpeed);
        public int PelletCount => Mathf.Max(1, pelletCount);
        public bool Automatic => automatic || primaryFireMode == CombatFireMode.FullAuto;
        public bool UsesAmmo => usesAmmo;
        public CombatRecoilProfile Recoil => recoil;
        public CombatSpreadProfile Spread => spread;
        public CombatBodyDamageProfile BodyDamage => bodyDamage;
        public CombatAttachmentSupport Attachments => attachments;
        public CombatAudioProfile Audio => audio;
        public CombatVfxProfile Vfx => vfx;
        public CombatWeaponVisualProfile Visual => visual;
        public CombatAnimationHookProfile AnimationHooks => animationHooks;

        public bool SupportsFireMode(CombatFireMode mode)
        {
            if (supportedFireModes == null || supportedFireModes.Length == 0)
            {
                return mode == primaryFireMode;
            }

            for (int i = 0; i < supportedFireModes.Length; i++)
            {
                if (supportedFireModes[i] == mode)
                {
                    return true;
                }
            }

            return false;
        }

        public CombatFireMode GetNextFireMode(CombatFireMode current)
        {
            if (supportedFireModes == null || supportedFireModes.Length <= 1)
            {
                return primaryFireMode;
            }

            for (int i = 0; i < supportedFireModes.Length; i++)
            {
                if (supportedFireModes[i] == current)
                {
                    return supportedFireModes[(i + 1) % supportedFireModes.Length];
                }
            }

            return supportedFireModes[0];
        }

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
            tacticalReloadTime = Mathf.Max(0f, tacticalReloadTime);
            emptyReloadTime = Mathf.Max(0f, emptyReloadTime);
            equipTime = Mathf.Max(0f, equipTime);
            magazineSize = Mathf.Max(1, magazineSize);
            startingReserveAmmo = Mathf.Max(0, startingReserveAmmo);
            effectiveRange = Mathf.Max(0f, effectiveRange);
            range = Mathf.Max(0f, range);
            if (range < effectiveRange)
            {
                range = effectiveRange;
            }

            bulletSpeed = Mathf.Max(0f, bulletSpeed);
            pelletCount = Mathf.Max(1, pelletCount);
            burstShotCount = Mathf.Max(1, burstShotCount);
            burstShotInterval = Mathf.Max(0.01f, burstShotInterval);
            boltActionCooldown = Mathf.Max(0f, boltActionCooldown);
            pumpActionCooldown = Mathf.Max(0f, pumpActionCooldown);
            if (supportedFireModes == null || supportedFireModes.Length == 0)
            {
                supportedFireModes = new[] { primaryFireMode };
            }
        }
    }
}
