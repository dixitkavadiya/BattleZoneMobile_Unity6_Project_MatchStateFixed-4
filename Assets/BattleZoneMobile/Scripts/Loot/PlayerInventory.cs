using System;
using UnityEngine;
using UnityEngine.Events;

namespace BattleZoneMobile
{
    [Serializable]
    public class InventoryChangedEvent : UnityEvent<int, string>
    {
    }

    [Serializable]
    public class BackpackChangedEvent : UnityEvent<string, int, int>
    {
    }

    public class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private WeaponController weaponController;
        [SerializeField] private Health playerHealth;
        [SerializeField] private PlayerEquipment equipment;
        [SerializeField] private int medkitHealAmount = 50;
        [SerializeField] private int bandageHealAmount = 18;
        [SerializeField] private int backpackCapacity = 120;

        public InventoryChangedEvent onInventoryChanged = new InventoryChangedEvent();
        public BackpackChangedEvent onBackpackChanged = new BackpackChangedEvent();

        private int medkits;
        private int bandages;
        private int lightAmmo;
        private int mediumAmmo;
        private int heavyAmmo;
        private int shellAmmo;
        private int backpackUsed;
        private int startingBackpackCapacity;
        private string lastPickupText = string.Empty;

        public int Medkits => medkits;
        public int Bandages => bandages;
        public int BackpackUsed => backpackUsed;
        public int BackpackCapacity => backpackCapacity;

        public void ConfigureForRuntime(WeaponController weapons, Health health)
        {
            weaponController = weapons;
            playerHealth = health;
            equipment = GetComponent<PlayerEquipment>();
        }

        private void Awake()
        {
            startingBackpackCapacity = backpackCapacity;
            if (weaponController == null)
            {
                weaponController = GetComponent<WeaponController>();
            }

            if (playerHealth == null)
            {
                playerHealth = GetComponent<Health>();
            }

            if (equipment == null)
            {
                equipment = GetComponent<PlayerEquipment>();
            }
        }

        public void AddLoot(LootKind kind, int amount)
        {
            int safeAmount = Mathf.Max(1, amount);

            switch (kind)
            {
                case LootKind.Pistol:
                    UnlockWeapon(WeaponSlot.Pistol, "Picked up Pistol");
                    break;
                case LootKind.AssaultRifle:
                    UnlockWeapon(WeaponSlot.AssaultRifle, "Picked up Assault Rifle");
                    break;
                case LootKind.SMG:
                    UnlockWeapon(WeaponSlot.SMG, "Picked up SMG");
                    break;
                case LootKind.Sniper:
                    UnlockWeapon(WeaponSlot.Sniper, "Picked up Sniper");
                    break;
                case LootKind.Shotgun:
                    UnlockWeapon(WeaponSlot.Shotgun, "Picked up Shotgun");
                    break;
                case LootKind.LightAmmo:
                    AddAmmo(AmmoKind.Light, safeAmount, 1, "light ammo");
                    break;
                case LootKind.MediumAmmo:
                    AddAmmo(AmmoKind.Medium, safeAmount, 1, "medium ammo");
                    break;
                case LootKind.HeavyAmmo:
                    AddAmmo(AmmoKind.Heavy, safeAmount, 2, "heavy ammo");
                    break;
                case LootKind.ShellAmmo:
                    AddAmmo(AmmoKind.Shell, safeAmount, 2, "shells");
                    break;
                case LootKind.Medkit:
                    if (TryUseBackpackSpace(safeAmount * 6))
                    {
                        medkits += safeAmount;
                        lastPickupText = safeAmount == 1 ? "Picked up Medkit" : $"+{safeAmount} medkits";
                    }
                    else
                    {
                        lastPickupText = "Backpack full";
                    }
                    break;
                case LootKind.Bandage:
                    if (TryUseBackpackSpace(safeAmount * 2))
                    {
                        bandages += safeAmount;
                        lastPickupText = safeAmount == 1 ? "Picked up Bandage" : $"+{safeAmount} bandages";
                    }
                    else
                    {
                        lastPickupText = "Backpack full";
                    }
                    break;
                case LootKind.ArmorPlate:
                    if (TryUseBackpackSpace(safeAmount * 4))
                    {
                        equipment?.RestoreArmorPlate(28f * safeAmount);
                        backpackUsed = Mathf.Max(0, backpackUsed - safeAmount * 4);
                        lastPickupText = safeAmount == 1 ? "Used Armor Plate" : $"Used {safeAmount} armor plates";
                        RuntimeAudioBank.Instance?.PlayPickup(transform.position);
                    }
                    else
                    {
                        lastPickupText = "Backpack full";
                    }
                    break;
                case LootKind.ArmorVest:
                    equipment?.EquipArmorTier(2);
                    lastPickupText = "Equipped Tier 2 Armor";
                    RuntimeAudioBank.Instance?.PlayPickup(transform.position);
                    break;
                case LootKind.Helmet:
                    equipment?.EquipHelmetTier(2);
                    lastPickupText = "Equipped Tier 2 Helmet";
                    RuntimeAudioBank.Instance?.PlayPickup(transform.position);
                    break;
                case LootKind.Backpack:
                    backpackCapacity = Mathf.Min(220, backpackCapacity + 40 * safeAmount);
                    lastPickupText = safeAmount == 1 ? "Equipped Backpack" : $"Equipped Backpack +{safeAmount}";
                    RuntimeAudioBank.Instance?.PlayPickup(transform.position);
                    break;
            }

            RaiseChanged();
        }

