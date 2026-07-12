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
        Backpack
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
        [SerializeField] private float spinSpeed = 65f;

        public LootKind Kind => kind;
        public LootRarity Rarity => rarity;
        public int Amount => amount;
        public string DisplayName => displayName;
        public string RarityLabel => rarity.ToString();
        public int BackpackCost => backpackCost;

        public void Configure(LootKind lootKind, int lootAmount, string lootDisplayName, int cost = 1, LootRarity lootRarity = LootRarity.Common)
        {
            kind = lootKind;
            rarity = lootRarity;
            amount = Mathf.Max(1, lootAmount);
            displayName = lootDisplayName;
            backpackCost = Mathf.Max(0, cost);
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        }

        public void PickUp(PlayerInventory inventory)
        {
            if (inventory == null)
            {
                return;
            }

            inventory.AddLoot(kind, amount);
            Destroy(gameObject);
        }
    }
}
