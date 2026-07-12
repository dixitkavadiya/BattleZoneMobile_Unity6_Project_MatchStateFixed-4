using System;
using UnityEngine;

namespace BattleZoneMobile
{
    [Serializable]
    public class ModularWeaponSlotState
    {
        public CombatLoadoutSlot slot;
        public AdvancedWeaponData data;
        public ModularWeaponBase weapon;
    }

    public class ModularWeaponLoadout : MonoBehaviour
    {
        [Header("Runtime References")]
        [SerializeField] private Camera aimCamera;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private Animator visualAnimator;
        [SerializeField] private ThirdPersonMobileController controller;
        [SerializeField] private CombatRecoilApplicator recoilApplicator;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private LayerMask hitMask = ~0;

        [Header("Slots")]
        [SerializeField] private ModularWeaponSlotState primary = new ModularWeaponSlotState { slot = CombatLoadoutSlot.Primary };
        [SerializeField] private ModularWeaponSlotState secondary = new ModularWeaponSlotState { slot = CombatLoadoutSlot.Secondary };
        [SerializeField] private ModularWeaponSlotState pistol = new ModularWeaponSlotState { slot = CombatLoadoutSlot.Pistol };
        [SerializeField] private CombatLoadoutSlot activeSlot = CombatLoadoutSlot.Pistol;

        private bool fireHeld;
        private bool controlsEnabled = true;

        public ModularWeaponBase CurrentWeapon => GetSlot(activeSlot).weapon;
        public AdvancedWeaponData CurrentData => GetSlot(activeSlot).data;
        public CombatLoadoutSlot ActiveSlot => activeSlot;
        public bool ControlsEnabled => controlsEnabled;

        public void ConfigureForRuntime(
            Camera camera,
            Transform muzzle,
            Animator animator,
            ThirdPersonMobileController playerController,
            CombatRecoilApplicator recoil,
            UIManager hud,
            LayerMask mask,
            AdvancedWeaponData[] startingWeapons)
        {
            aimCamera = camera;
            muzzlePoint = muzzle;
            visualAnimator = animator;
            controller = playerController;
            recoilApplicator = recoil;
            uiManager = hud;
            hitMask = mask;

            if (startingWeapons != null)
            {
                for (int i = 0; i < startingWeapons.Length; i++)
                {
                    TryEquipPickup(startingWeapons[i], false);
                }
            }

            if (pistol.weapon != null)
            {
                EquipSlot(CombatLoadoutSlot.Pistol);
            }
            else if (primary.weapon != null)
            {
                EquipSlot(CombatLoadoutSlot.Primary);
            }
            else
            {
                UpdateHud();
            }
        }

        private void Update()
        {
            if (!controlsEnabled || !fireHeld || CurrentWeapon == null || CurrentData == null)
            {
                return;
            }

            if (CurrentWeapon.CurrentFireMode == CombatFireMode.FullAuto)
            {
                CurrentWeapon.TryFire(CurrentWeapon.CurrentFireMode, true, IsAiming(), IsMoving(), IsCrouching(), IsProne());
                UpdateHud();
            }
        }

        public void FirePressed()
        {
            if (!controlsEnabled)
            {
                return;
            }

            fireHeld = true;
            ModularWeaponBase weapon = CurrentWeapon;
            if (weapon == null)
            {
                UpdateHud();
                return;
            }

            weapon.TryFire(weapon.CurrentFireMode, true, IsAiming(), IsMoving(), IsCrouching(), IsProne());
            UpdateHud();
        }

        public void FireReleased()
        {
            fireHeld = false;
        }

        public void ReloadPressed()
        {
            if (!controlsEnabled || CurrentWeapon == null)
            {
                return;
            }

            CurrentWeapon.RequestReload();
            UpdateHud();
        }

        public void SwitchNextWeapon()
        {
            CombatLoadoutSlot[] order = { CombatLoadoutSlot.Primary, CombatLoadoutSlot.Secondary, CombatLoadoutSlot.Pistol };
            int startIndex = Array.IndexOf(order, activeSlot);
            for (int offset = 1; offset <= order.Length; offset++)
            {
                CombatLoadoutSlot candidate = order[(Mathf.Max(0, startIndex) + offset) % order.Length];
                if (GetSlot(candidate).weapon != null)
                {
                    EquipSlot(candidate);
                    return;
                }
            }

            activeSlot = CombatLoadoutSlot.Primary;
            UpdateHud();
        }

        public void SwitchFireMode()
        {
            if (CurrentWeapon == null || CurrentData == null)
            {
                return;
            }

            CurrentWeapon.SwitchFireMode();
            UpdateHud();
        }

