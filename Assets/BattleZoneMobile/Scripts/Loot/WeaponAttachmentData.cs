using UnityEngine;

namespace BattleZoneMobile
{
    public enum WeaponAttachmentSlot
    {
        Optic,
        Muzzle,
        Magazine,
        Grip,
        Stock,
        Laser
    }

    [CreateAssetMenu(fileName = "AttachmentData", menuName = "BattleZone Mobile/Loot/Weapon Attachment")]
    public class WeaponAttachmentData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string attachmentId = "attachment_id";
        [SerializeField] private string displayName = "Attachment";
        [SerializeField] private WeaponAttachmentSlot slot = WeaponAttachmentSlot.Optic;
        [SerializeField] private LootRarity rarity = LootRarity.Common;
        [SerializeField] private int backpackCost = 4;

        [Header("Compatibility")]
        [SerializeField] private WeaponSlot[] compatibleWeaponSlots = new WeaponSlot[0];

        [Header("Modifiers")]
        [SerializeField, Min(0.1f)] private float recoilMultiplier = 1f;
        [SerializeField, Min(0.1f)] private float spreadMultiplier = 1f;
        [SerializeField, Min(0.1f)] private float reloadMultiplier = 1f;
        [SerializeField, Min(0.1f)] private float magazineMultiplier = 1f;
        [SerializeField, Min(0.1f)] private float fireRateMultiplier = 1f;
        [SerializeField] private bool suppressesFireAudio;

        public string AttachmentId => string.IsNullOrWhiteSpace(attachmentId) ? displayName.Replace(" ", "_").ToLowerInvariant() : attachmentId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? slot.ToString() : displayName;
        public WeaponAttachmentSlot Slot => slot;
        public LootRarity Rarity => rarity;
        public int BackpackCost => Mathf.Max(1, backpackCost);
        public float RecoilMultiplier => Mathf.Max(0.1f, recoilMultiplier);
        public float SpreadMultiplier => Mathf.Max(0.1f, spreadMultiplier);
        public float ReloadMultiplier => Mathf.Max(0.1f, reloadMultiplier);
        public float MagazineMultiplier => Mathf.Max(0.1f, magazineMultiplier);
        public float FireRateMultiplier => Mathf.Max(0.1f, fireRateMultiplier);
        public bool SuppressesFireAudio => suppressesFireAudio;

        public void ConfigureRuntime(
            string id,
            string name,
            WeaponAttachmentSlot attachmentSlot,
            LootRarity itemRarity,
            int cost,
            WeaponSlot[] compatibleSlots,
            float recoil,
            float spread,
            float reload,
            float magazine,
            float fireRate,
            bool suppressor)
        {
            attachmentId = id;
            displayName = name;
            slot = attachmentSlot;
            rarity = itemRarity;
            backpackCost = Mathf.Max(1, cost);
            compatibleWeaponSlots = compatibleSlots != null ? compatibleSlots : new WeaponSlot[0];
            recoilMultiplier = Mathf.Max(0.1f, recoil);
            spreadMultiplier = Mathf.Max(0.1f, spread);
            reloadMultiplier = Mathf.Max(0.1f, reload);
            magazineMultiplier = Mathf.Max(0.1f, magazine);
            fireRateMultiplier = Mathf.Max(0.1f, fireRate);
            suppressesFireAudio = suppressor;
        }

        public bool IsCompatible(WeaponDefinition definition)
        {
            if (definition == null || !WeaponSupportsSlot(definition))
            {
                return false;
            }

            if (compatibleWeaponSlots == null || compatibleWeaponSlots.Length == 0)
            {
                return true;
            }

            for (int i = 0; i < compatibleWeaponSlots.Length; i++)
            {
                if (compatibleWeaponSlots[i] == definition.slot)
                {
                    return true;
                }
            }

            return false;
        }

        private bool WeaponSupportsSlot(WeaponDefinition definition)
        {
            switch (slot)
            {
                case WeaponAttachmentSlot.Optic:
                    return definition.supportsOptic;
                case WeaponAttachmentSlot.Muzzle:
                    return definition.supportsMuzzle;
                case WeaponAttachmentSlot.Magazine:
                    return definition.supportsMagazine;
                case WeaponAttachmentSlot.Grip:
                    return definition.supportsGrip;
                case WeaponAttachmentSlot.Stock:
                    return definition.supportsStock;
                case WeaponAttachmentSlot.Laser:
                    return definition.supportsLaser;
                default:
                    return false;
            }
        }
    }
}
