using UnityEngine;

namespace BattleZoneMobile
{
    public class CombatDebugWindow : MonoBehaviour
    {
        [SerializeField] private bool showCombatDebugWindow;
        [SerializeField] private WeaponController legacyWeaponController;
        [SerializeField] private ModularWeaponBase modularWeapon;
        [SerializeField] private CombatRecoilApplicator recoilApplicator;
        [SerializeField] private Health localHealth;

        public bool ShowCombatDebugWindow
        {
            get => showCombatDebugWindow;
            set => showCombatDebugWindow = value;
        }

        public void Configure(WeaponController legacyController, ModularWeaponBase activeModularWeapon, CombatRecoilApplicator recoil, Health health)
        {
            legacyWeaponController = legacyController;
            modularWeapon = activeModularWeapon;
            recoilApplicator = recoil;
            localHealth = health;
        }

        public void SetActiveModularWeapon(ModularWeaponBase activeWeapon)
        {
            modularWeapon = activeWeapon;
        }

        private void OnGUI()
        {
            if (!showCombatDebugWindow)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(18f, 420f, 380f, 230f), "Combat Debug", GUI.skin.window);
            GUILayout.Label($"Legacy weapon: {BuildLegacyWeaponLine()}");
            GUILayout.Label($"Modular weapon: {BuildModularWeaponLine()}");
            GUILayout.Label($"Delivery: {BuildDeliveryLine()}");
            GUILayout.Label($"Ammo: {BuildAmmoLine()}");
            GUILayout.Label($"Crosshair recoil: {(recoilApplicator != null ? recoilApplicator.CurrentCrosshairBloom.ToString("0.00") : "n/a")}");
            GUILayout.Label($"Health: {(localHealth != null ? localHealth.CurrentHealth.ToString("0") : "n/a")}");
            GUILayout.Label("Debug window default is OFF. Toggle only while testing combat.");
            GUILayout.EndArea();
        }

        private string BuildLegacyWeaponLine()
        {
            if (legacyWeaponController == null || legacyWeaponController.CurrentWeapon == null || legacyWeaponController.CurrentWeapon.definition == null)
            {
                return "none";
            }

            return legacyWeaponController.CurrentWeapon.definition.displayName;
        }

        private string BuildModularWeaponLine()
        {
            return modularWeapon != null && modularWeapon.Data != null ? modularWeapon.Data.DisplayName : "none";
        }

        private string BuildDeliveryLine()
        {
            return modularWeapon != null && modularWeapon.Data != null ? modularWeapon.Data.Delivery.ToString() : "legacy raycast active";
        }

        private string BuildAmmoLine()
        {
            if (modularWeapon == null)
            {
                return "legacy controller";
            }

            return $"{modularWeapon.MagazineAmmo}/{modularWeapon.ReserveAmmo}";
        }
    }
}
