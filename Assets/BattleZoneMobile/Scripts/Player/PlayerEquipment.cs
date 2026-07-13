using UnityEngine;

namespace BattleZoneMobile
{
    public class PlayerEquipment : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField, Range(0, 3)] private int armorTier = 1;
        [SerializeField, Range(0, 3)] private int helmetTier = 1;
        [SerializeField] private float helmetDurability;

        private static readonly float[] ArmorValues = { 0f, 50f, 75f, 100f };
        private static readonly float[] HelmetValues = { 0f, 35f, 55f, 80f };
        private static readonly float[] HelmetMultipliers = { 1f, 0.9f, 0.76f, 0.62f };

        public int ArmorTier => armorTier;
        public int HelmetTier => helmetTier;
        public float ArmorDurability => health != null ? health.CurrentArmor : 0f;
        public float ArmorMaxDurability => health != null ? health.MaxArmor : 0f;
        public float HelmetDurability => helmetDurability;
        public float HelmetMaxDurability => HelmetValues[Mathf.Clamp(helmetTier, 0, HelmetValues.Length - 1)];
        public float HeadshotDamageMultiplier => HelmetMultipliers[Mathf.Clamp(helmetTier, 0, HelmetMultipliers.Length - 1)];

        public void ConfigureForRuntime(Health runtimeHealth)
        {
            health = runtimeHealth;
            helmetDurability = HelmetMaxDurability;
            ApplyEquipmentStats(true);
        }

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<Health>();
            }

            if (helmetDurability <= 0f && helmetTier > 0)
            {
                helmetDurability = HelmetMaxDurability;
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
            helmetDurability = HelmetMaxDurability;
        }

        public bool TryEquipArmorTier(int tier, out string message)
        {
            int clamped = Mathf.Clamp(tier, 0, 3);
            if (clamped < armorTier)
            {
                message = $"Kept Tier {armorTier} Vest";
                return false;
            }

            EquipArmorTier(clamped);
            message = clamped > 0 ? $"Equipped Tier {clamped} Vest" : "Removed Vest";
            return true;
        }

        public bool TryEquipHelmetTier(int tier, out string message)
        {
            int clamped = Mathf.Clamp(tier, 0, 3);
            if (clamped < helmetTier)
            {
                message = $"Kept Tier {helmetTier} Helmet";
                return false;
            }

            EquipHelmetTier(clamped);
            message = clamped > 0 ? $"Equipped Tier {clamped} Helmet" : "Removed Helmet";
            return true;
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
            float safeDamage = Mathf.Max(0f, damage);
            if (helmetTier <= 0 || helmetDurability <= 0f)
            {
                return safeDamage;
            }

            helmetDurability = Mathf.Max(0f, helmetDurability - safeDamage * 0.28f);
            float mitigated = safeDamage * HeadshotDamageMultiplier;
            if (helmetDurability <= 0f)
            {
                helmetTier = 0;
            }

            return mitigated;
        }

        public string BuildEquipmentSummary()
        {
            return $"Vest T{armorTier} {Mathf.CeilToInt(ArmorDurability)}/{Mathf.CeilToInt(ArmorMaxDurability)} | " +
                   $"Helmet T{helmetTier} {Mathf.CeilToInt(HelmetDurability)}/{Mathf.CeilToInt(HelmetMaxDurability)}";
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