        public void UseMedkit()
        {
            if (medkits > 0 && TryHeal(medkitHealAmount))
            {
                medkits--;
                backpackUsed = Mathf.Max(0, backpackUsed - 6);
                lastPickupText = "Used Medkit";
                RaiseChanged();
            }
            else if (bandages > 0 && TryHeal(bandageHealAmount))
            {
                bandages--;
                backpackUsed = Mathf.Max(0, backpackUsed - 2);
                lastPickupText = "Used Bandage";
                RaiseChanged();
            }
        }

        public void ResetInventory()
        {
            medkits = 0;
            bandages = 0;
            lightAmmo = 0;
            mediumAmmo = 0;
            heavyAmmo = 0;
            shellAmmo = 0;
            backpackUsed = 0;
            backpackCapacity = startingBackpackCapacity > 0 ? startingBackpackCapacity : backpackCapacity;
            lastPickupText = string.Empty;
            RaiseChanged();
        }

        public string BuildBackpackSummary()
        {
            string weapons = weaponController != null ? weaponController.BuildWeaponSummary() : "Weapons: None";
            string equipmentSummary = equipment != null ? equipment.BuildEquipmentSummary() : "Armor Tier 0 | Helmet Tier 0";
            return $"{weapons}\n" +
                   $"{equipmentSummary}\n" +
                   $"Backpack {backpackUsed}/{backpackCapacity}\n" +
                   $"Light Ammo: {lightAmmo}\n" +
                   $"Medium Ammo: {mediumAmmo}\n" +
                   $"Heavy Ammo: {heavyAmmo}\n" +
                   $"Shells: {shellAmmo}\n" +
                   $"Medkits: {medkits}\n" +
                   $"Bandages: {bandages}";
        }

        private void UnlockWeapon(WeaponSlot slot, string message)
        {
            weaponController?.UnlockWeapon(slot, 0);
            lastPickupText = message;
            RuntimeAudioBank.Instance?.PlayPickup(transform.position);
        }

        private void AddAmmo(AmmoKind ammoKind, int amount, int costPerRound, string label)
        {
            int accepted = AddAmmoToBackpack(ammoKind, amount, costPerRound);
            if (accepted > 0)
            {
                weaponController?.AddReserveAmmo(ammoKind, accepted);
                lastPickupText = $"+{accepted} {label}";
                RuntimeAudioBank.Instance?.PlayPickup(transform.position);
            }
            else
            {
                lastPickupText = "Backpack full";
            }
        }

        private int AddAmmoToBackpack(AmmoKind ammoKind, int amount, int costPerRound)
        {
            int accepted = Mathf.Min(amount, Mathf.Max(0, (backpackCapacity - backpackUsed) / Mathf.Max(1, costPerRound)));
            if (accepted <= 0)
            {
                return 0;
            }

            backpackUsed += accepted * Mathf.Max(1, costPerRound);

            switch (ammoKind)
            {
                case AmmoKind.Light:
                    lightAmmo += accepted;
                    break;
                case AmmoKind.Medium:
                    mediumAmmo += accepted;
                    break;
                case AmmoKind.Heavy:
                    heavyAmmo += accepted;
                    break;
                case AmmoKind.Shell:
                    shellAmmo += accepted;
                    break;
            }

            return accepted;
        }

        private bool TryUseBackpackSpace(int cost)
        {
            if (backpackUsed + cost > backpackCapacity)
            {
                return false;
            }

            backpackUsed += cost;
            return true;
        }

        private bool TryHeal(int amount)
        {
            if (playerHealth == null || !playerHealth.IsAlive || playerHealth.CurrentHealth >= playerHealth.MaxHealth)
            {
                return false;
            }

            playerHealth.Heal(amount);
            return true;
        }

        private void RaiseChanged()
        {
            onInventoryChanged.Invoke(medkits, lastPickupText);
            onBackpackChanged.Invoke(BuildBackpackSummary(), backpackUsed, backpackCapacity);
        }
    }
}
