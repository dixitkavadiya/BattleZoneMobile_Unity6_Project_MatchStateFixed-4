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
        public float recoilRecovery = 16f;
        public float adsFieldOfView = 48f;
        public float switchTime = 0.22f;
        public Color tracerColor = new Color(1f, 0.83f, 0.25f, 1f);
        public GameObject impactPrefab;
        public ParticleSystem muzzleFlashPrefab;

        [Header("Attachment Readiness")]
        public bool supportsOptic = true;
        public bool supportsMuzzle = true;
        public bool supportsMagazine = true;
        public bool supportsGrip = true;
    }
}
