using UnityEngine;

namespace BattleZoneMobile
{
    public class CombatAnimationEventBridge : MonoBehaviour
    {
        [SerializeField] private WeaponController legacyWeaponController;
        [SerializeField] private ModularWeaponBase modularWeapon;

        public WeaponController LegacyWeaponController => legacyWeaponController;

        public void Configure(WeaponController legacyController, ModularWeaponBase activeModularWeapon)
        {
            legacyWeaponController = legacyController;
            modularWeapon = activeModularWeapon;
        }

        public void SetActiveModularWeapon(ModularWeaponBase activeWeapon)
        {
            modularWeapon = activeWeapon;
        }

        public void AnimEvent_Fire()
        {
            modularWeapon?.OnAnimationFireEvent();
        }

        public void AnimEvent_ReloadCommit()
        {
            modularWeapon?.OnAnimationReloadCommit();
        }

        public void AnimEvent_ReloadFinished()
        {
            modularWeapon?.OnAnimationReloadFinished();
        }

        public void AnimEvent_EquipFinished()
        {
            modularWeapon?.Equip();
        }

        public void AnimEvent_UnequipFinished()
        {
            modularWeapon?.Unequip();
        }
    }
}
