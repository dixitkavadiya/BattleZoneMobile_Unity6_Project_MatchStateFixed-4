using UnityEngine;

namespace BattleZoneMobile
{
    public class PlayerEquipment : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField, Range(0, 3)] private int armorTier = 1;
        [SerializeField, Range(0, 3)] private int helmetTier = 1;

        private static readonly float[] ArmorValues = { 0f, 50f, 75f, 100f };
        private static readonly float[] HelmetMultipliers = { 1f, 0.9f, 0.76f, 0.62f };

        public int ArmorTier => armorTier;
        public int HelmetTier => helmetTier;
        public float HeadshotDamageMultiplier => HelmetMultipliers[Mathf.Clamp(helmetTier, 0, HelmetMultipliers.Length - 1)];

        public void ConfigureForRuntime(Health runtimeHealth)
        {
            health = runtimeHealth;
            ApplyEquipmentStats(true);
        }

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }
        }

        public void EquipArmorTier(int tier)
        {
            armorTier = Mathf.Clamp(tier, 0, 3);
            ApplyEquipmentStats(true);
        }

        public void EquipHelmetTier(int tier)
        {
            helmetTier = Mathf.Clamp(tier, 0, 3);
        }

        public void RestoreArmorPlate(float amount)
        {
            if (health == null)
            {
                return;
            }

            health.RestoreArmor(amount);
        }

        public float ApplyHeadshotMitigation(float damage)
        {
            return Mathf.Max(0f, damage) * HeadshotDamageMultiplier;
        }

        public string BuildEquipmentSummary()
        {
            return $"Armor Tier {armorTier} | Helmet Tier {helmetTier}";
        }

        private void ApplyEquipmentStats(bool refill)
        {
            if (health == null)
            {
                return;
            }

            int clampedTier = Mathf.Clamp(armorTier, 0, ArmorValues.Length - 1);
            health.SetMaxArmor(ArmorValues[clampedTier], refill);
        }
    }
}
