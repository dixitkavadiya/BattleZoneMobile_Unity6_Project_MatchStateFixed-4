using System;
using System.Collections;
using System.Collections.Generic;
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

    [Serializable]
    public class HealingProgressEvent : UnityEvent<string, float, bool>
    {
    }

    public class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private WeaponController weaponController;
        [SerializeField] private ModularWeaponLoadout modularWeaponLoadout;
        [SerializeField] private Health playerHealth;
        [SerializeField] private PlayerEquipment equipment;
        [SerializeField] private CharacterController characterController;

        [Header("Backpack")]
        [SerializeField] private int baseBackpackCapacity = 90;
        [SerializeField] private int tierOneBackpackCapacity = 120;
        [SerializeField] private int tierTwoBackpackCapacity = 180;
        [SerializeField] private int tierThreeBackpackCapacity = 240;

        [Header("Healing")]
        [SerializeField] private int medkitHealAmount = 100;
        [SerializeField] private int bandageHealAmount = 18;
        [SerializeField] private int energyHealAmount = 26;
        [SerializeField] private float medkitUseSeconds = 5.8f;
        [SerializeField] private float bandageUseSeconds = 2.8f;
        [SerializeField] private float energyUseSeconds = 3.5f;
        [SerializeField, Range(0.1f, 1f)] private float bandageHealthCap = 0.75f;
        [SerializeField] private bool allowMovementWhileHealing = true;
        [SerializeField] private float cancelUseMoveSpeed = 4.2f;

        public InventoryChangedEvent onInventoryChanged = new InventoryChangedEvent();
        public BackpackChangedEvent onBackpackChanged = new BackpackChangedEvent();
        public HealingProgressEvent onHealingProgress = new HealingProgressEvent();

        private readonly List<WeaponAttachmentData> storedAttachments = new List<WeaponAttachmentData>();
        private int medkits;
        private int bandages;
        private int energyItems;
        private int grenades;
        private int smokeGrenades;
        private int lightAmmo;
        private int mediumAmmo;
        private int heavyAmmo;
        private int shellAmmo;
        private int backpackTier;
        private int backpackCapacity;
        private int backpackUsed;
        private string lastPickupText = string.Empty;
        private Coroutine healingRoutine;

        public int Medkits => medkits;
        public int Bandages => bandages;
        public int EnergyItems => energyItems;
        public int Grenades => grenades;
        public int SmokeGrenades => smokeGrenades;
        public int BackpackTier => backpackTier;
        public int BackpackUsed => backpackUsed;
        public int BackpackCapacity => backpackCapacity;
        public bool IsUsingHealingItem => healingRoutine != null;

        public void ConfigureForRuntime(WeaponController weapons, Health health)
        {
            ConfigureForRuntime(weapons, health, null);
        }

        public void ConfigureForRuntime(WeaponController weapons, Health health, ModularWeaponLoadout loadout)
        {
            weaponController = weapons;
            modularWeaponLoadout = loadout;
            playerHealth = health;
            equipment = GetComponent<PlayerEquipment>();
            characterController = GetComponent<CharacterController>();
            EnsureCapacityInitialized();
        }

        private void Awake()
        {
            if (weaponController == null)
            {
                weaponController = GetComponent<WeaponController>();
            }

            if (playerHealth == null)
            {
                playerHealth = GetComponent<Health>();
            }

            if (modularWeaponLoadout == null)
            {
                modularWeaponLoadout = GetComponent<ModularWeaponLoadout>();
            }

            if (equipment == null)
            {
                equipment = GetComponent<PlayerEquipment>();
            }

            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }

            EnsureCapacityInitialized();
        }

        public void AddLoot(LootKind kind, int amount)
        {
            TryAddLoot(kind, amount, 0, null, null, 0, kind.ToString(), LootRarity.Common);
        }

        public bool TryAddLoot(LootItem loot)
        {
            if (loot == null)
            {
                return false;
            }

            return TryAddLoot(
                loot.Kind,
                loot.Amount,
                loot.Tier,
                loot.AttachmentData,
                loot.ItemData,
                loot.BackpackCost,
                loot.DisplayName,
                loot.Rarity);
        }

        public bool TryAddLoot(
            LootKind kind,
            int amount,
            int tier,
            WeaponAttachmentData attachmentData,
            InventoryItemData itemData,
            int explicitBackpackCost,
            string displayName,
            LootRarity rarity)
        {
            EnsureCapacityInitialized();
            int safeAmount = Mathf.Max(1, amount);
            int cost = explicitBackpackCost > 0 ? explicitBackpackCost : ResolveBackpackCost(kind, attachmentData);
            bool accepted = false;

            switch (kind)
            {
                case LootKind.Pistol:
                    accepted = UnlockWeapon(WeaponSlot.Pistol, string.IsNullOrEmpty(displayName) ? "Picked up Pistol" : $"Picked up {displayName}");
                    break;
                case LootKind.AssaultRifle:
                    accepted = UnlockWeapon(WeaponSlot.AssaultRifle, string.IsNullOrEmpty(displayName) ? "Picked up Assault Rifle" : $"Picked up {displayName}");
                    break;
                case LootKind.SMG:
                    accepted = UnlockWeapon(WeaponSlot.SMG, string.IsNullOrEmpty(displayName) ? "Picked up SMG" : $"Picked up {displayName}");
                    break;
                case LootKind.Sniper:
                    accepted = UnlockWeapon(WeaponSlot.Sniper, string.IsNullOrEmpty(displayName) ? "Picked up Sniper" : $"Picked up {displayName}");
                    break;
                case LootKind.Shotgun:
                    accepted = UnlockWeapon(WeaponSlot.Shotgun, string.IsNullOrEmpty(displayName) ? "Picked up Shotgun" : $"Picked up {displayName}");
                    break;
                case LootKind.LightAmmo:
                    accepted = AddAmmo(AmmoKind.Light, safeAmount, cost, "light ammo");
                    break;
                case LootKind.MediumAmmo:
                    accepted = AddAmmo(AmmoKind.Medium, safeAmount, cost, "medium ammo");
                    break;
                case LootKind.HeavyAmmo:
                    accepted = AddAmmo(AmmoKind.Heavy, safeAmount, cost, "heavy ammo");
                    break;
                case LootKind.ShellAmmo:
                    accepted = AddAmmo(AmmoKind.Shell, safeAmount, cost, "shells");
                    break;
                case LootKind.Medkit:
                    accepted = AddStackedItem(ref medkits, safeAmount, cost, "Medkit", "medkits");
                    break;
                case LootKind.Bandage:
                    accepted = AddStackedItem(ref bandages, safeAmount, cost, "Bandage", "bandages");
                    break;
                case LootKind.EnergyItem:
                    accepted = AddStackedItem(ref energyItems, safeAmount, cost, "Energy Item", "energy items");
                    break;
                case LootKind.Grenade:
                    accepted = AddStackedItem(ref grenades, safeAmount, cost, "Grenade", "grenades");
                    break;
                case LootKind.SmokeGrenade:
                    accepted = AddStackedItem(ref smokeGrenades, safeAmount, cost, "Smoke Grenade", "smoke grenades");
                    break;
                case LootKind.ArmorPlate:
                    accepted = RestoreArmorPlate(safeAmount, cost);
                    break;
                case LootKind.ArmorVest:
                    accepted = TryEquipVest(tier > 0 ? tier : Mathf.Clamp(safeAmount, 1, 3));
                    break;
                case LootKind.Helmet:
                    accepted = TryEquipHelmet(tier > 0 ? tier : Mathf.Clamp(safeAmount, 1, 3));
                    break;
                case LootKind.Backpack:
                    accepted = TryEquipBackpack(tier > 0 ? tier : Mathf.Clamp(safeAmount, 1, 3));
                    break;
                case LootKind.WeaponAttachment:
                    accepted = AddAttachment(attachmentData, cost);
                    break;
                case LootKind.Melee:
                    lastPickupText = "Melee slot prepared";
                    accepted = true;
                    RuntimeAudioBank.Instance?.PlayPickup(transform.position);
                    break;
                case LootKind.Throwable:
                    accepted = AddStackedItem(ref grenades, safeAmount, cost, "Throwable", "throwables");
                    break;
            }

            if (!accepted && string.IsNullOrEmpty(lastPickupText))
            {
                lastPickupText = "Cannot pick up item";
            }

            RaiseChanged();
            return accepted;
        }

        public void AddAdvancedWeapon(AdvancedWeaponData weaponData, int reserveAmmoBonus)
        {
            if (weaponData == null)
            {
                return;
            }

            modularWeaponLoadout?.TryEquipPickup(weaponData, true);
            if (TryMapLegacyWeaponSlot(weaponData, out WeaponSlot legacySlot))
            {
                weaponController?.UnlockWeapon(legacySlot, Mathf.Max(0, reserveAmmoBonus));
            }

            lastPickupText = $"Picked up {weaponData.DisplayName}";
            RuntimeAudioBank.Instance?.PlayPickup(transform.position);
            RaiseChanged();
        }

        public void UseMedkit()
        {
            StartHealingUse("Medkit", medkits, medkitUseSeconds, medkitHealAmount, 1f, () =>
            {
                medkits--;
                backpackUsed = Mathf.Max(0, backpackUsed - ResolveBackpackCost(LootKind.Medkit, null));
            });
        }

        public void UseBandage()
        {
            StartHealingUse("Bandage", bandages, bandageUseSeconds, bandageHealAmount, bandageHealthCap, () =>
            {
                bandages--;
                backpackUsed = Mathf.Max(0, backpackUsed - ResolveBackpackCost(LootKind.Bandage, null));
            });
        }

        public void UseEnergyItem()
        {
            StartHealingUse("Energy Item", energyItems, energyUseSeconds, energyHealAmount, 1f, () =>
            {
                energyItems--;
                backpackUsed = Mathf.Max(0, backpackUsed - ResolveBackpackCost(LootKind.EnergyItem, null));
            });
        }

        public void UseBestHealingItem()
        {
            if (playerHealth == null || !playerHealth.IsAlive)
            {
                return;
            }

            if (playerHealth.CurrentHealth <= playerHealth.MaxHealth * 0.45f && medkits > 0)
            {
                UseMedkit();
            }
            else if (bandages > 0 && playerHealth.CurrentHealth < playerHealth.MaxHealth * bandageHealthCap)
            {
                UseBandage();
            }
            else if (energyItems > 0 && playerHealth.CurrentHealth < playerHealth.MaxHealth)
            {
                UseEnergyItem();
            }
            else if (medkits > 0)
            {
                UseMedkit();
            }
        }

        public void UseOrEquipBestInventoryAction()
        {
            if (TryEquipFirstCompatibleAttachment())
            {
                return;
            }

            UseBestHealingItem();
        }

        public void DropLightestInventoryItem()
        {
            if (smokeGrenades > 0)
            {
                smokeGrenades--;
                backpackUsed = Mathf.Max(0, backpackUsed - ResolveBackpackCost(LootKind.SmokeGrenade, null));
                lastPickupText = "Dropped Smoke Grenade";
            }
            else if (grenades > 0)
            {
                grenades--;
                backpackUsed = Mathf.Max(0, backpackUsed - ResolveBackpackCost(LootKind.Grenade, null));
                lastPickupText = "Dropped Grenade";
            }
            else if (energyItems > 0)
            {
                energyItems--;
                backpackUsed = Mathf.Max(0, backpackUsed - ResolveBackpackCost(LootKind.EnergyItem, null));
                lastPickupText = "Dropped Energy Item";
            }
            else if (bandages > 0)
            {
                bandages--;
                backpackUsed = Mathf.Max(0, backpackUsed - ResolveBackpackCost(LootKind.Bandage, null));
                lastPickupText = "Dropped Bandage";
            }
            else if (storedAttachments.Count > 0)
            {
                WeaponAttachmentData attachment = storedAttachments[storedAttachments.Count - 1];
                storedAttachments.RemoveAt(storedAttachments.Count - 1);
                backpackUsed = Mathf.Max(0, backpackUsed - (attachment != null ? attachment.BackpackCost : ResolveBackpackCost(LootKind.WeaponAttachment, null)));
                lastPickupText = attachment != null ? $"Dropped {attachment.DisplayName}" : "Dropped attachment";
            }
            else
            {
                lastPickupText = "Nothing light to drop";
            }

            RaiseChanged();
        }

        public void CancelActiveUse(string reason)
        {
            if (healingRoutine == null)
            {
                return;
            }

            StopCoroutine(healingRoutine);
            healingRoutine = null;
            onHealingProgress.Invoke(reason, 0f, false);
            lastPickupText = reason;
            RaiseChanged();
        }

        public void ResetInventory()
        {
            if (healingRoutine != null)
            {
                StopCoroutine(healingRoutine);
                healingRoutine = null;
            }

            medkits = 0;
            bandages = 0;
            energyItems = 0;
            grenades = 0;
            smokeGrenades = 0;
            lightAmmo = 0;
            mediumAmmo = 0;
            heavyAmmo = 0;
            shellAmmo = 0;
            backpackTier = 0;
            backpackUsed = 0;
            backpackCapacity = Mathf.Max(1, baseBackpackCapacity);
            storedAttachments.Clear();
            lastPickupText = string.Empty;
            onHealingProgress.Invoke(string.Empty, 0f, false);
            RaiseChanged();
        }

        public string BuildBackpackSummary()
        {
            string weapons = weaponController != null ? weaponController.BuildWeaponSummary() : "Weapons: None";
            string equipmentSummary = equipment != null ? equipment.BuildEquipmentSummary() : "Vest T0 | Helmet T0";
            string attachments = BuildStoredAttachmentSummary();
            string currentAttachmentSummary = weaponController != null ? weaponController.BuildAttachmentSummary() : "Attachments: None";
            return $"{weapons}\n" +
                   $"{equipmentSummary}\n" +
                   $"Backpack T{backpackTier} {backpackUsed}/{backpackCapacity}\n" +
                   $"Light Ammo: {lightAmmo} | Medium: {mediumAmmo}\n" +
                   $"Heavy Ammo: {heavyAmmo} | Shells: {shellAmmo}\n" +
                   $"Medkits: {medkits} | Bandages: {bandages} | Energy: {energyItems}\n" +
                   $"Grenades: {grenades} | Smoke: {smokeGrenades}\n" +
                   $"{currentAttachmentSummary}\n" +
                   $"Stored Attachments: {attachments}";
        }

        private bool UnlockWeapon(WeaponSlot slot, string message)
        {
            weaponController?.UnlockWeapon(slot, 0);
            lastPickupText = message;
            RuntimeAudioBank.Instance?.PlayPickup(transform.position);
            return true;
        }

        private bool AddAmmo(AmmoKind ammoKind, int amount, int costPerRound, string label)
        {
            int accepted = AddAmmoToBackpack(ammoKind, amount, costPerRound);
            if (accepted > 0)
            {
                weaponController?.AddReserveAmmo(ammoKind, accepted);
                lastPickupText = $"+{accepted} {label}";
                RuntimeAudioBank.Instance?.PlayPickup(transform.position);
                return true;
            }

            lastPickupText = "Backpack full";
            return false;
        }

        private bool AddStackedItem(ref int count, int amount, int costPerItem, string singleName, string pluralName)
        {
            int accepted = AddGenericToBackpack(amount, costPerItem);
            if (accepted <= 0)
            {
                lastPickupText = "Backpack full";
                return false;
            }

            count += accepted;
            lastPickupText = accepted == 1 ? $"Picked up {singleName}" : $"+{accepted} {pluralName}";
            RuntimeAudioBank.Instance?.PlayPickup(transform.position);
            return true;
        }

        private bool AddAttachment(WeaponAttachmentData attachment, int fallbackCost)
        {
            if (attachment == null)
            {
                lastPickupText = "Missing attachment data";
                return false;
            }

            int cost = Mathf.Max(1, attachment.BackpackCost > 0 ? attachment.BackpackCost : fallbackCost);
            if (!TryUseBackpackSpace(cost))
            {
                lastPickupText = "Backpack full";
                return false;
            }

            storedAttachments.Add(attachment);
            lastPickupText = $"Picked up {attachment.DisplayName}";
            RuntimeAudioBank.Instance?.PlayPickup(transform.position);
            return true;
        }

        private bool TryEquipFirstCompatibleAttachment()
        {
            if (weaponController == null || storedAttachments.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < storedAttachments.Count; i++)
            {
                WeaponAttachmentData attachment = storedAttachments[i];
                if (attachment == null)
                {
                    continue;
                }

                WeaponAttachmentData replacement = weaponController.GetPotentialReplacementAttachment(attachment);
                int usedAfterRemovingStoredAttachment = Mathf.Max(0, backpackUsed - attachment.BackpackCost);
                if (replacement != null && usedAfterRemovingStoredAttachment + replacement.BackpackCost > backpackCapacity)
                {
                    continue;
                }

                if (weaponController.TryAttachAttachment(attachment, out string message))
                {
                    storedAttachments.RemoveAt(i);
                    backpackUsed = usedAfterRemovingStoredAttachment;
                    WeaponAttachmentData replaced = weaponController.LastReplacedAttachment;
                    if (replaced != null)
                    {
                        storedAttachments.Add(replaced);
                        backpackUsed = Mathf.Min(backpackCapacity, backpackUsed + replaced.BackpackCost);
                    }

                    lastPickupText = message;
                    RuntimeAudioBank.Instance?.PlayPickup(transform.position);
                    RaiseChanged();
                    return true;
                }
            }

            lastPickupText = "No compatible attachment";
            RaiseChanged();
            return false;
        }

        private bool RestoreArmorPlate(int amount, int costPerItem)
        {
            int accepted = AddGenericToBackpack(amount, costPerItem);
            if (accepted <= 0)
            {
                lastPickupText = "Backpack full";
                return false;
            }

            equipment?.RestoreArmorPlate(28f * accepted);
            backpackUsed = Mathf.Max(0, backpackUsed - accepted * Mathf.Max(1, costPerItem));
            lastPickupText = accepted == 1 ? "Used Armor Plate" : $"Used {accepted} armor plates";
            RuntimeAudioBank.Instance?.PlayPickup(transform.position);
            return true;
        }

        private bool TryEquipVest(int tier)
        {
            if (equipment == null)
            {
                lastPickupText = "No armor slot";
                return false;
            }

            bool accepted = equipment.TryEquipArmorTier(tier, out string message);
            lastPickupText = message;
            if (accepted)
            {
                RuntimeAudioBank.Instance?.PlayPickup(transform.position);
            }

            return accepted;
        }

        private bool TryEquipHelmet(int tier)
        {
            if (equipment == null)
            {
                lastPickupText = "No helmet slot";
                return false;
            }

            bool accepted = equipment.TryEquipHelmetTier(tier, out string message);
            lastPickupText = message;
            if (accepted)
            {
                RuntimeAudioBank.Instance?.PlayPickup(transform.position);
            }

            return accepted;
        }

        private bool TryEquipBackpack(int tier)
        {
            int clampedTier = Mathf.Clamp(tier, 1, 3);
            int capacity = CapacityForTier(clampedTier);
            if (capacity <= backpackCapacity)
            {
                lastPickupText = $"Kept Tier {backpackTier} Backpack";
                return false;
            }

            backpackTier = clampedTier;
            backpackCapacity = capacity;
            lastPickupText = $"Equipped Tier {backpackTier} Backpack";
            RuntimeAudioBank.Instance?.PlayPickup(transform.position);
            return true;
        }

        private void StartHealingUse(string label, int count, float duration, int healAmount, float healthCap01, Action consumeAction)
        {
            if (healingRoutine != null)
            {
                lastPickupText = "Already using item";
                RaiseChanged();
                return;
            }

            if (count <= 0)
            {
                lastPickupText = $"No {label}";
                RaiseChanged();
                return;
            }

            if (!CanHealToCap(healthCap01))
            {
                lastPickupText = "Health already high";
                RaiseChanged();
                return;
            }

            healingRoutine = StartCoroutine(HealingRoutine(label, duration, healAmount, healthCap01, consumeAction));
        }

        private IEnumerator HealingRoutine(string label, float duration, int healAmount, float healthCap01, Action consumeAction)
        {
            float elapsed = 0f;
            float safeDuration = Mathf.Max(0.1f, duration);
            onHealingProgress.Invoke(label, 0f, true);

            while (elapsed < safeDuration)
            {
                if (playerHealth == null || !playerHealth.IsAlive)
                {
                    healingRoutine = null;
                    onHealingProgress.Invoke("Use cancelled", 0f, false);
                    yield break;
                }

                if (!allowMovementWhileHealing && IsMovingTooFastForHealing())
                {
                    healingRoutine = null;
                    lastPickupText = "Use cancelled by movement";
                    onHealingProgress.Invoke(lastPickupText, 0f, false);
                    RaiseChanged();
                    yield break;
                }

                elapsed += Time.deltaTime;
                onHealingProgress.Invoke(label, Mathf.Clamp01(elapsed / safeDuration), true);
                yield return null;
            }

            consumeAction?.Invoke();
            HealToCap(healAmount, healthCap01);
            lastPickupText = $"Used {label}";
            healingRoutine = null;
            onHealingProgress.Invoke(string.Empty, 0f, false);
            RaiseChanged();
        }

        private bool CanHealToCap(float healthCap01)
        {
            if (playerHealth == null || !playerHealth.IsAlive)
            {
                return false;
            }

            float cap = Mathf.Max(1f, playerHealth.MaxHealth * Mathf.Clamp01(healthCap01));
            return playerHealth.CurrentHealth < cap - 0.5f;
        }

        private void HealToCap(int amount, float healthCap01)
        {
            if (playerHealth == null || amount <= 0)
            {
                return;
            }

            float cap = Mathf.Max(1f, playerHealth.MaxHealth * Mathf.Clamp01(healthCap01));
            float heal = Mathf.Min(amount, Mathf.Max(0f, cap - playerHealth.CurrentHealth));
            playerHealth.Heal(heal);
        }

        private bool IsMovingTooFastForHealing()
        {
            if (characterController == null)
            {
                return false;
            }

            Vector3 velocity = characterController.velocity;
            velocity.y = 0f;
            return velocity.magnitude > cancelUseMoveSpeed;
        }

        private int AddAmmoToBackpack(AmmoKind ammoKind, int amount, int costPerRound)
        {
            int accepted = AddGenericToBackpack(amount, costPerRound);
            if (accepted <= 0)
            {
                return 0;
            }

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

        private int AddGenericToBackpack(int amount, int costPerItem)
        {
            int safeCost = Mathf.Max(1, costPerItem);
            int accepted = Mathf.Min(Mathf.Max(1, amount), Mathf.Max(0, (backpackCapacity - backpackUsed) / safeCost));
            if (accepted <= 0)
            {
                return 0;
            }

            backpackUsed += accepted * safeCost;
            return accepted;
        }

        private bool TryUseBackpackSpace(int cost)
        {
            int safeCost = Mathf.Max(0, cost);
            if (backpackUsed + safeCost > backpackCapacity)
            {
                return false;
            }

            backpackUsed += safeCost;
            return true;
        }

        private int ResolveBackpackCost(LootKind kind, WeaponAttachmentData attachment)
        {
            switch (kind)
            {
                case LootKind.HeavyAmmo:
                case LootKind.ShellAmmo:
                    return 2;
                case LootKind.Medkit:
                    return 6;
                case LootKind.Bandage:
                    return 2;
                case LootKind.EnergyItem:
                    return 4;
                case LootKind.Grenade:
                    return 8;
                case LootKind.SmokeGrenade:
                case LootKind.Throwable:
                    return 6;
                case LootKind.ArmorPlate:
                    return 4;
                case LootKind.WeaponAttachment:
                    return attachment != null ? attachment.BackpackCost : 4;
                default:
                    return 1;
            }
        }

        private int CapacityForTier(int tier)
        {
            switch (Mathf.Clamp(tier, 0, 3))
            {
                case 1:
                    return Mathf.Max(baseBackpackCapacity, tierOneBackpackCapacity);
                case 2:
                    return Mathf.Max(tierOneBackpackCapacity, tierTwoBackpackCapacity);
                case 3:
                    return Mathf.Max(tierTwoBackpackCapacity, tierThreeBackpackCapacity);
                default:
                    return Mathf.Max(1, baseBackpackCapacity);
            }
        }

        private string BuildStoredAttachmentSummary()
        {
            if (storedAttachments.Count == 0)
            {
                return "None";
            }

            List<string> labels = new List<string>();
            for (int i = 0; i < storedAttachments.Count; i++)
            {
                WeaponAttachmentData attachment = storedAttachments[i];
                if (attachment != null)
                {
                    labels.Add($"{attachment.DisplayName} [{attachment.Rarity}]");
                }
            }

            return labels.Count == 0 ? "None" : string.Join(", ", labels);
        }

        private void EnsureCapacityInitialized()
        {
            if (backpackCapacity <= 0)
            {
                backpackCapacity = Mathf.Max(1, baseBackpackCapacity);
            }
        }

        private void RaiseChanged()
        {
            onInventoryChanged.Invoke(medkits, lastPickupText);
            onBackpackChanged.Invoke(BuildBackpackSummary(), backpackUsed, backpackCapacity);
        }

        private static bool TryMapLegacyWeaponSlot(AdvancedWeaponData weaponData, out WeaponSlot slot)
        {
            slot = WeaponSlot.Pistol;
            if (weaponData == null)
            {
                return false;
            }

            switch (weaponData.WeaponType)
            {
                case CombatWeaponType.AssaultRifle:
                    slot = WeaponSlot.AssaultRifle;
                    return true;
                case CombatWeaponType.SMG:
                    slot = WeaponSlot.SMG;
                    return true;
                case CombatWeaponType.Sniper:
                case CombatWeaponType.DMR:
                    slot = WeaponSlot.Sniper;
                    return true;
                case CombatWeaponType.Shotgun:
                    slot = WeaponSlot.Shotgun;
                    return true;
                case CombatWeaponType.Pistol:
                    slot = WeaponSlot.Pistol;
                    return true;
                default:
                    return false;
            }
        }
    }
}
