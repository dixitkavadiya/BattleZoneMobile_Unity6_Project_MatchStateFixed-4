using UnityEngine;

namespace BattleZoneMobile
{
    public class AdvancedWeaponPickup : MonoBehaviour
    {
        [SerializeField] private AdvancedWeaponData weaponData;
        [SerializeField] private int reserveAmmoBonus;
        [SerializeField] private TextMesh nameLabel;

        public AdvancedWeaponData WeaponData => weaponData;
        public string DisplayName => weaponData != null ? weaponData.DisplayName : "Unknown Weapon";
        public string RarityLabel => weaponData != null ? weaponData.Rarity.ToString() : "Common";
        public int ReserveAmmoBonus => reserveAmmoBonus;
        private bool pickupConsumed;

        public void Configure(AdvancedWeaponData data, int reserveBonus, TextMesh label)
        {
            weaponData = data;
            reserveAmmoBonus = Mathf.Max(0, reserveBonus);
            nameLabel = label;
            RefreshLabel();
        }

        private void LateUpdate()
        {
            if (nameLabel == null || Camera.main == null)
            {
                return;
            }

            Transform labelTransform = nameLabel.transform;
            Vector3 direction = labelTransform.position - Camera.main.transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                labelTransform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }

        public bool TryPickUp(PlayerInventory inventory)
        {
            if (inventory == null || weaponData == null || pickupConsumed)
            {
                return false;
            }

            pickupConsumed = true;
            inventory.AddAdvancedWeapon(weaponData, reserveAmmoBonus);
            Destroy(gameObject);
            return true;
        }

        public void PickUp(PlayerInventory inventory)
        {
            TryPickUp(inventory);
        }

        private void RefreshLabel()
        {
            if (nameLabel == null)
            {
                return;
            }

            nameLabel.text = weaponData != null ? weaponData.DisplayName : "Weapon";
            nameLabel.anchor = TextAnchor.MiddleCenter;
            nameLabel.alignment = TextAlignment.Center;
            nameLabel.characterSize = 0.18f;
            nameLabel.color = new Color(1f, 0.94f, 0.56f, 1f);
            Font legacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (legacyFont != null)
            {
                nameLabel.font = legacyFont;
                MeshRenderer renderer = nameLabel.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = legacyFont.material;
                }
            }
        }
    }
}
