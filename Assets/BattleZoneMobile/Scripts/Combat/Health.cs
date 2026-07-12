using System;
using UnityEngine;
using UnityEngine.Events;

namespace BattleZoneMobile
{
    [Serializable]
    public class HealthChangedEvent : UnityEvent<float, float>
    {
    }

    [Serializable]
    public class ArmorChangedEvent : UnityEvent<float, float>
    {
    }

    [Serializable]
    public class DamageTakenEvent : UnityEvent<float, Vector3, Vector3, GameObject>
    {
    }

    [Serializable]
    public class DeathEvent : UnityEvent<GameObject>
    {
    }

    public class Health : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float maxArmor = 50f;
        [SerializeField, Range(0f, 1f)] private float armorAbsorption = 0.72f;
        [SerializeField] private bool resetOnAwake = true;

        [Header("Events")]
        public HealthChangedEvent onHealthChanged = new HealthChangedEvent();
        public ArmorChangedEvent onArmorChanged = new ArmorChangedEvent();
        public DamageTakenEvent onDamageTaken = new DamageTakenEvent();
        public DeathEvent onDied = new DeathEvent();

        private float currentHealth;
        private float currentArmor;
        private bool dead;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float MaxArmor => maxArmor;
        public float CurrentArmor => currentArmor;
        public bool IsAlive => !dead && currentHealth > 0f;

        private void Awake()
        {
            if (resetOnAwake)
            {
                ResetHealth();
            }
        }

        public void ResetHealth()
        {
            dead = false;
            currentHealth = maxHealth;
            currentArmor = maxArmor;
            onHealthChanged.Invoke(currentHealth, maxHealth);
            onArmorChanged.Invoke(currentArmor, maxArmor);
        }

        public void SetMaxHealth(float value, bool refill)
        {
            maxHealth = Mathf.Max(1f, value);
            if (refill)
            {
                ResetHealth();
            }
            else
            {
                currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
                onHealthChanged.Invoke(currentHealth, maxHealth);
            }
        }

        public void SetMaxArmor(float value, bool refill)
        {
            maxArmor = Mathf.Max(0f, value);
            if (refill)
            {
                currentArmor = maxArmor;
            }
            else
            {
                currentArmor = Mathf.Clamp(currentArmor, 0f, maxArmor);
            }

            onArmorChanged.Invoke(currentArmor, maxArmor);
        }

        public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, GameObject source)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            float remainingDamage = amount;
            if (currentArmor > 0f && armorAbsorption > 0f)
            {
                float absorbed = Mathf.Min(currentArmor, amount * armorAbsorption);
                currentArmor = Mathf.Max(0f, currentArmor - absorbed);
                remainingDamage = Mathf.Max(0f, amount - absorbed);
                onArmorChanged.Invoke(currentArmor, maxArmor);
            }

            currentHealth = Mathf.Max(0f, currentHealth - remainingDamage);
            onDamageTaken.Invoke(amount, hitPoint, hitNormal, source);
            onHealthChanged.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0f)
            {
                dead = true;
                onDied.Invoke(source);
            }
        }

        public void Heal(float amount)
        {
            if (dead || amount <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            onHealthChanged.Invoke(currentHealth, maxHealth);
        }

        public void RestoreArmor(float amount)
        {
            if (dead || amount <= 0f || maxArmor <= 0f)
            {
                return;
            }

            currentArmor = Mathf.Min(maxArmor, currentArmor + amount);
            onArmorChanged.Invoke(currentArmor, maxArmor);
        }

        public void ReviveWithValues(float healthAmount, float armorAmount)
        {
            dead = false;
            currentHealth = Mathf.Clamp(healthAmount, 1f, maxHealth);
            currentArmor = Mathf.Clamp(armorAmount, 0f, maxArmor);
            onHealthChanged.Invoke(currentHealth, maxHealth);
            onArmorChanged.Invoke(currentArmor, maxArmor);
        }
    }
}
