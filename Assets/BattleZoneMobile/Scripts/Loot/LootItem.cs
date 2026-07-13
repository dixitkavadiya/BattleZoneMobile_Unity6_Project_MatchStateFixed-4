using UnityEngine;

namespace BattleZoneMobile
{
    public enum LootKind
    {
        Pistol,
        AssaultRifle,
        SMG,
        Sniper,
        Shotgun,
        LightAmmo,
        MediumAmmo,
        HeavyAmmo,
        ShellAmmo,
        Medkit,
        Bandage,
        ArmorPlate,
        ArmorVest,
        Helmet,
        Backpack,
        EnergyItem,
        Grenade,
        SmokeGrenade,
        Melee,
        Throwable,
        WeaponAttachment
    }

    public enum LootRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic
    }

    public class LootItem : MonoBehaviour
    {
        [SerializeField] private LootKind kind = LootKind.MediumAmmo;
        [SerializeField] private LootRarity rarity = LootRarity.Common;
        [SerializeField] private int amount = 30;
        [SerializeField] private string displayName = "Ammo";
        [SerializeField] private int backpackCost = 1;
        [SerializeField] private int tier;
        [SerializeField] private InventoryItemData itemData;
        [SerializeField] private WeaponAttachmentData attachmentData;
        [SerializeField] private float spinSpeed = 65f;

        private bool pickupConsumed;

        public LootKind Kind => itemData != null ? itemData.Kind : kind;
        public LootRarity Rarity => itemData != null ? itemData.Rarity : rarity;
        public int Amount => itemData != null ? itemData.Quantity : amount;
        public string DisplayName => itemData != null ? itemData.DisplayName : displayName;
        public string RarityLabel => Rarity.ToString();
        public int BackpackCost => itemData != null ? itemData.BackpackCost : backpackCost;
        public int Tier => itemData != null ? itemData.Tier : tier;
        public InventoryItemData ItemData => itemData;
        public WeaponAttachmentData AttachmentData => itemData != null && itemData.AttachmentData != null ? itemData.AttachmentData : attachmentData;

        public void Configure(LootKind lootKind, int lootAmount, string lootDisplayName, int cost = 1, LootRarity lootRarity = LootRarity.Common)
        {
            Configure(lootKind, lootAmount, lootDisplayName, cost, lootRarity, 0, null, null);
        }

        public void Configure(
            LootKind lootKind,
            int lootAmount,
            string lootDisplayName,
            int cost,
            LootRarity lootRarity,
            int lootTier,
            WeaponAttachmentData attachment,
            InventoryItemData dataReference)
        {
            kind = lootKind;
            rarity = lootRarity;
            amount = Mathf.Max(1, lootAmount);
            displayName = lootDisplayName;
            backpackCost = Mathf.Max(0, cost);
            tier = Mathf.Max(0, lootTier);
            attachmentData = attachment;
            itemData = dataReference;
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        }

        public bool TryPickUp(PlayerInventory inventory)
        {
            if (inventory == null || pickupConsumed)
            {
                return false;
            }

            if (!inventory.TryAddLoot(this))
            {
                return false;
            }

            pickupConsumed = true;
            Destroy(gameObject);
            return true;
        }

        public void PickUp(PlayerInventory inventory)
        {
            TryPickUp(inventory);
        }
    }
}
