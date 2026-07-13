using UnityEngine;

namespace BattleZoneMobile
{
    [CreateAssetMenu(fileName = "InventoryItemData", menuName = "BattleZone Mobile/Loot/Inventory Item")]
    public class InventoryItemData : ScriptableObject
    {
        [SerializeField] private string itemId = "item_id";
        [SerializeField] private string displayName = "Inventory Item";
        [SerializeField] private LootKind kind = LootKind.Medkit;
        [SerializeField] private LootRarity rarity = LootRarity.Common;
        [SerializeField] private int quantity = 1;
        [SerializeField] private int backpackCost = 1;
        [SerializeField] private int tier;
        [SerializeField] private WeaponAttachmentData attachmentData;

        public string ItemId => string.IsNullOrWhiteSpace(itemId) ? displayName.Replace(" ", "_").ToLowerInvariant() : itemId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? kind.ToString() : displayName;
        public LootKind Kind => kind;
        public LootRarity Rarity => rarity;
        public int Quantity => Mathf.Max(1, quantity);
        public int BackpackCost => Mathf.Max(0, backpackCost);
        public int Tier => Mathf.Max(0, tier);
        public WeaponAttachmentData AttachmentData => attachmentData;

        public void ConfigureRuntime(string id, string name, LootKind itemKind, LootRarity itemRarity, int amount, int cost, int itemTier, WeaponAttachmentData attachment)
        {
            itemId = id;
            displayName = name;
            kind = itemKind;
            rarity = itemRarity;
            quantity = Mathf.Max(1, amount);
            backpackCost = Mathf.Max(0, cost);
            tier = Mathf.Max(0, itemTier);
            attachmentData = attachment;
        }
    }
}
