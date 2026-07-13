using UnityEngine;

namespace BattleZoneMobile
{
    public enum WeaponSlot
    {
        AssaultRifle,
        SMG,
        Sniper,
        Shotgun,
        Pistol
    }

    public enum AmmoKind
    {
        Light,
        Medium,
        Heavy,
        Shell
    }

    public enum WeaponRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic
    }

    [CreateAssetMenu(fileName = "WeaponDefinition", menuName = "BattleZone Mobile/Weapon Definition")]
    public class WeaponDefinition : ScriptableObject
    {
        [Header("Identity")]
        public WeaponSlot slot = WeaponSlot.Pistol;
        public string displayName = "Pistol";
        public WeaponRarity rarity = WeaponRarity.Common;
        public string attachmentProfile = "Base";

        [Header("Ammo")]
        public AmmoKind ammoKind = AmmoKind.Light;
        public int magazineSize = 12;
        public int startingReserveAmmo = 36;

        [Header("Combat")]
        public float damage = 20f;
        public float range = 120f;
        public float fireRate = 3f;
        public float reloadTime = 1.4f;
        public bool automatic;
        public bool usesAmmo = true;
        public int pelletCount = 1;

        [Header("Feel")]
        [Range(0f, 6f)] public float spreadAngle = 1.2f;
        public float adsSpreadMultiplier = 0.55f;
        public float hipSpreadMultiplier = 1f;
        public float cameraKick = 0.5f;
        public float cameraVerticalKick = 0.5f;
        public float cameraHorizontalKick = 0.08f;
        public float recoilRecovery = 16f;
        public float weaponKickDistance = 0.035f;
        public float weaponPitchKick = 1.5f;
        public float weaponReturnSpeed = 18f;
        public float crosshairFireBloom = 42f;
        public float crosshairMovementBloom = 26f;
        public float crosshairAimMultiplier = 0.58f;
        public float crosshairCrouchMultiplier = 0.74f;
        public float crosshairProneMultiplier = 0.56f;
        public float crosshairRecoverySpeed = 9f;
        public float aimSway = 0.08f;
        public float scopedBreathingSway = 0.04f;
        public bool showDamageNumbers = true;
        public float adsFieldOfView = 48f;
        public float switchTime = 0.22f;
        public Color tracerColor = new Color(1f, 0.83f, 0.25f, 1f);
        public GameObject impactPrefab;
        public ParticleSystem muzzleFlashPrefab;

        [Header("Audio Hooks")]
        public AudioClip fireAudioOverride;
        public AudioClip suppressedFireAudioOverride;
        public AudioClip reloadAudioOverride;
        public AudioClip dryFireAudioOverride;
        public AudioClip hitConfirmAudioOverride;
        public AudioClip headshotConfirmAudioOverride;
        public AudioClip killConfirmAudioOverride;

        [Header("Attachment Readiness")]
        public bool supportsOptic = true;
        public bool supportsMuzzle = true;
        public bool supportsMagazine = true;
        public bool supportsGrip = true;
        public bool supportsStock = true;
        public bool supportsLaser = true;
    }
}