        public bool TryEquipPickup(AdvancedWeaponData data, bool makeActive = true)
        {
            if (data == null)
            {
                return false;
            }

            bool primaryOccupied = primary.weapon != null;
            CombatLoadoutSlot targetSlot = CombatWeaponRoster.ResolveLoadoutSlot(data, primaryOccupied && secondary.weapon == null);
            if (targetSlot == CombatLoadoutSlot.Primary && primary.weapon != null && secondary.weapon == null && data.WeaponType != CombatWeaponType.Pistol)
            {
                targetSlot = CombatLoadoutSlot.Secondary;
            }

            ModularWeaponSlotState slot = GetSlot(targetSlot);
            ReplaceSlot(slot, data);
            if (makeActive)
            {
                EquipSlot(targetSlot);
            }
            else
            {
                UpdateHud();
            }

            return true;
        }

        public void EquipSlot(CombatLoadoutSlot slot)
        {
            ModularWeaponSlotState previous = GetSlot(activeSlot);
            previous.weapon?.Unequip();

            activeSlot = slot;
            ModularWeaponSlotState next = GetSlot(activeSlot);
            next.weapon?.Equip();
            fireHeld = false;
            visualAnimator?.SetTrigger("WeaponSwitch");
            UpdateHud();
        }

        public void SetControlsEnabled(bool enabled)
        {
            controlsEnabled = enabled;
            if (!controlsEnabled)
            {
                fireHeld = false;
            }
        }

        private void ReplaceSlot(ModularWeaponSlotState slot, AdvancedWeaponData data)
        {
            if (slot.weapon != null)
            {
                Destroy(slot.weapon);
            }

            slot.data = data;
            slot.weapon = CreateWeaponComponent(data);
            slot.weapon.Initialize(BuildContext(), data);
            slot.weapon.Unequip();
        }

        private ModularWeaponBase CreateWeaponComponent(AdvancedWeaponData data)
        {
            switch (data.WeaponType)
            {
                case CombatWeaponType.AssaultRifle:
                    return gameObject.AddComponent<AssaultRifleWeapon>();
                case CombatWeaponType.SMG:
                    return gameObject.AddComponent<SMGWeapon>();
                case CombatWeaponType.Sniper:
                    return gameObject.AddComponent<SniperWeapon>();
                case CombatWeaponType.DMR:
                    return gameObject.AddComponent<DMRWeapon>();
                case CombatWeaponType.Shotgun:
                    return gameObject.AddComponent<ShotgunWeapon>();
                case CombatWeaponType.Melee:
                    return gameObject.AddComponent<MeleeWeapon>();
                case CombatWeaponType.Throwable:
                    return gameObject.AddComponent<ThrowableWeapon>();
                default:
                    return gameObject.AddComponent<PistolWeapon>();
            }
        }

        private ModularWeaponRuntimeContext BuildContext()
        {
            return new ModularWeaponRuntimeContext
            {
                owner = gameObject,
                aimCamera = aimCamera,
                muzzlePoint = muzzlePoint,
                visualAnimator = visualAnimator,
                controller = controller,
                recoilApplicator = recoilApplicator,
                impactFallbackPrefab = CurrentData != null && CurrentData.Vfx != null ? CurrentData.Vfx.impactPrefab : null,
                hitMask = hitMask
            };
        }

        private ModularWeaponSlotState GetSlot(CombatLoadoutSlot slot)
        {
            switch (slot)
            {
                case CombatLoadoutSlot.Secondary:
                    return secondary;
                case CombatLoadoutSlot.Pistol:
                    return pistol;
                default:
                    return primary;
            }
        }

        private bool IsAiming()
        {
            return controller != null && controller.IsAiming;
        }

        private bool IsMoving()
        {
            CharacterController characterController = GetComponent<CharacterController>();
            Vector3 velocity = characterController != null ? characterController.velocity : Vector3.zero;
            velocity.y = 0f;
            return velocity.sqrMagnitude > 0.12f;
        }

        private bool IsCrouching()
        {
            return controller != null && controller.IsCrouching;
        }

        private bool IsProne()
        {
            return controller != null && controller.IsProne;
        }

        private void UpdateHud()
        {
            if (uiManager == null)
            {
                return;
            }

            ModularWeaponBase weapon = CurrentWeapon;
            AdvancedWeaponData data = CurrentData;
            if (weapon == null || data == null)
            {
                uiManager.SetAdvancedWeaponHud("Empty Hands", "EMPTY", "NONE", 0, 0, false, "[]");
                return;
            }

            uiManager.SetAdvancedWeaponHud(
                data.DisplayName,
                activeSlot.ToString().ToUpperInvariant(),
                weapon.CurrentFireMode.ToString(),
                weapon.MagazineAmmo,
                weapon.ReserveAmmo,
                weapon.IsReloading,
                "[]");
        }
    }
}
